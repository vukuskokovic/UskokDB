using MySqlConnector;

var x = () => new MySqlConnection("Uid=root;Server=127.0.0.1;Database=terminko;Allow User Variables=True");

await UskokDB.MySql.TableInitUtil.InitAllTablesAndDisposeAsync(x);