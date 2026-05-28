using LessonOop.Core.Entities;

namespace LessonOop.Core.DataAccess;

public interface IUserRepository
{
    Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken);
    Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
}
