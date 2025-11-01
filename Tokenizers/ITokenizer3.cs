using System.Collections.Generic;

namespace YesSql.Samples.FullText.Tokenizers
{
    public interface ITokenizer3
    {
        IEnumerable<string> Tokenize3(string text);
    }
}
