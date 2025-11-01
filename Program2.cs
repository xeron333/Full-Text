using System;
using System.IO;
using System.Threading.Tasks;
using YesSql;
using YesSql.Samples.FullText.Indexes;
using YesSql.Samples.FullText.Models;
using YesSql.Sql;
using YesSql.Provider.Sqlite;

namespace YesSql.Samples.FullText
{
    //program par AndreySurkov
    public class Program2
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Program par AndreySurkov");
            var filename = "yessql0.db";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var configuration = new Configuration()
                .UseSqLite($"Data Source={filename};Cache=Shared")
                ;

            var store = await StoreFactory.CreateAndInitializeAsync(configuration);

            // creating article without any index
            await using (var session = store.CreateSession())
            {
                await session.SaveAsync(new Article { Content = "This is a green fox" });
                await session.SaveChangesAsync();
            }

            // Recreate store to emulate late Index appending
            store.Dispose();
            store = await StoreFactory.CreateAndInitializeAsync(configuration);
            await using (var connection = store.Configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);
                var builder = new SchemaBuilder(store.Configuration, transaction);

                await builder.CreateReduceIndexTableAsync<ArticleByWord>(table => table
                    .Column<int>("Count")
                    .Column<string>("Word")
                );

                await transaction.CommitAsync();
            }

            // register available indexes
            store.RegisterIndexes<ArticleIndexProvider>();

            // Update document
            await using (var session = store.CreateSession())
            {
                var someArticle = await session.Query<Article>().FirstOrDefaultAsync();
                someArticle.Content = "This is a green wolf";
                await session.SaveAsync(someArticle);
                await session.SaveChangesAsync();

            }

            // Find any documents 
            await using (var session = store.CreateSession())
            {
                Console.WriteLine("Simple term: 'green'");
                var simple = await session
                    .Query<Article, ArticleByWord>(x => x.Word == "green")
                    .ListAsync();

                foreach (var article in simple)
                {
                    Console.WriteLine(article.Content);
                }
            }
        }
    }
}