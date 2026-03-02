using System;
using System.Collections.Generic;

namespace LessonCatch
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в бота!");
            Console.WriteLine("Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit");

            int maxTaskCount = 0;
            while (true)
            {
                try
                {
                    Console.WriteLine("Введите максимально допустимое количество задач");
                    string? inputCount = Console.ReadLine();
                    maxTaskCount = ParseAndValidateInt(inputCount, 1, 100);
                    break;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            int maxTaskLength = 0;
            while (true)
            {
                try
                {
                    Console.WriteLine("Введите максимально допустимую длину задачи");
                    string? inputLength = Console.ReadLine();
                    maxTaskLength = ParseAndValidateInt(inputLength, 1, 100);
                    break;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            string? userName = null;
            var taskList = new List<string>();
            bool isRunning = true;

            while (isRunning)
            {
                Console.Write("> ");
                string? input = Console.ReadLine()?.Trim();

                try
                {
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
                            HandleAddTask(ref taskList, maxTaskCount, maxTaskLength);
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
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskLengthLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (DuplicateTaskException ex)
                {
                    Console.WriteLine(ex.Message);
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

        static void ValidateString(string? str)
        {
            if (str == null)
                throw new ArgumentException("Строка не может быть null.");
            if (str.Length == 0)
                throw new ArgumentException("Строка не может быть пустой.");
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не может состоять только из пробелов.");
        }

        static void HandleAddTask(ref List<string> taskList, int maxTaskCount, int maxTaskLength)
        {
            Console.Write("Пожалуйста, введите описание задачи: ");
            string? userTask = Console.ReadLine()?.Trim();

            ValidateString(userTask);

            if (taskList.Count >= maxTaskCount)
                throw new TaskCountLimitException(maxTaskCount);

            if (userTask!.Length > maxTaskLength)
                throw new TaskLengthLimitException(userTask.Length, maxTaskLength);

            if (taskList.Contains(userTask))
                throw new DuplicateTaskException(userTask);

            taskList.Add(userTask);
            Console.WriteLine($"Задача '{userTask}' добавлена.");
        }

        static void HandleShowTask(ref List<string> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("Список задач пуст. Используйте команду /addtask");
                return;
            }
            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {taskList[i]}");
            }
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
            string? taskNum = Console.ReadLine();

            int taskIntNum = ParseAndValidateInt(taskNum, 1, taskList.Count);

            string deletedTask = taskList[taskIntNum - 1];
            taskList.RemoveAt(taskIntNum - 1);

            Console.WriteLine($"Задача '{deletedTask}' была удалена.");
        }

        static void HandleStart(ref string? userName)
        {
            Console.Write("Пожалуйста, введите ваше имя: ");
            string? name = Console.ReadLine()?.Trim();

            ValidateString(name);

            userName = name!;
            Console.WriteLine($"Привет, {userName}. Теперь доступна команда /echo.");
        }

        static void HandleHelp()
        {
            Console.WriteLine("Справка по использованию:");
            Console.WriteLine("- /start — начать работу и ввести имя");
            Console.WriteLine("- /help — показать эту справку");
            Console.WriteLine("- /info — показать информацию о программе");
            Console.WriteLine("- /echo <текст> — повторяет введённый текст (работает после /start)");
            Console.WriteLine("- /addtask — добавить задачу");
            Console.WriteLine("- /showtasks — показать задачи");
            Console.WriteLine("- /removetask — удалить задачу");
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
