using ConsoleApp_Lab1_release.Infrastructure.Scheduler;
using System.Diagnostics;

namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Поток(Деталь)
    /// </summary>
    internal class Process
    {
        private readonly object _stateLock = new();
        private readonly object _resourcesLock = new();
        private TaskState _state = TaskState.NotInitialized;
        private readonly List<int> _acquiredResources = new();


        /// <summary>
        /// Уникальный идентификатор потока
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Приоритет выполнения
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Время выполнения
        /// </summary>
        public int CpuBurst { get; set; }
        
        /// <summary>
        /// Кол-во деталей
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Список требуемых станок(ресурсов)
        /// </summary>
        public List<int> RequiredResources { get; set; } 

        /// <summary>
        /// Оставшееся время выполнения
        /// </summary>
        public int RemainingTime { get; set; }

        /// <summary>
        /// История событий
        /// </summary>
        public List<(DateTime Timestamp, string EventName)> EventHistory { get; } = new();

        public Action<ResourceManager> ExecuteLogic { get; }
        public CancellationTokenSource Cts { get; } = new();


        /// <summary>
        /// Конструктор потока
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <param name="priority">Приоритет выполнения</param>
        /// <param name="cpuBurst">Время выполнения</param>
        /// <param name="count">Кол-во</param>
        /// <param name="requiredResources">Список требуемых ресурсов</param>
        public Process(int id, string name, 
            int priority, int cpuBurst, 
            int count, List<int> requiredResources, Action<ResourceManager> execute)
        {
            Id = id;
            Name = name;
            Priority = priority;
            CpuBurst = cpuBurst;
            Count = count;
            RequiredResources = requiredResources;
            RemainingTime = cpuBurst;
            ExecuteLogic = execute;
        }

        public async Task ExecuteAsync(ResourceManager manager)
        {
            var sw = Stopwatch.StartNew();
            await Task.Run(() =>
            {
                try
                {
                    ExecuteLogic?.Invoke(manager);
                }
                finally
                {
                    Cts.Dispose();
                }

                while (sw.ElapsedMilliseconds < CpuBurst)
                {
                    if (Cts.Token.IsCancellationRequested) break;
                }
            }, Cts.Token);
        }

        public TaskState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        public IReadOnlyList<int> AcquiredResources
        {
            get
            {
                lock (_resourcesLock)
                {
                    return _acquiredResources.AsReadOnly();
                }
            }
        }

        public void SetState(TaskState newState)
        {
            lock (_stateLock)
            {
                _state = newState;
            }
        }

        public bool TryAddAcquiredResource(int resourceId)
        {
            lock (_resourcesLock)
            {
                if (_acquiredResources.Contains(resourceId)) return false;
                _acquiredResources.Add(resourceId);
                return true;
            }
        }

        public bool TryReleaseResource(int resourceId)
        {
            lock (_resourcesLock)
            {
                return _acquiredResources.Remove(resourceId);
            }
        }


    }

    /// <summary>
    /// Перечисление для состояния задачи
    /// </summary>
    public enum TaskState
    {
        NotInitialized, // Задача не инициализированна
        Waiting,     // Задача ожидает выполнения
        Executing,   // Задача выполняется
        Completed    // Задача завершена
    }
}
