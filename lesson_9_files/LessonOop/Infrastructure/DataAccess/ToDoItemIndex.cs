namespace LessonOop.Infrastructure.DataAccess;

public class ToDoItemIndex
{
    public Dictionary<Guid, Guid> Items { get; set; } = new();
}
