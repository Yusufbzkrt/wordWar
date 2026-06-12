namespace KelimeOyunu.Application.Helpers;

/// <summary>
/// İki string arasındaki Levenshtein mesafesini hesaplar.
/// Fuzzy matching için kullanılır — 1-2 harflik typo'lar tolere edilir.
/// </summary>
public static class LevenshteinDistance
{
    public static int Calculate(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        int sourceLength = source.Length;
        int targetLength = target.Length;

        var matrix = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= targetLength; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                int cost = (char.ToLowerInvariant(source[i - 1]) == char.ToLowerInvariant(target[j - 1])) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(
                        matrix[i - 1, j] + 1,      // Silme
                        matrix[i, j - 1] + 1),      // Ekleme
                    matrix[i - 1, j - 1] + cost);   // Değiştirme
            }
        }

        return matrix[sourceLength, targetLength];
    }
}
