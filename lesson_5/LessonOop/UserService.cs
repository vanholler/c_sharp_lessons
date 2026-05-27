namespace LessonOop;

public class UserService : IUserService
{
    private readonly List<ToDoUser> _users = new();

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        var user = new ToDoUser(telegramUserId, telegramUserName);
        _users.Add(user);
        return user;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        foreach (var user in _users)
        {
            if (user.TelegramUserId == telegramUserId)
                return user;
        }

        return null;
    }
}
