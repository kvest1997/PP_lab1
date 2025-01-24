using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Op1 { get; set; }
        public int Op2 { get; set; }
    }
}
