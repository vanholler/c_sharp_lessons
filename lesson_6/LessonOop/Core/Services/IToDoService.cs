using LessonOop.Core.Entities;

namespace LessonOop.Core.Services;

public interface IToDoService
{
    IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
    IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
    ToDoItem Add(ToDoUser user, string name);
    void MarkCompleted(Guid id);
    void Delete(Guid id);
    IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
}
