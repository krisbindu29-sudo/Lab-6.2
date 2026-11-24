using System;
using System.IO;

namespace Lab6_Task2
{
    // Власний клас виключення для обробки помилок введення даних з клавіатури
    public class InvalidDataInputException : Exception
    {
        public InvalidDataInputException(string message) : base(message) { }
    }

    // ІНТЕРФЕЙС (за методичкою ЛР №6): лише оголошення методів,
    // без модифікаторів доступу і без реалізації.
    // Сюди винесено два методи статистики за кількістю відвідувачів
    public interface IVisitorStatistics
    {
        // Середня кількість відвідувачів
        double GetAverageVisitors(Appointment[] appointments);

        // Прийом з мінімальною кількістю відвідувачів
        Appointment GetMinVisitorsAppointment(Appointment[] appointments);
    }

    // Абстрактний базовий клас "DoctorBase" (поля та контракт для нащадків)
    public abstract class DoctorBase
    {
        // Спільні властивості для всіх похідних класів
        public string LastName { get; set; }
        public int Experience { get; set; }

        // Конструктор за замовчуванням
        public DoctorBase() { }

        // Параметризований конструктор базового класу
        public DoctorBase(string lastName, int experience)
        {
            LastName = lastName;
            Experience = experience;
        }

        // Зчитування записів з текстового файлу у масив Appointment[]
        public static Appointment[] ReadFromFile(string filePath)
        {
            // Якщо файлу немає - створюємо початкову базу
            if (!File.Exists(filePath))
            {
                CreateInitialDatabase(filePath);
            }

            string[] lines = File.ReadAllLines(filePath);

            // Підраховуємо кількість рядків потрібного формату для розміру масиву
            int validCount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    string[] parts = lines[i].Split(';');
                    if (parts.Length == 5)
                    {
                        validCount++;
                    }
                }
            }

            Appointment[] appointments = new Appointment[validCount];
            int index = 0;

