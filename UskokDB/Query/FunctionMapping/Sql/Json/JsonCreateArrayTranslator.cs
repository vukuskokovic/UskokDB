using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.Sql.Json;

public class JsonCreateArrayTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method0 = typeof(QueryFunctions.Sql)
        .GetMethods()
        .First(m => m.Name == nameof(QueryFunctions.Sql.JsonCreateArray) && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Method1 = typeof(QueryFunctions.Sql)
        .GetMethods()
        .First(m => m.Name == nameof(QueryFunctions.Sql.JsonCreateArray) && m.GetGenericArguments().Length == 3);

    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams, 
        ref int propertyIndex, out Type? outType)
    {
        var exp = methodCall.Arguments[0];
        
        if (exp is UnaryExpression { NodeType: ExpressionType.Quote } quoted)
            exp = quoted.Operand;

        if (exp is not LambdaExpression lambda)
            throw new UskokDbException("JsonCreateArray, could not find LambdaExpression");
        
        var body = lambda.Body;
        
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            body = convert.Operand;
        }
        
        outType = methodCall.Method.GetGenericArguments()[0];

        var shouldJsonObjectBeCreated = !DbIO.PrimitiveTypes.Contains(outType);

        if (shouldJsonObjectBeCreated)
        {
            if (body is not MemberInitExpression memberInit) throw new UskokDbException("JsonCreateArray does not have a MemberInitExpression");
            
            StringBuilder builder = new("JSON_ARRAYAGG(JSON_OBJECT(");

            int i = 0;
            int count = memberInit.Bindings.Count;
            foreach (var memberBinding in memberInit.Bindings)
            {
                if (memberBinding is not MemberAssignment memberAssignment)
                {
                    throw new UskokDbException("JsonCreateArray assigment is not member assigment");
                }

                builder.Append('\'');
                builder.Append(memberBinding.Member.Name);
                builder.Append('\'');
                builder.Append(',');
                builder.Append(queryContext.AppendExpression(memberAssignment.Expression, namePrefix, dbParams, ref propertyIndex, out _));
                i++;
                if (i < count)
                {
                    builder.Append(',');
                }
            }
        
            builder.Append("))");
            return builder.ToString();
        }

        else
        {
            StringBuilder builder = new("JSON_ARRAYAGG(");
            builder.Append(queryContext.AppendExpression(body, namePrefix, dbParams, ref propertyIndex, out _));
            builder.Append(')');
            return builder.ToString();
        }
    }
}