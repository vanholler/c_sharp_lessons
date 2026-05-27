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

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoRepository.GetAllByUserId(userId);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _toDoRepository.GetActiveByUserId(userId);
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        ValidateString(name);

        if (_toDoRepository.CountActive(user.UserId) >= _maxTaskCount)
            throw new TaskCountLimitException(_maxTaskCount);

        if (name.Length > _maxTaskLength)
            throw new TaskLengthLimitException(name.Length, _maxTaskLength);

        if (_toDoRepository.ExistsByName(user.UserId, name))
            throw new DuplicateTaskException(name);

        var newTask = new ToDoItem(user, name);
        _toDoRepository.Add(newTask);
        return newTask;
    }

    public void MarkCompleted(Guid id)
    {
        var task = _toDoRepository.Get(id);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;
        _toDoRepository.Update(task);
    }

    public void Delete(Guid id)
    {
        var task = _toDoRepository.Get(id);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        _toDoRepository.Delete(id);
    }

    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        return _toDoRepository.Find(
            user.UserId,
            item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
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
