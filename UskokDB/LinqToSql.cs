using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB;

public static class LinqToSql
{
    public static DbPopulateParamsResult Convert<T>(Expression<Func<T, bool>> expression, List<DbParam>? paramsList = null) where T : class, new()
    {
        var result = new DbPopulateParamsResult()
        {
            Params = paramsList ?? []
        };
        result.CompiledText = CompileExpression<T>(expression.Body, result.Params);
        return result;
    }

    public static string CompileExpression<T>(Expression expression, List<DbParam> paramList) where T : class, new()
    {
        if(expression is BinaryExpression binaryExpression)
        {
            var type = binaryExpression.NodeType;
            string joiner = type switch
            {
                ExpressionType.And or ExpressionType.AndAlso => " AND ",
                ExpressionType.Or or ExpressionType.OrElse => " OR ",
                ExpressionType.GreaterThan => " > ",
                ExpressionType.GreaterThanOrEqual => " >= ",
                ExpressionType.LessThan => " < ",
                ExpressionType.LessThanOrEqual => " <= ",
                ExpressionType.Equal => " = ",
                ExpressionType.NotEqual => " != ",
                ExpressionType.Add => " + ",
                ExpressionType.Subtract =>  " - ",
                ExpressionType.Modulo => " % ",
                ExpressionType.Multiply => " * ",
                ExpressionType.Divide => " / ",
                _ => " JOIN "
            };
            return $"({string.Join(joiner, CompileExpression<T>(binaryExpression.Left, paramList), CompileExpression<T>(binaryExpression.Right, paramList))})";
        }
        if(expression is UnaryExpression unaryExpression)
        {
            string joiner = unaryExpression.NodeType switch
            {
                ExpressionType.Negate => " - ",
                ExpressionType.Convert => "",
                _ => $" UNKNOWN JOINER({unaryExpression.NodeType}) "
            };
            return $"{joiner}{CompileExpression<T>(unaryExpression.Operand, paramList)}";
        }
        if(expression is ConstantExpression constantExpression)
        {
            return AddParam(paramList, constantExpression.Value);
        }
        if(expression is MemberExpression memberExpression)
        {
            var memberInfo = memberExpression.Member;
            var memberType = memberInfo.DeclaringType;
            var memberName = memberInfo.Name;
            if (memberType == typeof(T))
            {
                var sqlName = TypeMetadata<T>.NameToPropertyMap[memberName].PropertyName;
                return sqlName;
            }
            else
            {
                var paramValue = memberType == null ? null : GetMemberValue(memberExpression);

                return AddParam(paramList, paramValue);
            }
        }

        throw new UskokDbInvalidLinqExpression(expression);
        
    }

    private static string AddParam(List<DbParam> paramList, object? value)
    {
        int paramIndex = paramList.Count;
        var paramName = $"@p_{paramIndex}";
        DbParam newParam = new DbParam()
        {
            Name = paramName,
            Value = value
        };
        paramList.Add(newParam);
        return paramName;
    }

    static object? GetMemberValue(MemberExpression memberExp)
    {
        if (memberExp.Member is PropertyInfo propInfo)
        {
            if (propInfo.GetMethod?.IsStatic == true)
            {
                // Static property - Get value without instance
                return propInfo.GetValue(null);
            }
            else
            {
                // Instance property - Extract instance first
                object? instance = GetInstanceObject(memberExp.Expression);
                return propInfo.GetValue(instance);
            }
        }
        else if (memberExp.Member is FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                // Static field - Get value without instance
                return fieldInfo.GetValue(null);
            }
            else
            {
                // Instance field - Extract instance first
                object instance = GetInstanceObject(memberExp.Expression)!;
                return fieldInfo.GetValue(instance);
            }
        }

        throw new InvalidOperationException("Could not extract member value");
    }

    // Helper method to extract the instance from a MemberExpression
    static object? GetInstanceObject(Expression? expression)
    {
        if (expression is ConstantExpression constantExp)
        {
            return constantExp.Value; // The instance itself
        }
        else if (expression is MemberExpression memberExp)
        {
            return GetMemberValue(memberExp); // Recursively evaluate the instance
        }

        throw new InvalidOperationException("Unsupported expression type for instance extraction");
    }

}