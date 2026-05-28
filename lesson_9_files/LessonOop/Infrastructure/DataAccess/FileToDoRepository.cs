using System.Text.Json;
using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Infrastructure.DataAccess;

public class FileToDoRepository : IToDoRepository
{
    private const string IndexFileName = "index.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _baseFolder;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileToDoRepository(string baseFolder)
    {
        _baseFolder = baseFolder;
        Directory.CreateDirectory(_baseFolder);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var items = new List<ToDoItem>();
        string userFolder = GetUserFolderPath(userId);
        if (!Directory.Exists(userFolder))
            return items;

        foreach (string filePath in Directory.EnumerateFiles(userFolder, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await ReadItemAsync(filePath, cancellationToken);
            if (item != null)
                items.Add(item);
        }

        return items;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await GetAllByUserIdAsync(userId, cancellationToken);
        return items.Where(item => item.State == ToDoItemState.Active).ToList();
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var index = await LoadIndexAsync(cancellationToken);
            if (!index.Items.TryGetValue(id, out Guid userId))
                return null;

            string filePath = GetItemFilePath(userId, id);
            if (!File.Exists(filePath))
                return null;

            return await ReadItemAsync(filePath, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string userFolder = GetUserFolderPath(item.User.UserId);
        Directory.CreateDirectory(userFolder);

        string filePath = GetItemFilePath(item.User.UserId, item.Id);
        string json = JsonSerializer.Serialize(item, JsonOptions);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            var index = await LoadIndexAsync(cancellationToken);
            index.Items[item.Id] = item.User.UserId;
            await SaveIndexAsync(index, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetItemFilePath(item.User.UserId, item.Id);
        string json = JsonSerializer.Serialize(item, JsonOptions);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath))
                throw new ArgumentException("Задача с таким id не найдена.");

            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var index = await LoadIndexAsync(cancellationToken);
            if (!index.Items.TryGetValue(id, out Guid userId))
                return;

            string filePath = GetItemFilePath(userId, id);
            if (File.Exists(filePath))
                File.Delete(filePath);

            index.Items.Remove(id);
            await SaveIndexAsync(index, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
    {
        var items = await GetAllByUserIdAsync(userId, cancellationToken);
        return items.Any(item => item.Name == name);
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await GetActiveByUserIdAsync(userId, cancellationToken);
        return items.Count;
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(
        Guid userId,
        Func<ToDoItem, bool> predicate,
        CancellationToken cancellationToken)
    {
        var items = await GetAllByUserIdAsync(userId, cancellationToken);
        return items.Where(predicate).ToList();
    }

    private string GetUserFolderPath(Guid userId) => Path.Combine(_baseFolder, userId.ToString());

    private string GetItemFilePath(Guid userId, Guid itemId) =>
        Path.Combine(GetUserFolderPath(userId), $"{itemId}.json");

    private string GetIndexFilePath() => Path.Combine(_baseFolder, IndexFileName);

    private async Task<ToDoItemIndex> LoadIndexAsync(CancellationToken cancellationToken)
    {
        string indexPath = GetIndexFilePath();
        if (!File.Exists(indexPath))
        {
            var index = await ScanIndexAsync(cancellationToken);
            await SaveIndexAsync(index, cancellationToken);
            return index;
        }

        string json = await File.ReadAllTextAsync(indexPath, cancellationToken);
        return JsonSerializer.Deserialize<ToDoItemIndex>(json, JsonOptions) ?? new ToDoItemIndex();
    }

    private async Task SaveIndexAsync(ToDoItemIndex index, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(index, JsonOptions);
        await File.WriteAllTextAsync(GetIndexFilePath(), json, cancellationToken);
    }

    private async Task<ToDoItemIndex> ScanIndexAsync(CancellationToken cancellationToken)
    {
        var index = new ToDoItemIndex();

        if (!Directory.Exists(_baseFolder))
            return index;

        foreach (string userFolder in Directory.EnumerateDirectories(_baseFolder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Guid.TryParse(Path.GetFileName(userFolder), out Guid userId))
                continue;

            foreach (string filePath in Directory.EnumerateFiles(userFolder, "*.json"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await ReadItemAsync(filePath, cancellationToken);
                if (item != null)
                    index.Items[item.Id] = userId;
            }
        }

        return index;
    }

    private static async Task<ToDoItem?> ReadItemAsync(string filePath, CancellationToken cancellationToken)
    {
        string json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<ToDoItem>(json, JsonOptions);
    }
}
