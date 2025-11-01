using Dapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Samples.FullText.Indexes;
using YesSql.Samples.FullText.Models;
using YesSql.Sql;
using YesSql.Services;
using static System.Collections.Specialized.BitVector32;
using static YesSql.Services.DefaultQuery;

namespace YesSql.Samples.FullText
{
    public class SearchRequest
    {
        public int Skip { get; set; }   // combien d’éléments sauter (pour la pagination)
        public int Take { get; set; }   // combien d’éléments prendre
        //public string Search { get; set; } // éventuellement un mot-clé
    }
    public class Program1
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Program1 Mon Projet travail");
            var filename = "yessql0.db";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var configuration = new Configuration()
                .UseSqLite($"Data Source={filename};Cache=Shared")
                ;

            var store = await StoreFactory.CreateAndInitializeAsync(configuration);
            //store.RegisterIndexes<ArticleIndexProvider>();
            //await store.InitializeAsync(); // 🟢 crée les tables d'index manquantes
            // creating article without any index
            await using (var session = store.CreateSession())
            {
                await session.SaveAsync(new Article { Nom = "Premier", Content = "This is a first fox" });
                await session.SaveChangesAsync();
            }

            // Recreate store to emulate late Index appending
            store.Dispose();
            store = await StoreFactory.CreateAndInitializeAsync(configuration);
            await using (var connection = store.Configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);
                /*SchemaBuilder est une classe utilitaire YesSql qui sert à créer des tables, colonnes, index, etc. 
                 * de façon déclarative(comme un mini ORM de schéma).*/
                var builder = new SchemaBuilder(store.Configuration, transaction);
                /*“Crée une table d’index associée à la classe ArticleByWord, avec deux colonnes :
                    Count(int) et Word(string).”*/
                // on crée la table d’index d’abord pour préparer le terrain.
                /*créer la table physique dans la base SQL
                    C’est la partie “schéma SQL”.*/
                /*Crée une vraie table SQL (ex. ArticleByWord) dans la base de données.
                    Déclare les colonnes (ici Word et Count).
                    Configure le stockage physique de l’index.*/
                /*À retenir : cette méthode ne dit rien sur comment remplir la table,
                 * elle dit juste comment elle est faite.*/
                await builder.CreateReduceIndexTableAsync<ArticleByWord>(table => table
                    .Column<int>("Count")
                    .Column<string>("Word")//voir Notes
                );
                await builder.CreateMapIndexTableAsync<ArticleByNom>(table => table
                    .Column<string>("Nom")
                    .Column<int>("NomIsNull"));
                //Validation
                await transaction.CommitAsync();
                var tables = await connection.QueryAsync<string>(
                    "SELECT name FROM sqlite_master WHERE type='table';"
                            );
                foreach (var t in tables)
                    Console.WriteLine("TABLE: " + t);

            }
            // register available indexes
            //fournisseur d’index (ArticleIndexProvider) qui explique comment indexer mes entités Article.*/
            store.RegisterIndexes<ArticleIndexProvider>();
            store.RegisterIndexes<ArticleByNomProvider>();
            await store.InitializeAsync(); // <-- crée toutes les tables d’index

            using (var session = store.CreateSession())
            {
                await session.SaveAsync(new Article { Nom = "Second", Content = "This is a! test, of A tokenizer++ behavior." });
                {
                    //Title = "Découverte de YesSql",

                    await session.SaveChangesAsync();
                }
                var simple1 = await session
                    .Query<Article>()
                    .ListAsync();
                foreach (var article in simple1)
                {
                    //Console.WriteLine("simple1 : " + article.Content);
                    Console.WriteLine("simple1 : " + article.Nom + "," + article.Content);
                }
            }
            // Find any documents 
            await using (var session = store.CreateSession())
            {
                var results = await session.QueryIndex<ArticleByWord>().ListAsync();
                foreach (var r in results)
                {
                    Console.WriteLine($"results= {r.Word} : {r.Count}");
                }
            }

            using (var session = store.CreateSession())
            {
                await session.SaveAsync(new Article { Nom = "", Content = "YesSql est un moteur de base de données orienté documents." });
                {
                    //Title = "Découverte de YesSql",

                    await session.SaveChangesAsync();
                }
                var simple1 = await session
                    .Query<Article>()
                    .ListAsync();
                foreach (var article in simple1)
                {
                    Console.WriteLine("simple1 : " + article.Nom + "," + article.Content);
                }
            }
            using (var session = store.CreateSession())
            {
                var results = await session.QueryIndex<ArticleByWord>().ListAsync();
                foreach (var r in results)
                {
                    Console.WriteLine($"{r.Word} : {r.Count}");
                }
            }

            using (var session = store.CreateSession())
            {
                await session.SaveAsync(new Article { Nom = "Quatrième", Content = "This is a fourth fox" });
                {
                    //Title = "Découverte de YesSql",

                    await session.SaveChangesAsync();
                }
                var pre_query = await session
                    .Query<Article>()
                    .ListAsync();
                foreach (var article in pre_query)
                {
                    Console.WriteLine("pre_query : " + article.Nom + "," + article.Content);
                }
                /*var query = await session
                    .Query<Article,ArticleByNom>().Where(x => x.Nom == "Second")
                    .ListAsync();*/
                var query = session.Query<Article, ArticleByNom>().Where(x => x.Nom == "Second");
                IEnumerable<Article> articles = await query.ListAsync();
                foreach (var article in articles)
                {
                    //Console.WriteLine("query : " + article.Nom);
                    Console.WriteLine($"query le dernier résultat est  : {article.Nom}, {article.Content}");
                }
                var req = new SearchRequest
                {
                    Skip = 0,   // on commence à 0 → première page
                    Take = 10,  // on veut 10 résultats
                    //Search = "Paris"
                };
                /*| Élément                                                                                     | Indice                                               | Conclusion                   |
                   | ------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ---------------------------- |
                    | Les méthodes `.OrderBy()`, `.ThenBy()`, `.Skip()`, `.Take()`, `.ListAsync()` sont utilisées | Ces méthodes appartiennent à `IQuery<T>` dans YesSql | `query2` est un `IQuery<T>`   |
                    | Le type final attendu est `IEnumerable<UserProfileIndex>`                                   | `ListAsync()` renvoie `IEnumerable<T>`               | Donc `T = UserProfileIndex`  |
                    | Le code utilise YesSql (`using YesSql;`, `session.QueryIndex<UserProfileIndex>()`)          | Ces méthodes viennent de YesSql                      | Donc `query` vient de YesSql |*/
                //IEnumerable<ArticleByNom> query2; 
                var query2 = session
                    .QueryIndex<ArticleByNom>();
                IEnumerable<ArticleByNom> searchResults = await query2
                    //.OrderBy(x => x.Nom == null)
                    //.OrderBy(nameof(ArticleByNom.Nom)) // tri par la colonne Nom
                    //.OrderBy(x => x.Nom)
                    
                    //.OrderBy(x => x.NomIsNull)   // d’abord les non-nulls
                    //.OrderBy(x => x.NomIsNull) // les 1 (non-null) d’abord
                    //.OrderBy(x => x.Nom)
                    
                    .OrderBy(x => x.NomIsNull)
                    .ThenByDescending(x => x.Nom) // les 1 (non-null) d’abord
                    /*if (Nom == null)
                        valeur = 1;
                            else
                        valeur = 0;*/


                    .Skip(req.Skip)
                    .Take(req.Take)
                    .ListAsync();
                foreach (var a in searchResults)
                {
                    Console.WriteLine($"Nom trié : {a.Nom}");
                }
            }
        }
    }
}
/*| Code                                    | Résultat              | Faut - il `await` ? |
| --------------------------------------- | ---------------------- | ----------------- |
| `var query = session.Query<Article>();` | `IQuery < Article >`      | NON |
| `var list = await query.ListAsync();`   | `IEnumerable < Article >` | OUI |*/

