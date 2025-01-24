using ConsoleApp_Lab1_release.Controllers;
using ConsoleApp_Lab1_release.Infrastructure;
using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Infrastructure.Scheduler;
using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //// Чтение входных данных
            //var (pa, qt, maxT, maxP, resources, processes) = InputReader.ReadInput("input.txt");

            //// Инициализация менеджера ресурсов
            //ResourceManager resourceManager;
            //switch (pa)
            //{
            //    case "RoundRobin":
            //        resourceManager = new RoundRobin(qt, maxT, maxP);
            //        break;
            //    case "LCFS":
            //        resourceManager = new LCFS(qt, maxT, maxP);
            //        break;
            //    default:
            //        throw new ArgumentNullException("Не правильно выбранный планировщик");
            //}

            //foreach (var resource in resources)
            //{
            //    resourceManager.AddResource(resource);
            //}
            //foreach (var process in processes)
            //{
            //    resourceManager.AddProcess(process);
            //}            

            //// Запуск планировщика
            //var systemStates = new List<string>();
            //resourceManager.Execute(systemStates);


            // Запись выходных данных
            //OutputWriter.WriteOutput("output.txt", resources, processes, systemStates.Count * qt, systemStates);

            var manager = new ParallelProcessManager(SchedulerType.RoundRobin);
            manager.SetOperands(1, 3); // Устанавливаем операнды
            manager.InitializeResourcesAndProcesses();
            manager.Run();
        }
    }
}