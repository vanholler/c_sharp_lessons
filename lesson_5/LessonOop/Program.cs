using Otus.ToDoList.ConsoleBot;

namespace LessonOop;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в бота!");
        Console.WriteLine("Доступные команды: /start, /help, /info, /addtask, /showtasks, /showalltasks, /completetask, /removetask");
        Console.WriteLine("Для выхода нажмите Ctrl+C");

        int maxTaskCount = ReadIntSetting("Введите максимально допустимое количество задач", 1, 100);
        int maxTaskLength = ReadIntSetting("Введите максимально допустимую длину задачи", 1, 100);

        var userService = new UserService();
        var toDoService = new ToDoService(maxTaskCount, maxTaskLength);
        var handler = new UpdateHandler(userService, toDoService);
        var botClient = new ConsoleBotClient();

        try
        {
            botClient.StartReceiving(handler);
        }
        catch (Exception ex)
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
