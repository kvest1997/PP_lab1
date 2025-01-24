namespace ConsoleApp_Lab1_release.Infrastructure.PetriNet
{
    /// <summary>
    /// Представляет место в сети Петри, 
    /// содержащее токены (ресурсы или состояния).
    /// </summary>
    internal class Place
    {
        /// <summary>
        /// Уникальное имя места
        /// </summary>
        public string Name { get; } 
        
        /// <summary>
        /// Количество токенов
        /// </summary>
        public int Tokens { get; set; } 

        /// <summary>
        /// Конструктор места
        /// </summary>
        /// <param name="name">Название места</param>
        /// <param name="initialTokens">Кол-во мест</param>
        public Place(string name, int initialTokens = 0)
        {
            Name = name;
            Tokens = initialTokens;
        }
    }
}
