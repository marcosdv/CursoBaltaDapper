using CursoBaltaDapper.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

                SelectLike(connection, "api");
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

        static void DeleteCategory(SqlConnection connection)
        {
            var deleteSQL = "DELETE FROM [Category] WHERE [Id] = @Id";
            var rows = connection.Execute(deleteSQL, new
            {
                Id = new Guid("09ce0b7b-cfca-497b-92c0-3290ad9d5142"),
            });
            Console.WriteLine($"{rows} registros excluidos");
        }

        static void CreateManyCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var category2 = new Category();
            category2.Id = Guid.NewGuid();
            category2.Title = "categoria Nova";
            category2.Url = "categoria-nova";
            category2.Description = "Categoria nova";
            category2.Order = 9;
            category2.Summary = "Categoria nova";
            category2.Featured = false;

            var insertSQL =
                @"INSERT INTO [Category] VALUES (
                    @Id, @Title, @Url, @Summary, @Order, @Description, @Featured
                )";

            var rows = connection.Execute(insertSQL, new[]{
                new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured,
                },
                new
                {
                    category2.Id,
                    category2.Title,
                    category2.Url,
                    category2.Summary,
                    category2.Order,
                    category2.Description,
                    category2.Featured,
                }
            });

            Console.WriteLine($"{rows} linhas inseridas");
        }

        static void ExecuteProcedure(SqlConnection connection)
        {
            var procedure = "[spDeleteStudent]";
            var pars = new { StudentId = "5db94713-7c21-3e20-8d1b-471000000000" };

            var affectedsRows = connection.Execute(procedure, pars, commandType: CommandType.StoredProcedure);

            Console.WriteLine($"{affectedsRows} linhas afetadas");
        }

        static void ExecuteReadProcedure(SqlConnection connection)
        {
            var procedure = "[spGetCoursesByCategory]";
            var pars = new { CategoryId = "5db94713-7c21-3e20-8d1b-471000000000" };

            var courses = connection.Query(procedure, pars, commandType: CommandType.StoredProcedure);

            foreach(var item in courses)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void ExecuteScalar(SqlConnection connection)
        {
            var category = new Category();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSQL = @"
                INSERT INTO [Category]
                OUTPUT inserted.[Id]
                VALUES (
                    NEWID(), @Title, @Url, @Summary, @Order, @Description, @Featured
                )";

            var id = connection.ExecuteScalar<Guid>(insertSQL, new
            {
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured,
            });

            Console.WriteLine($"A categoria inserida foi {id}");
        }

        static void ReadView(SqlConnection connection)
        {
            var sql = "SELECT * FROM [vwCourses]";
            var courses = connection.Query(sql);
            foreach (var item in courses)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }

        static void OneToOne(SqlConnection connection)
        {
            var sql = @"
                SELECT * FROM CareerItem
                INNER JOIN Course ON Course.Id = CareerItem.CourseId
            ";

            var items = connection.Query<CareerItem, Course, CareerItem>(
                sql,
                (careerItem, course) => {
                    careerItem.Course = course;
                    return careerItem;
                },
                splitOn: "Id"
            );

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - Curso: {item.Course.Title}");
            }
        }

        static void OneToMany(SqlConnection connection)
        {
            var sql = @"
                SELECT C.Id, C.Title, I.CareerId, I.Title
                FROM Career C
                INNER JOIN CareerItem I ON I.CareerId = C.Id
                ORDER BY C.Title
            ";

            var careers = new List<Career>();

            var items = connection.Query<Career, CareerItem, Career>(
                sql,
                (career, item) => {
                    var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
                    if (car == null)
                    {
                        car = career;
                        car.Items.Add(item);
                        careers.Add(car);
                    }
                    else
                    {
                        car.Items.Add(item);
                    }

                    return career;
                },
                splitOn: "CareerId"
            );

            foreach (var career in careers)
            {
                Console.WriteLine($"Carreira: {career.Title}");
                foreach (var item in career.Items)
                {
                    Console.WriteLine($"Curso: {item.Title}");
                }
            }
        }

        static void QueryMultiple(SqlConnection connection)
        {
            var query = "SELECT * FROM [Category]; SELECT * FROM [Course]";

            using (var multi = connection.QueryMultiple(query))
            {
                var categories = multi.Read<Category>();
                var courses = multi.Read<Course>();

                foreach(var item in categories)
                {
                    Console.WriteLine(item.Title);
                }

                foreach (var item in courses)
                {
                    Console.WriteLine(item.Title);
                }
            }
        }

        static void SelectIn(SqlConnection connection)
        {
            var query = "SELECT * FROM [Career] WHERE [Id] IN @Id";

            var careers = connection.Query<Career>(query, new { 
                Id = new[]
                {
                    "01ae8a85-b4e8-4194-a0f1-1c6190af54cb",
                    "e6730d1c-6870-4df3-ae68-438624e04c72"
                }
            });

            foreach (var item in careers)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void SelectLike(SqlConnection connection, string term)
        {
            var query = "SELECT * FROM [Course] WHERE [Title] LIKE @Busca";

            var courses = connection.Query<Course>(query, new
            {
                Busca = $"%{term}%"
            });

            foreach (var item in courses)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void Transactions(SqlConnection connection)
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

            using (var transaction = connection.BeginTransaction())
            {
                var rows = connection.Execute(insertSQL, new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured,
                }, transaction);

                transaction.Commit();
                //transaction.Rollback();

                Console.WriteLine($"{rows} linhas inseridas");
            }
        }
    }
}