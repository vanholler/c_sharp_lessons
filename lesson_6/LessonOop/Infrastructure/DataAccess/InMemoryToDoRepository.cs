using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _items = new();

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _items.Where(item => item.User.UserId == userId).ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _items
            .Where(item => item.User.UserId == userId && item.State == ToDoItemState.Active)
            .ToList();
    }

    public ToDoItem? Get(Guid id)
    {
        return _items.FirstOrDefault(item => item.Id == id);
    }

    public void Add(ToDoItem item)
    {
        _items.Add(item);
    }

    public void Update(ToDoItem item)
    {
    }

    public void Delete(Guid id)
    {
        var task = Get(id);
        if (task != null)
            _items.Remove(task);
    }

    public bool ExistsByName(Guid userId, string name)
    {
        return _items.Any(item => item.User.UserId == userId && item.Name == name);
    }

    public int CountActive(Guid userId)
    {
        return _items.Count(item => item.User.UserId == userId && item.State == ToDoItemState.Active);
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _items
            .Where(item => item.User.UserId == userId && predicate(item))
            .ToList();
    }
}
