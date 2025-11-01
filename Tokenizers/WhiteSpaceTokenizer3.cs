using System.Collections.Generic;
using YesSql.Samples.FullText.Tokenizers;

public class WhiteSpaceTokenizer3 : ITokenizer3
{
    public IEnumerable<string> Tokenize3(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var current = new System.Text.StringBuilder();

        foreach (var ch in text)
        {
            if (char.IsLetter(ch))
            {
                current.Append(char.ToLowerInvariant(ch)); // met tout en minuscule
            }
            else if (current.Length > 0)
            {
                yield return current.ToString(); // mot terminé
                current.Clear();
            }
        }

        if (current.Length > 0)
            yield return current.ToString(); // dernier mot
    }
}

