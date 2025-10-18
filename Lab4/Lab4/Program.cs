using System.Text;

public class FuzzySearch
{
    private const string AnsiYellow = "\u001b[33m";
    private const string AnsiReset = "\u001b[0m";

    public static int CalculateLevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1)) return s2.Length;
        if (string.IsNullOrEmpty(s2)) return s1.Length;

        int n = s1.Length;
        int m = s2.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    public static List<(string substring, int startIndex, int distance)> FindAllPossibleSubstrings(
        string mainText, string searchSubstring, int maxDistance)
    {
        var allPossibleMatches = new List<(string substring, int startIndex, int distance)>();

        if (string.IsNullOrWhiteSpace(mainText) || string.IsNullOrWhiteSpace(searchSubstring) || maxDistance < 0)
        {
            return allPossibleMatches;
        }

        int searchLen = searchSubstring.Length;
        int mainLen = mainText.Length;

        int minSubstringLength = Math.Max(1, searchLen - maxDistance);
        int maxSubstringLength = searchLen + maxDistance;

        for (int i = 0; i < mainLen; i++)
        {
            for (int len = minSubstringLength; len <= maxSubstringLength; len++)
            {
                if (i + len > mainLen) break;

                string currentSubstring = mainText.Substring(i, len);
                int distance = CalculateLevenshteinDistance(searchSubstring.ToLower(), currentSubstring.ToLower());

                if (distance <= maxDistance)
                {
                    allPossibleMatches.Add((currentSubstring, i, distance));
                }
            }
        }
        return allPossibleMatches;
    }

    public static List<(string substring, int startIndex, int distance)> SelectExclusiveNonOverlapping(
        List<(string substring, int startIndex, int distance)> allMatches)
    {
        if (allMatches == null || allMatches.Count == 0)
        {
            return new List<(string substring, int startIndex, int distance)>();
        }

        var sortedMatches = allMatches
            .OrderBy(m => m.startIndex)
            .ThenBy(m => m.distance)
            .ThenBy(m => m.substring.Length)
            .ToList();

        var selectedMatches = new List<(string substring, int startIndex, int distance)>();
        int lastEndTime = 0;

        foreach (var currentMatch in sortedMatches)
        {
            if (currentMatch.startIndex >= lastEndTime)
            {
                bool isTrimmedOrExtendedAtEdge = false;
                int searchLen = currentMatch.substring.Length;

                selectedMatches.Add(currentMatch);
                lastEndTime = currentMatch.startIndex + currentMatch.substring.Length;
            }
        }

        return selectedMatches;
    }

    public static string HighlightSubstrings(string mainText, List<(string substring, int startIndex, int distance)> foundSubstrings)
    {
        if (string.IsNullOrEmpty(mainText) || foundSubstrings == null || foundSubstrings.Count == 0)
        {
            return mainText ?? "";
        }

        StringBuilder sb = new StringBuilder();
        int currentTextIndex = 0;

        foreach (var match in foundSubstrings)
        {
            if (match.startIndex < currentTextIndex)
            {
                continue;
            }

            if (match.startIndex > currentTextIndex)
            {
                sb.Append(mainText.Substring(currentTextIndex, match.startIndex - currentTextIndex));
            }

            sb.Append(AnsiYellow);
            sb.Append(match.substring);
            sb.Append(AnsiReset);

            currentTextIndex = match.startIndex + match.substring.Length;
        }

        if (currentTextIndex < mainText.Length)
        {
            sb.Append(mainText.Substring(currentTextIndex));
        }

        return sb.ToString();
    }


    public static void Main(string[] args)
    {
        string mainText = GetMainTextFromUser();
        string searchSubstring = GetSearchSubstringFromUser();
        int maxDistance = GetMaxDistanceFromUser();

        Console.WriteLine("\n--- Результаты поиска ---");

        var allPossibleMatches = FindAllPossibleSubstrings(mainText, searchSubstring, maxDistance);
        var selectedNonOverlappingMatches = SelectExclusiveNonOverlapping(allPossibleMatches);

        if (selectedNonOverlappingMatches.Count > 0)
        {
            Console.WriteLine($"Исходный текст с подсвеченными подстроками, близкими к '{searchSubstring}' (макс. расстояние {maxDistance}):\n");
            string highlightedText = HighlightSubstrings(mainText, selectedNonOverlappingMatches);
            Console.WriteLine(highlightedText);
        }
        else
        {
            Console.WriteLine($"Не найдено приблизительных подстрок для '{searchSubstring}' (макс. расстояние {maxDistance}, эксклюзивные вхождения).");
            Console.WriteLine($"Исходный текст:\n{mainText}");
        }
    }

    private static string GetMainTextFromUser()
    {
        string text;
        Console.WriteLine("\nВведите текст, в котором будет осуществляться поиск:");
        text = Console.ReadLine();

        return text;
    }

    private static string GetSearchSubstringFromUser()
    {
        string query;
        while (true)
        {
            Console.Write("\nВведите текст, который вы хотите найти как подстроку: ");
            query = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(query))
            {
                return query;
            }
            else
            {
                Console.WriteLine("Пожалуйста, введите непустой текст для поиска подстроки.");
            }
        }
    }

    private static int GetMaxDistanceFromUser()
    {
        int maxDistance;
        while (true)
        {
            Console.Write("Введите максимально допустимое расстояние (целое число): ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out maxDistance) && maxDistance >= 0)
            {
                return maxDistance;
            }
            else
            {
                Console.WriteLine("Некорректный ввод.");
            }
        }
    }
}