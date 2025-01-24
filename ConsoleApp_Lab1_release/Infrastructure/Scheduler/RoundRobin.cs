using ConsoleApp_Lab1_release.Models;
using System.Collections.Concurrent;

namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    internal class RoundRobin : ResourceManager
    {
        private readonly Queue<Process> _queue = new Queue<Process>();

        public RoundRobin(int quantumTime = 100, int maxT = 1000, int maxP = 10)
            : base(quantumTime, maxT, maxP) { }


        public override void AddProcess(Process process)
        {
            var tempList = _queue.ToList();
            tempList.Add(process);
            tempList = tempList
                .OrderByDescending(p => (double)p.Priority / MaxP)
                .ToList();

            _queue.Clear();
            foreach (var p in tempList) _queue.Enqueue(p);

            Processes = _queue.ToList();
            base.AddProcess(process);
        }

        /// <summary>
        /// Получаем очередь процессов по принципу Round Robin
        /// </summary>
        protected override IReadOnlyCollection<Process> GetProcessQueue() 
            => _queue.ToList().AsReadOnly();

    }
 }
