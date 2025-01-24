namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Ресурс(Станок)
    /// </summary>
    internal class Resource
    {
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

        public object Lock = new object();

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
    }
}
