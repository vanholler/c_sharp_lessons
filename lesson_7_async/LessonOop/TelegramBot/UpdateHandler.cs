using LessonOop.Core.Entities;
using LessonOop.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace LessonOop.TelegramBot;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    public UpdateHandler(
        IUserService userService,
        IToDoService toDoService,
        IToDoReportService toDoReportService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoReportService = toDoReportService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
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
                await botClient.SendMessage(chat, "Пожалуйста, введите команду.", ct);
                return;
            }

            var user = await _userService.GetUserAsync(telegramUserId, ct);
            string command = GetCommandName(text);

            if (user == null && command is not "/help" and not "/info" and not "/start")
            {
                await botClient.SendMessage(chat, "Сначала введите команду /start.", ct);
                return;
            }

            switch (command)
            {
                case "/start":
                    await HandleStartAsync(botClient, chat, telegramUserId, telegramUserName, ct);
                    break;

                case "/help":
                    await HandleHelpAsync(botClient, chat, ct);
                    break;

                case "/info":
                    await HandleInfoAsync(botClient, chat, ct);
                    break;

                case "/addtask":
                    await HandleAddTaskAsync(botClient, chat, user!, text, ct);
                    break;

                case "/removetask":
                    await HandleRemoveTaskAsync(botClient, chat, user!, text, ct);
                    break;

                case "/showtasks":
                    await HandleShowTasksAsync(botClient, chat, user!, ct);
                    break;

                case "/showalltasks":
                    await HandleShowAllTasksAsync(botClient, chat, user!, ct);
                    break;

                case "/completetask":
                    await HandleCompleteTaskAsync(botClient, chat, user!, text, ct);
                    break;

                case "/report":
                    await HandleReportAsync(botClient, chat, user!, ct);
                    break;

                case "/find":
                    await HandleFindAsync(botClient, chat, user!, text, ct);
                    break;

                case "/exit":
                    await HandleExitAsync(botClient, chat, user, ct);
                    break;

                default:
                    await botClient.SendMessage(chat,
                        $"Неизвестная команда: {text}\nДоступные команды: /start, /help, /info, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /report, /find, /exit",
                        ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(chat, ex.Message, ct);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        Console.WriteLine("Произошла ошибка при обработке обновления:");
        Console.WriteLine("Type: " + exception.GetType());
        Console.WriteLine("Message: " + exception.Message);
        Console.WriteLine("StackTrace: " + exception.StackTrace);
        if (exception.InnerException != null)
            Console.WriteLine("InnerException: " + exception.InnerException);
        return Task.CompletedTask;
    }

    private async Task HandleStartAsync(
        ITelegramBotClient botClient,
        Chat chat,
        long telegramUserId,
        string telegramUserName,
        CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(telegramUserId, ct);
        if (user == null)
            user = await _userService.RegisterUserAsync(telegramUserId, telegramUserName, ct);

        await botClient.SendMessage(chat,
            $"Привет, {user.TelegramUserName}. Теперь доступны команды для работы с задачами.",
            ct);
    }

    private static async Task HandleHelpAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
    {
        await botClient.SendMessage(chat,
            "Справка по использованию:\n" +
            "- /start — начать работу\n" +
            "- /help — показать эту справку\n" +
            "- /info — показать информацию о программе\n" +
            "- /addtask <название> — добавить задачу\n" +
            "- /showtasks — показать только активные задачи\n" +
            "- /showalltasks — показать все задачи\n" +
            "- /completetask <id> — завершить задачу по id\n" +
            "- /removetask <номер> — удалить задачу по номеру\n" +
            "- /report — статистика по задачам\n" +
            "- /find <префикс> — найти задачи по началу названия\n" +
            "- /exit — завершить работу (или Ctrl+C)",
            ct);
    }

    private static async Task HandleInfoAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
    {
        await botClient.SendMessage(chat,
            "Версия программы: 1.5.0\n" +
            "Дата создания: 14.12.2025\n" +
            "Имитация работы команд в Telegram",
            ct);
    }

    private async Task HandleAddTaskAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        string input,
        CancellationToken ct)
    {
        string? taskName = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(taskName))
        {
            await botClient.SendMessage(chat, "Используйте: /addtask <название задачи>", ct);
            return;
        }

        var task = await _toDoService.AddAsync(user, taskName.Trim(), ct);
        await botClient.SendMessage(chat, $"Задача '{task.Name}' добавлена.", ct);
    }

    private async Task HandleRemoveTaskAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        string input,
        CancellationToken ct)
    {
        var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
        if (tasks.Count == 0)
        {
            await botClient.SendMessage(chat, "Список задач пуст.", ct);
            return;
        }

        string? numberPart = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(numberPart) || !int.TryParse(numberPart, out int taskNumber))
        {
            await botClient.SendMessage(chat, "Используйте: /removetask <номер>", ct);
            return;
        }

        if (taskNumber < 1 || taskNumber > tasks.Count)
        {
            await botClient.SendMessage(chat, $"Номер задачи должен быть от 1 до {tasks.Count}.", ct);
            return;
        }

        var taskToDelete = tasks[taskNumber - 1];
        await _toDoService.DeleteAsync(taskToDelete.Id, ct);
        await botClient.SendMessage(chat, $"Задача '{taskToDelete.Name}' была удалена.", ct);
    }

    private async Task HandleShowTasksAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        CancellationToken ct)
    {
        var tasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);
        await PrintTasksAsync(botClient, chat, tasks, "Активных задач нет.", ct);
    }

    private async Task HandleShowAllTasksAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        CancellationToken ct)
    {
        var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
        if (tasks.Count == 0)
        {
            await botClient.SendMessage(chat, "Список задач пуст.", ct);
            return;
        }

        foreach (var item in tasks)
        {
            await botClient.SendMessage(chat,
                $"({item.State}) {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}",
                ct);
        }
    }

    private async Task HandleCompleteTaskAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        string input,
        CancellationToken ct)
    {
        string? idPart = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(idPart))
        {
            await botClient.SendMessage(chat, "Используйте: /completetask <id>", ct);
            return;
        }

        if (!Guid.TryParse(idPart, out Guid taskId))
        {
            await botClient.SendMessage(chat, "Неверный формат id задачи.", ct);
            return;
        }

        var userTasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
        if (!userTasks.Any(item => item.Id == taskId))
        {
            await botClient.SendMessage(chat, "Задача с таким id не найдена.", ct);
            return;
        }

        await _toDoService.MarkCompletedAsync(taskId, ct);
        await botClient.SendMessage(chat, "Задача отмечена как выполненная.", ct);
    }

    private async Task HandleReportAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        CancellationToken ct)
    {
        var stats = await _toDoReportService.GetUserStatsAsync(user.UserId, ct);
        await botClient.SendMessage(chat,
            $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}. " +
            $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};",
            ct);
    }

    private async Task HandleFindAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        string input,
        CancellationToken ct)
    {
        string? namePrefix = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(namePrefix))
        {
            await botClient.SendMessage(chat, "Используйте: /find <префикс>", ct);
            return;
        }

        var tasks = await _toDoService.FindAsync(user, namePrefix.Trim(), ct);
        await PrintTasksAsync(botClient, chat, tasks, "Задачи не найдены.", ct);
    }

    private static async Task HandleExitAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser? user,
        CancellationToken ct)
    {
        string goodbye = user != null
            ? $"До свидания, {user.TelegramUserName}!"
            : "До свидания!";
        await botClient.SendMessage(chat, goodbye, ct);
        Environment.Exit(0);
    }

    private static async Task PrintTasksAsync(
        ITelegramBotClient botClient,
        Chat chat,
        IReadOnlyList<ToDoItem> tasks,
        string emptyMessage,
        CancellationToken ct)
    {
        if (tasks.Count == 0)
        {
            await botClient.SendMessage(chat, emptyMessage, ct);
            return;
        }

        foreach (var item in tasks)
        {
            await botClient.SendMessage(chat,
                $"{item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}",
                ct);
        }
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
