using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB.Query;

public class QueryContext<T> where T : class, new()
{
    private DbContext DbContext { get; set; }
    private IQueryable Creator { get; set; }
    private List<JoinData> Joins { get; } = [];
    private HashSet<IQueryable> Queryables { get; } = [];
    private Tuple<Expression, Type, bool>? SelectData { get; set; }
    internal QueryContext(IQueryable creator, DbContext context)
    {
        Creator = creator;
        Queryables.Add(creator);
        DbContext = context;
    }

    private Expression? WhereExpression { get; set; } = null;
    private List<(Expression, bool)> OrderByExpressions { get; } = [];
    

    private QueryContext<T> AddJoin(JoinType type, Expression expression, IQueryable joinOn, List<IQueryable> queryables)
    {
        Joins.Add(new JoinData()
        {
            Expression = expression,
            JoinType = type,
            Queryables = queryables,
            JoinOn = joinOn
        });
        Queryables.Add(joinOn);
        foreach (var q in queryables)
        {
            Queryables.Add(q);
        }
        return this;
    }
    
    public QueryContext<T> Join<T0>(Queryable<T0> queryable, Expression<Func<T, T0, bool>> selector) => AddJoin(JoinType.Inner, selector.Body, queryable, []);
    public QueryContext<T> Join<T0, T1>(Queryable<T0> queryable, Expression<Func<T0, T1, bool>> selector) => AddJoin(JoinType.Inner, selector.Body, queryable, []);
    public QueryContext<T> LeftJoin<T0>(Queryable<T0> queryable, Expression<Func<T, T0, bool>> selector) => AddJoin(JoinType.Left, selector.Body, queryable, []);
    public QueryContext<T> LeftJoin<T0, T1>(Queryable<T0> queryable, Expression<Func<T0, T1, bool>> selector) => AddJoin(JoinType.Left, selector.Body, queryable, []);

    private QueryContext<T> SetWere(Expression exp)
    {
        WhereExpression = exp;
        return this;
    }
    public QueryContext<T> Where<T0>(Expression<Func<T0, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1>(Expression<Func<T0, T1, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2>(Expression<Func<T0, T1, T2, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4,bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, bool>> selector) => SetWere(selector.Body);

    public QueryContext<T> OrderBy<T0>(Expression<Func<T0, object>> expression)
    {
        OrderByExpressions.Add((expression.Body, false));
        return this;
    }
    
    public QueryContext<T> OrderByDescending<T0>(Expression<Func<T0, object>> expression)
    {
        OrderByExpressions.Add((expression.Body, true));
        return this;
    }


    private Task<object?> FinishSelect(Expression expression, Type readType, bool isList)
    {
        SelectData = new Tuple<Expression, Type, bool>(expression, readType, isList);
        return CompileAndRead();
    }

    public async Task<TRead?> SelectOneAsync<TRead, T0>(Expression<Func<T0, TRead>> selector) where TRead : class, new()
        => (TRead?)(await FinishSelect(selector.Body, typeof(TRead), false));
    
    public async Task<TRead?> SelectOneAsync<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector) where TRead : class, new()
        => (TRead?)(await FinishSelect(selector.Body, typeof(TRead), false));
    
    public async Task<List<TRead>> Select<TRead, T0>(Expression<Func<T0, TRead>> selector) where TRead : class, new()
        => (List<TRead>)(await FinishSelect(selector.Body, typeof(TRead), true))!;
    
    public async Task<List<TRead>> Select<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector) where TRead : class, new()
        => (List<TRead>)(await FinishSelect(selector.Body, typeof(TRead), true))!;


