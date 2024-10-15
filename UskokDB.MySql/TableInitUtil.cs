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
    public static string TableInitString {get;}
    internal class TableItem : IComparable
    {
        public string InitQuery {get;}
        public List<Type> ForeignKeys {get;}
        public Type TableType {get;}
        public TableItem(string initQuery, List<Type> foreignKeys, Type tableType){
            InitQuery = initQuery;
            ForeignKeys = foreignKeys;
            TableType = tableType;
        }
        public int CompareTo(object? obj)
        {
            if(obj is not TableItem item)return 0;
            if(item.ForeignKeys.Contains(GetType()))return -1;
            if(ForeignKeys.Contains(item.TableType))return 1;
            if(item.ForeignKeys.Count > ForeignKeys.Count)return -1;
            if(item.ForeignKeys.Count < ForeignKeys.Count)return 1;
            return 0;
        }
    }

    static TableInitUtil()
    {
        BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        List<TableItem> tables = [];
        foreach(var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())){
            
            if (type.ContainsGenericParameters || !InheritsTypeFullSearch(type, typeof(DbTable<>), out var genericType)) continue;
            var value = type.GetProperty("MySqlTableInitString", flags)?.GetValue(null);
            if(value is not string tableInit)continue;
            var propertyListValue = typeof(TypeMetadata<>).MakeGenericType(genericType).GetProperty("Properties", flags)?.GetValue(null);
            if(propertyListValue is not List<TypeMetadataProperty> list)continue;
            List<Type> foreignKeys = [];
            foreach(var property in list){
                if(PropertyUtil.GetPropertyForeignKey(property) is not Type tableType)continue;
                foreignKeys.Add(tableType);
            }
            tables.Add(new TableItem(tableInit, foreignKeys, genericType));
            
        }
        tables.Sort();
        TableInitString = string.Join(";\n", tables.Select(x => x.InitQuery));
    }

    private static bool InheritsTypeFullSearch(Type? type, Type toSearch, out Type genericType)
    {
        genericType = null!;
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == toSearch)
            {
                genericType = type.GenericTypeArguments[0];
                return true;
            }
            type = type.BaseType;
        }

        return false;
    }
    public static int InitAllTables(IDbConnection connection) => connection.Execute(TableInitString);
    public static Task<int> InitAllTablesAsync(DbConnection connection) => connection.ExecuteAsync(TableInitString);
}