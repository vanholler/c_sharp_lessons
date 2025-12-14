using System;
using System.Diagnostics;
using System.Reflection;

namespace BotApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в бота!");
            Console.WriteLine("Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit");

            string? userName = null;
            string? task = null;
            var taskList = new List<string>();
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

                    case "/addtask":
                        HandleAddTask(ref taskList); 
                        break;

                    case "/removetask":
                        HandleRemoveTask(ref taskList);
                        break;

                    case "/showtasks":
                        HandleShowTask(ref taskList);
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
                            Console.WriteLine("Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit"
                                + (userName != null ? ", /echo <текст>" : ""));
                        }
                        break;
                }
            }
        }

        static void HandleAddTask(ref List<string> taskList)
        {
            Console.Write("Пожалуйста, введите описание задачи: ");
            string? userTask = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userTask))
            {
                Console.WriteLine("Задача не может быть пустой. Попробуйте ещё раз.");
                return;
            }

            taskList.Add(userTask);
            Console.WriteLine($"Задача  '{userTask}' добавлена. /echo.");
        }

        static void HandleShowTask(ref List<string> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("Список задач пуст. Используйте команду/addtask");
                return;
            }
            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {taskList[i]}");
            }
            // string.Join(", ", taskList) альтернатиный способ - списка в консоль.
        }

        static void HandleRemoveTask(ref List<string> taskList)
        {

            if (taskList.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.WriteLine("Вот ваш список задач:");
            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {taskList[i]}");
            }
            Console.Write("Введите номер задачи для удаления: ");
            string taskNum = Console.ReadLine();

            if (!int.TryParse(taskNum, out int taskIntNum))
            {
                Console.WriteLine("Некорректный ввод. Введите число.");
                return;
            }
            Console.WriteLine($"{taskIntNum}");

            if (taskIntNum < 0 || taskIntNum - 1 >= taskList.Count )
            {
                Console.WriteLine($"Такого значения нет");
                return;
            }

            string deletedTask = taskList[taskIntNum - 1];
            taskList.RemoveAt(taskIntNum - 1);

            Console.WriteLine($"Задача '{deletedTask}', была удалена");
        }

        static void HandleStart(ref string? userName)
        {
            Console.Write("Пожалуйста, введите ваше имя: ");
            string? name = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(name))
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
            Console.WriteLine("Версия программы: 1.1.0");
            Console.WriteLine("Дата создания: 14.12.2025");
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

            if (string.IsNullOrEmpty(echoText))
            {
                Console.WriteLine($"{userName}, пожалуйста, укажите текст после /echo (например: /echo Привет)");
                return;
            }

            Console.WriteLine($"{userName}, вы ввели: \"{echoText}\"");
        }
    }
}