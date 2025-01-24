using ConsoleApp_Lab1_release.Controllers;

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


            //// Запись выходных данных
            //OutputWriter.WriteOutput("output.txt", resources, processes, systemStates.Count * qt, systemStates);

            Console.WriteLine("Введите первый операнд: ");
            int op1 = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Введите второй операнд: ");
            int op2 = Convert.ToInt32(Console.ReadLine());


            var manager = new ParallelProcessManager(SchedulerType.RoundRobin);
            manager.SetOperands(op1, op2); // Устанавливаем операнды
            manager.Run();
        }
    }
}