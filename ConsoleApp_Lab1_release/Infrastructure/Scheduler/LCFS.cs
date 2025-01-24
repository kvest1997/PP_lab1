using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Infrastructure.Scheduler
{
    /// <summary>
    /// Реализация алгоритма LCFS
    /// </summary>
    internal class LCFS : ResourceManager
    {
        private readonly Stack<Process> _stack = new Stack<Process>();

        public LCFS(int quantumTime = 100, int maxT = 1000, int maxP = 10) 
            : base(quantumTime, maxT, maxP) { }

        
        public override void AddProcess(Process process)
        {
            _stack.Push(process);
            Processes = _stack.ToList();
            base.AddProcess(process);
        }

        /// <summary>
        /// Получаем очередь процессов по принципу LCFS (последний пришел, первый вышел)
        /// </summary>
        protected override IReadOnlyCollection<Process> GetProcessQueue() 
            => _stack.Reverse().ToList().AsReadOnly();
    }
}
