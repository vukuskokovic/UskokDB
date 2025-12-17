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

    private class ForeignKeyData
    {
        public string ColumnName = null!;
        public string TableName = null!;
        public string ForeignColumnName = null!;
        public ForeignKeyAction OnDelete = ForeignKeyAction.DbDefault;
        public ForeignKeyAction OnUpdate = ForeignKeyAction.DbDefault;
    }

    private class IndexData
    {
        public bool IsUnique = false;
        public string Name = null!;
        public string ColumnName = null!;
    }
    /// <summary>
    /// The main function for the table creation
    /// </summary>
    /// <param name="builder">The builder to append to</param>
    /// <param name="tableType">The type of the table</param>
    private static void AddTableString(StringBuilder builder, Type tableType)
    {
        List<IndexData> indexes = [];
        List<string> primaryKeys = [];
        List<ForeignKeyData> foreignKeys = [];
        var tableName = GetTableName(tableType);
        var properties = (List<TypeMetadataProperty>)MetadataType.MakeGenericType(tableType).GetProperty("Properties")!.GetValue(null)!;
        builder.Append("CREATE TABLE IF NOT EXISTS ");
        builder.Append(tableName);
        builder.Append(' ');
        builder.Append('(');
        for (var i = 0; i < properties.Count; i++)
        {
            var isLast = i == properties.Count - 1;

            AddPropertyString(tableName, properties[i], builder, out var isPrimaryKey, out var propertyForeignKey, out var indexData);
            if (isPrimaryKey)
                primaryKeys.Add(properties[i].PropertyName);
            
            if(propertyForeignKey != null)
                foreignKeys.Add(propertyForeignKey);

            if (!isLast)
                builder.Append(", ");
            
            if(indexData != null)
                indexes.Add(indexData);
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
            int l = foreignKeys.Count;
            for (int i = 0; i < l; i++)
            {
                AddForeignKeyString(builder, foreignKeys[i]);
                if (i + 1 != l)
                {
                    builder.Append(", ");
                }
            }
        }

        builder.Append(");\n");
        if (indexes.Count > 0)
        {
            foreach (var indexData in indexes)
            {
                AddIndexString(builder, indexData.IsUnique, indexData.Name, tableName, [indexData.ColumnName]);
            }
        }

        /*foreach (var compositeIndexAttribute in tableType.GetCustomAttributes<CompositeIndexAttribute>())
        {
            AddIndexString(builder, compositeIndexAttribute.IsUnique,
                $"CIX_{string.Join("_", compositeIndexAttribute.Columns)}", tableName,
                compositeIndexAttribute.Columns);
        }*/
    }

    private static void AddIndexString(StringBuilder builder, bool isUnique, string name, string tableName, string[] columns)
    {
        builder.Append("CREATE ");
        if (isUnique)
            builder.Append("UNIQUE ");

        builder.Append("INDEX IF NOT EXISTS ");
        builder.Append(name);
        builder.Append(" ON ");
        builder.Append(tableName);
        builder.Append('(');
        builder.Append(string.Join(",", columns));
        builder.Append(");\n");
    }

    /// <summary>
    /// Adds the foreign key constraint to the column
    /// </summary>
    /// <param name="builder">The table StringBuilder</param>
    /// <param name="fkData">Data for the foreignkey</param>
    private static void AddForeignKeyString(StringBuilder builder, ForeignKeyData fkData)
    {
        builder.Append("FOREIGN KEY (");
        builder.Append(fkData.ColumnName);
        builder.Append(") REFERENCES ");
        builder.Append(fkData.TableName);
        builder.Append('(');
        builder.Append(fkData.ForeignColumnName);
        builder.Append(')');
        if (fkData.OnDelete != ForeignKeyAction.DbDefault)
        {
            builder.Append(" ON DELETE ");
            AddForeignKeyAction(builder, fkData.OnDelete);
        }

        if (fkData.OnUpdate != ForeignKeyAction.DbDefault)
        {
            builder.Append(" ON UPDATE ");
            AddForeignKeyAction(builder, fkData.OnUpdate);
        }
    }

    /// <summary>
    /// Adds the action for the foreignkey(CASCADE, UPDATE, SETNULL...)
    /// </summary>
    /// <param name="builder">The table StringBuilder</param>
    /// <param name="action">Action</param>
    private static void AddForeignKeyAction(StringBuilder builder, ForeignKeyAction action)
    {
        builder.Append(action switch
        {
            ForeignKeyAction.Cascade => "CASCADE",
            ForeignKeyAction.NoAction =>  "NO ACTION",
            ForeignKeyAction.SetNull => "SET NULL",
            ForeignKeyAction.SetDefault => "SET DEFAULT",
            ForeignKeyAction.Restrict =>  "RESTRICT",
            // Again will never happen but this is my error, so I should follow the string below
            ForeignKeyAction.DbDefault => "How did this even happen",
            // Will never happen unless someone does stupid shit like (ForeignKeyAction)10
            // In which case they should follow the string below
            _ => "Kill yourself"
        });
    }

    /// <summary>
    /// Adding the column/property to the table string
    /// </summary>
    /// <param name="tableName">Name of the table used for the exception</param>
    /// <param name="property">Property metadata</param>
    /// <param name="builder">The table string builder</param>
    /// <param name="isPrimaryKey">Out is primary key</param>
    /// <param name="foreignKey">ForeignKey data</param>
    /// <param name="indexData">Index Data</param>
    /// <exception cref="UskokDbException"></exception>
    /// <exception cref="UskokDbUnsupportedDbTypeException"></exception>
    private static void AddPropertyString(string tableName, TypeMetadataProperty property, StringBuilder builder, out bool isPrimaryKey, out ForeignKeyData? foreignKey, out IndexData? indexData)
    {
        indexData = null;
        foreignKey = null;
        isPrimaryKey = false;
        var propertyInfo = property.PropertyInfo;
        MaxLengthAttribute? maxLengthAttr = null;
        bool isAutoIncrement = false;
        bool isUnique = false;
        bool isNotNull = false;
        string? typeOverride = null;
        foreach(var attr in propertyInfo.GetCustomAttributes())
        {
            if (attr is MaxLengthAttribute max) maxLengthAttr = max;
            else if (attr is AutoIncrementAttribute) isAutoIncrement = true;
            else if (attr is KeyAttribute) isPrimaryKey = true;
            else if (attr is ColumnNotNullAttribute) isNotNull = true;
            else if (attr is UniqueAttribute) isUnique = true;
            else if(attr is OverrideDbTypeAttribute overrideDbType) typeOverride = overrideDbType.DbType;
            else if (attr is DbEnumAttribute dbEnum)
            {
                if (!UskokDb.SqlDialectSet || UskokDb.SqlDialect != SqlDialect.MySql) throw new UskokDbException("Enum is only supported in mysql");
                typeOverride = $"ENUM ({string.Join(",", dbEnum.Values.Select(x => $"'{x}'"))})";
            }
            /*else if (attr is IndexAttribute indexAttr)
            {
                indexData = new IndexData()
                {
                    ColumnName = property.PropertyName,
                    IsUnique = indexAttr.IsUnique,
                    Name = $"IX_{property.PropertyName}_{tableName}"
                };
            }*/
            else
            {
                var type = attr.GetType();   
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != ForeignKeyAttrType) continue;

                var foreignTableType = type.GenericTypeArguments[0];
                var foreignTableName = GetTableName(foreignTableType);
                if (type.GetProperty("ColumnName")?.GetValue(attr) is not string foreignColumnName) continue;
                var onDelete = (ForeignKeyAction)type.GetProperty("OnDelete")!.GetValue(attr)!;
                var onUpdate = (ForeignKeyAction)type.GetProperty("OnUpdate")!.GetValue(attr)!;
                
                
                foreignKey = new ForeignKeyData()
                {
                    ColumnName = property.PropertyName,
                    TableName = foreignTableName,
                    ForeignColumnName = foreignColumnName,
                    OnDelete = onDelete,
                    OnUpdate = onUpdate,
                };
            }
        }

        builder.Append(property.PropertyName);
        builder.Append(' ');

        AddPropertyTableType(tableName, builder, property.Type, maxLengthAttr?.Length, out var addNotNull, typeOverride);
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
    
    /// <summary>
    /// Adding the type for the column/property (VARCHAR, TEXT, INT)
    /// </summary>
    /// <param name="tableName">The name of the table(used for the exception)</param>
    /// <param name="builder">The builder of the table</param>
    /// <param name="type">Type of the property/column</param>
    /// <param name="maxLength">Max length used for VARCHAR</param>
    /// <param name="addNotNull">Should NOT NULL be added</param>
    /// <param name="customTypeName">Override the column type</param>
    /// <exception cref="InvalidOperationException">When unable to get the property type</exception>
    private static void AddPropertyTableType(string tableName, StringBuilder builder, Type type, int? maxLength, out bool addNotNull, string? customTypeName = null)
    {
        addNotNull = true;
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            addNotNull = false;
            type = underlyingType;
        }

        if (customTypeName != null)
        {
            builder.Append(customTypeName);
            return;
        }
        
        if (type == typeof(string))
        {
            addNotNull = false;
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
            AddPropertyTableType(tableName, builder, type.GetEnumUnderlyingType(), maxLength, out addNotNull);
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
            AddPropertyTableType(tableName, builder, converter.GetTableType(), maxLength, out addNotNull, customTypeName);
            return;
        }

        if (DbIO.ShouldJsonBeUsedForType(type))
        {
            builder.Append("JSON");
            return;
        }

        throw new InvalidOperationException($"Could not get type in table ({tableName}) of type {type}");
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
            AddTableString(builder, tableType);
        }

        return builder.ToString();
    }
}
