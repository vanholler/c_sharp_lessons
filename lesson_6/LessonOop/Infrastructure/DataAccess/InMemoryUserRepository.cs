using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new();

    public ToDoUser? GetUser(Guid userId)
    {
        return _users.FirstOrDefault(user => user.UserId == userId);
    }

    public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
    {
        return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
    }

    public void Add(ToDoUser user)
    {
        _users.Add(user);
    }
}
