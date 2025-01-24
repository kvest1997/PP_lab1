namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Результат ресурса
    /// </summary>
    internal class ResultResource : Resource
    {
        public ResultResource(int id, string name, int capacity) : base(id, name, capacity)
        {
            Data = new Dictionary<int, bool>();
        }

        /// <summary>
        /// Добавление результата
        /// </summary>
        /// <param name="processId">Id процесса</param>
        /// <param name="result">Результат</param>
        public void AddResult(int processId, bool result)
        {
            UpdateData<Dictionary<int, bool>>(results =>
            {
                results ??= new Dictionary<int, bool>();
                results[processId] = result;
                Data = results;
            });
        }

        /// <summary>
        /// Получение Результата
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, bool> GetResults()
        {
            return GetData<Dictionary<int, bool>>() ?? new Dictionary<int, bool>();
        }
    }
}
