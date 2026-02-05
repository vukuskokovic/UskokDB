using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB.Query;

public class QueryContext<T> : IJoinable<T>, IQueryContext, ISelectable, IOrderable<T>, IGroupable<T>, IQueryable<T>, IInstantQueryable<T>, ILimitable<T> where T : class, new()
{
    private DbContext DbContext { get; set; }
    private IQueryItem Creator { get; set; }
    private List<JoinData> Joins { get; } = [];
    private HashSet<IQueryItem> QueryItems { get; } = [];
    private Tuple<Expression, Type>? SelectData { get; set; }
    private string? OverrideSelect { get; set; }
    private int? LimitValue { get; set; } = null;
    internal QueryContext(IQueryItem creator, DbContext context)
    {
        Creator = creator;
        QueryItems.Add(creator);
        DbContext = context;
    }

    private Expression? WhereExpression { get; set; } = null;
    private List<(Expression, bool)> OrderByExpressions { get; } = [];
    private List<Expression> GroupByExpressions { get; } = [];
    

    private QueryContext<T> AddJoin(JoinType type, Expression expression, IQueryItem joinOn)
    {
        Joins.Add(new JoinData()
        {
            Expression = expression,
            JoinType = type,
            JoinOn = joinOn
        });
        QueryItems.Add(joinOn);
        return this;
    }

