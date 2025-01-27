using ConsoleApp_Lab1_release.Infrastructure.Scheduler;
using ConsoleApp_Lab1_release.Infrastructure.Scheduler.SchedulerAlgoritms;
using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Controllers
{
    /// <summary>
    /// Класс для реализации проверки операторов
    /// </summary>
    public class ParallelProcessManager
    {
        private readonly ResourceManager _resourceManager;
        private readonly List<string> _systemLog = new List<string>();
        private Operands _currentOperands = new Operands();

        public ParallelProcessManager(SchedulerType schedulerType, int quantumTime = 100)
        {
            _resourceManager = schedulerType switch
            {
                SchedulerType.RoundRobin => new RoundRobin(quantumTime),
                SchedulerType.LCFS => new LCFS(quantumTime),
                _ => throw new ArgumentException("Invalid scheduler type")
            };
        }

        public void SetOperands(int op1, int op2)
        {
            if (op1 < 0 || op1 > 3 || op2 < 0 || op2 > 3)
                throw new ArgumentException("Операнды должны быть 2-битными (0-3)");

            _currentOperands.Op1Bits[0] = (op1 & 0b01) != 0; // Младший бит
            _currentOperands.Op1Bits[1] = (op1 & 0b10) != 0; // Старший бит

            _currentOperands.Op2Bits[0] = (op2 & 0b01) != 0;
            _currentOperands.Op2Bits[1] = (op2 & 0b10) != 0;
        }

        /// <summary>
        /// Инициализация данных
        /// </summary>
        private void InitializeResourcesAndProcesses()
        {
            // Добавляем ресурсы
            // Создаем ресурсы для битов
            var op1Bit0 = new DataResource<bool>(1, "Op1Bit0", 1, _currentOperands.Op1Bits[0]);
            var op1Bit1 = new DataResource<bool>(2, "Op1Bit1", 1, _currentOperands.Op1Bits[1]);
            var op2Bit0 = new DataResource<bool>(3, "Op2Bit0", 1, _currentOperands.Op2Bits[0]);
            var op2Bit1 = new DataResource<bool>(4, "Op2Bit1", 1, _currentOperands.Op2Bits[1]);

            var compareResult = new ResultResource(5, "CompareResult", 2);

            _resourceManager.AddResource(op1Bit0);
            _resourceManager.AddResource(op1Bit1);
            _resourceManager.AddResource(op2Bit0);
            _resourceManager.AddResource(op2Bit1);
            _resourceManager.AddResource(compareResult);

            // Создаем процессы
            var bit0CompareProcess = new Process(
            id: 1,
            name: "Bit0Compare",
            priority: 2,
            cpuBurst: 100,
            count: 1,
            requiredResources: new List<int> { 1, 3, 5 },
            execute: resources =>
            {
                var op1Bit0 = (DataResource<bool>)resources[0];
                var op2Bit0 = (DataResource<bool>)resources[1];
                var compareResult = (ResultResource)resources[2];

                bool result = Convert.ToInt32(op1Bit0.Data) == Convert.ToInt32(op2Bit0.Data);
                compareResult.AddResult(1, result);
                return true;
            });


            var bit1CompareProcess = new Process(
            id: 2,
            name: "Bit1Compare",
            priority: 2,
            cpuBurst: 100,
            count: 1,
            requiredResources: new List<int> { 2, 4, 5 },
            execute: resources =>
            {
                var op1Bit1 = (DataResource<bool>)resources[0];
                var op2Bit1 = (DataResource<bool>)resources[1];
                var compareResult = (ResultResource)resources[2];

                bool result = Convert.ToInt32(op1Bit1.Data) == Convert.ToInt32(op2Bit1.Data);
                compareResult.AddResult(2, result);
                return true;
            });

            // Процесс финального сравнения
            var finalCompareProcess = new Process(
                id: 3,
                name: "FinalCompare",
                priority: 1,
                cpuBurst: 700,
                count: 1,
                requiredResources: new List<int> { 5 },
                execute: resources =>
                {
                    var resultResource = (ResultResource)resources[0];
                    var results = resultResource.GetResults();

                    // Явная проверка наличия ключей 1 и 2
                    if (results.TryGetValue(1, out bool bit0Result) &&
                        results.TryGetValue(2, out bool bit1Result))
                    {
                        bool finalResult = bit0Result && bit1Result;
                        resultResource.AddResult(3, finalResult);
                        return true;
                    }
                    return false; // Повторить позже
                }
            );

            _resourceManager.AddProcess(bit0CompareProcess); 
            _resourceManager.AddProcess(bit1CompareProcess);
            _resourceManager.AddProcess(finalCompareProcess);
        }

        /// <summary>
        /// Запуск процесса
        /// </summary>
        public void Run()
        {
            InitializeResourcesAndProcesses();
            _resourceManager.Execute(_systemLog);
            PrintResults();
        }

        /// <summary>
        /// Вывод результатов
        /// </summary>
        private void PrintResults()
        {
            var resultResource = _resourceManager.GetResource<ResultResource>(5);
            var results = resultResource.GetResults();

            Console.WriteLine("\n=== Результаты сравнения ===");
            Console.WriteLine($"Бит 0: {results[1]}");
            Console.WriteLine($"Бит 1: {results[2]}");
            Console.WriteLine($"Итог: {((bool)results[3] ? "Равны" : "Не равны")}");
        }
    }

    /// <summary>
    /// Перечисление видов планировщиков управления
    /// </summary>
    public enum SchedulerType
    {
        RoundRobin,
        LCFS
    }
}