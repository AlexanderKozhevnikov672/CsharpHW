using System.Text.Json;

namespace TaskHub
{
    public enum Priority
    {
        Low,
        Medium,
        High,
    }

    public enum Status
    {
        New,
        InProgress,
        Done,
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Status Status { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {Name} | Priority: {Priority} | Status: {Status} | Deadline: {Deadline:yyyy-MM-dd HH:mm}";
        }
    }

    public delegate void TaskOverdueHandler(TaskItem task);

    public class TaskManager : IDisposable
    {
        private List<TaskItem> _tasks = new List<TaskItem>();
        private int _nextId = 1;
        private readonly object _lock = new object();

        public event TaskOverdueHandler? TaskOverdue;

        public void AddTask(TaskItem task)
        {
            lock (_lock)
            {
                task.Id = _nextId++;
                _tasks.Add(task);
            }
        }

        public List<TaskItem> GetAllTasks()
        {
            lock (_lock)
            {
                return _tasks.ToList();
            }
        }

        public List<TaskItem> GetTasksByPredicate(Func<TaskItem, bool> predicate)
        {
            lock (_lock)
            {
                return _tasks.Where(predicate).ToList();
            }
        }

        public bool EditTask(int id, Action<TaskItem> editAction)
        {
            lock (_lock)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == id);
                if (task == null)
                {
                    return false;
                }

