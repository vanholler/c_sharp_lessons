using LessonOop.Core.Entities;

namespace LessonOop.Core.DataAccess;

public interface IUserRepository
{
    ToDoUser? GetUser(Guid userId);
    ToDoUser? GetUserByTelegramUserId(long telegramUserId);
    void Add(ToDoUser user);
}
