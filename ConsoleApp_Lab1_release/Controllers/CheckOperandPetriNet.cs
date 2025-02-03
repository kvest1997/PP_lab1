using ConsoleApp_Lab1_release.Infrastructure.PetriNet;
using ConsoleApp_Lab1_release.Models;

namespace ConsoleApp_Lab1_release.Controllers
{
    internal class CheckOperandPetriNet
    {
        public PetriNetRun Petri { get; } = new PetriNetRun();
        private Operands Operands { get; } = new Operands();

        public CheckOperandPetriNet(int operand1, int operand2)
        {
            Operands.Op1Bits[0] = (operand1 & 0b10) != 0;
            Operands.Op1Bits[1] = (operand1 & 0b01) != 0;
            Operands.Op2Bits[0] = (operand2 & 0b10) != 0;
            Operands.Op2Bits[1] = (operand2 & 0b01) != 0;

            Console.WriteLine($"Операнд 1: {Convert.ToInt32(Operands.Op1Bits[0])}{Convert.ToInt32(Operands.Op1Bits[1])}");
            Console.WriteLine($"Операнд 2: {Convert.ToInt32(Operands.Op2Bits[0])}{Convert.ToInt32(Operands.Op2Bits[1])}");

            BuildPetriNetStructure();
            InitializeOperandBits();
            ExecuteWorkflow();
        }

        private void BuildPetriNetStructure()
        {
            // Добавляем места согласно схеме
            AddPlaces();
            AddTransitions();
            ConnectArcs();
        }

        private void AddPlaces()
        {
            // Основные биты операндов
            Petri.AddPlace("Bit1_0", Convert.ToInt32(Operands.Op1Bits[0]));
            Petri.AddPlace("Bit1_1", Convert.ToInt32(Operands.Op1Bits[1]));
            Petri.AddPlace("Bit2_0", Convert.ToInt32(Operands.Op2Bits[0]));
            Petri.AddPlace("Bit2_1", Convert.ToInt32(Operands.Op2Bits[1]));

            // Сервисные места
            Petri.AddPlace("EqualsBit1", 1);
            Petri.AddPlace("EqualsBit2", 1);
            Petri.AddPlace("NotEqual1");
            Petri.AddPlace("NotEqual2");
            Petri.AddPlace("Results");
        }

        private void AddTransitions()
        {
            // Базовые переходы для обработки битов
            Petri.AddTransition("GetBit1_0", () => true);
            Petri.AddTransition("GetBit1_1", () => true);
            Petri.AddTransition("GetBit2_0", () => true);
            Petri.AddTransition("GetBit2_1", () => true);

            // Переходы сравнения
            Petri.AddTransition("GetNotEq1", () => Petri.GetTokens("EqualsBit1") >= 2);
            Petri.AddTransition("GetNotEq2", () => Petri.GetTokens("EqualsBit2") >= 2);
            Petri.AddTransition("GetResultBit", () =>
                Petri.GetTokens("EqualsBit1") > 0  &&
                Petri.GetTokens("EqualsBit2") > 0 );
        }

        private void ConnectArcs()
        {
            // Соединения для GetBit1_0
            AddWeightedArc("Bit1_0", "GetBit1_0", 1);
            AddWeightedArc("GetBit1_0", "EqualsBit1", 1);

            // Соединения для GetBit1_1
            AddWeightedArc("Bit1_1", "GetBit1_1", 1);
            AddWeightedArc("GetBit1_1", "EqualsBit2", 1);

            // Соединения для GetBit2_0
            AddWeightedArc("Bit2_0", "GetBit2_0", 1);
            AddWeightedArc("GetBit2_0", "EqualsBit1", 1);

            // Соединения для GetBit2_1
            AddWeightedArc("Bit2_1", "GetBit2_1", 1);
            AddWeightedArc("GetBit2_1", "EqualsBit2", 1);

            // Логика сравнения
            AddWeightedArc("EqualsBit1", "GetNotEq1", 2);
            AddWeightedArc("GetNotEq1", "NotEqual1", 2);

            AddWeightedArc("EqualsBit2", "GetNotEq2", 2);
            AddWeightedArc("GetNotEq2", "NotEqual2", 2);

            AddWeightedArc("EqualsBit1", "GetResultBit", 1);
            AddWeightedArc("EqualsBit2", "GetResultBit", 1);
            AddWeightedArc("GetResultBit", "Results", 1);
        }

        private void AddWeightedArc(string source, string target, int weight)
        {
            for (int i = 0; i < weight; i++)
            {
                if (Petri.Transitions.Any(t => t.Name == target))
                    Petri.AddInputArc(target, source);
                else
                    Petri.AddOutputArc(source, target);
            }
        }

        private void InitializeOperandBits()
        {
            // Переносим токены из исходных битов в систему
            TryFire("GetBit1_0");
            TryFire("GetBit1_1");
            TryFire("GetBit2_0");
            TryFire("GetBit2_1");
        }

        private void ExecuteWorkflow()
        {
            // Последовательность выполнения переходов
            var executionOrder = new[] { "GetNotEq1", "GetNotEq2", "GetResultBit" };
            foreach (var transition in executionOrder)
            {
                TryFire(transition);
            }
        }

        private void TryFire(string transition)
        {
            if (Petri.CanFire(transition))
                Petri.Fire(transition);

            Console.WriteLine("===Состояние сети петри===");
            Console.WriteLine($"\nПереход: {transition}");
            foreach (var place in Petri.Places)
            {
                Console.WriteLine($"Место {place.Name} - {place.Tokens}");
            }


            var tra = Petri.Transitions.FirstOrDefault(x => x.Name == transition);


            foreach (var input in tra.InputPlaces)
            {
                Console.WriteLine($"Входные месте {input.Name} - {input.Tokens}");
            }

            foreach (var output in tra.OutputPlaces)
            {
                Console.WriteLine($"Выходные месте {output.Name} - {output.Tokens}");
            }

        }

        public int GetComparisonResult() =>
            Petri.GetTokens("Results") > 0 ? 1 : 0;
    }
}
