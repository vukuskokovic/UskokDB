using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UskokDB.Attributes;

namespace UskokDB;

internal static class DbInitilization
{

    private static readonly Dictionary<Type, Dictionary<DbType, string>> TypeInDatabase = new()
    {
        [typeof(int)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "INT" },
            { DbType.PostgreSQL, "INTEGER" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(uint)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "INT UNSIGNED" },
            { DbType.PostgreSQL, "INTEGER" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(short)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "SMALLINT" },
            { DbType.PostgreSQL, "SMALLINT" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(ushort)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "SMALLINT UNSIGNED" },
            { DbType.PostgreSQL, "SMALLINT" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(long)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "BIGINT" },
            { DbType.PostgreSQL, "BIGINT" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(ulong)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "BIGINT UNSIGNED" },
            { DbType.PostgreSQL, "BIGINT" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(float)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "FLOAT" },
            { DbType.PostgreSQL, "REAL" },
            { DbType.SQLite, "REAL" }
        },
        [typeof(double)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "DOUBLE" },
            { DbType.PostgreSQL, "DOUBLE PRECISION" },
            { DbType.SQLite, "REAL" }
        },
        [typeof(decimal)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "DECIMAL" },
            { DbType.PostgreSQL, "NUMERIC" },
            { DbType.SQLite, "NUMERIC" }
        },
        [typeof(byte)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "TINYINT" },
            { DbType.PostgreSQL, "SMALLINT" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(bool)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "TINYINT(1)" },
            { DbType.PostgreSQL, "BOOLEAN" },
            { DbType.SQLite, "INTEGER" }
        },
        [typeof(char)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "CHAR(1)" },
            { DbType.PostgreSQL, "CHAR(1)" },
            { DbType.SQLite, "TEXT" }
        },
        [typeof(DateTime)] = new Dictionary<DbType, string>
        {
            { DbType.MySQL, "DATETIME" },
            { DbType.PostgreSQL, "TIMESTAMP" },
            { DbType.SQLite, "TEXT" }
        }
    };

    private static readonly Type metadataType = typeof(TypeMetadata<>);
    private static readonly Type dbTableType = typeof(DbTable<>);
    private static readonly Type foreignKeyAttrType = typeof(ForeignKeyAttribute<>);
    private static Type? GetDbTableType(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DbTable<>))
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;
    }

    private static string GetTableName(Type tableType)
    {
        return dbTableType.MakeGenericType(tableType).GetProperty("TableName", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as string ?? tableType.Name; 
    }

    private static void AddTableString(DbContext context, StringBuilder builder, Type tableType)
    {
        List<string> primaryKeys = new();
        /// t1: columnName, t2: tableName, t3: foreignColumnName
        List<Tuple<string, string, string>> foreignKeys = new();
        var tableName = GetTableName(tableType);
        List<TypeMetadataProperty> properties = (List<TypeMetadataProperty>)metadataType.MakeGenericType(tableType).GetProperty("Properties", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        builder.Append("CREATE TABLE IF NOT EXISTS `");
        builder.Append(tableName);
        builder.Append("` ");
        builder.Append('(');
        for (var i = 0; i < properties.Count; i++)
        {
            var isLast = i == properties.Count - 1;

            AddPropertyString(context, tableName, properties[i], builder, out var isPrimaryKey, out var propertyForeignKey);
            if (isPrimaryKey)
                primaryKeys.Add(properties[i].PropertyName);
            
            if(propertyForeignKey != null)
                foreignKeys.Add(propertyForeignKey);

            if (!isLast)
                builder.Append(", ");
        }

        if(primaryKeys.Count > 0)
        {
            builder.Append(", PRIMARY KEY (");
            builder.Append(string.Join(',', primaryKeys));
            builder.Append(')');
        }
        if(foreignKeys.Count > 0)
        {
            builder.Append(", ");
            builder.Append(string.Join(",", foreignKeys.Select(fkey => $"FOREIGN KEY ({fkey.Item1}) REFERENCES {fkey.Item2}({fkey.Item3})")));
        }

        builder.Append(");\n");
    }

    private static void AddPropertyTableType(DbContext context, string tableName, StringBuilder builder, Type type, int? maxLength, string? customTypeName = null)
    {
        if (type == typeof(string))
        {
            if (customTypeName != null)
            {
                builder.Append(customTypeName);
                return;
            }

            if (maxLength.HasValue)
            {
                builder.Append("VARCHAR(");
                builder.Append(maxLength.Value);
                builder.Append(')');
                return;
            }

            builder.Append("TEXT");
            return;
        }
        if (type.IsEnum)
        {
            AddPropertyTableType(context, tableName, builder, type.GetEnumUnderlyingType(), maxLength);
            return;
        }

        if (TypeInDatabase.TryGetValue(type, out var dbMappings) && dbMappings.TryGetValue(context.DbType, out var dbTypeString))
        {
            builder.Append(dbTypeString);
            return;
        }

        if (context.DbIOOptions.ParameterConverters.TryGetValue(type, out var converter))
        {
            maxLength = converter.GetCustomMaxLength();
            customTypeName = converter.GetCustomTypeInTable();
            AddPropertyTableType(context, tableName, builder, converter.GetTableType(), maxLength, customTypeName);
            return;
        }

        if (context.DbIO.ShouldJsonBeUsedForType(type))
        {
            builder.Append("JSON");
            return;
        }

        throw new InvalidOperationException($"Could not get type in table ({tableName}) of type {type}");
    }

    private static void AddPropertyString(DbContext context, string tableName, TypeMetadataProperty property, StringBuilder builder, out bool isPrimaryKey, out Tuple<string, string, string>? foreignKey)
    {
        foreignKey = null;
        isPrimaryKey = false;
        var propertyInfo = property.PropertyInfo;
        MaxLengthAttribute? maxLengthAttr = null;
        AutoIncrementAttribute? autoIncrementAttr = null;
        ColumnNotNullAttribute? notNullAttr = null;
        foreach(var attr in propertyInfo.GetCustomAttributes())
        {
            if (attr is MaxLengthAttribute max) maxLengthAttr = max;
            else if (attr is AutoIncrementAttribute aI) autoIncrementAttr = aI;
            else if (attr is KeyAttribute) isPrimaryKey = true;
            else if (attr is ColumnNotNullAttribute nN) notNullAttr = nN;
            else
            {
                var type = attr.GetType();   
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != foreignKeyAttrType) continue;

                var foreignTableType = type.GenericTypeArguments[0];
                var foreignTableName = GetTableName(foreignTableType);
                if (type.GetProperty("ColumnName")?.GetValue(attr) is not string foreignColumnName) continue;
                foreignKey = new Tuple<string, string, string>(property.PropertyName, foreignTableName, foreignColumnName);
            }
        }

        builder.Append(property.PropertyName);
        builder.Append(' ');

        AddPropertyTableType(context, tableName, builder, property.Type, maxLengthAttr?.Length);
        if(autoIncrementAttr != null)
        {
            if(context.DbType == DbType.MySQL)
                builder.Append(" AUTO_INCREMENT");
            else if (context.DbType == DbType.PostgreSQL)
                builder.Append(" SERIAL");
            else if (context.DbType == DbType.SQLite)
            {
                if (!isPrimaryKey) throw new Exception($"Auto increment cannot be used without being the primary key in SQLite, '{tableName}' column '{property.PropertyName}'");

                builder.Append(" AUTOINCREMENT");
            }
        }
        if(notNullAttr != null)
            builder.Append(" NOT NULL");
    }

    internal static string CreateDbString(DbContext context)
    {
        StringBuilder builder = new();
        var properties = context.GetType().GetProperties();
        foreach (var property in properties)
        {
            var pType = property.PropertyType;
            var dbTableType = GetDbTableType(pType);
            if (dbTableType == null || pType.GetCustomAttribute<NotMappedAttribute>() is not null) continue;

            var tableType = dbTableType.GenericTypeArguments[0];
            AddTableString(context, builder, tableType);
        }

        return builder.ToString();
    }
}
