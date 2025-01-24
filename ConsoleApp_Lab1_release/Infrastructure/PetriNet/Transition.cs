namespace ConsoleApp_Lab1_release.Infrastructure.PetriNet
{
    /// <summary>
    /// Класс перехода
    /// </summary>
    internal class Transition
    {
        /// <summary>
        /// Уникальное имя перехода
        /// </summary>
        public string Name { get; } // Уникальное имя перехода

        /// <summary>
        /// Входные места
        /// </summary>
        public List<Place> InputPlaces { get; } // Входные места

        /// <summary>
        /// Выходные места
        /// </summary>
        public List<Place> OutputPlaces { get; } // Выходные места

        /// <summary>
        /// Условие срабатывания
        /// </summary>
        public Func<bool> Condition { get; } // Условие срабатывания

        /// <summary>
        /// Конструктор перехода
        /// </summary>
        /// <param name="name">Название перехода</param>
        /// <param name="condition">Условие срабатывания</param>
        public Transition(string name, Func<bool> condition)
        {
            Name = name;
            Condition = condition;
            InputPlaces = new List<Place>();
            OutputPlaces = new List<Place>();
        }

        /// <summary>
        /// Добавление входного места
        /// </summary>
        /// <param name="place">Место перехода</param>
        public void AddInputPlace(Place place)
        {
            InputPlaces.Add(place);
        }

        /// <summary>
        /// Добавление выходного места
        /// </summary>
        /// <param name="place">Место перехода</param>
        public void AddOutputPlace(Place place)
        {
            OutputPlaces.Add(place);
        }

        /// <summary>
        /// Проверка возможности срабатывания
        /// </summary>
        /// <returns>True - если возможно перейти
        /// False - Если нет возможности перейти</returns>
        public bool CanFire()
        {
            // Проверяем, достаточно ли токенов во входных местах
            foreach (var place in InputPlaces)
            {
                if (place.Tokens == 0)
                {
                    return false;
                }
            }
            // Проверяем условие перехода
            return Condition();
        }

        /// <summary>
        /// Срабатывание перехода
        /// </summary>
        /// <exception cref="InvalidOperationException">Исключение: если переход не может сработать</exception>
        public void Fire()
        {
            if (!CanFire())
            {
                throw new InvalidOperationException($"Переход {Name} не может сработать.");
            }

            // Убираем токены из входных мест
            foreach (var place in InputPlaces)
            {
                place.Tokens--;
            }

            // Добавляем токены в выходные места
            foreach (var place in OutputPlaces)
            {
                place.Tokens++;
            }
        }
    }
}
