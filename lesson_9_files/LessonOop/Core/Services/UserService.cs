using LessonOop.Core.DataAccess;
using LessonOop.Core.Entities;

namespace LessonOop.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ToDoUser> RegisterUserAsync(
        long telegramUserId,
        string telegramUserName,
        CancellationToken cancellationToken)
    {
        var user = new ToDoUser(telegramUserId, telegramUserName);
        await _userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    public Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        return _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);
    }
}
