using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class DictionaryData
{
    public string Type { get; set; }
    public Dictionary<string, List<string>> Words { get; set; } = new();
}

class User
{
    public string Login { get; set; }
    public string Password { get; set; }
    public DateTime BirthDate { get; set; }
    public List<int> Results { get; set; } = new();
}

class Question
{
    public string Text { get; set; }
    public List<string> Options { get; set; }
    public List<int> Correct { get; set; }
}

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Словники");
            Console.WriteLine("2. Вікторина");
            Console.WriteLine("0. Вихід");

            switch (Console.ReadLine())
            {
                case "1": DictionaryMenu(); break;
                case "2": QuizLogin(); break;
                case "0": return;
            }
        }
    }

    static void DictionaryMenu()
    {
        Console.Write("Назва файлу словника: ");
        string file = Console.ReadLine() + ".json";

        DictionaryData data = File.Exists(file)
            ? JsonSerializer.Deserialize<DictionaryData>(File.ReadAllText(file))
            : new DictionaryData();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Створити словник");
            Console.WriteLine("2. Додати слово");
            Console.WriteLine("3. Пошук перекладу");
            Console.WriteLine("4. Видалити слово");
            Console.WriteLine("0. Назад");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("Тип словника: ");
                    data.Type = Console.ReadLine();
                    Save(file, data);
                    break;

                case "2":
                    Console.Write("Слово: ");
                    string word = Console.ReadLine();
                    Console.Write("Переклади (через кому): ");
                    var tr = Console.ReadLine().Split(',').Select(x => x.Trim()).ToList();

                    if (!data.Words.ContainsKey(word))
                        data.Words[word] = new List<string>();

                    foreach (var t in tr)
                        if (!data.Words[word].Contains(t))
                            data.Words[word].Add(t);

                    Save(file, data);
                    break;

                case "3":
                    Console.Write("Слово: ");
                    word = Console.ReadLine();
                    if (data.Words.ContainsKey(word))
                        Console.WriteLine(string.Join(", ", data.Words[word]));
                    else
                        Console.WriteLine("Не знайдено");
                    Console.ReadKey();
                    break;

                case "4":
                    Console.Write("Слово: ");
                    data.Words.Remove(Console.ReadLine());
                    Save(file, data);
                    break;

                case "0": return;
            }
        }
    }

    static void QuizLogin()
    {
        var users = Load<List<User>>("users.json") ?? new List<User>();

        Console.Clear();
        Console.Write("Логін: ");
        string login = Console.ReadLine();
        Console.Write("Пароль: ");
        string pass = Console.ReadLine();

        var user = users.FirstOrDefault(u => u.Login == login);

        if (user == null)
        {
            Console.Write("Дата народження (yyyy-mm-dd): ");
            user = new User
            {
                Login = login,
                Password = pass,
                BirthDate = DateTime.Parse(Console.ReadLine())
            };
            users.Add(user);
        }
        else if (user.Password != pass)
        {
            Console.WriteLine("Невірний пароль");
            Console.ReadKey();
            return;
        }

        QuizMenu(user, users);
    }

    static void QuizMenu(User user, List<User> users)
    {
        var quizzes = Load<Dictionary<string, List<Question>>>("quizzes.json");

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Нова вікторина");
            Console.WriteLine("2. Мої результати");
            Console.WriteLine("0. Вихід");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.WriteLine("Доступні розділи:");
                    foreach (var q in quizzes.Keys)
                        Console.WriteLine(q);

                    Console.Write("Розділ: ");
                    int score = StartQuiz(quizzes[Console.ReadLine()]);
                    user.Results.Add(score);
                    Save("users.json", users);
                    Console.WriteLine($"Результат: {score}/20");
                    Console.ReadKey();
                    break;

                case "2":
                    foreach (var r in user.Results)
                        Console.WriteLine(r);
                    Console.ReadKey();
                    break;

                case "0": return;
            }
        }
    }

    static int StartQuiz(List<Question> questions)
    {
        var rnd = new Random();
        int score = 0;

        foreach (var q in questions.OrderBy(x => rnd.Next()).Take(20))
        {
            Console.Clear();
            Console.WriteLine(q.Text);
            for (int i = 0; i < q.Options.Count; i++)
                Console.WriteLine($"{i}. {q.Options[i]}");

            var answers = Console.ReadLine().Split().Select(int.Parse).ToList();
            if (answers.OrderBy(x => x).SequenceEqual(q.Correct.OrderBy(x => x)))
                score++;
        }
        return score;
    }

    static void Save<T>(string file, T data)
    {
        File.WriteAllText(file,
            JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    static T Load<T>(string file)
    {
        return File.Exists(file)
            ? JsonSerializer.Deserialize<T>(File.ReadAllText(file))
            : default;
    }
}
