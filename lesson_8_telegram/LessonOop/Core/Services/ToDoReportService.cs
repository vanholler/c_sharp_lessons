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

    public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tasks = await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
        int completed = tasks.Count(item => item.State == ToDoItemState.Completed);
        int active = tasks.Count(item => item.State == ToDoItemState.Active);

        return (tasks.Count, completed, active, DateTime.UtcNow);
    }
}
