namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Ресурс(Станок)
    /// </summary>
    internal class Resource
    {
        private readonly object _dataLock = new();
        public object Data { get; protected set; }

        /// <summary>
        /// Уникальный Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Кол-во изделий которое оно может обрабатывать одновременно
        /// </summary>
        public int Capacity { get; set; } 

        /// <summary>
        /// Доступные слоты на текущий момент
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Объект для блокировки
        /// </summary>
        public object LockObject = new object();

        /// <summary>
        /// Конструктор ресурса
        /// </summary>
        /// <param name="id">Уникальный Id</param>
        /// <param name="name">Уникальное название</param>
        /// <param name="capacity">Кол</param>
        public Resource(int id, string name, int capacity)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
            AvailableSlots = capacity;
        }

        public virtual T GetData<T>() where T : class
        {
            lock (_dataLock)
            {
                return Data as T;
            }
        }

        public virtual void UpdateData<T>(Action<T> updateAction) where T : class
        {
            lock (_dataLock)
            {
                if (Data is T typedData)
                {
                    updateAction(typedData);
                }
            }
        }

        public bool TryAcquireSlot()
        {
            lock (LockObject)
            {
                if (AvailableSlots <= 0) return false;
                AvailableSlots--;
                return true;
            }
        }

        public void ReleaseSlot()
        {
            lock (LockObject)
            {
                AvailableSlots = Math.Min(AvailableSlots + 1, Capacity);
            }
        }
    }
}
