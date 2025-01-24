namespace ConsoleApp_Lab1_release.Infrastructure.PetriNet
{
    /// <summary>
    /// Представляет место в сети Петри, 
    /// содержащее токены (ресурсы или состояния).
    /// </summary>
    internal class Place
    {
        public string Name { get; } // Уникальное имя места
        public int Tokens { get; set; } // Количество токенов

        public Place(string name, int initialTokens = 0)
        {
            Name = name;
            Tokens = initialTokens;
        }
    }
}
