using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Infrastructure
{
    internal class OutputWriter
    {
        public static void WriteOutput(string filePath, 
            List<Resource> resources, 
            List<Process> processes, 
            int totalTime, 
            List<string> systemStates)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("NR: " + resources.Count);
                foreach (var resource in resources)
                {
                    writer.WriteLine($"Resource {resource.Id}: {resource.Name}, Capacity: {resource.Capacity}");
                }

                writer.WriteLine("NP: " + processes.Count);
                foreach (var process in processes)
                {
                    writer.WriteLine($"Process {process.Id}: {process.Name}, Priority: {process.Priority}, CPU Burst: {process.CpuBurst}, Count: {process.Count}");
                }

                writer.WriteLine("T: " + totalTime);

                for (int i = 0; i < systemStates.Count; i++)
                {
                    writer.WriteLine($"{i:D3}: {systemStates[i]}");
                }
            }
        }
    }
}