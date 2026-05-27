namespace LessonOop;

public class ToDoService : IToDoService
{
    private readonly List<ToDoItem> _tasks = new();
    private readonly int _maxTaskCount;
    private readonly int _maxTaskLength;

    public ToDoService(int maxTaskCount, int maxTaskLength)
    {
        _maxTaskCount = maxTaskCount;
        _maxTaskLength = maxTaskLength;
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _tasks.Where(t => t.User.UserId == userId).ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        ValidateString(name);

        var userTasks = GetAllByUserId(user.UserId);
        if (userTasks.Count >= _maxTaskCount)
            throw new TaskCountLimitException(_maxTaskCount);

        if (name.Length > _maxTaskLength)
            throw new TaskLengthLimitException(name.Length, _maxTaskLength);

        foreach (var item in userTasks)
        {
            if (item.Name == name)
                throw new DuplicateTaskException(name);
        }

        var newTask = new ToDoItem(user, name);
        _tasks.Add(newTask);
        return newTask;
    }

    public void MarkCompleted(Guid id)
    {
        var task = FindTaskById(id);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;
    }

    public void Delete(Guid id)
    {
        var task = FindTaskById(id);
        if (task == null)
            throw new ArgumentException("Задача с таким id не найдена.");

        _tasks.Remove(task);
    }

    private ToDoItem? FindTaskById(Guid id)
    {
        foreach (var item in _tasks)
        {
            if (item.Id == id)
                return item;
        }

        return null;
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