    #region Joins
    public QueryContext<T> Join<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0: class, new() => AddJoin(JoinType.Inner, selector.Body, queryItem);
    public QueryContext<T> Join<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector) where T0: class, new() => AddJoin(JoinType.Inner, selector.Body, queryItem);
    public QueryContext<T> LeftJoin<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0: class, new() => AddJoin(JoinType.Left, selector.Body, queryItem);
    public QueryContext<T> LeftJoin<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector)  where T0: class, new() => AddJoin(JoinType.Left, selector.Body, queryItem);
    #endregion
    #region Where
    private QueryContext<T> SetWere(Expression exp)
    {
        WhereExpression = exp;
        return this;
    }
    public QueryContext<T> Where(Expression<Func<T, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0>(Expression<Func<T0, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1>(Expression<Func<T0, T1, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2>(Expression<Func<T0, T1, T2, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4,bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, bool>> selector) => SetWere(selector.Body);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, bool>> selector) => SetWere(selector.Body);
    #endregion
    #region OrderBy/GroupBy

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
    
    public QueryContext<T> OrderBy(Expression<Func<T, object>> expression)
    {
        OrderByExpressions.Add((expression.Body, false));
        return this;
    }
    
    public QueryContext<T> OrderByDescending(Expression<Func<T, object>> expression)
    {
        OrderByExpressions.Add((expression.Body, true));
        return this;
    }
    
    public QueryContext<T> GroupBy<T0>(Expression<Func<T0, object>> expression)
    {
        GroupByExpressions.Add(expression.Body);
        return this;
    }
    
    public QueryContext<T> GroupBy(Expression<Func<T, object>> expression)
    {
        GroupByExpressions.Add(expression.Body);
        return this;
    }

    #endregion
    #region  Select

    private Task<List<TRead>> FinishSelect<TRead>(Expression expression, bool printToConsole = false) where TRead : class, new ()
    {
        SelectData = new Tuple<Expression, Type>(expression, typeof(TRead));
        return DbContext.QueryAsync<TRead>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection));
    }
    
    private Task<TRead?> FinishSelectOne<TRead>(Expression expression, bool printToConsole = false) where TRead : class, new ()
    {
        SelectData = new Tuple<Expression, Type>(expression, typeof(TRead));
        return DbContext.QuerySingleAsync<TRead>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection));
    }

    public Task<TRead?> SelectOneAsync<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    
    public Task<TRead?> SelectOneAsync<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelectOne<TRead>(selector.Body, printToConsole);
    //=====================================================================
    public Task<List<TRead>> Select<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    
    public Task<List<TRead>> Select<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false) where TRead : class, new()
        => FinishSelect<TRead>(selector.Body, printToConsole);
    public Task<bool> Exists(bool printToConsole = false)
    {
        OverrideSelect = "1";
        Limit(1);
        return DbContext.ExistsAsync(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection));
    }

    public Task<int> Count(bool printToConsole = false)
    {
        OverrideSelect = "COUNT(*)";
        return DbContext.ExecuteScalar<int>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection));
    }

    #endregion
    #region Instant queries
    private string BuildInstantQuerySelect() => $"{Creator.GetName()}.*";
    public Task<T?> QuerySingleAsync(CancellationToken cancellationToken = default, bool printToConsole = false)
    {
        OverrideSelect = BuildInstantQuerySelect();
        return DbContext.QuerySingleAsync<T>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection), cancellationToken);
    }

    public Task<T?> QuerySingleWhereAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default,
        bool printToConsole = false)
    {
        OverrideSelect = BuildInstantQuerySelect();
        SetWere(where.Body);
        return DbContext.QuerySingleAsync<T>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection), cancellationToken);
    }

    public Task<List<T>> QueryAsync(CancellationToken cancellationToken = default, bool printToConsole = false)
    {
        OverrideSelect = BuildInstantQuerySelect();
        return DbContext.QueryAsync<T>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection), cancellationToken);
    }

    public Task<List<T>> QueryWhereAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default, bool printToConsole = false)
    {
        OverrideSelect = BuildInstantQuerySelect();
        SetWere(where.Body);
        return DbContext.QueryAsync<T>(this.Compile(printToConsole).CreateCommandWithConnection(this.DbContext.DbConnection), cancellationToken);
    }

    #endregion

    public QueryContext<T> Limit(int limit)
    {
        LimitValue = limit;
        return this;
    }

    public DbPopulateParamsResult Compile(bool printToConsole = false)
    {
        var finalQuery  = new StringBuilder();
        List<DbParam> dbParams = [];
        foreach (var q in QueryItems)
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
            finalQuery.AppendLine(AppendExpression(WhereExpression, "@where_", dbParams, ref x, out _));
        }

        if (GroupByExpressions.Count > 0)
        {
            int paramIndex = 0;
            finalQuery.Append("GROUP BY ");
            for (int i = 0; i < GroupByExpressions.Count; i++)
            {
                if (i > 0) finalQuery.Append(", ");
                finalQuery.Append(AppendExpression(GroupByExpressions[i], "@group_", dbParams, ref paramIndex, out _));
            }
            finalQuery.AppendLine();
        }
        
        if (OrderByExpressions.Count > 0)
        {
            int paramIndex = 0;
            finalQuery.Append("ORDER BY ");
            for (int i = 0; i < OrderByExpressions.Count; i++)
            {
                if (i > 0) finalQuery.Append(", ");

                var (expr, desc) = OrderByExpressions[i];
                finalQuery.Append(AppendExpression(expr, "@o_", dbParams, ref paramIndex, out _));
                finalQuery.Append(desc ? " DESC" : string.Empty);
            }
            finalQuery.AppendLine();
        }

        if (LimitValue != null)
        {
            var x = 0;
            var paramName = AddParam(dbParams, LimitValue.Value, "@limit_", ref x);
            finalQuery.AppendLine($"LIMIT {paramName}");
        }


        var compiledQueryText = finalQuery.ToString();
        if (printToConsole)
        {
            Console.WriteLine(compiledQueryText);
        }
        return new DbPopulateParamsResult()
        {
            CompiledText = compiledQueryText,
            Params = dbParams
        };
    }
    
    private string CompileSelect(List<DbParam> paramList)
    {
        if (OverrideSelect != null)
        {
            return OverrideSelect;
        }
        if (SelectData == null)
        {
            throw new UskokDbException("SelectData is null, in other terms you queries but no select was provided");
        }
        var (expression, readType) = SelectData;
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

                builder.Append(AppendExpression(memberAssignment.Expression, "@select_p_", paramList, ref pIndex, out _));
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

        builder.AppendLine(AppendExpression(binaryExpression, $"@j_p_{joinIndex}_", paramList, ref propIndex, out _));
    }

    private readonly Dictionary<object, string> _alreadyExistingParams = [];
    public string AddParam(List<DbParam> paramList, object value, string namePrefix, ref int propertyIndex)
    {
        if (_alreadyExistingParams.TryGetValue(value, out var existingParamName))
        {
            return existingParamName;
        }
        
        var name = $"{namePrefix}{propertyIndex++}";
        paramList.Add(new DbParam()
        {
            Name = name,
            Value = value
        });

        _alreadyExistingParams[value] = name;

        return name;
    }

    private IQueryItem? GetQueryableInQuery(Type type)
    {
        return QueryItems.FirstOrDefault(q => q.GetUnderlyingType() == type);
    }
    
    public string AppendExpression(Expression expression, string namePrefix, List<DbParam> dbParams, ref int propertyIndex, out Type? outputType)
    {
        outputType = null;
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } u)
        {
            return AppendExpression(u.Operand, namePrefix, dbParams, ref propertyIndex, out outputType);
        }
        if (expression is ConditionalExpression conditionalExpression)
        {
            var testCompiled = AppendExpression(conditionalExpression.Test, namePrefix, dbParams, ref propertyIndex, out _);
            var ifYesCompiled = AppendExpression(conditionalExpression.IfTrue, namePrefix, dbParams, ref propertyIndex, out _);
            var ifNoCompiled = AppendExpression(conditionalExpression.IfFalse, namePrefix, dbParams, ref propertyIndex, out _);
            return $"CASE WHEN {testCompiled} THEN {ifYesCompiled} ELSE {ifNoCompiled} END";
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
                ExpressionType.Add => " + ",
                ExpressionType.Subtract => " - ",
                ExpressionType.And or ExpressionType.AndAlso => " AND ",
                ExpressionType.Or or ExpressionType.OrElse => " OR ",
                _ => throw new UskokDbException(
                    $"Binary expression node type: {binaryExpression.NodeType} not supported")
            };
            
            var leftExpCompiled = AppendExpression(binaryExpression.Left, namePrefix, dbParams, ref propertyIndex, out var leftType);
            var rightExpCompiled = AppendExpression(binaryExpression.Right, namePrefix, dbParams, ref propertyIndex, out var rightType);
            
            
            
            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                return $"COALESCE({leftExpCompiled}, {rightExpCompiled})";
            }

            if (binaryExpression.NodeType == ExpressionType.Add)
            {
                if (rightType != null && leftType != null)
                {
                    if (rightType == typeof(string) && leftType == typeof(string))
                    {
                        outputType = typeof(string);
                        if (UskokDb.SqlDialect == SqlDialect.MySql)
                        {
                            return $"CONCAT({leftExpCompiled}, {rightExpCompiled})";
                        }
                        else
                        { 
                            return $"({leftExpCompiled} || {rightExpCompiled})";
                        }
                    }
                }
                
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

            outputType = constantExpression.Value.GetType();
            return AddParam(dbParams, constantExpression.Value, namePrefix, ref propertyIndex);
        }
        if (expression is MemberExpression memberExpression)
        {
            var holdingType = memberExpression.Expression?.Type ?? memberExpression.Member.DeclaringType;
            if (holdingType == null)
            {
                throw new UskokDbException("Member expression doesn't have a declaring type");
            }
            
            var queryable = GetQueryableInQuery(holdingType);

            if (queryable == null)
            {
                if (UskokDb.MemberTranslators.TryGetValue(memberExpression.Member, out var memberTranslator))
                {
                    return memberTranslator.Translate(this, memberExpression, namePrefix, dbParams, ref propertyIndex,
                        out outputType);
                }
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
                outputType = value.GetType();
                if (value is IEnumerable enumerable and not string)
                {
                    StringBuilder builder = new StringBuilder("(");
                    bool anyFound = false;
                    foreach (var enumerableItem in enumerable)
                    {
                        var paramName = AddParam(dbParams,  enumerableItem, namePrefix, ref propertyIndex);
                        builder.Append(paramName);
                        builder.Append(", ");
                        anyFound = true;
                    }

                    builder.Length -= 2;
                    builder.Append(")");

                    if (!anyFound) 
                        return null!;
                    
                    return builder.ToString();
                }
                return AddParam(dbParams, value, namePrefix, ref propertyIndex);
            }


            var propertyMetadata = queryable.GetMetadataPropertyFromName(memberExpression.Member.Name);
            outputType = propertyMetadata.Type;
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

        if (expression is MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            
            if (UskokDb.MethodTranslators.TryGetValue(method, out var translator))
            {
                return translator.Translate(this, methodCallExpression, namePrefix, dbParams, ref propertyIndex,
                    out outputType);
            }
            else
            {
                Console.WriteLine($"{methodCallExpression.Method.Name}");
                Console.WriteLine("Method call not found");
            }
        }
        
        return "<EMPTY>";
    }

    private const string NullVale = "NULL";
}