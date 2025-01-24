namespace ConsoleApp_Lab1_release.Infrastructure.PetriNet
{
    /// <summary>
    /// Реализация сети Петри для блокировки ресурсов
    /// </summary>
    internal class PetriNetRun
    {
        /// <summary>
        /// Список мест
        /// </summary>
        public List<Place> Places { get; } = new List<Place>();

        /// <summary>
        /// Список переходов
        /// </summary>
        public List<Transition> Transitions { get; } = new List<Transition>();

        private readonly object _sync = new object();

        /// <summary>
        /// Добавление места
        /// </summary>
        /// <param name="name">Название места</param>
        /// <param name="initialTokens">Кол-во мест</param>
        public void AddPlace(string name, int initialTokens = 0)
        {
            Places.Add(new Place(name, initialTokens));
        }

        /// <summary>
        /// Добавление перехода
        /// </summary>
        /// <param name="name">Название перехода</param>
        /// <param name="condition">Возможность перехода</param>
        public void AddTransition(string name, Func<bool> condition)
        {
            Transitions.Add(new Transition(name, condition));
        }

        /// <summary>
        /// Добавление входной дуги (место -> переход)
        /// </summary>
        /// <param name="transitionName"></param>
        /// <param name="placeName"></param>
        /// <exception cref="ArgumentException">Исключение: если переход не найден</exception>
        public void AddInputArc(string transitionName, string placeName)
        {
            var transition = Transitions.FirstOrDefault(t => t.Name == transitionName);
            var place = Places.FirstOrDefault(p => p.Name == placeName);

            if (transition == null || place == null)
            {
                throw new ArgumentException("Переход или место не найдены.");
            }

            transition.AddInputPlace(place);
        }

        /// <summary>
        /// Добавление выходной дуги (переход -> место)
        /// </summary>
        /// <param name="transitionName">Название перехода</param>
        /// <param name="placeName">Место перехода</param>
        /// <exception cref="ArgumentException">Исключение: если переход или место не найден</exception>
        public void AddOutputArc(string transitionName, string placeName)
        {
            var transition = Transitions.FirstOrDefault(t => t.Name == transitionName);
            var place = Places.FirstOrDefault(p => p.Name == placeName);

            if (transition == null || place == null)
            {
                throw new ArgumentException("Переход или место не найдены.");
            }

            transition.AddOutputPlace(place);
        }

        /// <summary>
        /// Проверка возможности срабатывания перехода
        /// </summary>
        /// <param name="transitionName">Название перехода</param>
        /// <returns>Возращает true - если переход возможен
        /// false - если переход не возможен</returns>
        /// <exception cref="ArgumentException">Исключение: если переход не найден</exception>
        public bool CanFire(string transitionName)
        {
            lock (_sync)
            {
                var transition = Transitions.FirstOrDefault(t => t.Name == transitionName);
                if (transition == null)
                {
                    throw new ArgumentException("Переход не найден.");
                }

                return transition.CanFire();
            }
        }

        /// <summary>
        /// Срабатывание перехода
        /// </summary>
        /// <param name="transitionName">Название перехода</param>
        /// <exception cref="ArgumentException">Исключение: если переход не найден</exception>
        public void Fire(string transitionName)
        {
            lock (_sync)
            {
                var transition = Transitions.FirstOrDefault(t => t.Name == transitionName);
                if (transition == null)
                {
                    throw new ArgumentException("Переход не найден.");
                }

                transition.Fire();
            }
        }

        /// <summary>
        /// Получение количества токенов в месте
        /// </summary>
        /// <param name="placeName">Название места</param>
        /// <returns>Кол-во токенов в месте</returns>
        /// <exception cref="ArgumentException">Исключение: если место не найдено</exception>
        public int GetTokens(string placeName)
        {
            var place = Places.FirstOrDefault(p => p.Name == placeName);
            if (place == null)
            {
                throw new ArgumentException("Место не найдено.");
            }

            return place.Tokens;
        }
    }
}