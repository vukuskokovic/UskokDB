using System;
using System.Collections.Generic;

namespace UskokDB.Query;

public class ImmutableTempItems<T>(IEnumerable<T> items) : TempItems<T>(items), IDisposable
    where T : class, new()
{
    private bool WasCompiled { get; set; } = false;
    private List<DbParam> ParamList { get; set; } = [];
    private string? CompiledText { get; set; } = "";
    
    public override string? PreQuery(List<DbParam> paramList)
    {
        if (WasCompiled)
        {
            paramList.AddRange(ParamList);
            return CompiledText;
        }

        CompiledText = base.PreQuery(ParamList);
        WasCompiled = true;
        paramList.AddRange(ParamList);
        return CompiledText;
    }

    public void Dispose()
    {
        ParamList.Clear();
        ParamList = null!;
    }
}