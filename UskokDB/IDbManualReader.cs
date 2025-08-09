using System.Data.Common;

namespace UskokDB;

public interface IDbManualReader
{
    public void ReadValue(DbDataReader reader);
}