/*| Méthode                | Table interrogée                | Accès aux champs     | Filtrage/tri possible ? |
| ---------------------- | ------------------------------- | -------------------- | ----------------------- |
| `Query<TDocument>()`   | `Document`                      | JSON                 | Non (ou difficile)      |
| `QueryIndex<TIndex>()` | `TIndex` (MapIndex/ReduceIndex) | Colonnes SQL réelles | Oui, complet            |
*/

/*| Élément                | Rôle                                                                                 |
| ---------------------- | ------------------------------------------------------------------------------------ |
| `Article`              | Ton **document principal**, stocké dans la table `Document` (le JSON complet)        |
| `ArticleByNom`         | Ton **index SQL**, qui ne contient que la colonne `Nom` (pour filtrer rapidement)    |
| `ArticleByNomProvider` | Ton **fournisseur d’index** (le “plan de mapping” entre `Article` et `ArticleByNom`) |*/

/*
| Méthode |                        Ce qu’elle fait                             | Ce qu’elle ne fait pas  |
| ----------------------------- | ------------------------------------------- | ----------------------- |
| `CreateReduceIndexTableAsync` | Crée la table SQL pour l’index              | Ne remplit pas la table |
| `RegisterIndexes`             | Définit la logique de mise à jour des index | Ne crée pas la table    |*/

