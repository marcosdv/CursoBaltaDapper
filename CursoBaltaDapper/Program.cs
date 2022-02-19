using CursoBaltaDapper.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System;

namespace CursoBaltaDapper
{
    class Program
    {
        static void Main(string[] args)
        {
            const string connection_string = "Server=MDV-NOTE;Database=balta;Integrated Security=true;Trusted_Connection=true;";

            

            using (var connection = new SqlConnection(connection_string))
            {
                ListCategories(connection);

                CreateCategory(connection);
                UpdateCategory(connection);

                ListCategories(connection);
            }
        }

        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>("SELECT [Id], [Title] FROM Category");
            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }

        static void CreateCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSQL =
                @"INSERT INTO [Category] VALUES (
                    @Id, @Title, @Url, @Summary, @Order, @Description, @Featured
                )";

            var rows = connection.Execute(insertSQL, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured,
            });

            Console.WriteLine($"{rows} linhas inseridas");
        }

        static void UpdateCategory(SqlConnection connection)
        {
            var updateSQL = "UPDATE [Category] SET [Title} = @Title WHERE [Id] = @Id";
            var rows = connection.Execute(updateSQL, new
            {
                Id = new Guid("09ce0b7b-cfca-497b-92c0-3290ad9d5142"),
                Title = "Backend 2022"
            });
            Console.WriteLine($"{rows} registros atualizados");
        }
    }
}
