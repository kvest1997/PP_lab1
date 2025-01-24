namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    internal abstract partial class ResourceManager
    {
        /// <summary>
        /// Статус ресурса
        /// </summary>
        private class ResourceState
        {
            /// <summary>
            /// Id ресурса
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Название ресурса
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Кол-во занятых мест
            /// </summary>
            public int Available { get; set; }

            /// <summary>
            /// Общее кол-во мест
            /// </summary>
            public int Total { get; set; }
        }
    }
}