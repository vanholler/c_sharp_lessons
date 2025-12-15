using System;

namespace BotApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в бота!");
            Console.WriteLine("Доступные команды: /start, /help, /info, /exit");

            string? userName = null;
            bool isRunning = true;

            while (isRunning)
            {
                Console.Write("> ");
                string? input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Пожалуйста, введите команду.");
                    continue;
                }

                switch (input.ToLower())
                {
                    case "/start":
                        HandleStart(ref userName);
                        break;

                    case "/help":
                        HandleHelp();
                        break;

                    case "/info":
                        HandleInfo();
                        break;

                    case "/exit":
                        isRunning = false;
                        Console.WriteLine(userName != null
                            ? $"До свидания, {userName}!"
                            : "До свидания!");
                        break;

                    default:
                        if (input.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
                        {
                            HandleEcho(input, userName);
                        }
                        else
                        {
                            Console.WriteLine($"Неизвестная команда: {input}");
                            Console.WriteLine("Доступные команды: /start, /help, /info, /exit"
                                + (userName != null ? ", /echo <текст>" : ""));
                        }
                        break;
                }
            }
        }

        static void HandleStart(ref string? userName)
        {
            Console.Write("Пожалуйста, введите ваше имя: ");
            string? name = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Имя не может быть пустым. Попробуйте ещё раз.");
                return;
            }

            userName = name;
            Console.WriteLine($"Привет, {userName}. Теперь доступна команда /echo.");
        }

        static void HandleHelp()
        {
            Console.WriteLine("Справка по использованию:");
            Console.WriteLine("- /start — начать работу и ввести имя");
            Console.WriteLine("- /help — показать эту справку");
            Console.WriteLine("- /info — показать информацию о программе");
            Console.WriteLine("- /echo <текст> — повторяет введённый текст (работает после /start)");
            Console.WriteLine("- /exit — завершить работу");
        }

        static void HandleInfo()
        {
            Console.WriteLine("Версия программы: 1.0.0");
            Console.WriteLine("Дата создания: 02.12.2025");
            Console.WriteLine("Имитация работы команд в Telegram");
        }

        static void HandleEcho(string input, string? userName)
        {
            if (userName == null)
            {
                Console.WriteLine("Сначала введите команду /start и укажите имя.");
                return;
            }

            string echoText = input.Substring(5).TrimStart();

            if (string.IsNullOrWhiteSpace(echoText))
            {
                Console.WriteLine($"{userName}, пожалуйста, укажите текст после /echo (например: /echo Привет)");
                return;
            }

            Console.WriteLine($"{userName}, вы ввели: \"{echoText}\"");
        }
    }
}