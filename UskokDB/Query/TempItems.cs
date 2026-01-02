using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UskokDB.Query;

public class TempItems<T> : Queryable<T> where T : class
{
    private List<T> Items { get; }
    
    public TempItems(IEnumerable<T> items)
    {
        if (items is List<T> list)
        {
            Items = list;
            return;
        }

        Items = items.ToList();
        if (Items.Count == 0) throw new UskokDbException("Temp items is empty");
    }
    
    
    // ReSharper disable once StaticMemberInGenericType
    private static string? _name;
    private string Name
    {
        get
        {
            if (_name != null) return _name;

            _name = $"temp_items_{typeof(T).Name.ToLower()}";

            return _name;
        }
    }

    public override string GetName() => Name;

    public override Type GetUnderlyingType() => typeof(T);


    private string CompilePreQuery(List<DbParam> paramList)
    {
        StringBuilder builder = new StringBuilder("WITH ");
        builder.Append(Name);
        builder.AppendLine(" AS (");
        int propertiesCount = TypeMetadata<T>.Properties.Count;
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            for (int propertyIndex = 0; propertyIndex < propertiesCount; propertyIndex++)
            {
                var propertyMetadata = TypeMetadata<T>.Properties[propertyIndex];
                var propertyName = $"@{Name}_{propertyMetadata.PropertyName}_{i}";
                if (propertyIndex == 0) builder.Append("\tSELECT ");
                else builder.Append("\t       ");

                builder.Append(propertyName);
                if (i == 0)
                {
                    builder.Append(" AS ");
                    builder.Append(propertyMetadata.PropertyName);
                }
                if (propertyIndex + 1 != propertiesCount) builder.Append(',');

                builder.Append('\n');
                paramList.Add(new DbParam()
                {
                    Name = propertyName,
                    Value = propertyMetadata.GetMethod(item)
                });
            }
            
            if(i+1 < Items.Count) builder.AppendLine("\tUNION ALL");
        }
        
        builder.AppendLine(")");
        return builder.ToString();
    }
    public override string? PreQuery(List<DbParam> paramList)
    {
        return CompilePreQuery(paramList);
    }
}