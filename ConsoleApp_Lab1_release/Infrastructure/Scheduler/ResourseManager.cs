using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Models;
using System.Text;

namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    /// <summary>
    /// Абстрактный класс для управления планировщика
    /// </summary>
    internal abstract partial class ResourceManager
    {
        /// <summary>
        /// Ресурсы
        /// </summary>
        public List<Resource> Resources { get; set; } = new List<Resource>();

        /// <summary>
        /// Потоки(Задачи)
        /// </summary>
        protected List<Process> Processes { get; set; } = new List<Process>();

        /// <summary>
        /// Сеть петри
        /// </summary>
        public PetriNetRun PetriNet { get; set; } = new PetriNetRun();

        /// <summary>
        /// Квант времеи
        /// </summary>
        public int QuantumTime { get; set; }

        /// <summary>
        /// Для синхронизации вывода информации в консоль
        /// </summary>
        private static readonly object _consoleLock = new object();

        protected int MaxT { get; }
        protected int MaxP { get; }

        /// <summary>
        /// Абстрактный метод для получения очереди процессов от планировщика
        /// </summary>
        protected abstract IReadOnlyCollection<Process> GetProcessQueue();

        /// <summary>
        /// Конструктор менеджера
        /// </summary>
        /// <param name="quantumTime">Квант времени</param>
        public ResourceManager(int quantumTime = 100, int maxT = 1000, int maxP = 10)
        {
            MaxT = maxT;
            MaxP = maxP;
            QuantumTime = quantumTime;
        }

        /// <summary>
        /// Выполнение задач
        /// </summary>
        /// <param name="systemStates">Логи</param>
        public void Execute(List<string> systemStates)
        {
            // Получаем порядок выполнения задач от планировщика
            var processQueue = GetProcessQueue();

            // Запускаем задачи параллельно
            var tasks = new List<Task>();

            foreach (var process in processQueue)
            {
                var task = Task.Run(() => ExecuteProcessAsync(process, systemStates));
                tasks.Add(task);
            }

            // Ожидаем завершения всех задач
            Task.WaitAll(tasks.ToArray());
            PrintFinalProcessReport(systemStates);
        }

        /// <summary>
        /// Метод для выполнения отдельного процесса
        /// </summary>
        private async Task ExecuteProcessAsync(Process process, List<string> systemStates)
        {
            int quantum = 0;
            var retryCount = 0;
            const int maxRetries = 3;

            while (process.State != TaskState.Completed & retryCount < maxRetries)
            {
                lock (_consoleLock)
                {
                    PrintDynamicProcessInfo(quantum, systemStates);
                }

                // Пытаемся захватить ресурсы
                AcquireResources(process);

                lock (_consoleLock)
                {
                    PrintDynamicProcessInfo(quantum, systemStates);
                }

                if (process.State == TaskState.Executing)
                {
                    // Выполняем процесс
                    int timeToExecute = Math.Min(process.RemainingTime, QuantumTime);

                    process.ExecuteLogic?.Invoke(this);

                    process.RemainingTime -= timeToExecute;

                    // Если процесс завершил выполнение, освобождаем ресурсы
                    if (process.RemainingTime <= 0)
                    {
                        process.SetState(TaskState.Completed);
                        ReleaseResources(process);
                        retryCount++;
                    }
                    else
                    {
                        ReleaseResources(process);
                        process.SetState(TaskState.Waiting);
                        retryCount++;
                    }
                    retryCount = 0;
                }
                else
                {
                    var delay = Math.Min(100 * (int)Math.Pow(2, retryCount), 1000);
                    Thread.Sleep(delay);
                    retryCount++;
                }

                quantum++;
                lock (_consoleLock)
                {
                    PrintDynamicProcessInfo(quantum, systemStates);
                }
            }
        }

        /// <summary>
        /// Добавить ресурсы
        /// </summary>
        /// <param name="resource">Объект ресурса</param>
        public void AddResource(Resource resource)
        {
            Resources.Add(resource);
            PetriNet.AddPlace($"Ресурс_{resource.Id}_Свободен", resource.Capacity);
            PetriNet.AddPlace($"Ресурс_{resource.Id}_Занят", 0);
        }

        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="process">Объект задачи</param>
        public virtual void AddProcess(Process process)
        {
            PetriNet.AddPlace($"Процесс_{process.Id}_Ожидает", process.RequiredResources.Count);
            PetriNet.AddPlace($"Процесс_{process.Id}_Запущен", 0);

            // Создаем переходы для всех требуемых ресурсов
            foreach (var resourceId in process.RequiredResources)
            {
                var resource = Resources.FirstOrDefault(r => r.Id == resourceId);
                if (resource == null) throw new InvalidOperationException($"Resource {resourceId} not found");

                CreateResourceTransitions(process, resourceId);
            }
        }

        /// <summary>
        /// Создание входных\выхожных дуг
        /// </summary>
        /// <param name="process">Объект процесса</param>
        /// <param name="resourceId">Id ресурса</param>
        private void CreateResourceTransitions(Process process, int resourceId)
        {
            string transitionName = $"Получить_ресурс_{resourceId}_с_помощью_Процесса_{process.Id}";
            PetriNet.AddTransition(transitionName, () => PetriNet.GetTokens($"Ресурс_{resourceId}_Свободен") > 0);

            PetriNet.AddInputArc(transitionName, $"Ресурс_{resourceId}_Свободен");
            PetriNet.AddInputArc(transitionName, $"Процесс_{process.Id}_Ожидает");
            PetriNet.AddOutputArc(transitionName, $"Ресурс_{resourceId}_Занят");
            PetriNet.AddOutputArc(transitionName, $"Процесс_{process.Id}_Запущен");

            transitionName = $"Освободить_ресурс_{resourceId}_с_помощью_Процесса_{process.Id}";
            PetriNet.AddTransition(transitionName, () => PetriNet.GetTokens($"Ресурс_{resourceId}_Занят") > 0);

            PetriNet.AddInputArc(transitionName, $"Ресурс_{resourceId}_Занят");
            PetriNet.AddInputArc(transitionName, $"Процесс_{process.Id}_Запущен");
            PetriNet.AddOutputArc(transitionName, $"Ресурс_{resourceId}_Свободен");
            PetriNet.AddOutputArc(transitionName, $"Процесс_{process.Id}_Ожидает");
        }

        /// <summary>
        /// Получение ресурсов
        /// </summary>
        /// <param name="process">Задача</param>
        /// <exception cref="InvalidOperationException">Исключение когда не найден ресурс</exception>
        private void AcquireResources(Process process)
        {
            // Проверяем, завершен ли процесс
            if (process.State == TaskState.Completed)
                return;

            var requiredResources = process.RequiredResources
                .OrderBy(id => id)
                .Select(id => Resources.FirstOrDefault(r => r.Id == id))
                .Where(r => r != null)
                .ToList();

            var lockedResources = requiredResources
                .Select(r =>
                {
                    Monitor.Enter(r.LockObject);
                    return r;
                }).ToList();

            try
            {
                bool canAcquire = requiredResources.All(res =>
                    PetriNet.CanFire($"Получить_ресурс_{res.Id}_с_помощью_Процесса_{process.Id}"));

                process.SetState(canAcquire ? TaskState.Executing : TaskState.Waiting);

                if (!canAcquire) return;

                foreach (var resource in requiredResources)
                {
                    string transitionName = $"Получить_ресурс_{resource.Id}_с_помощью_Процесса_{process.Id}";

                    PetriNet.Fire(transitionName);
                    Console.WriteLine($"Переход сработал: {transitionName}");

                    if (!TryAcquireResource(resource, process))
                    {
                        process.SetState(TaskState.Waiting);
                    }

                    process.EventHistory.Add((DateTime.Now, transitionName));
                }

            }
            finally
            {
                // Разблокировка в обратном порядке
                lockedResources
                    .AsEnumerable()
                    .Reverse()
                    .ToList()
                    .ForEach(r => Monitor.Exit(r.LockObject));
            }
        }

        /// <summary>
        /// Вспомогательный метод для захвата ресурсов
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        private bool TryAcquireResource(Resource resource, Process process)
        {
            if (!resource.TryAcquireSlot()) return false;

            if (!process.TryAddAcquiredResource(resource.Id))
            {
                resource.ReleaseSlot();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Вспомогательный метод для освобождения ресурсов
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        private bool TryReleaseResource(Resource resource, Process process)
        {
            if (!process.TryReleaseResource(resource.Id)) return false;

            resource.ReleaseSlot();
            return true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="process">Задача</param>
        /// <exception cref="InvalidOperationException">Исключение если ресурс не найден</exception>
        private void ReleaseResources(Process process)
        {
            var resources = process.RequiredResources
                .OrderByDescending(id => id)
                .Select(id => Resources.FirstOrDefault(r => r.Id == id))
                .Where(r => r != null)
                .ToList();

            if (!resources.Any()) return;
            var lockedResources = resources
                .Select(r =>
                {
                    Monitor.Enter(r.LockObject);
                    return r;
                }).ToList();

            try
            {
                // 3. Освобождаем ресурсы
                foreach (var resource in resources)
                {
                    string transitionName = $"Освободить_ресурс_{resource.Id}_с_помощью_Процесса_{process.Id}";

                    if (!PetriNet.CanFire(transitionName))
                        throw new InvalidOperationException($"Переход {transitionName} не сработал");

                    PetriNet.Fire(transitionName);
                    Console.WriteLine($"Переход сработал: {transitionName}"); // Вывод в консоль
                    process.EventHistory.Add((Timestamp: DateTime.Now, EventName: $"{transitionName}"));

                    if (!TryReleaseResource(resource, process))
                    {
                        throw new InvalidOperationException($"Не удалось освободить ресурс {resource.Id} для процесса {process.Id}");
                    }
                }

                // 4. Обновляем состояние процесса
                process.SetState(process.RemainingTime > 0
                    ? TaskState.Waiting
                    : TaskState.Completed);
            }
            finally
            {
                // 5. Гарантированная разблокировка
                lockedResources.ForEach(r => Monitor.Exit(r.LockObject));
            }
        }

        /// <summary>
        /// Вывод финального отчета
        /// </summary>
        /// <param name="systemStates">Логи</param>
        private void PrintFinalProcessReport(List<string> systemStates)
        {
            lock (_consoleLock) // Синхронизация вывода
            {
                var report = new StringBuilder("\nФИНАЛЬНЫЙ ОТЧЕТ О ПРОЦЕССАХ\n");
                systemStates.Add(report.ToString());
                Console.WriteLine(report);

                foreach (var process in Processes.OrderBy(p => p.Id))
                {
                    var processReport = new StringBuilder();

                    // 1. Алфавит процесса (на основе реально произошедших событий)
                    var alphabet = process.EventHistory
                        .Select(e => NormalizeEventName(e.EventName))
                        .Distinct()
                        .ToHashSet();

                    // 2. Префиксная форма (анализ последовательности событий)
                    var prefixForm = new List<string>();
                    var acquiredResources = new List<int>();
                    foreach (var entry in process.EventHistory)
                    {
                        var eventName = NormalizeEventName(entry.EventName);
                        if (eventName.StartsWith("Захват"))
                        {
                            var resId = ParseResourceId(entry.EventName);
                            if (resId.HasValue) acquiredResources.Add(resId.Value);
                        }
                        else if (eventName.StartsWith("Освобождение"))
                        {
                            var resId = ParseResourceId(entry.EventName);
                            if (resId.HasValue) acquiredResources.Remove(resId.Value);
                        }

                        if (!prefixForm.Contains(eventName)) // Упрощенный вариант
                            prefixForm.Add(eventName);
                    }

                    // 3. Полный протокол с группировкой по квантам времени
                    var protocolGroups = process.EventHistory
                        .GroupBy(e => e.Timestamp.ToString("HH:mm:ss.fff"))
                        .OrderBy(g => g.Key);

                    // 4. Параллельная композиция (на основе использованных ресурсов)
                    var parallelProcesses = Processes
                        .Where(p => p.Id != process.Id &&
                            p.RequiredResources.Intersect(process.RequiredResources).Any())
                        .Select(p => p.Name);

                    // Формирование отчета
                    processReport.AppendLine($"\n[Процесс {process.Id}] {process.Name}");
                    processReport.AppendLine($"Состояние: {GetStateName(process.State)}");
                    processReport.AppendLine($"Алфавит: {{{string.Join(", ", alphabet)}}}");
                    processReport.AppendLine($"Префикс: {string.Join(" → ", prefixForm)}");

                    processReport.AppendLine("\nДетальный протокол:");
                    foreach (var group in protocolGroups)
                    {
                        processReport.AppendLine($"{group.Key}:");
                        foreach (var entry in group)
                        {
                            processReport.AppendLine($"  {NormalizeEventName(entry.EventName)}");
                        }
                    }

                    processReport.AppendLine("\nПараллельные процессы:");
                    processReport.AppendLine(parallelProcesses.Any()
                        ? string.Join(" || ", parallelProcesses.Prepend(process.Name))
                        : "Нет параллельных взаимодействий");

                    // Атомарный вывод
                    var finalReport = processReport.ToString();
                    systemStates.Add(finalReport);
                    Console.WriteLine(finalReport);
                }
            }
        }

        /// <summary>
        /// Получение ресурсов
        /// </summary>
        /// <typeparam name="T">Тип ресурса</typeparam>
        /// <param name="id">Id ресурса</param>
        /// <returns>Объект ресурса</returns>
        /// <exception cref="KeyNotFoundException">Исключение: если ресурс не найден</exception>
        public T GetResource<T>(int id) where T : Resource
        {
            var resource = Resources.FirstOrDefault(r => r.Id == id) as T;
            return resource ?? throw new KeyNotFoundException($"Ресурс {id} типа {typeof(T).Name} не найден");
        }

        /// <summary>
        /// Нормализация события из истории
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <returns>Измененное название</returns>
        private string NormalizeEventName(string eventName)
        {
            return eventName
                .Replace("Получить_ресурс_", "Захват ресурса ")
                .Replace("Освободить_ресурс_", "Освобождение ресурса ")
                .Replace("_с_помощью_Процесса_", " (Процесс ")
                .Replace("_", "") + ")";
        }

        /// <summary>
        /// Получение Id ресурса
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <returns>Id ресурса</returns>
        private int? ParseResourceId(string eventName)
        {
            var parts = eventName.Split('_');
            if (parts.Length > 2 && int.TryParse(parts[2], out int id))
                return id;
            return null;
        }

        /// <summary>
        /// Динамический вывод информации
        /// </summary>
        /// <param name="quantum">Квант времени</param>
        /// <param name="systemStates">Логи</param>
        public void PrintDynamicProcessInfo(int quantum, List<string> systemStates)
        {
            lock (_consoleLock) // Полная блокировка всех операций вывода
            {
                // 1. Собираем снимок состояния системы
                var snapshot = new SystemSnapshot
                {
                    Timestamp = DateTime.Now,
                    Quantum = quantum,
                    Resources = Resources.Select(r => new ResourceState
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Available = r.AvailableSlots,
                        Total = r.Capacity
                    }).ToList(),
                    Processes = Processes.Select(p => new ProcessState
                    {
                        Id = p.Id,
                        Name = p.Name,
                        State = p.State,
                        AcquiredResources = new List<int>(p.AcquiredResources),
                        RemainingTime = p.RemainingTime
                    }).ToList()
                };

                // 2. Формируем отчет на основе снимка
                var report = new StringBuilder();
                report.AppendLine($"\n=== Квант времени: {snapshot.Quantum} ===");
                report.AppendLine($"Время снимка: {snapshot.Timestamp:HH:mm:ss.fff}");

                report.AppendLine("\nСостояние ресурсов:");
                foreach (var res in snapshot.Resources)
                {
                    report.AppendLine($"[R{res.Id}] {res.Name}: {res.Available}/{res.Total}");
                }

                report.AppendLine("\nСостояние процессов:");
                foreach (var proc in snapshot.Processes.OrderBy(p => p.Id))
                {
                    report.AppendLine($"[P{proc.Id}] {proc.Name}");
                    report.AppendLine($"  Статус: {GetStateName(proc.State)}");
                    report.AppendLine($"  Ресурсы: {(proc.AcquiredResources.Any() ? string.Join(", ", proc.AcquiredResources) : "нет")}");
                    report.AppendLine($"  Осталось: {proc.RemainingTime}мс");
                    report.AppendLine("-----------------------------------");
                }

                // 3. Атомарные операции вывода
                string output = report.ToString();
                systemStates.Add(output); // Добавляем в логи
                Console.WriteLine(output); // Вывод в консоль
            }
        }


        /// <summary>
        /// Получение имени статуса
        /// </summary>
        /// <param name="state">Стаус</param>
        /// <returns>Название статуса</returns>
        private string GetStateName(TaskState state)
        {
            return state switch
            {
                TaskState.NotInitialized => "Не инициализирован",
                TaskState.Waiting => "Ожидание ресурсов",
                TaskState.Executing => "Выполнение",
                TaskState.Completed => "Завершён",
                _ => "Неизвестное состояние"
            };
        }
    }
}