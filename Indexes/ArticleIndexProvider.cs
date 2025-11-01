using System;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using YesSql.Indexes;
using YesSql.Samples.FullText.Models;
using YesSql.Samples.FullText.Tokenizers;

namespace YesSql.Samples.FullText.Indexes
{
    public class ArticleIndexProvider : IndexProvider<Article>
    {//👉 Cela signifie : « Cette classe définit des index pour les objets de type Article ».
        /*Quand YesSql sauvegarde un Article, il regarde tous les IndexProvider<Article> enregistrés et exécute
         * leur logique d’indexation.*/
        public override void Describe(DescribeContext<Article> context)
        {
            var tokenizer2 = new WhiteSpaceTokenizer2();//pour Program2.cs program par AndreySurkov
            //var tokenizer2 = new WhiteSpaceTokenizer2();
            var filter = new StopWordFilter();

            context
                .For<ArticleByWord, string>()
                .Map(article => filter//découper en mots (Map)
                    .Filter(tokenizer2.Tokenize2(article.Content))
                    .Select(x => new ArticleByWord { Word = x, Count = 1 })
                    )
                .Group(article => article.Word)//grouper par mot (Group)
                .Reduce(group => new ArticleByWord//compter (Reduce)
                {
                    Word = group.Key,
                    Count = group.Sum(y => y.Count)
                })
                .Delete((index, map) =>          
                    {
                        index.Count -= map.Sum(x => x.Count);
                        // if Count == 0 then delete the index
                        return index.Count > 0 ? index : null;
                    });
        }
    }
 }
/*“Je veux créer un index de type ArticleByWord.
                                                Cet index sera organisé selon une clé de type string (le mot).”*/
//➡️ ArticleByWord (et une table de liaison ArticleByWord_Document).*/

/*.Map(article => ...)

👉 La phase Map est exécutée pour chaque document Article.
Ici, on :

Tokenize (découpe) le contenu en mots avec WhiteSpaceTokenizer()

Filtre les mots inutiles avec StopWordFilter()

Et pour chaque mot, on crée un ArticleByWord :*/
//new ArticleByWord { Word = x, Count = 1 }
/*Donc pour :

"This is a green fox"
on aura : */

//| Word | Count |
//| ----- | ----- |
//| this  | 1 |
//| is    | 1 |
//| a     | 1 |
//| green | 1 |
//| fox   | 1 |*/
/* .Group(article => article.Word)

👉 On regroupe ensuite les résultats par mot.
C’est l’équivalent SQL de GROUP BY Word.*/

/*.Reduce(group => new ArticleByWord { ... })

👉 C’est la phase de réduction(comme dans MapReduce).
Elle permet de combiner les entrées ayant la même clé(Word) et de calculer des totaux:*/
/*.Delete((index, map)
 * 👉 Gère ce qui se passe quand un document Article est supprimé:

            On diminue le Count

Si le Count tombe à 0, on supprime la ligne de l’index*/

/*| Étape | Action |                                 Effet SQL |
| ------ | ---------------------------------------- | --------------------------- |
| Map   | Découpe chaque `Article.Content` en mots | Insert dans `ArticleByWord` |
| Group  | Regroupe par `Word`                      | GROUP BY Word               |
| Reduce | Compte le nombre d’occurrences           | SUM(Count)                  |
| Delete | Nettoie les index orphelins              | DELETE                      |*/

