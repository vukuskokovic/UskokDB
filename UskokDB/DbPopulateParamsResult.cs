using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace UskokDB;

public class DbParam
{
    public string Name { get; set; } = null!;
    public object? Value { get; set; } = null!;
}

public class DbPopulateParamsResult
{
    public string CompiledText { get; set; } = null!;
    public List<DbParam> Params { get; set; } = [];

    public DbCommand CreateCommandWithConnection(DbConnection dbConnection)
    {
        var command = dbConnection.CreateCommand();
        command.CommandText = CompiledText;
        AddParamsToCommand(command);
        return command;
    }

    public void AddParamsToCommand(DbCommand command)
    {
        foreach (var param in Params)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = param.Name;
            dbParameter.Value = DbIO.WriteValue(param.Value);
            command.Parameters.Add(dbParameter);
        }
    }
}