                editAction(task);
                return true;
            }
        }

        public bool DeleteTask(int id)
        {
            lock (_lock)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == id);
                if (task == null)
                {
                    return false;
                }

                return _tasks.Remove(task);
            }
        }

        public (int total, int completed, int overdue, int low, int medium, int high) GetStatistics()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                int total = _tasks.Count;
                int completed = _tasks.Count(t => t.Status == Status.Done);
                int overdue = _tasks.Count(t => t.Deadline < now && t.Status != Status.Done);
                int low = _tasks.Count(t => t.Priority == Priority.Low);
                int medium = _tasks.Count(t => t.Priority == Priority.Medium);
                int high = _tasks.Count(t => t.Priority == Priority.High);
                return (total, completed, overdue, low, medium, high);
            }
        }

        public void CheckOverdueTasks()
        {
            List<TaskItem> overdueList;
            lock (_lock)
            {
                var now = DateTime.Now;
                overdueList = _tasks.Where(t => t.Deadline < now && t.Status != Status.Done).ToList();
            }

            foreach (var task in overdueList)
            {
                TaskOverdue?.Invoke(task);
            }
        }

        public List<TaskItem> GetTasksForSerialization()
        {
            lock (_lock)
            {
                return _tasks.ToList();
            }
        }

        public void LoadTasks(List<TaskItem> tasks)
        {
            lock (_lock)
            {
                _tasks = tasks;
                if (_tasks.Any())
                {
                    _nextId = _tasks.Max(t => t.Id) + 1;
                }
                else
                {
                    _nextId = 1;
                }
            }
        }

        public void Dispose() { }

        public static List<TaskItem> ValidateAndFixTasks(List<TaskItem> tasks)
        {
            if (tasks == null)
            {
                return new List<TaskItem>();
            }

            var validTasks = new List<TaskItem>();
            var usedIds = new HashSet<int>();

            foreach (var task in tasks)
            {
                if (task == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(task.Name))
                {
                    task.Name = "Unnamed Task";
                }

                if (task.Description == null)
                {
                    task.Description = string.Empty;
                }

                if (!Enum.IsDefined(typeof(Priority), task.Priority))
                {
                    task.Priority = Priority.Medium;
                }

                if (!Enum.IsDefined(typeof(Status), task.Status))
                {
                    task.Status = Status.New;
                }

                if (task.Deadline == DateTime.MinValue || task.Deadline == DateTime.MaxValue)
                {
                    task.Deadline = DateTime.Now.AddDays(1);
                }

                if (task.Id <= 0 || usedIds.Contains(task.Id))
                {
                    task.Id = -1;
                }
                else
                {
                    usedIds.Add(task.Id);
                }

                validTasks.Add(task);
            }

            int nextId = 1;
            foreach (var task in validTasks.OrderBy(t => t.Id > 0 ? t.Id : int.MaxValue))
            {
                if (task.Id == -1)
                {
                    task.Id = nextId;
                }
                else
                {
                    nextId = task.Id;
                }

                nextId++;
            }

            validTasks.Sort((t1, t2) => t1.Id.CompareTo(t2.Id));
            return validTasks;
        }
    }

    public static class FileService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public static async Task SaveToFileAsync(string filePath, List<TaskItem> tasks)
        {
            try
            {
                string json = JsonSerializer.Serialize(tasks, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
                Console.WriteLine("Tasks saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tasks: {ex.Message}");
            }
        }

        public static async Task<List<TaskItem>> LoadFromFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found. Starting with empty task list.");
                    return new List<TaskItem>();
                }

                string json = await File.ReadAllTextAsync(filePath);
                var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);
                return tasks ?? new List<TaskItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
                return new List<TaskItem>();
            }
        }
    }

    public class OverdueMonitor : IDisposable
    {
        private readonly TaskManager _taskManager;
        private readonly CancellationTokenSource _cts;
        private readonly Task _monitorTask;
        private readonly int _checkIntervalSeconds;

        public OverdueMonitor(TaskManager taskManager, int checkIntervalSeconds = 5)
        {
            _taskManager = taskManager;
            _checkIntervalSeconds = checkIntervalSeconds;
            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(RunMonitorAsync);
        }

        private async Task RunMonitorAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkIntervalSeconds * 1000, _cts.Token);
                    _taskManager.CheckOverdueTasks();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in background monitor: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _monitorTask.Wait(1000); } catch { }
            _cts.Dispose();
        }
    }

    public static class ConsoleUI
    {
        public static void ShowMenu()
        {
            Console.WriteLine("===== TASK HUB =====");
            Console.WriteLine("1. Create task");
            Console.WriteLine("2. View all tasks");
            Console.WriteLine("3. View completed tasks");
            Console.WriteLine("4. View uncompleted tasks");
            Console.WriteLine("5. View high priority tasks");
            Console.WriteLine("6. Edit task");
            Console.WriteLine("7. Delete task");
            Console.WriteLine("8. Search tasks");
            Console.WriteLine("9. Show statistics");
            Console.WriteLine("10. Save tasks to file");
            Console.WriteLine("11. Load tasks from file");
            Console.WriteLine("0. Exit");
            Console.Write("Your choice: ");
        }

        public static TaskItem CreateTaskFromInput(int idHint = 0)
        {
            var task = new TaskItem();

            Console.Write("Name: ");
            task.Name = Console.ReadLine() ?? string.Empty;

            Console.Write("Description: ");
            task.Description = Console.ReadLine() ?? string.Empty;

            Console.Write("Priority (Low/Medium/High): ");
            string prio = Console.ReadLine() ?? string.Empty;
            task.Priority = Enum.TryParse(prio, true, out Priority p) ? p : Priority.Medium;

            Console.Write("Deadline (yyyy-MM-dd HH:mm): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime dt))
            {
                task.Deadline = dt;
            }
            else
            {
                task.Deadline = DateTime.Now.AddDays(1);
            }

            Console.Write("Status (New/InProgress/Done): ");
            string stat = Console.ReadLine() ?? string.Empty;
            task.Status = Enum.TryParse(stat, true, out Status s) ? s : Status.New;

            return task;
        }

        public static void ShowTasks(List<TaskItem> tasks, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            if (!tasks.Any())
            {
                Console.WriteLine("No tasks found.");
            }
            else
            {
                foreach (var t in tasks)
                {
                    Console.WriteLine(t);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    class Program
    {
        static TaskManager _taskManager = new TaskManager();
        static OverdueMonitor? _monitor;
        static string _dataFile = "tasks.json";

        static async Task Main()
        {
            Console.WriteLine("Welcome to TaskHub!");
            _monitor = new OverdueMonitor(_taskManager, 5);

            bool running = true;
            while (running)
            {
                Console.Clear();

                var overdueTasks = _taskManager.GetTasksByPredicate(t => t.Deadline < DateTime.Now && t.Status != Status.Done);
                if (overdueTasks.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("=== OVERDUE TASKS ===");
                    foreach (var task in overdueTasks)
                    {
                        Console.WriteLine($"[ALERT] Task #{task.Id} '{task.Name}' is overdue! Deadline was {task.Deadline:yyyy-MM-dd HH:mm}");
                    }
                    Console.ResetColor();
                    Console.WriteLine();
                }

                ConsoleUI.ShowMenu();
                string choice = Console.ReadLine() ?? string.Empty;
                switch (choice)
                {
                    case "1":
                        CreateTask();
                        break;

                    case "2":
                        ViewAllTasks();
                        break;

                    case "3":
                        ViewCompletedTasks();
                        break;

                    case "4":
                        ViewUncompletedTasks();
                        break;

                    case "5":
                        ViewHighPriorityTasks();
                        break;

                    case "6":
                        EditTask();
                        break;

                    case "7":
                        DeleteTask();
                        break;

                    case "8":
                        SearchTasks();
                        break;

                    case "9":
                        ShowStatistics();
                        break;

                    case "10":
                        await SaveTasksAsync();
                        break;

                    case "11":
                        await LoadTasksAsync();
                        break;

                    case "0":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Press any key.");
                        Console.ReadKey();
                        break;
                }
            }

            _monitor.Dispose();
            _taskManager.Dispose();
            Console.WriteLine("Goodbye!");
        }

        static void CreateTask()
        {
            Console.Clear();
            Console.WriteLine("--- Create New Task ---");
            var task = ConsoleUI.CreateTaskFromInput();
            _taskManager.AddTask(task);
            Console.WriteLine("Task created successfully!");
            Console.ReadKey();
        }

        static void ViewAllTasks() => ConsoleUI.ShowTasks(_taskManager.GetAllTasks(), "All Tasks");
        static void ViewCompletedTasks() => ConsoleUI.ShowTasks(_taskManager.GetTasksByPredicate(t => t.Status == Status.Done), "Completed Tasks");
        static void ViewUncompletedTasks() => ConsoleUI.ShowTasks(_taskManager.GetTasksByPredicate(t => t.Status != Status.Done), "Uncompleted Tasks");
        static void ViewHighPriorityTasks() => ConsoleUI.ShowTasks(_taskManager.GetTasksByPredicate(t => t.Priority == Priority.High), "High Priority Tasks");

        static void EditTask()
        {
            Console.Clear();
            Console.Write("Enter task ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                Console.ReadKey();
                return;
            }

            bool success = _taskManager.EditTask(id, task =>
            {
                Console.WriteLine($"Editing task: {task.Name}");

                Console.Write("New Name (leave empty to keep): ");
                string? name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    task.Name = name;
                }

                Console.Write("New Description (leave empty to keep): ");
                string? desc = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    task.Description = desc;
                }

                Console.Write("New Priority (Low/Medium/High, leave empty to keep): ");
                string? prio = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(prio) && Enum.TryParse(prio, true, out Priority p))
                {
                    task.Priority = p;
                }

                Console.Write("New Status (New/InProgress/Done, leave empty to keep): ");
                string? stat = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(stat) && Enum.TryParse(stat, true, out Status s))
                {
                    task.Status = s;
                }

                Console.Write("New Deadline (yyyy-MM-dd HH:mm, leave empty to keep): ");
                string? date = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime dt))
                {
                    task.Deadline = dt;
                }
            });

            Console.WriteLine(success ? "Task updated." : "Task not found.");
            Console.ReadKey();
        }

        static void DeleteTask()
        {
            Console.Clear();
            Console.Write("Enter task ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID."); Console.ReadKey(); return;
            }

            bool deleted = _taskManager.DeleteTask(id);
            Console.WriteLine(deleted ? "Task deleted." : "Task not found.");
            Console.ReadKey();
        }

        static void SearchTasks()
        {
            Console.Clear();
            Console.WriteLine("Search by: 1 - Name, 2 - Status, 3 - Priority");
            string opt = Console.ReadLine() ?? string.Empty;
            List<TaskItem> results = new List<TaskItem>();
            switch (opt)
            {
                case "1":
                    Console.Write("Enter name (substring): ");
                    string name = Console.ReadLine() ?? string.Empty;
                    results = _taskManager.GetTasksByPredicate(t => t.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                    break;

                case "2":
                    Console.Write("Enter status (New/InProgress/Done): ");
                    if (Enum.TryParse(Console.ReadLine(), true, out Status status))
                        results = _taskManager.GetTasksByPredicate(t => t.Status == status);
                    break;

                case "3":
                    Console.Write("Enter priority (Low/Medium/High): ");
                    if (Enum.TryParse(Console.ReadLine(), true, out Priority priority))
                        results = _taskManager.GetTasksByPredicate(t => t.Priority == priority);
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    Console.ReadKey();
                    return;
            }

            ConsoleUI.ShowTasks(results, "Search Results");
        }

        static void ShowStatistics()
        {
            var stats = _taskManager.GetStatistics();
            Console.Clear();
            Console.WriteLine("--- STATISTICS ---");
            Console.WriteLine($"Total tasks:        {stats.total}");
            Console.WriteLine($"Completed tasks:    {stats.completed}");
            Console.WriteLine($"Overdue tasks:      {stats.overdue}");
            Console.WriteLine($"Priority Low:       {stats.low}");
            Console.WriteLine($"Priority Medium:    {stats.medium}");
            Console.WriteLine($"Priority High:      {stats.high}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static async Task SaveTasksAsync()
        {
            var tasks = _taskManager.GetTasksForSerialization();
            await FileService.SaveToFileAsync(_dataFile, tasks);
            Console.ReadKey();
        }

        static async Task LoadTasksAsync()
        {
            var tasks = await FileService.LoadFromFileAsync(_dataFile);
            var validTasks = TaskManager.ValidateAndFixTasks(tasks);
            _taskManager.LoadTasks(validTasks);
            Console.WriteLine($"Loaded {validTasks.Count} tasks from file.");
            Console.ReadKey();
        }
    }
}
