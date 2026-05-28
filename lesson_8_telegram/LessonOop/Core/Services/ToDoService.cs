using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;
using LessonOop.Core.Exceptions;

namespace LessonOop.Core.Services;

public class ToDoService : IToDoService
{
    private readonly IToDoRepository _toDoRepository;
    private readonly int _maxTaskCount;
    private readonly int _maxTaskLength;

    public ToDoService(IToDoRepository toDoRepository, int maxTaskCount, int maxTaskLength)
    {
        _toDoRepository = toDoRepository;
        _maxTaskCount = maxTaskCount;
        _maxTaskLength = maxTaskLength;
    }

    public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
    }

    public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken cancellationToken)
    {
        ValidateString(name);

        int activeCount = await _toDoRepository.CountActiveAsync(user.UserId, cancellationToken);
        if (activeCount >= _maxTaskCount)
            throw new TaskCountLimitException(_maxTaskCount);

        if (name.Length > _maxTaskLength)
            throw new TaskLengthLimitException(name.Length, _maxTaskLength);

        if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, cancellationToken))
            throw new DuplicateTaskException(name);

        var newTask = new ToDoItem(user, name);
        await _toDoRepository.AddAsync(newTask, cancellationToken);
        return newTask;
    }

    public async Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _toDoRepository.GetAsync(id, cancellationToken);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;
        await _toDoRepository.UpdateAsync(task, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _toDoRepository.GetAsync(id, cancellationToken);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        await _toDoRepository.DeleteAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<ToDoItem>> FindAsync(
        ToDoUser user,
        string namePrefix,
        CancellationToken cancellationToken)
    {
        return _toDoRepository.FindAsync(
            user.UserId,
            item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase),
            cancellationToken);
    }

    private static void ValidateString(string? str)
    {
        if (str == null)
            throw new ArgumentException("Строка не может быть null.");
        if (str.Length == 0)
            throw new ArgumentException("Строка не может быть пустой.");
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Строка не может состоять только из пробелов.");
    }
}
