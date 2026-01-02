using System;
using System.Collections.Generic;

namespace UskokDB.Query;

internal interface IReadNode
{
    
}

internal sealed class QueryPropertyNode : IReadNode
{
    internal Type ColumnType { get; set; } = null!;
}

internal sealed class QueryReadNode : IReadNode
{
    internal Type NodeReadType { get; set; } = null!;
    internal int? IdentifierIndex { get; set; }
    internal List<IReadNode> Nodes { get; set; } = [];
}

