using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Core.Services;

public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        var tasks = _toDoRepository.GetAllByUserId(userId);
        int completed = tasks.Count(item => item.State == ToDoItemState.Completed);
        int active = tasks.Count(item => item.State == ToDoItemState.Active);

        return (tasks.Count, completed, active, DateTime.UtcNow);
    }
}
