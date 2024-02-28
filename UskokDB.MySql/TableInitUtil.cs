using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UskokDB.MySql;

public static class TableInitUtil
{
    private static bool InheritsTypeFullSearch(Type? type, Type toSearch)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == toSearch)
            {
                return true;
            }
            type = type.BaseType;
        }

        return false;
    }

    public static string CreateInitString(){
        List<string> tables = [];
        foreach(var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())){
            
            if (type.ContainsGenericParameters || !InheritsTypeFullSearch(type, typeof(MySqlTable<>))) continue;
            var value = type.GetProperty("MySqlTableInitString", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)?.GetValue(null);
            if(value is not string tableInit)continue;
            tables.Add(tableInit);
        }
        return string.Join(";\n", tables);
    }
    public static int InitAllTables(IDbConnection connection) => connection.Execute(CreateInitString());
    public static Task<int> InitAllTablesAsync(DbConnection connection) => connection.ExecuteAsync(CreateInitString());
}