            // Заповнюємо масив екземплярами похідного класу Appointment
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(';');
                // Стаж (parts[1]) і кількість відвідувачів (parts[3]) мають бути числами
                if (parts.Length == 5 &&
                    int.TryParse(parts[1], out int exp) &&
                    int.TryParse(parts[3], out int visitors))
                {
                    appointments[index] = new Appointment(
                        parts[0],
                        exp,
                        parts[2],
                        visitors,
                        parts[4]
                    );
                    index++;
                }
            }

            // Якщо частину рядків не вдалося розпізнати - вкорочуємо масив
            if (index < validCount)
            {
                Array.Resize(ref appointments, index);
            }

            return appointments;
        }

        // Створення початкової бази з 5 тестовими записами
        private static void CreateInitialDatabase(string filePath)
        {
            string[] initialRecords = new string[]
            {
                "Коваленко;12;Понеділок;15;Первинний огляд пацієнтів",
                "Довженко;12;Вівторок;8;Консультація після здачі аналізів",
                "Фіщук;1;Середа;22;Плановий огляд та виписка рецептів",
                "Коваленко;7;Четвер;5;Терміновий позаплановий прийом",
                "Хмара;10;П'ятниця;19;Профілактичний медогляд працівників"
            };
            File.WriteAllLines(filePath, initialRecords);
        }

        // Збереження масиву записів у текстовий файл
        public static void SaveToFile(string filePath, Appointment[] appointments)
        {
            string[] lines = new string[appointments.Length];
            for (int i = 0; i < appointments.Length; i++)
            {
                Appointment a = appointments[i];
                lines[i] = $"{a.LastName};{a.Experience};{a.Day};{a.VisitorsCount};{a.Comment}";
            }
            File.WriteAllLines(filePath, lines);
        }

        // Додавання запису: створюємо новий масив на 1 елемент більший
        public static void AddRecord(string filePath, Appointment newAppointment)
        {
            Appointment[] current = ReadFromFile(filePath);
            Appointment[] updated = new Appointment[current.Length + 1];

            for (int i = 0; i < current.Length; i++)
            {
                updated[i] = current[i];
            }
            updated[current.Length] = newAppointment;

            SaveToFile(filePath, updated);
        }

        // Редагування запису за індексом
        public static void EditRecord(string filePath, int index, Appointment updatedAppointment)
        {
            Appointment[] appointments = ReadFromFile(filePath);
            if (index >= 0 && index < appointments.Length)
            {
                appointments[index] = updatedAppointment;
                SaveToFile(filePath, appointments);
            }
            else
            {
                throw new InvalidDataInputException($"Запису з номером [{index}] не існує!");
            }
        }

        // Видалення запису за індексом (решта елементів копіюється у новий менший масив)
        public static void DeleteRecord(string filePath, int index)
        {
            Appointment[] current = ReadFromFile(filePath);
            if (index < 0 || index >= current.Length)
            {
                throw new InvalidDataInputException($"Запису з номером [{index}] не існує!");
            }

            Appointment[] updated = new Appointment[current.Length - 1];
            int newIdx = 0;

            for (int i = 0; i < current.Length; i++)
            {
                if (i == index) continue;  // пропускаємо видалений запис
                updated[newIdx] = current[i];
                newIdx++;
            }

            SaveToFile(filePath, updated);
        }

        // Єдиний абстрактний метод, що залишився в класі (два інші перенесено в інтерфейс)
        public abstract Appointment GetLongestCommentAppointment(Appointment[] appointments);
    }

    // Похідний клас: успадковує абстрактний клас DoctorBase І реалізує інтерфейс IVisitorStatistics
    public class Appointment : DoctorBase, IVisitorStatistics
    {
        public string Day { get; set; }
        public int VisitorsCount { get; set; }
        public string Comment { get; set; }

        public Appointment() : base() { }

        // Параметри лікаря передаються в конструктор базового класу
        public Appointment(string lastName, int experience, string day, int visitorsCount, string comment)
            : base(lastName, experience)
        {
            Day = day;
            VisitorsCount = visitorsCount;
            Comment = comment;
        }

        // ---- Реалізація методів ІНТЕРФЕЙСУ (public, без слова override) ----

        // Середня кількість відвідувачів
        public double GetAverageVisitors(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return 0;

            double sum = 0;
            for (int i = 0; i < appointments.Length; i++)
            {
                sum += appointments[i].VisitorsCount;  // накопичуємо суму
            }
            return sum / appointments.Length;
        }

        // Прийом з мінімальною кількістю відвідувачів
        public Appointment GetMinVisitorsAppointment(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return null;

            Appointment minApp = appointments[0];  // початково мінімум - перший елемент
            for (int i = 1; i < appointments.Length; i++)
            {
                if (appointments[i].VisitorsCount < minApp.VisitorsCount)
                {
                    minApp = appointments[i];
                }
            }
            return minApp;
        }

        // ---- Реалізація абстрактного методу БАЗОВОГО КЛАСУ (з override) ----

        // Прийом з найдовшим коментарем
        public override Appointment GetLongestCommentAppointment(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return null;

            Appointment longestApp = appointments[0];
            for (int i = 1; i < appointments.Length; i++)
            {
                if (appointments[i].Comment.Length > longestApp.Comment.Length)
                {
                    longestApp = appointments[i];
                }
            }
            return longestApp;
        }

        // Рядкове подання одного запису
        public override string ToString()
        {
            return $"Лікар: {LastName} (стаж: {Experience} р.) | День: {Day} | Відвідувачів: {VisitorsCount} | Коментар: {Comment}";
        }
    }

    // Головний клас програми - в кінці файлу
    class Program
    {
        static string filePath = "doctors.txt";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // Один об'єкт похідного класу, до якого є ДВА посилання:
            Appointment obj = new Appointment();
            DoctorBase doctorService = obj;        // посилання на абстрактний клас
            IVisitorStatistics statistics = obj;   // посилання на інтерфейс

            while (true)
            {
                Console.WriteLine("\n МЕНЮ ЗАВДАННЯ 2 (Абстрактний клас DoctorBase + інтерфейс IVisitorStatistics) ");
                Console.WriteLine("[v] - Вивести інформацію з файла на екран");
                Console.WriteLine("[d] - Додати новий запис");
                Console.WriteLine("[r] - Редагувати запис");
                Console.WriteLine("[x] - Видалити запис");
                Console.WriteLine("[c] - Обчислити та вивести результати ");
                Console.WriteLine("[Enter] - Вихід з програми");
                Console.Write("Натисніть відповідну клавішу: ");

                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();

                // Enter - завершення роботи
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("Роботу завершено.");
                    break;
                }

                try
                {
                    switch (char.ToLower(key.KeyChar))
                    {
                        case 'v':
                            DisplayRecords();
                            break;
                        case 'd':
                            AddNewRecord();
                            break;
                        case 'r':
                            EditExistingRecord();
                            break;
                        case 'x':
                            DeleteExistingRecord();
                            break;
                        case 'c':
                            // Передаємо обидва посилання: клас і інтерфейс
                            ShowCalculations(doctorService, statistics);
                            break;
                        default:
                            Console.WriteLine("Невідома клавіша! Спробуйте ще раз.");
                            break;
                    }
                }
                catch (InvalidDataInputException ex)
                {
                    Console.WriteLine($"\n[ПОМИЛКА ВВЕДЕННЯ]: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[ПОМИЛКА]: {ex.Message}");
                }
            }
        }

        // Виведення всіх записів на екран
        static void DisplayRecords()
        {
            Appointment[] appointments = DoctorBase.ReadFromFile(filePath);
            Console.WriteLine("\n--- Вміст бази даних (doctors.txt) ---");
            if (appointments.Length == 0)
            {
                Console.WriteLine("База даних порожня.");
                return;
            }

            for (int i = 0; i < appointments.Length; i++)
            {
                Console.WriteLine($"[{i}] {appointments[i]}");
            }
        }

        // Додавання нового запису
        static void AddNewRecord()
        {
            Console.WriteLine("\n--- Додавання запису ---");
            Appointment app = ReadAppointmentFromKeyboard();
            DoctorBase.AddRecord(filePath, app);
            Console.WriteLine("Запис успішно збережено у файл!");
        }

        // Редагування запису за номером
        static void EditExistingRecord()
        {
            DisplayRecords();
            Console.Write("\nВведіть номер запису для редагування: ");
            if (!int.TryParse(Console.ReadLine(), out int index))
                throw new InvalidDataInputException("Номер запису повинен бути цілим числом!");

            Console.WriteLine("Введіть оновлені дані:");
            Appointment updated = ReadAppointmentFromKeyboard();
            DoctorBase.EditRecord(filePath, index, updated);
            Console.WriteLine("Запис успішно відредаговано!");
        }

        // Видалення запису за номером
        static void DeleteExistingRecord()
        {
            DisplayRecords();
            Console.Write("\nВведіть номер запису для знищення: ");
            if (!int.TryParse(Console.ReadLine(), out int index))
                throw new InvalidDataInputException("Номер запису повинен бути цілим числом!");

            DoctorBase.DeleteRecord(filePath, index);
            Console.WriteLine("Запис успішно знищено!");
        }

        // Обчислення: два методи викликаються через ІНТЕРФЕЙС, третій - через абстрактний КЛАС
        static void ShowCalculations(DoctorBase service, IVisitorStatistics stats)
        {
            Appointment[] appointments = DoctorBase.ReadFromFile(filePath);
            if (appointments.Length == 0)
            {
                Console.WriteLine("Немає даних для обчислення!");
                return;
            }

            Console.WriteLine("\n--- Результати обчислень ---");

            // Метод інтерфейсу
            Console.WriteLine($"1) Середня кількість відвідувачів: {stats.GetAverageVisitors(appointments):F2}");

            // Метод інтерфейсу
            Appointment minVisitorsApp = stats.GetMinVisitorsAppointment(appointments);
            Console.WriteLine($"2) Прийом з мінімальною кількістю відвідувачів: {minVisitorsApp.Day} ({minVisitorsApp.VisitorsCount} відв.)");

            // Метод абстрактного класу DoctorBase (перевизначений в Appointment)
            Appointment longestCommentApp = service.GetLongestCommentAppointment(appointments);
            Console.WriteLine($"3) Прийом з найдовшим коментарем: {longestCommentApp.Day} -> \"{longestCommentApp.Comment}\"");
        }

        // Зчитування та перевірка даних запису з клавіатури
        static Appointment ReadAppointmentFromKeyboard()
        {
            Console.Write("Прізвище лікаря: ");
            string lastName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidDataInputException("Прізвище не може бути порожнім!");
            lastName = lastName.Replace(';', ',');  // ';' - роздільник у файлі, тому замінюємо

            Console.Write("Стаж (років): ");
            if (!int.TryParse(Console.ReadLine(), out int exp) || exp < 0)
                throw new InvalidDataInputException("Стаж має бути цілим невід'ємним числом!");

            Console.Write("День тижня: ");
            string day = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(day))
                throw new InvalidDataInputException("День прийому не може бути порожнім!");
            day = day.Replace(';', ',');

            Console.Write("Кількість відвідувачів: ");
            if (!int.TryParse(Console.ReadLine(), out int visitors) || visitors < 0)
                throw new InvalidDataInputException("Кількість відвідувачів має бути цілим невід'ємним числом!");

            Console.Write("Коментар: ");
            string comment = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(comment))
                throw new InvalidDataInputException("Коментар не може бути порожнім!");
            comment = comment.Replace(';', ',');

            return new Appointment(lastName, exp, day, visitors, comment);
        }
    }
}