using YesSql.Indexes;
using YesSql.Samples.FullText.Models;

public class ArticleByNomProvider : IndexProvider<Article>
{
    public override void Describe(DescribeContext<Article> context)
    {
        context.For<ArticleByNom>()
            .Map(article => new ArticleByNom
            {
                Nom = article.Nom ,
                NomIsNull = string.IsNullOrEmpty(article.Nom) ? 1 : 0

                //NomIsNull = article.Nom == null ? 1 : 0
            });
    }
}
/* context.For<ArticleByNom>()
    .Map(article => new ArticleByNom { Nom = article.Nom });

tu dis à YesSql :

“Pour chaque document de type Article, crée une ligne dans la table d’index ArticleByNom
où la colonne Nom = la propriété Nom de l’article.”*/

/*En pratique :

1️/ Document d’origine (stocké comme JSON)
Id	Type	                                Content
1	YesSql.Samples.FullText.Models.Article	{"Nom":"Premier","Content":"Texte ..."}

Résultat de la projection .Map(...) :

| Id | DocumentId | Nom     |
| -- | ---------- | ------- |
| 1  | 1          | Premier |*/

/*Pourquoi c’est important

L’appel à .Map(...) fait la projection :

il “lit” ton document complet, et ne retient que les morceaux utiles pour les requêtes.

Ensuite, quand tu fais :

var results = await session.Query<Article, ArticleByNom>(x => x.Nom == "Premier").ListAsync();

➡️ YesSql exécute un SQL du genre :

SELECT Document.*
FROM Document
JOIN ArticleByNom ON ArticleByNom.DocumentId = Document.Id
WHERE ArticleByNom.Nom = 'Premier';

et te retourne les Article correspondants.

Résumé visuel :

| Étape                  | Rôle                                                         | Exemple                        |
| ---------------------- | ------------------------------------------------------------ | ------------------------------ |
| `.For<ArticleByNom>()` | On déclare le type d’index à remplir                         | → crée la table `ArticleByNom` |
| `.Map(...)`            | On définit comment construire chaque                         | → Nom = article.Nom            |
                           ligne d’index à partir d’un `Article` */                       



