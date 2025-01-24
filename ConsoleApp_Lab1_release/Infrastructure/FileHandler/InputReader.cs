using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Infrastructure
{
    internal class InputReader
    {
        public static (string, int, int, int, List<Resource>, List<Process>) ReadInput(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var index = 0;

            // Чтение способа планирования
            string pa = lines[index++].Split(':')[1].Trim();

            // Чтение кванта времени
            int qt = int.Parse(lines[index++].Split(':')[1].Trim());

            // Чтение максимального времени CPU burst
            int maxT = int.Parse(lines[index++].Split(':')[1].Trim());

            // Чтение максимального приоритета
            int maxP = int.Parse(lines[index++].Split(':')[1].Trim());

            // Чтение количества ресурсов
            int nr = int.Parse(lines[index++].Split(':')[1].Trim());
            var resources = new List<Resource>();
            for (int i = 0; i < nr; i++)
            {
                var parts = lines[index++].Split(',');
                int id = int.Parse(parts[0].Trim());
                string name = parts[1].Trim();
                int capacity = int.Parse(parts[2].Trim());
                resources.Add(new Resource(id, name, capacity));
            }

            // Чтение количества потоков
            int np = int.Parse(lines[index++].Split(':')[1].Trim());
            var processes = new List<Process>();
            for (int i = 0; i < np; i++)
            {
                var parts = lines[index++].Split(',');
                int id = int.Parse(parts[0].Trim());
                string name = parts[1].Trim();
                int priority = int.Parse(parts[2].Trim());
                int cpuBurst = int.Parse(parts[3].Trim());

                if (cpuBurst > maxT)
                    throw new ArgumentException($"CPU Burst процесса {id} превышает MaxT.");
                if (priority > maxP)
                    throw new ArgumentException($"Приоритет процесса {id} превышает MaxP.");

                int count = int.Parse(parts[4].Trim());
                var requiredResources = parts[5].Trim().Split(' ').Select(int.Parse).ToList();
                processes.Add(new Process(id, name, priority, cpuBurst, count, requiredResources, () =>
                {
                    Task.Delay(cpuBurst);
                }));
            }

            return (pa, qt, maxT, maxP, resources, processes);
        }
    }
}