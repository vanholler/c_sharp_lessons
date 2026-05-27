using LessonOop.Core.Entities;

namespace LessonOop.Core.Services;

public interface IUserService
{
    Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
    Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
}
