using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using YesSql.Samples.FullText.Indexes;

namespace YesSql.Samples.FullText.Tokenizers
{
    public class WhiteSpaceTokenizer : ITokenizer/*Rôle : découper le texte en mots(tokens) selon les caractères
                                                  * non alphabétiques.

    Utilisation : utilisée par ArticleIndexProvider pour extraire les mots du champ Content.

    Effet : crée les entrées dans la table ArticleByWord, une par mot (et par document).*/
                                                 //Elle implémente l’interface ITokenizer
    {
        public IEnumerable<string> Tokenize(string text)
        {
            var start = 0;//start marque le début du mot courant dans la chaîne.
            for (var cur = 0; cur < text.Length; cur++)
            {/*Tant que le caractère est une lettre, on continue (on est “dans” un mot).

                Dès qu’on tombe sur un séparateur (espace, ponctuation, chiffre, etc.), on sort du mot.*/
                if (Char.IsLetter(text[cur])) continue;

                if (cur - start > 1)
                {
                    yield return text.Substring(start, cur - start);
                }

                start = cur + 1;
            }

            if (start != text.Length)
            {
                yield return text.Substring(start);
            }
        }
    }
}
/*Le mot-clé yield en C# est un peu spécial : il permet de créer un itérateur de manière simple,
 * sans écrire toute la logique à la main.

🧠 1. Le rôle de yield return

Quand tu vois :
yield return text.Substring(start, cur - start);
👉 Cela ne renvoie pas immédiatement une liste complète,
mais fournit un élément à la fois à celui qui parcourt la méthode.
🧩 2. Exemple concret

Imaginons cette méthode :

IEnumerable<int> CompterJusquA3()
{
    yield return 1;
    yield return 2;
    yield return 3;
}

Et quand tu fais :

foreach (var nombre in CompterJusquA3())
{
    Console.WriteLine(nombre);
}

Tu obtiens :

1
2
3
Mais sous le capot :

Le foreach appelle la méthode pas entièrement d’un coup.

À chaque yield return, l’exécution suspend la méthode et revient plus tard pour continuer là où elle s’était arrêtée.

💡 C’est comme un générateur Python (yield fait la même chose en Python).

🧩 3. Avantage dans ton code

Dans WhiteSpaceTokenizer, la méthode :

public IEnumerable<string> Tokenize(string text)

ne crée pas une liste complète (genre List<string>).
Elle produit les mots un par un à mesure qu’ils sont trouvés.

Ainsi, le code qui l’utilise peut faire :
foreach (var token in tokenizer.Tokenize("This is a green wolf"))
{
    Console.WriteLine(token);
}
Et Tokenize() va être “pausée” et “reprise” à chaque mot trouvé :
→ trouve "This" → yield return "This"
→ continue, trouve "is" → yield return "is"
→ etc.
⚡️ Résultat : moins de mémoire utilisée, et pas besoin de tout stocker avant de parcourir.

Résumé rapide :

| Terme          | Explication                                                             |
| -------------- | ----------------------------------------------------------------------- |
| `yield return` | renvoie un élément de la séquence, sans quitter complètement la méthode |
| `yield break`  | arrête la séquence avant la fin                                         |
| Retourne       | un `IEnumerable<T>` (pas une liste, mais un générateur)                 |
| Avantage       | plus léger, plus rapide, surtout pour du texte long                     |*/

