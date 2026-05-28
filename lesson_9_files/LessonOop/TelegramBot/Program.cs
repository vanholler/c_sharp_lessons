using LessonOop.Core.Services;
using LessonOop.Infrastructure.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace LessonOop.TelegramBot;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в бота!");
        Console.WriteLine("Доступные команды: /start, /help, /info, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /report, /find, /exit");

        int maxTaskCount = ReadIntSetting("Введите максимально допустимое количество задач", 1, 100);
        int maxTaskLength = ReadIntSetting("Введите максимально допустимую длину задачи", 1, 100);

        const string usersFolder = "data/users";
        const string tasksFolder = "data/tasks";

        var userRepository = new FileUserRepository(usersFolder);
        var toDoRepository = new FileToDoRepository(tasksFolder);

        var userService = new UserService(userRepository);
        var toDoService = new ToDoService(toDoRepository, maxTaskCount, maxTaskLength);
        var reportService = new ToDoReportService(toDoRepository);

        string? botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(botToken))
        {
            Console.WriteLine("Переменная окружения TELEGRAM_BOT_TOKEN не задана.");
            return;
        }

        var handler = new UpdateHandler(userService, toDoService, reportService);
        var botClient = new TelegramBotClient(botToken);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message],
            DropPendingUpdates = true
        };

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };

        try
        {
            await botClient.SetMyCommands(
                handler.MenuCommands,
                cancellationToken: cts.Token);

            botClient.StartReceiving(handler, receiverOptions, cts.Token);

            var me = await botClient.GetMe(cts.Token);
            Console.WriteLine($"{me.FirstName} запущен!");
            Console.WriteLine("Нажмите клавишу A для выхода");

            while (!cts.IsCancellationRequested)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.A)
                {
                    cts.Cancel();
                    break;
                }

                me = await botClient.GetMe(cts.Token);
                Console.WriteLine($"Бот: @{me.Username} ({me.FirstName})");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine("Произошла непредвиденная ошибка: ");
            Console.WriteLine("Type: " + ex.GetType());
            Console.WriteLine("Message: " + ex.Message);
            Console.WriteLine("StackTrace: " + ex.StackTrace);
            Console.WriteLine("InnerException: " + ex.InnerException);
        }
    }

    static int ReadIntSetting(string prompt, int min, int max)
    {
        while (true)
        {
            try
            {
                Console.WriteLine(prompt);
                string? input = Console.ReadLine();
                return ParseAndValidateInt(input, min, max);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Строка не может быть пустой.");

        if (!int.TryParse(str.Trim(), out int result))
            throw new ArgumentException("Введено не число.");

        if (result < min || result > max)
            throw new ArgumentException($"Значение должно быть от {min} до {max}.");

        return result;
    }
}
