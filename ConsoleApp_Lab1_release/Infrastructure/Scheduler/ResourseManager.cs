using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    /// <summary>
    /// Абстрактный класс для управления планировщика
    /// </summary>
    internal abstract class ResourceManager
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
        public void Execute(List<string> systemStates, Action action)
        {
            // Получаем порядок выполнения задач от планировщика
            var processQueue = GetProcessQueue();

            // Запускаем задачи параллельно
            var tasks = new List<Task>();

            foreach (var process in processQueue)
            {
                var task = Task.Run(() => ExecuteProcess(process, systemStates, action));
                tasks.Add(task);
            }

            // Ожидаем завершения всех задач
            Task.WaitAll(tasks.ToArray());
            PrintFinalProcessReport(systemStates); // Добавленный вызов
        }

        /// <summary>
        /// Метод для выполнения отдельного процесса
        /// </summary>
        private void ExecuteProcess(Process process, List<string> systemStates, Action action)
        {
            int quantum = 0;

            while (process.State != TaskState.Completed)
            {
                //Выводи информацию на начало цикла выполнения
                PrintDynamicProcessInfo(quantum, systemStates);

                // Пытаемся захватить ресурсы
                AcquireResources(process);

                if (process.State == TaskState.Executing)
                {
                    // Выполняем процесс
                    int timeToExecute = Math.Min(process.RemainingTime, QuantumTime);
                    action?.Invoke();
                    //Thread.Sleep(timeToExecute); // Имитация выполнения
                    process.RemainingTime -= timeToExecute;

                    //Выводим информацию во время выполнения
                    PrintDynamicProcessInfo(quantum, systemStates);

                    // Если процесс завершил выполнение, освобождаем ресурсы
                    if (process.RemainingTime <= 0)
                    {
                        process.State = TaskState.Completed;
                    }

                    ReleaseResources(process);
                }

                quantum++;
            }

            // Если процесс не завершен, ждем следующего кванта времени
            if (process.State != TaskState.Completed)
            {
                Thread.Sleep(QuantumTime); // Ожидание следующего кванта времени
            }

            //Выводим инфромацию в конце процесса
            PrintDynamicProcessInfo(quantum, systemStates);
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
            // Создаем переходы для ресурса один раз
            foreach (var process in Processes.Where(p => p.RequiredResources.Contains(resource.Id)))
            {
                CreateResourceTransitions(process, resource.Id);
            }
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
        public void AcquireResources(Process process)
        {
            // Проверяем, завершен ли процесс
            if (process.State == TaskState.Completed)
            {
                return;
            }

            foreach (var resourceId in process.RequiredResources)
            {
                var resource = Resources.FirstOrDefault(r => r.Id == resourceId);

                if (resource == null)
                {
                    throw new InvalidOperationException($"Resource {resourceId} not found.");
                }

                if (!TryAcquireResources(resource)) return;

                string transitionName = $"Получить_ресурс_{resourceId}_с_помощью_Процесса_{process.Id}";

                if (PetriNet.CanFire(transitionName))
                {
                    PetriNet.Fire(transitionName);
                    Console.WriteLine($"Переход сработал: {transitionName}"); // Вывод в консоль

                    process.State = TaskState.Executing;
                    process.AcquiredResources.Add(resourceId);
                    process.EventHistory.Add((Timestamp: DateTime.Now, EventName: $"{transitionName}"));
                    resource.AvailableSlots--; // Уменьшаем доступные слоты
                }
                else
                {
                    process.State = TaskState.Waiting;
                    return;
                }
            }
        }


        private bool TryAcquireResources(Resource resource)
        {
            if (resource == null || resource.AvailableSlots == 0)
            {
                // Если хотя бы один ресурс недоступен, возвращаем false
                return false;
            }
            return true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="process">Задача</param>
        /// <exception cref="InvalidOperationException">Исключение если ресурс не найден</exception>
        public void ReleaseResources(Process process)
        {
            foreach (var resourceId in process.RequiredResources)
            {
                var resource = Resources.FirstOrDefault(r => r.Id == resourceId);
                if (resource == null)
                {
                    throw new InvalidOperationException($"Resource {resourceId} not found.");
                }
                string transitionName = $"Освободить_ресурс_{resourceId}_с_помощью_Процесса_{process.Id}";

                if (PetriNet.CanFire(transitionName))
                {
                    PetriNet.Fire(transitionName);
                    Console.WriteLine($"Переход сработал: {transitionName}"); // Вывод в консоль

                    process.AcquiredResources.Remove(resourceId);
                    process.EventHistory.Add((Timestamp: DateTime.Now, EventName: $"{transitionName}"));
                    resource.AvailableSlots++; // Увеличиваем доступные слоты
                }
            }
            process.State = TaskState.Waiting;
            if (process.RemainingTime <= 0)
            {
                process.State = TaskState.Completed;
            }
        }

        public void PrintDynamicProcessInfo(int quantum, List<string> systemState)
        {
            string currentSystemState = string.Empty;
            systemState.Add($"\nКвант времени: {quantum}");

            currentSystemState += $"Квант времени: {quantum}\n";

            systemState.Add("Текущее состояние процессов и их протоколов:");

            currentSystemState += "Текущее состояние процессов и их протоколов:\n";

            // Вывод состояния ресурсов
            foreach (var resource in Resources)
            {
                string resourceState = (resource.AvailableSlots < resource.Capacity) ? "Занят" : "Свободен";

                systemState.Add($"Ресурс {resource.Id} ({resource.Name}): {resourceState}");
                currentSystemState += $"Ресурс {resource.Id} ({resource.Name}): {resourceState}\n";

            }

            // Вывод состояния процессов
            foreach (var process in Processes)
            {
                string status = process.State switch
                {
                    TaskState.NotInitialized => "Не инициализирован",
                    TaskState.Waiting => "Ожидает",
                    TaskState.Executing => "Выполняется",
                    TaskState.Completed => "Завершен",
                    _ => "Неизвестно"
                };

                string resources = process.AcquiredResources.Any()
                    ? string.Join(", ", process.AcquiredResources)
                    : "Нет";

                // Алфавит процесса
                var alphabet = new List<string> { "Захват ресурсов", "Выполнение работы", "Освобождение ресурсов" };

                // Префиксная форма (последовательность выполненных действий)
                var prefixForm = new List<string>();
                if (process.AcquiredResources.Any()) prefixForm.Add("Захват ресурсов");
                if (process.State == TaskState.Executing || process.State == TaskState.Completed) prefixForm.Add("Выполнение работы");
                if (process.State == TaskState.Completed) prefixForm.Add("Освобождение ресурсов");

                // Протокол (последовательность событий)
                var protocol = new List<string>();
                if (process.AcquiredResources.Any()) protocol.Add($"Захват ресурсов: {string.Join(", ", process.AcquiredResources)}");
                if (process.State == TaskState.Executing || process.State == TaskState.Completed) protocol.Add($"Выполнение работы: {process.CpuBurst - process.RemainingTime} мс из {process.CpuBurst} мс");
                if (process.State == TaskState.Completed) protocol.Add("Освобождение ресурсов");

                // Вывод информации о процессе
                systemState.Add($"Процесс {process.Id} ({process.Name}):");
                currentSystemState += $"Процесс {process.Id} ({process.Name}):";

                systemState.Add($"- Состояние: {status}");
                currentSystemState += $"- Состояние: {status}\n";

                systemState.Add($"- Занятые ресурсы: {resources}");
                currentSystemState += $"- Занятые ресурсы: {resources}\n";

                systemState.Add($"- Оставшееся время: {process.RemainingTime} мс");
                currentSystemState += $"- Оставшееся время: {process.RemainingTime} мс\n";

                systemState.Add($"- Алфавит: {string.Join(", ", alphabet)}");
                currentSystemState += $"- Алфавит: {string.Join(", ", alphabet)}\n";

                systemState.Add($"- Префиксная форма: {string.Join(" -> ", prefixForm)}");
                currentSystemState += $"- Префиксная форма: {string.Join(" -> ", prefixForm)}\n";

                systemState.Add($"- Протокол: {string.Join(" -> ", protocol)}");
                currentSystemState += $"- Протокол: {string.Join(" -> ", protocol)}\n";
                Console.WriteLine(currentSystemState);
            }
        }

        public void PrintFinalProcessReport(List<string> systemStates)
        {
            Console.WriteLine("\nФИНАЛЬНЫЙ ОТЧЕТ О ПРОЦЕССАХ");
            systemStates.Add("\nФИНАЛЬНЫЙ ОТЧЕТ О ПРОЦЕССАХ");

            foreach (var process in Processes)
            {
                // 1. Алфавит процесса
                var alphabet = new HashSet<string>();
                foreach (var resourceId in process.RequiredResources)
                {
                    alphabet.Add($"Захват_ресурса_{resourceId}");
                    alphabet.Add($"Освобождение_ресурса_{resourceId}");
                }
                alphabet.Add("Выполнение_работы");

                // 2. Префиксная форма (последовательность событий)
                var prefixForm = new List<string>();
                if (process.AcquiredResources.Count != 0)
                    prefixForm.Add($"Захват_ресурса_{string.Join(",", process.AcquiredResources)}");

                prefixForm.Add("Выполнение_работы");

                if (process.State == TaskState.Completed)
                    prefixForm.Add($"Освобождение_ресурса_{string.Join(",", process.AcquiredResources)}");

                // 3. Полный протокол (вся история событий)
                var fullProtocol = new List<string>();
                foreach (var entry in process.EventHistory) // Предполагается наличие поля EventHistory в Process
                {
                    fullProtocol.Add($"{entry.Timestamp}: {entry.EventName}"); // Формат: "Время: Событие"
                }

                // 4. Оператор параллельной композиции
                var parallelProcesses = Processes
                    .Where(p => p.Id != process.Id && p.AcquiredResources.Intersect(process.AcquiredResources).Any())
                    .Select(p => p.Name);

                // Вывод информации
                Console.WriteLine($"\nПроцесс {process.Id} ({process.Name}):");
                Console.WriteLine($"Алфавит: {{{string.Join(", ", alphabet)}}}");
                Console.WriteLine($"Префиксная форма: {string.Join(" → ", prefixForm)}");
                Console.WriteLine($"Полный протокол:\n{string.Join("\n", fullProtocol)}");
                Console.WriteLine($"Параллельная композиция: {process.Name} || {string.Join(" || ", parallelProcesses)}");

                // Логирование для systemStates
                systemStates.Add($"\nПроцесс {process.Id} ({process.Name}):");
                systemStates.Add($"Алфавит: {{{string.Join(", ", alphabet)}}}");
                systemStates.Add($"Префиксная форма: {string.Join(" → ", prefixForm)}");
                systemStates.Add($"Полный протокол:\n{string.Join("\n", fullProtocol)}");
                systemStates.Add($"Параллельная композиция: {process.Name} || {string.Join(" || ", parallelProcesses)}");
            }
        }
    }
}