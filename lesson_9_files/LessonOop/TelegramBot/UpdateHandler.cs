using LessonOop.Core.Entities;
using LessonOop.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

    public IReadOnlyList<BotCommand> MenuCommands { get; } =
    [
        new() { Command = "start", Description = "Начать работу" },
        new() { Command = "help", Description = "Показать справку" },
        new() { Command = "info", Description = "Информация о программе" },
        new() { Command = "addtask", Description = "Добавить задачу: /addtask <текст>" },
        new() { Command = "showtasks", Description = "Показать активные задачи" },
        new() { Command = "showalltasks", Description = "Показать все задачи" },
        new() { Command = "completetask", Description = "Завершить задачу: /completetask <id>" },
        new() { Command = "removetask", Description = "Удалить задачу: /removetask <номер>" },
        new() { Command = "report", Description = "Статистика по задачам" },
        new() { Command = "find", Description = "Поиск задач: /find <префикс>" }
    ];

    private static readonly ReplyKeyboardMarkup NotRegisteredKeyboard = new(
        [[new KeyboardButton("/start")]])
    {
        ResizeKeyboard = true
    };

    private static readonly ReplyKeyboardMarkup RegisteredKeyboard = new(
    [
        [new KeyboardButton("/showalltasks"), new KeyboardButton("/showtasks")],
        [new KeyboardButton("/report")]
    ])
    {
        ResizeKeyboard = true
    };

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message is not { Text: not null, From: not null, Chat: not null } message)
            return;

        var chat = message.Chat;
        string text = message.Text.Trim();
        long telegramUserId = message.From.Id;
        string telegramUserName = message.From.Username ?? message.From.Id.ToString();
        ToDoUser? user = null;

        try
        {
            if (string.IsNullOrEmpty(text))
            {
                await SendTextAsync(botClient, chat.Id, "Пожалуйста, введите команду.", null, ct);
                return;
            }

            user = await _userService.GetUserAsync(telegramUserId, ct);
            string command = GetCommandName(text);

            if (user == null && command is not "/help" and not "/info" and not "/start")
            {
                await SendTextAsync(botClient, chat.Id, "Сначала введите команду /start.", NotRegisteredKeyboard, ct);
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
                    await SendTextAsync(botClient, chat.Id,
                        $"Неизвестная команда: {text}\nДоступные команды: /start, /help, /info, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /report, /find, /exit",
                        user == null ? NotRegisteredKeyboard : RegisteredKeyboard,
                        ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            await SendTextAsync(botClient, chat.Id, ex.Message, user == null ? NotRegisteredKeyboard : RegisteredKeyboard, ct);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        Console.WriteLine("Произошла ошибка при обработке обновления:");
        Console.WriteLine("Source: " + source);
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

        await SendTextAsync(botClient, chat.Id,
            $"Привет, {user.TelegramUserName}. Теперь доступны команды для работы с задачами.",
            RegisteredKeyboard,
            ct);
    }

    private static async Task HandleHelpAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
    {
        await SendTextAsync(botClient, chat.Id,
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
            "- /exit — завершить работу (или клавиша A в консоли)",
            null,
            ct);
    }

    private static async Task HandleInfoAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
    {
        await SendTextAsync(botClient, chat.Id,
            "Версия программы: 1.5.0\n" +
            "Дата создания: 14.12.2025\n" +
            "Имитация работы команд в Telegram",
            null,
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
            await SendTextAsync(botClient, chat.Id, "Используйте: /addtask <название задачи>", RegisteredKeyboard, ct);
            return;
        }

        var task = await _toDoService.AddAsync(user, taskName.Trim(), ct);
        await SendTextAsync(botClient, chat.Id, $"Задача '{task.Name}' добавлена.", RegisteredKeyboard, ct);
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
            await SendTextAsync(botClient, chat.Id, "Список задач пуст.", RegisteredKeyboard, ct);
            return;
        }

        string? numberPart = GetCommandArgument(input);
        if (string.IsNullOrWhiteSpace(numberPart) || !int.TryParse(numberPart, out int taskNumber))
        {
            await SendTextAsync(botClient, chat.Id, "Используйте: /removetask <номер>", RegisteredKeyboard, ct);
            return;
        }

        if (taskNumber < 1 || taskNumber > tasks.Count)
        {
            await SendTextAsync(botClient, chat.Id, $"Номер задачи должен быть от 1 до {tasks.Count}.", RegisteredKeyboard, ct);
            return;
        }

        var taskToDelete = tasks[taskNumber - 1];
        await _toDoService.DeleteAsync(taskToDelete.Id, ct);
        await SendTextAsync(botClient, chat.Id, $"Задача '{taskToDelete.Name}' была удалена.", RegisteredKeyboard, ct);
    }

    private async Task HandleShowTasksAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        CancellationToken ct)
    {
        var tasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);
        await PrintTasksAsync(botClient, chat.Id, tasks, "Активных задач нет.", true, ct);
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
            await SendTextAsync(botClient, chat.Id, "Список задач пуст.", RegisteredKeyboard, ct);
            return;
        }

        foreach (var item in tasks)
        {
            await SendTextAsync(botClient, chat.Id,
                $"({item.State}) {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - `{item.Id}`",
                RegisteredKeyboard,
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
            await SendTextAsync(botClient, chat.Id, "Используйте: /completetask <id>", RegisteredKeyboard, ct);
            return;
        }

        if (!Guid.TryParse(idPart, out Guid taskId))
        {
            await SendTextAsync(botClient, chat.Id, "Неверный формат id задачи.", RegisteredKeyboard, ct);
            return;
        }

        var userTasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
        if (!userTasks.Any(item => item.Id == taskId))
        {
            await SendTextAsync(botClient, chat.Id, "Задача с таким id не найдена.", RegisteredKeyboard, ct);
            return;
        }

        await _toDoService.MarkCompletedAsync(taskId, ct);
        await SendTextAsync(botClient, chat.Id, "Задача отмечена как выполненная.", RegisteredKeyboard, ct);
    }

    private async Task HandleReportAsync(
        ITelegramBotClient botClient,
        Chat chat,
        ToDoUser user,
        CancellationToken ct)
    {
        var stats = await _toDoReportService.GetUserStatsAsync(user.UserId, ct);
        await SendTextAsync(botClient, chat.Id,
            $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}. " +
            $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};",
            RegisteredKeyboard,
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
            await SendTextAsync(botClient, chat.Id, "Используйте: /find <префикс>", RegisteredKeyboard, ct);
            return;
        }

        var tasks = await _toDoService.FindAsync(user, namePrefix.Trim(), ct);
        await PrintTasksAsync(botClient, chat.Id, tasks, "Задачи не найдены.", true, ct);
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
        await SendTextAsync(botClient, chat.Id, goodbye, RegisteredKeyboard, ct);
        Environment.Exit(0);
    }

    private static async Task PrintTasksAsync(
        ITelegramBotClient botClient,
        long chatId,
        IReadOnlyList<ToDoItem> tasks,
        string emptyMessage,
        bool wrapId,
        CancellationToken ct)
    {
        if (tasks.Count == 0)
        {
            await SendTextAsync(botClient, chatId, emptyMessage, RegisteredKeyboard, ct);
            return;
        }

        foreach (var item in tasks)
        {
            string id = wrapId ? $"`{item.Id}`" : item.Id.ToString();
            await SendTextAsync(botClient, chatId,
                $"{item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {id}",
                RegisteredKeyboard,
                ct);
        }
    }

    private static Task SendTextAsync(
        ITelegramBotClient botClient,
        long chatId,
        string text,
        ReplyKeyboardMarkup? keyboard,
        CancellationToken ct)
    {
        return botClient.SendMessage(
            chatId: chatId,
            text: text,
            cancellationToken: ct,
            replyMarkup: keyboard);
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
