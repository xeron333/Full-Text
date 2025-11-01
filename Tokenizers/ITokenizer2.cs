using System.Collections.Generic;

namespace YesSql.Samples.FullText.Tokenizers
{
    public interface ITokenizer2
    {
        IEnumerable<string> Tokenize2(string text);
    }
}
