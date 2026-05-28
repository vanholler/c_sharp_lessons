using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _items = new();

    public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<ToDoItem> result = _items.Where(item => item.User.UserId == userId).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<ToDoItem> result = _items
            .Where(item => item.User.UserId == userId && item.State == ToDoItemState.Active)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var task = _items.FirstOrDefault(item => item.Id == id);
        if (task != null)
            _items.Remove(task);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool exists = _items.Any(item => item.User.UserId == userId && item.Name == name);
        return Task.FromResult(exists);
    }

    public Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int count = _items.Count(item => item.User.UserId == userId && item.State == ToDoItemState.Active);
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<ToDoItem>> FindAsync(
        Guid userId,
        Func<ToDoItem, bool> predicate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<ToDoItem> result = _items
            .Where(item => item.User.UserId == userId && predicate(item))
            .ToList();
        return Task.FromResult(result);
    }
}
