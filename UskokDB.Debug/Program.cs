using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UskokDB;
using UskokDB.MySql;

var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
Console.WriteLine(connection.ExecuteScalar<int>("SELECT SUM(price) FROM scalar;"));
Console.WriteLine(connection.ExecuteScalar<bool>("SELECT EXISTs(SELECT * FROM scalar WHERE 0=1)"));
Console.ReadKey();