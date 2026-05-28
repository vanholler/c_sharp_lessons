namespace LessonOop.Core.Entities;

public class ToDoItem
{
    public Guid Id { get; set; }
    public ToDoUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ToDoItemState State { get; set; }
    public DateTime? StateChangedAt { get; set; }

    public ToDoItem()
    {
    }

    public ToDoItem(ToDoUser user, string name)
    {
        Id = Guid.NewGuid();
        User = user;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        State = ToDoItemState.Active;
        StateChangedAt = null;
    }
}
