using System;

namespace UskokDB
{
    public interface IParameterConverter
    {
        public object? Read(object value);

        public object? Write(object value);

        public Type GetTableType();

        public string? GetCustomTypeInTable();
        public int? GetCustomMaxLength();
    }

    public class DefaultParameterConverter<TParameterType, TColumnType> : IParameterConverter
    {
        private readonly Func<TParameterType, TColumnType> _writeFunc;
        private readonly Func<TColumnType, TParameterType> _readFunc;
        public int? MaxLength { get; }
        public string? TypeName { get; }
        public DefaultParameterConverter(Func<TParameterType, TColumnType> writeFunc, Func<TColumnType, TParameterType> readFunc, int? maxLength = null, string? typeName = null)
        {
            _writeFunc = writeFunc;
            _readFunc = readFunc;
            MaxLength = maxLength;
            MaxLength = maxLength;
            TypeName = typeName;
        }

        public object? Read(object value)
        {
            if(value is not TColumnType columnType) return null;

            return _readFunc(columnType);
        }

        public object? Write(object value)
        {
            if(value is not TParameterType parameterType) return null;

            return _writeFunc(parameterType);
        }

        public int? GetCustomMaxLength() => MaxLength;
        public string? GetCustomTypeInTable() => TypeName;
        public Type GetTableType() => typeof(TColumnType);
    }
}