    private object ReadRow(DbDataReader reader, List<TypeMetadataProperty> properties)
    {
        return null;
    }
    private async Task<object?> CompileAndRead()
    {
        if (SelectData == null) throw new UskokDbException("SelectData is null but CompileAndRead is called?");
        await DbContext.OpenConnectionIfNotOpen();
        #if !NETSTANDARD2_0
        await
        #endif
        using var connection = Compile().CreateCommandWithConnection(DbContext.DbConnection);
        
        #if !NETSTANDARD2_0
        await
        #endif
        using var reader = await connection.ExecuteReaderAsync();
        if (!SelectData.Item3 && !reader.HasRows)
            return null;
        
        List<TypeMetadataProperty> metadataProperties = TypeMetadata.GetMetadataProperties(SelectData.Item2);
        
        List<object> rowList = [];
        
        
        //If is not multiple(!List<TRead>)
        if (!SelectData.Item3)
        {
            return ReadRow(reader, metadataProperties);
        }
        
        Console.WriteLine("Should read list");
        object? list;
        
        return null;
    }
    public DbPopulateParamsResult Compile()
    {
        var finalQuery  = new StringBuilder();
        List<DbParam> dbParams = [];
        foreach (var q in Queryables)
        {
            var compiledString = q.PreQuery(dbParams);
            if (compiledString == null) continue;

            finalQuery.Append(compiledString);
        }

        finalQuery.Append("SELECT ");
        finalQuery.Append(CompileSelect(dbParams));
        finalQuery.Append(" FROM ");
        finalQuery.Append(Creator.GetName());
        finalQuery.Append('\n');
        var joinIndex = 0;
        foreach (var join in Joins)
        {
            CompileJoin(finalQuery, dbParams, join, joinIndex);
            joinIndex++;
        }

        if (WhereExpression != null)
        {
            finalQuery.Append("WHERE ");
            int x = 0;
            finalQuery.AppendLine(AppendExpression(WhereExpression, "@where_", dbParams, ref x));
        }

        if (OrderByExpressions.Count > 0)
        {
            int paramIndex = 0;
            finalQuery.Append("ORDER BY ");
            for (int i = 0; i < OrderByExpressions.Count; i++)
            {
                if (i > 0) finalQuery.Append(", ");

                var (expr, desc) = OrderByExpressions[i];
                finalQuery.Append(AppendExpression(expr, "@o_", dbParams, ref paramIndex));
                finalQuery.Append(desc ? " DESC" : string.Empty);
            }
            finalQuery.AppendLine();
        }


        var compiledQueryText = finalQuery.ToString();
        Console.WriteLine(compiledQueryText);
        return new DbPopulateParamsResult()
        {
            CompiledText = compiledQueryText,
            Params = dbParams
        };
    }
    
    private string CompileSelect(List<DbParam> paramList)
    {
        if (SelectData == null) throw new UskokDbException("Select data is null");
        var (expression, readType, _) = SelectData;
        if (expression is not MemberInitExpression memberInitExpression)
            throw new Exception($"Select is not MemberInitExpression, Type: {expression.GetType().FullName}");
        
        
        StringBuilder builder = new StringBuilder();
        var properties = TypeMetadata.GetMetadataProperties(readType);
        int i = 0;
        int pIndex = 0;
        foreach (var propertyMetadata in properties)
        {
            foreach (var binding in memberInitExpression.Bindings)
            {
                if(binding is not MemberAssignment memberAssignment)throw new Exception("Binding in Select not MemberAssignment");
                
                if(propertyMetadata.PropertyInfo.Name != memberAssignment.Member.Name)continue;

                builder.Append(AppendExpression(memberAssignment.Expression, "@select_p_", paramList, ref pIndex));
            }
            
            i++;
            if (properties.Count != i) builder.Append(", ");
        }
        
        return builder.ToString();
    }

    private void CompileJoin(StringBuilder builder, List<DbParam> paramList, JoinData joinData, int joinIndex)
    {
        var name = joinData.JoinOn.GetName();
        builder.Append(joinData.JoinType == JoinType.Inner ? "JOIN " : "LEFT JOIN ");
        builder.Append(name);
        builder.Append(" ON ");
        if (joinData.Expression is not BinaryExpression binaryExpression)
        {
            throw new UskokDbException("First expression in join is on a binary expression i.e. x==3");
        }

        int propIndex = 0;

        builder.AppendLine(AppendExpression(binaryExpression, $"@j_p_{joinIndex}_", paramList, ref propIndex));
    }

    private string AddParam(List<DbParam> paramList, object value, string namePrefix, ref int propertyIndex)
    {
        var name = $"{namePrefix}{propertyIndex++}";
        paramList.Add(new DbParam()
        {
            Name = name,
            Value = value
        });

        return name;
    }

    private IQueryable? GetQueryableInQuery(Type type)
    {
        return Queryables.FirstOrDefault(q => q.GetUnderlyingType() == type);
    }
    
