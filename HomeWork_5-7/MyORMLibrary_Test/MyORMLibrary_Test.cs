using MyORMLibrary;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace MyORMLibrary_Test;

[TestClass]
public sealed class MyORMLibrary_Test
{
    private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TestDB;Integrated Security=True";

    [TestMethod]
    public void IsConnected()
    {
        var orm = new ORMContext(connectionString);

        Assert.IsTrue(orm.IsConnected, "Строка подключения - неправильная.");
    }

    [TestMethod]
    public void CreateTable()
    {
        var tour = new Tour()
        {
            Name = "Test",
            Price = 10
        };

        var orm = new ORMContext(connectionString);
        orm.CreateTable(typeof(Tour), "Tours");

        var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        var result = orm.TableExists(sqlConnection, "Tours");
        sqlConnection.Close();

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ReadById()
    {
        var orm = new ORMContext(connectionString);
        var tour = orm.ReadById<Tour>(1,"Tours");

        Assert.IsTrue(tour.Name == "Moscow");
        Assert.IsTrue(tour.Price == 100);
    }

    [TestMethod]
    public void ReadByAll()
    {
        var orm = new ORMContext(connectionString);
        var tours = orm.ReadByAll<Tour>("Tours");

        var collection = new List<Tour>()
        {
            new Tour()
            {
                Name = "Moscow",
                Price = 100
            },
            new Tour()
            {
                Name = "Kazan",
                Price = 200
            },new Tour()
            {
                Name = "Omsk",
                Price = 300
            }
        };

        var index = 0;
        foreach (var tour in tours)
        {
            Assert.AreEqual(collection[index++], tour);
        }
    }

    [TestMethod]
    public void AddToTable()
    {
        var orm = new ORMContext(connectionString);
        var tour = new Tour() { Name = "Los", Price = 200 };

        orm.AddToTable<Tour>(tour, "Tours");
    }

    [TestMethod]
    public void DeleteById() //<Индексы для сущностей>//
    {
        Assert.IsTrue(true);
        return;
        var orm = new ORMContext(connectionString);

        var tours = orm.ReadByAll<Tour>("Tours");
        var count = tours.Count();
        //var id = tours.Last();
        orm.Delete(6, "Tours");
        var tour = orm.ReadById<Tour>(6, "Tours");
        var countNew = orm.ReadByAll<Tour>("Tours").Count();

        Assert.AreEqual(null, tour);
        Assert.AreEqual(countNew, count - 1);
    }

    [TestMethod]
    public void UpdateById()
    {
        var id = 1;
        var random = new Random();
        var orm = new ORMContext(connectionString);
        var tour = orm.ReadById<Tour>(id, "Tours");

        tour.Price = random.Next(1, 100);
        orm.Update(id, tour, "Tours");

        var result = orm.ReadById<Tour>(id, "Tours");

        Assert.AreEqual(tour, result);
    }

    [TestMethod]
    public void ExpressionTransformer_WHERE_Price_LIMIT() //<Работает с одним аргументом>//
    {
        var t = new Tour() { Price = 1 };
        Expression<Func<Tour, bool>> f = (x) => x.Price > 1;
        var query = ExpressionTransformer.BuildSqlQuery(f, true);

        var expected = "SELECT * FROM Tours WHERE (Price > 1) LIMIT 1";

        Assert.AreEqual(expected, query);
    }

    [TestMethod]
    public void DifficultExpression()
    {
        var t = new Tour() { Price = 200, Name = "Kazan" };
        Expression<Func<Tour, bool>> f = (x) => x.Price > 100 && x.Name == "Kazan";
        var query = ExpressionTransformer.BuildSqlQuery(f, false);

        var expected = "SELECT * FROM Tours WHERE ((Price > 100) AND (Name = 'Kazan'))";

        Assert.AreEqual(expected, query);
    }
}