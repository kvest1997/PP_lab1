using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Infrastructure.Scheduler;
using ConsoleApp_Lab1_release.Models;
using System;
using System.Collections.Generic;

namespace ConsoleApp_Lab1_release.Controllers
{
    public class ParallelProcessManager
    {
        private readonly ResourceManager _resourceManager;
        private readonly List<string> _systemLog = new List<string>();
        private int _operand1;
        private int _operand2;
        private int _result;

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
            _operand1 = op1;
            _operand2 = op2;
        }

        public void InitializeResourcesAndProcesses()
        {
            // Добавляем ресурсы
            var dataResource = new Resource(1, "DataResource", 1);
            var resultResource = new Resource(2, "ResultResource", 1);
            _resourceManager.AddResource(dataResource);
            _resourceManager.AddResource(resultResource);

            // Создаем процессы
            var process1 = new Process(
                id: 1,
                name: "Process1",
                priority: 3,
                cpuBurst: 200,
                count: 1,
                requiredResources: new List<int> { 1, 2 }
            );
            // Создаем процессы
            var process2 = new Process(
                id: 2,
                name: "Process2",
                priority: 1,
                cpuBurst: 200,
                count: 1,
                requiredResources: new List<int> { 1, 2 }
            );
            // Создаем процессы
            var process3 = new Process(
                id: 3,
                name: "Process3",
                priority: 2,
                cpuBurst: 200,
                count: 1,
                requiredResources: new List<int> { 1, 2 }
            );
            _resourceManager.AddProcess(process1);
            _resourceManager.AddProcess(process2);
            _resourceManager.AddProcess(process3);
        }

        public void Run()
        {
            _resourceManager.Execute(_systemLog, () =>
            {
                _result = Convert.ToInt32(_operand1 == _operand2);
            });
            PrintResults();
        }

        private void PrintResults()
        {
            Console.WriteLine("\n=== Результат проверки ===");
            Console.WriteLine($"Операнд 1: {_operand1}, Операнд 2: {_operand2}");
            Console.WriteLine($"Результат: {_result}");
        }
    }

    public enum SchedulerType
    {
        RoundRobin,
        LCFS
    }
}