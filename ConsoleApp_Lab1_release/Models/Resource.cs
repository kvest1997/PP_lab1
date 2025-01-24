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
        public object LockObject = new();

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

        /// <summary>
        /// Получение данных ресурса
        /// </summary>
        /// <typeparam name="T">Тип ресурса</typeparam>
        /// <returns>Данные</returns>
        public virtual T GetData<T>() where T : class
        {
            lock (_dataLock)
            {
                return Data as T;
            }
        }

        /// <summary>
        /// Обновление данных ресурса
        /// </summary>
        /// <typeparam name="T">Тип ресурса</typeparam>
        /// <param name="updateAction">Делегат обновления данных</param>
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

        /// <summary>
        /// Попытка получить слот
        /// </summary>
        /// <returns></returns>
        public bool TryAcquireSlot()
        {
            lock (_dataLock)
            {
                if (AvailableSlots <= 0) return false;
                AvailableSlots--;
                return true;
            }
        }

        /// <summary>
        /// Выпуск слота
        /// </summary>
        public void ReleaseSlot()
        {
            lock (_dataLock)
            {
                AvailableSlots = Math.Min(AvailableSlots + 1, Capacity);
            }
        }
    }
}
