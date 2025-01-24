using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_Lab1_release.Models
{
    internal class ResultResource : Resource
    {
        public ResultResource(int id, string name, int capacity) : base(id, name, capacity)
        {
            Data = new Dictionary<int, bool>();
        }

        public void AddResult(int processId, bool result)
        {
            UpdateData<Dictionary<int, bool>>(results =>
            {
                results ??= new Dictionary<int, bool>();
                results[processId] = result;
                Data = results;
            });
        }

        public Dictionary<int, bool> GetResults()
        {
            return GetData<Dictionary<int, bool>>() ?? new Dictionary<int, bool>();
        }
    }
}
