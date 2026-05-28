using System.Text.Json;
using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Infrastructure.DataAccess;

public class FileUserRepository : IUserRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _baseFolder;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileUserRepository(string baseFolder)
    {
        _baseFolder = baseFolder;
        Directory.CreateDirectory(_baseFolder);
    }

    public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetUserFilePath(userId);
        if (!File.Exists(filePath))
            return null;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await ReadUserAsync(filePath, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_baseFolder))
            return null;

        foreach (string filePath in Directory.EnumerateFiles(_baseFolder, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await ReadUserAsync(filePath, cancellationToken);
            if (user?.TelegramUserId == telegramUserId)
                return user;
        }

        return null;
    }

    public async Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetUserFilePath(user.UserId);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            string json = JsonSerializer.Serialize(user, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private string GetUserFilePath(Guid userId) => Path.Combine(_baseFolder, $"{userId}.json");

    private static async Task<ToDoUser?> ReadUserAsync(string filePath, CancellationToken cancellationToken)
    {
        string json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<ToDoUser>(json, JsonOptions);
    }
}
