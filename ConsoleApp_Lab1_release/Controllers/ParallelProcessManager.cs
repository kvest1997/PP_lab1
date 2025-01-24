using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Infrastructure.Scheduler;
using ConsoleApp_Lab1_release.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ConsoleApp_Lab1_release.Controllers
{
    /// <summary>
    /// Класс для реализации проверки операторов
    /// </summary>
    public class ParallelProcessManager
    {
        private readonly ResourceManager _resourceManager;
        private readonly List<string> _systemLog = new List<string>();
        private int _operand1;
        private int _operand2;

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

        /// <summary>
        /// Инициализация данных
        /// </summary>
        private void InitializeResourcesAndProcesses()
        {
            // Добавляем ресурсы
            var dataResource = new DataResource<Operands>(1, "DataResource", 1, data: new Operands { Op1 = _operand1, Op2 = _operand2});
            
            var resultResource = new ResultResource(2, "ResultResource", 1);

            _resourceManager.AddResource(dataResource);
            _resourceManager.AddResource(resultResource);

            // Создаем процессы
            var process1 = new Process(
                id: 1,
                name: "Process1",
                priority: 3,
                cpuBurst: 400,
                count: 1,
                requiredResources: new List<int> { 1, 2 },
                execute: manager =>
                {
                    var data = manager.GetResource<DataResource<Operands>>(1).GetOperands();
                    var result = data.Op1 > data.Op2;
                    manager.GetResource<ResultResource>(2).AddResult(1, result);
                }
            );
            // Создаем процессы
            var process2 = new Process(
                id: 2,
                name: "Process2",
                priority: 1,
                cpuBurst: 500,
                count: 1,
                requiredResources: new List<int> { 1, 2 },
                execute: manager =>
                {
                    var data = manager.GetResource<DataResource<Operands>>(1).GetOperands();
                    var result = data.Op1 < data.Op2;
                    manager.GetResource<ResultResource>(2).AddResult(2, result);
                }
            );
            // Создаем процессы
            var process3 = new Process(
                id: 3,
                name: "Process3",
                priority: 2,
                cpuBurst: 200,
                count: 1,
                requiredResources: new List<int> { 1, 2 },
                execute: manager =>
                {
                    var data = manager.GetResource<DataResource<Operands>>(1).GetOperands();
                    var result = data.Op1 == data.Op2;
                    manager.GetResource<ResultResource>(2).AddResult(3, result);
                }
            );
            _resourceManager.AddProcess(process1);
            _resourceManager.AddProcess(process2);
            _resourceManager.AddProcess(process3);
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
            var resultResource = _resourceManager.GetResource<ResultResource>(2);
            var results = resultResource.GetResults();

            Console.WriteLine("\n=== Результат проверки ===");
            foreach (var item in results)
            {
                Console.WriteLine($"{item.Key} - {item.Value}");
            }
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