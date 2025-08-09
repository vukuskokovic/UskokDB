using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UskokDB.Attributes;

namespace UskokDB;

internal static class DbInitialization
{
    static DbInitialization()
    {
        if (!UskokDb.SqlDialectSet)
        {
            throw new UskokDbSqlDialectNotSet();
        }
    }
    
    private static readonly Dictionary<Type, Dictionary<SqlDialect, string>> TypeInDatabase = new()
    {
        [typeof(int)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "INT" },
            { SqlDialect.PostgreSql, "INTEGER" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(uint)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "INT UNSIGNED" },
            { SqlDialect.PostgreSql, "INTEGER" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(short)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "SMALLINT" },
            { SqlDialect.PostgreSql, "SMALLINT" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(ushort)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "SMALLINT UNSIGNED" },
            { SqlDialect.PostgreSql, "SMALLINT" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(long)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "BIGINT" },
            { SqlDialect.PostgreSql, "BIGINT" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(ulong)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "BIGINT UNSIGNED" },
            { SqlDialect.PostgreSql, "BIGINT" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(float)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "FLOAT" },
            { SqlDialect.PostgreSql, "REAL" },
            { SqlDialect.SqLite, "REAL" }
        },
        [typeof(double)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "DOUBLE" },
            { SqlDialect.PostgreSql, "DOUBLE PRECISION" },
            { SqlDialect.SqLite, "REAL" }
        },
        [typeof(decimal)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "DECIMAL" },
            { SqlDialect.PostgreSql, "NUMERIC" },
            { SqlDialect.SqLite, "NUMERIC" }
        },
        [typeof(byte)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "TINYINT" },
            { SqlDialect.PostgreSql, "SMALLINT" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(bool)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "TINYINT(1)" },
            { SqlDialect.PostgreSql, "BOOLEAN" },
            { SqlDialect.SqLite, "INTEGER" }
        },
        [typeof(char)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "CHAR(1)" },
            { SqlDialect.PostgreSql, "CHAR(1)" },
            { SqlDialect.SqLite, "TEXT" }
        },
        [typeof(DateTime)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "DATETIME" },
            { SqlDialect.PostgreSql, "TIMESTAMP" },
            { SqlDialect.SqLite, "TEXT" }
        },
        [typeof(DateTime?)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "DATETIME" },
            { SqlDialect.PostgreSql, "TIMESTAMP" },
            { SqlDialect.SqLite, "TEXT" }
        },
        [typeof(Guid)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "VARCHAR(36)" },
            { SqlDialect.PostgreSql, "VARCHAR(36)" },
            { SqlDialect.SqLite, "VARCHAR(36)" }
        },
        [typeof(Guid?)] = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.MySql, "VARCHAR(36)" },
            { SqlDialect.PostgreSql, "VARCHAR(36)" },
            { SqlDialect.SqLite, "VARCHAR(36)" }
        }
    };

    private static readonly Type MetadataType = typeof(TypeMetadata<>);
    private static readonly Type DbTableType = typeof(DbTable<>);
    private static readonly Type ForeignKeyAttrType = typeof(ForeignKeyAttribute<>);
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
        return DbTableType.MakeGenericType(tableType).GetProperty("TableName", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as string ?? tableType.Name; 
    }

    private static void AddTableString(DbContext context, StringBuilder builder, Type tableType)
    {
        List<string> primaryKeys = [];
        // t1: columnName, t2: tableName, t3: foreignColumnName
        List<Tuple<string, string, string>> foreignKeys = [];
        var tableName = GetTableName(tableType);
        var properties = (List<TypeMetadataProperty>)MetadataType.MakeGenericType(tableType).GetProperty("Properties")!.GetValue(null)!;
        builder.Append("CREATE TABLE IF NOT EXISTS ");
        builder.Append(tableName);
        builder.Append(' ');
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
            builder.Append(string.Join(",", primaryKeys));
            builder.Append(')');
        }
        if(foreignKeys.Count > 0)
        {
            builder.Append(", ");
            builder.Append(string.Join(",", foreignKeys.Select(foreignKey => $"FOREIGN KEY ({foreignKey.Item1}) REFERENCES {foreignKey.Item2}({foreignKey.Item3})")));
        }

        builder.Append(");\n");
    }

    private static void AddPropertyTableType(DbContext context, string tableName, StringBuilder builder, Type type, int? maxLength, out bool addNotNull, string? customTypeName = null)
    {
        addNotNull = true;
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            addNotNull = false;
            type = underlyingType;
        }

        if (type == typeof(string))
        {
            addNotNull = false;
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
            AddPropertyTableType(context, tableName, builder, type.GetEnumUnderlyingType(), maxLength, out addNotNull);
            return;
        }

        if (TypeInDatabase.TryGetValue(type, out var dbMappings) && dbMappings.TryGetValue(UskokDb.SqlDialect, out var dbTypeString))
        {
            builder.Append(dbTypeString);
            return;
        }

        if (DbIOOptions.ParameterConverters.TryGetValue(type, out var converter))
        {
            maxLength = converter.GetCustomMaxLength();
            customTypeName = converter.GetCustomTypeInTable();
            AddPropertyTableType(context, tableName, builder, converter.GetTableType(), maxLength, out addNotNull, customTypeName);
            return;
        }

        if (DbIO.ShouldJsonBeUsedForType(type))
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
        bool isAutoIncrement = false;
        bool isUnique = false;
        bool isNotNull = false;
        foreach(var attr in propertyInfo.GetCustomAttributes())
        {
            if (attr is MaxLengthAttribute max) maxLengthAttr = max;
            else if (attr is AutoIncrementAttribute) isAutoIncrement = true;
            else if (attr is KeyAttribute) isPrimaryKey = true;
            else if (attr is ColumnNotNullAttribute) isNotNull = true;
            else if (attr is UniqueAttribute) isUnique = true;
            else
            {
                var type = attr.GetType();   
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != ForeignKeyAttrType) continue;

                var foreignTableType = type.GenericTypeArguments[0];
                var foreignTableName = GetTableName(foreignTableType);
                if (type.GetProperty("ColumnName")?.GetValue(attr) is not string foreignColumnName) continue;
                foreignKey = new Tuple<string, string, string>(property.PropertyName, foreignTableName, foreignColumnName);
            }
        }

        builder.Append(property.PropertyName);
        builder.Append(' ');

        AddPropertyTableType(context, tableName, builder, property.Type, maxLengthAttr?.Length, out var addNotNull);
        if(isAutoIncrement)
        {
            switch (UskokDb.SqlDialect)
            {
                case SqlDialect.MySql:
                    builder.Append(" AUTO_INCREMENT");
                    break;
                case SqlDialect.PostgreSql:
                    builder.Append(" SERIAL");
                    break;
                case SqlDialect.SqLite when !isPrimaryKey:
                    throw new UskokDbException($"Auto increment cannot be used without being the primary key in SQLite, '{tableName}' column '{property.PropertyName}'");
                case SqlDialect.SqLite:
                    builder.Append(" AUTOINCREMENT");
                    break;
                default:
                    throw new UskokDbUnsupportedDbTypeException();
            }
        }
        else if(isUnique){
            builder.Append(" UNIQUE");
        }
        if(addNotNull || isNotNull)
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
