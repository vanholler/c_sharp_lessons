using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace LessonOop;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    public UpdateHandler(IUserService userService, IToDoService toDoService)
    {
        _userService = userService;
        _toDoService = toDoService;
    }

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        var message = update.Message;
        var chat = message.Chat;
        string text = message.Text.Trim();
        long telegramUserId = message.From.Id;
        string telegramUserName = message.From.Username ?? message.From.Id.ToString();

        try
        {
            if (string.IsNullOrEmpty(text))
            {
                botClient.SendMessage(chat, "Пожалуйста, введите команду.");
                return;
            }

            var user = _userService.GetUser(telegramUserId);
            string command = GetCommandName(text);

            if (user == null && command is not "/help" and not "/info" and not "/start")
            {
                botClient.SendMessage(chat, "Сначала введите команду /start.");
                return;
            }

            switch (command)
            {
                case "/start":
                    HandleStart(botClient, chat, telegramUserId, telegramUserName);
                    break;

                case "/help":
                    HandleHelp(botClient, chat);
                    break;

                case "/info":
                    HandleInfo(botClient, chat);
                    break;

                case "/addtask":
                    HandleAddTask(botClient, chat, user!, text);
                    break;

                case "/removetask":
                    HandleRemoveTask(botClient, chat, user!, text);
                    break;

                case "/showtasks":
                    HandleShowTasks(botClient, chat, user!);
                    break;

                case "/showalltasks":
                    HandleShowAllTasks(botClient, chat, user!);
                    break;

                case "/completetask":
                    HandleCompleteTask(botClient, chat, user!, text);
                    break;

                case "/exit":
                    HandleExit(botClient, chat, user);
                    break;

                default:
                    botClient.SendMessage(chat,
                        $"Неизвестная команда: {text}\nДоступные команды: /start, /help, /info, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /exit");
                    break;
            }
        }
        catch (Exception ex)
        {
            botClient.SendMessage(chat, ex.Message);
        }
    }

    private void HandleStart(
        ITelegramBotClient botClient,
        Chat chat,
        long telegramUserId,
        string telegramUserName)
    {
        var user = _userService.GetUser(telegramUserId);
        if (user == null)
            user = _userService.RegisterUser(telegramUserId, telegramUserName);

        botClient.SendMessage(chat,
            $"Привет, {user.TelegramUserName}. Теперь доступны команды для работы с задачами.");
    }

    private static void HandleHelp(ITelegramBotClient botClient, Chat chat)
    {
        botClient.SendMessage(chat,
            "Справка по использованию:\n" +
            "- /start — начать работу\n" +
            "- /help — показать эту справку\n" +
            "- /info — показать информацию о программе\n" +
            "- /addtask <название> — добавить задачу\n" +
            "- /showtasks — показать только активные задачи\n" +
            "- /showalltasks — показать все задачи\n" +
            "- /completetask <id> — завершить задачу по id\n" +
            "- /removetask <номер> — удалить задачу по номеру\n" +
            "- /exit — завершить работу (или Ctrl+C)");
    }

    private static void HandleInfo(ITelegramBotClient botClient, Chat chat)
    {
        botClient.SendMessage(chat,
            "Версия программы: 1.3.0\n" +
            "Дата создания: 14.12.2025\n" +
            "Имитация работы команд в Telegram");
    }

    private void HandleAddTask(ITelegramBotClient botClient, Chat chat, ToDoUser user, string input)
    {
        string? taskName = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(taskName))
        {
            botClient.SendMessage(chat, "Используйте: /addtask <название задачи>");
            return;
        }

        var task = _toDoService.Add(user, taskName.Trim());
        botClient.SendMessage(chat, $"Задача '{task.Name}' добавлена.");
    }

    private void HandleRemoveTask(ITelegramBotClient botClient, Chat chat, ToDoUser user, string input)
    {
        var tasks = _toDoService.GetAllByUserId(user.UserId);
        if (tasks.Count == 0)
        {
            botClient.SendMessage(chat, "Список задач пуст.");
            return;
        }

        string? numberPart = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(numberPart) || !int.TryParse(numberPart, out int taskNumber))
        {
            botClient.SendMessage(chat, "Используйте: /removetask <номер>");
            return;
        }

        if (taskNumber < 1 || taskNumber > tasks.Count)
        {
            botClient.SendMessage(chat, $"Номер задачи должен быть от 1 до {tasks.Count}.");
            return;
        }

        var taskToDelete = tasks[taskNumber - 1];
        _toDoService.Delete(taskToDelete.Id);
        botClient.SendMessage(chat, $"Задача '{taskToDelete.Name}' была удалена.");
    }

    private void HandleShowTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user)
    {
        var tasks = _toDoService.GetActiveByUserId(user.UserId);
        if (tasks.Count == 0)
        {
            botClient.SendMessage(chat, "Активных задач нет.");
            return;
        }

        foreach (var item in tasks)
        {
            botClient.SendMessage(chat,
                $"{item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
        }
    }

    private void HandleShowAllTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user)
    {
        var tasks = _toDoService.GetAllByUserId(user.UserId);
        if (tasks.Count == 0)
        {
            botClient.SendMessage(chat, "Список задач пуст.");
            return;
        }

        foreach (var item in tasks)
        {
            botClient.SendMessage(chat,
                $"({item.State}) {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
        }
    }

    private void HandleCompleteTask(ITelegramBotClient botClient, Chat chat, ToDoUser user, string input)
    {
        string? idPart = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(idPart))
        {
            botClient.SendMessage(chat, "Используйте: /completetask <id>");
            return;
        }

        if (!Guid.TryParse(idPart, out Guid taskId))
        {
            botClient.SendMessage(chat, "Неверный формат id задачи.");
            return;
        }

        var userTasks = _toDoService.GetAllByUserId(user.UserId);
        bool belongsToUser = false;
        foreach (var item in userTasks)
        {
            if (item.Id == taskId)
            {
                belongsToUser = true;
                break;
            }
        }

        if (!belongsToUser)
        {
            botClient.SendMessage(chat, "Задача с таким id не найдена.");
            return;
        }

        _toDoService.MarkCompleted(taskId);
        botClient.SendMessage(chat, "Задача отмечена как выполненная.");
    }

    private static void HandleExit(ITelegramBotClient botClient, Chat chat, ToDoUser? user)
    {
        string goodbye = user != null
            ? $"До свидания, {user.TelegramUserName}!"
            : "До свидания!";
        botClient.SendMessage(chat, goodbye);
        Environment.Exit(0);
    }

    private static string GetCommandName(string input)
    {
        int spaceIndex = input.IndexOf(' ');
        return spaceIndex < 0 ? input.ToLower() : input[..spaceIndex].ToLower();
    }

    private static string? GetCommandArgument(string input)
    {
        int spaceIndex = input.IndexOf(' ');
        if (spaceIndex < 0 || spaceIndex == input.Length - 1)
            return null;

        return input[(spaceIndex + 1)..].Trim();
    }
}
