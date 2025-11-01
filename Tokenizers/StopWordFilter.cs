using System;
using System.Collections.Generic;
using System.Linq;
using YesSql.Samples.FullText.Tokenizers;
using static System.Net.Mime.MediaTypeNames;

namespace YesSql.Samples.FullText.Tokenizers
{
    public class StopWordFilter : ITokenFilter
    {
        public IEnumerable<string> Filter(IEnumerable<string> tokens)
        {
            return tokens.Where(token => token.Length >= 2);
        }
    }
}

/*Explication ligne par ligne :
| Élément                                            | Description |
| -------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `public class StopWordFilter : ITokenFilter`       | Cette classe implémente l’interface `ITokenFilter` 
                                                        (ce qui signifie qu’elle doit définir une méthode 
                                                        `Filter(IEnumerable<string> tokens)`). |
| `Filter(IEnumerable<string> tokens)`               | C’est la méthode appelée pour **filtrer** les tokens
                                                        (les mots découpés du texte).                                                          |
| `return tokens.Where(token => token.Length >= 2);` | Ici, tu appliques un **filtre très simple** : 
                                                            tu ne gardes que les tokens (mots) dont la longueur
                                                        est **au moins 2 caractères**.            |*/


/*using System;
using System.Collections.Generic;
using System.Linq;

namespace YesSql.Samples.FullText.Filters
{
    public class StopWordFilter : ITokenFilter
    {
        private static readonly HashSet<string> _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "are", "as", "at", "be", "but", "by",
            "for", "if", "in", "into", "is", "it",
            "no", "not", "of", "on", "or", "such",
            "that", "the", "their", "then", "there", "these",
            "they", "this", "to", "was", "will", "with"
        };

        public IEnumerable<string> Filter(IEnumerable<string> tokens)
        {
            return tokens.Where(t => !_stopWords.Contains(t));
        }
    }
}*/

/*La classe StopWordFilter dans YesSql (souvent utilisée avec le module FullText) sert à supprimer les mots vides 
 * (stop words) d’un texte avant l’indexation ou la recherche plein texte.
 🧩 Contexte

Dans le moteur de recherche interne de YesSql, on passe souvent par une chaîne de traitements pour préparer le texte :

Tokenization → Découpe le texte en mots (tokens)
→ ex : "Bonjour le monde" → ["Bonjour", "le", "monde"]

Filtrage des stop words → Supprime les mots sans intérêt pour la recherche
→ ex : ["Bonjour", "le", "monde"] → ["Bonjour", "monde"]

C’est justement le rôle de StopWordFilter.
Exemple typique de code (tiré de YesSql.Samples.FullText)

using System;
using System.Collections.Generic;
using System.Linq;

namespace YesSql.Samples.FullText.Filters
{
    public class StopWordFilter : ITokenFilter
    {
        private static readonly HashSet<string> _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "are", "as", "at", "be", "but", "by",
            "for", "if", "in", "into", "is", "it",
            "no", "not", "of", "on", "or", "such",
            "that", "the", "their", "then", "there", "these",
            "they", "this", "to", "was", "will", "with"
        };

        public IEnumerable<string> Filter(IEnumerable<string> tokens)
        {
            return tokens.Where(t => !_stopWords.Contains(t));
        }
    }
}*/

//*Explication du code :
//| Élément                            | Rôle                                                                                                                         |
//| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
//| `ITokenFilter`                     | Interface que tous les filtres de tokens doivent implémenter. Elle contient 
//|                                    |   une méthode `Filter(IEnumerable<string> tokens)` |
//| `_stopWords`                       | Liste (ici un `HashSet` pour plus de rapidité) de mots à ignorer                                                             |
//| `Filter(...)`                      | Reçoit les tokens et renvoie ceux qui **ne sont pas** dans la liste des mots vides                                           |
//| `StringComparer.OrdinalIgnoreCase` | Permet de faire la comparaison **sans tenir compte de la casse** ("The" = "the")                                           |



