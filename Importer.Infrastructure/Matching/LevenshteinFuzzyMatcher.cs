using Importer.Domain.Entities;
using Importer.Domain.Interfaces;

namespace Importer.Infrastructure.Matching
{
    public class LevenshteinFuzzyMatcher : IFuzzyMatcher
    {
        public Client? Match(string? extractedName, List<Client> officialClients)
        {
            if (string.IsNullOrWhiteSpace(extractedName))
                return null;

            extractedName = extractedName.Trim();
            Client? best = null;
            int bestScore = int.MaxValue;

            foreach (var c in officialClients)
            {
                var full = c.FullName;
                int score = Levenshtein(extractedName, full);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return bestScore <= 5 ? best : null; 
        }

        private static int Levenshtein(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            var d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
    }
}