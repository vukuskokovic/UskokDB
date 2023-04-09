using System;

namespace UskokDB
{
    public interface IParamterConverter
    {
        public object? Read(object value);

        public object? Write(object value);

        public Type GetTableType();

        public string? GetCustomTypeInTable();
        public int? GetCustomMaxLength();
    }

    public class DefaultParamterConverter<ParamterType, ColumnType> : IParamterConverter
    {
        private readonly Func<ParamterType, ColumnType> WriteFunc;
        private readonly Func<ColumnType, ParamterType> ReadFunc;
        public int? MaxLength { get; }
        public string? TypeName { get; }
        public DefaultParamterConverter(Func<ParamterType, ColumnType> writeFunc, Func<ColumnType, ParamterType> readFunc, int? maxLength = null, string? typeName = null)
        {
            WriteFunc = writeFunc;
            ReadFunc = readFunc;
            MaxLength = maxLength;
            MaxLength = maxLength;
            TypeName = typeName;
        }

        public object? Read(object value)
        {
            if(value is not ColumnType columnType) return null;

            return ReadFunc(columnType);
        }

        public object? Write(object value)
        {
            if(value is not ParamterType  paramterType) return null;

            return WriteFunc(paramterType);
        }

        public int? GetCustomMaxLength() => MaxLength;
        public string? GetCustomTypeInTable() => TypeName;
        public Type GetTableType() => typeof(ColumnType);
    }
}
