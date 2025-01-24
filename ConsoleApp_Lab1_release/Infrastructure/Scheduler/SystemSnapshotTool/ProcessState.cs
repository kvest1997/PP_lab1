using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    internal abstract partial class ResourceManager
    {
        /// <summary>
        /// Статус процесса
        /// </summary>
        private class ProcessState
        {
            /// <summary>
            /// Id процесса
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Название процесса
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Статус процесса
            /// </summary>
            public TaskState State { get; set; }

            /// <summary>
            /// Занятые ресурсы
            /// </summary>
            public List<int> AcquiredResources { get; set; }

            /// <summary>
            /// Времени осталось
            /// </summary>
            public int RemainingTime { get; set; }
        }
    }
}