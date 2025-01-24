namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    internal abstract partial class ResourceManager
    {
        /// <summary>
        /// Вспомогательные классы для снимка системы
        /// </summary>
        private class SystemSnapshot
        {
            /// <summary>
            /// Время снимка
            /// </summary>
            public DateTime Timestamp { get; set; }
            
            /// <summary>
            /// Квант времени
            /// </summary>
            public int Quantum { get; set; }

            /// <summary>
            /// Ресурсы
            /// </summary>
            public List<ResourceState> Resources { get; set; }

            /// <summary>
            /// Процессы
            /// </summary>
            public List<ProcessState> Processes { get; set; }
        }
    }
}