    private string AppendExpression(Expression expression, string namePrefix, List<DbParam> dbParams, ref int propertyIndex)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } u)
        {
            return AppendExpression(u.Operand, namePrefix, dbParams, ref propertyIndex);
        }
        if (expression is BinaryExpression binaryExpression)
        {
            string joiner = binaryExpression.NodeType switch
            {
                ExpressionType.Equal => " = ",
                ExpressionType.NotEqual => " != ",
                ExpressionType.GreaterThan => " > ",
                ExpressionType.GreaterThanOrEqual => " >= ",
                ExpressionType.LessThan => " < ",
                ExpressionType.LessThanOrEqual => " <= ",
                ExpressionType.Coalesce => " ?? ",
                ExpressionType.And or ExpressionType.AndAlso => " AND ",
                ExpressionType.Or or ExpressionType.OrElse => " OR ",
                _ => throw new UskokDbException(
                    $"Binary expression node type: {binaryExpression.NodeType} not supported")
            };
            
            var leftExpCompiled = AppendExpression(binaryExpression.Left, namePrefix, dbParams, ref propertyIndex);
            var rightExpCompiled = AppendExpression(binaryExpression.Right, namePrefix, dbParams, ref propertyIndex);
            
            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                return $"COALESCE({leftExpCompiled}, {rightExpCompiled})";
            }
            
            if(binaryExpression.NodeType is ExpressionType.NotEqual or ExpressionType.Equal && (leftExpCompiled == NullVale || rightExpCompiled == NullVale))
            {
                joiner = binaryExpression.NodeType == ExpressionType.Equal ? " IS " : " IS NOT ";
            }
            return $"({leftExpCompiled}{joiner}{rightExpCompiled})";
        }
        if(expression is ConstantExpression constantExpression)
        {
            if (constantExpression.Value == null) return NullVale;
            
            return AddParam(dbParams, constantExpression.Value, namePrefix, ref propertyIndex);
        }

        if (expression is MemberExpression memberExpression)
        {
            var holdingType = memberExpression.Member.DeclaringType;
            if (holdingType == null)
            {
                throw new UskokDbException("Member expression doesn't have a declaring type");
            }
            var queryable = GetQueryableInQuery(holdingType);

            if (queryable == null)
            {
                var value = Expression
                    .Lambda(
                        Expression.Convert(memberExpression, typeof(object))
                    )
                    .Compile()
                    .DynamicInvoke();

                if (value == null)
                {
                    return NullVale;
                }
                
                return AddParam(dbParams, value, namePrefix, ref propertyIndex);
            }


            var propertyMetadata = queryable.GetMetadataPropertyFromName(memberExpression.Member.Name);
            return $"{queryable.GetName()}.{propertyMetadata.PropertyName}";
        }

        if (expression is ParameterExpression parameterExpression)
        {
            if (parameterExpression.Type.IsGenericType &&
                parameterExpression.Type.GetGenericTypeDefinition() == typeof(Many<>))
            {
                var typeProperties =
                    TypeMetadata.GetMetadataProperties(parameterExpression.Type.GenericTypeArguments[0]);

                var x = typeProperties.Select(x => x.PropertyName);
                return string.Join(", ", x);
            }

            else
            {
                var typeProperties =
                    TypeMetadata.GetMetadataProperties(parameterExpression.Type);

                var queryable = GetQueryableInQuery(parameterExpression.Type);
                if (queryable == null)
                    throw new UskokDbException($"Queryable not found in query, type: {parameterExpression.Type.Name}");
                
                var x = typeProperties.Select(x => $"{queryable.GetName()}.{x.PropertyName}");
                return string.Join(", ", x);
            }
        }
        Console.WriteLine(expression.NodeType);

        return "<EMPTY>";
    }

    private const string NullVale = "NULL";
}

public interface IQueryable
{
    public string GetName();
    public Type GetUnderlyingType();
    public string? PreQuery(List<DbParam> paramList);

    public Type GetTypeMetadata();

    public TypeMetadataProperty GetMetadataPropertyFromName(string name);
}

public abstract class Queryable<T> : IQueryable
{
    private static Type? _metadataType;
    private static Dictionary<string, TypeMetadataProperty>? _nameToPropertyMap;
    public abstract string GetName();
    public abstract Type GetUnderlyingType();
    public abstract string? PreQuery(List<DbParam> paramList);
    
    public Type GetTypeMetadata()
    {
        _metadataType ??= typeof(TypeMetadata<>).MakeGenericType(typeof(T));
        return _metadataType;
    }

    private Dictionary<string, TypeMetadataProperty> PropertyToNameMap()
    {
        _nameToPropertyMap ??= (Dictionary<string, TypeMetadataProperty>)GetTypeMetadata().GetProperty("NameToPropertyMap")!.GetValue(null);
        return _nameToPropertyMap;
    }

    public TypeMetadataProperty GetMetadataPropertyFromName(string name)
    {
        return PropertyToNameMap()[name];
    }
}