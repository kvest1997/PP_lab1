namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Поток(Деталь)
    /// </summary>
    internal class Process
    {
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
        /// Статус
        /// </summary>
        public TaskState State { get; set; } = TaskState.NotInitialized;

        /// <summary>
        /// Занятые ресурсы
        /// </summary>
        public List<int> AcquiredResources { get; set; } = new List<int>();

        /// <summary>
        /// Оставшееся время выполнения
        /// </summary>
        public int RemainingTime { get; set; }

        /// <summary>
        /// История событий
        /// </summary>
        public List<(DateTime Timestamp, string EventName)> EventHistory { get; } = new List<(DateTime, string)>();

        /// <summary>
        /// Конструктор потока
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <param name="priority">Приоритет выполнения</param>
        /// <param name="cpuBurst">Время выполнения</param>
        /// <param name="count">Кол-во</param>
        /// <param name="requiredResources">Список требуемых ресурсов</param>
        public Process(int id, string name, int priority, int cpuBurst, int count, List<int> requiredResources)
        {
            Id = id;
            Name = name;
            Priority = priority;
            CpuBurst = cpuBurst;
            Count = count;
            RequiredResources = requiredResources;
            RemainingTime = cpuBurst;
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
