namespace ConsoleApp_Lab1_release.Models
{
    /// <summary>
    /// Данные ресурса
    /// </summary>
    /// <typeparam name="T">Тип ресурсов</typeparam>
    internal class DataResource<T> : Resource
    {
        public DataResource(int id, string name, int capacity, T data) : base(id, name, capacity)
        {
            Data = data;
        }

        public Operands GetOperands() => GetData<Operands>() ?? throw new InvalidOperationException("Данные не инициализированны");
    }

    /// <summary>
    /// Класс операндов
    /// </summary>
    public class Operands
    {
        public bool[] Op1Bits { get; set; } = new bool[2]; // Бит 0 и бит 1
        public bool[] Op2Bits { get; set; } = new bool[2];
    }
}
