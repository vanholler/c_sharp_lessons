using System;
using System.Collections.Generic;

namespace LessonOop
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в бота!");
            Console.WriteLine("Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /exit");

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

            ToDoUser? currentUser = null;
            var taskList = new List<ToDoItem>();
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
                            HandleStart(ref currentUser);
                            break;

                        case "/help":
                            HandleHelp();
                            break;

                        case "/info":
                            HandleInfo();
                            break;

                        case "/addtask":
                            HandleAddTask(ref taskList, currentUser, maxTaskCount, maxTaskLength);
                            break;

                        case "/removetask":
                            HandleRemoveTask(ref taskList);
                            break;

                        case "/showtasks":
                            HandleShowTask(ref taskList);
                            break;

                        case "/showalltasks":
                            HandleShowAllTask(ref taskList);
                            break;

                        case "/exit":
                            isRunning = false;
                            Console.WriteLine(currentUser != null
                                ? $"До свидания, {currentUser.TelegramUserName}!"
                                : "До свидания!");
                            break;

                        default:
                            if (input.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
                            {
                                HandleEcho(input, currentUser);
                            }
                            else if (input.StartsWith("/completetask", StringComparison.OrdinalIgnoreCase))
                            {
                                HandleCompleteTask(input, ref taskList);
                            }
                            else
                            {
                                Console.WriteLine($"Неизвестная команда: {input}");
                                Console.WriteLine("Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /showalltasks, /completetask, /removetask, /exit");
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

        static void HandleAddTask(ref List<ToDoItem> taskList, ToDoUser? currentUser, int maxTaskCount, int maxTaskLength)
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала введите команду /start и укажите имя.");
                return;
            }

            Console.Write("Пожалуйста, введите описание задачи: ");
            string? userTask = Console.ReadLine()?.Trim();

            ValidateString(userTask);

            if (taskList.Count >= maxTaskCount)
                throw new TaskCountLimitException(maxTaskCount);

            if (userTask!.Length > maxTaskLength)
                throw new TaskLengthLimitException(userTask.Length, maxTaskLength);

            bool hasDuplicate = false;
            foreach (var item in taskList)
            {
                if (item.Name == userTask)
                {
                    hasDuplicate = true;
                    break;
                }
            }

            if (hasDuplicate)
                throw new DuplicateTaskException(userTask);

            var newTask = new ToDoItem(currentUser, userTask);
            taskList.Add(newTask);
            Console.WriteLine($"Задача '{userTask}' добавлена.");
        }

        static void HandleShowTask(ref List<ToDoItem> taskList)
        {
            int activeCount = 0;
            foreach (var item in taskList)
            {
                if (item.State == ToDoItemState.Active)
                {
                    Console.WriteLine($"{item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
                    activeCount++;
                }
            }

            if (activeCount == 0)
            {
                Console.WriteLine("Активных задач нет.");
            }
        }

        static void HandleShowAllTask(ref List<ToDoItem> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            foreach (var item in taskList)
            {
                Console.WriteLine($"({item.State}) {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
            }
        }

        static void HandleCompleteTask(string input, ref List<ToDoItem> taskList)
        {
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Console.WriteLine("Используйте: /completetask <id>");
                return;
            }

            if (!Guid.TryParse(parts[1], out Guid taskId))
            {
                Console.WriteLine("Неверный формат id задачи.");
                return;
            }

            ToDoItem? foundTask = null;
            foreach (var item in taskList)
            {
                if (item.Id == taskId)
                {
                    foundTask = item;
                    break;
                }
            }

            if (foundTask == null)
            {
                Console.WriteLine("Задача с таким id не найдена.");
                return;
            }

            foundTask.State = ToDoItemState.Completed;
            foundTask.StateChangedAt = DateTime.UtcNow;
            Console.WriteLine("Задача отмечена как выполненная.");
        }

        static void HandleRemoveTask(ref List<ToDoItem> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.WriteLine("Вот ваш список задач:");
            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {taskList[i].Name}");
            }
            Console.Write("Введите номер задачи для удаления: ");
            string? taskNum = Console.ReadLine();

            int taskIntNum = ParseAndValidateInt(taskNum, 1, taskList.Count);

            string deletedTask = taskList[taskIntNum - 1].Name;
            taskList.RemoveAt(taskIntNum - 1);

            Console.WriteLine($"Задача '{deletedTask}' была удалена.");
        }

        static void HandleStart(ref ToDoUser? currentUser)
        {
            Console.Write("Пожалуйста, введите ваше имя: ");
            string? name = Console.ReadLine()?.Trim();

            ValidateString(name);

            currentUser = new ToDoUser(name!);
            Console.WriteLine($"Привет, {currentUser.TelegramUserName}. Теперь доступна команда /echo.");
        }

        static void HandleHelp()
        {
            Console.WriteLine("Справка по использованию:");
            Console.WriteLine("- /start — начать работу и ввести имя");
            Console.WriteLine("- /help — показать эту справку");
            Console.WriteLine("- /info — показать информацию о программе");
            Console.WriteLine("- /echo <текст> — повторяет введённый текст (работает после /start)");
            Console.WriteLine("- /addtask — добавить задачу");
            Console.WriteLine("- /showtasks — показать только активные задачи");
            Console.WriteLine("- /showalltasks — показать все задачи");
            Console.WriteLine("- /completetask <id> — завершить задачу по id");
            Console.WriteLine("- /removetask — удалить задачу");
            Console.WriteLine("- /exit — завершить работу");
        }

        static void HandleInfo()
        {
            Console.WriteLine("Версия программы: 1.2.0");
            Console.WriteLine("Дата создания: 14.12.2025");
            Console.WriteLine("Имитация работы команд в Telegram");
        }

        static void HandleEcho(string input, ToDoUser? currentUser)
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала введите команду /start и укажите имя.");
                return;
            }

            string echoText = input.Substring(5).TrimStart();

            if (string.IsNullOrEmpty(echoText))
            {
                Console.WriteLine($"{currentUser.TelegramUserName}, пожалуйста, укажите текст после /echo (например: /echo Привет)");
                return;
            }

            Console.WriteLine($"{currentUser.TelegramUserName}, вы ввели: \"{echoText}\"");
        }
    }
}
