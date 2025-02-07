using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB;

public static class LinqToSql
{
    public static string Convert<T>(DbContext context, Expression<Func<T, bool>> expression) where T : class, new()
    {
        return CompileExpression<T>(context, expression.Body);
    }

    private static string CompileExpression<T>(DbContext context, Expression expression) where T : class, new()
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
            return $"({string.Join(joiner, CompileExpression<T>(context, binaryExpression.Left), CompileExpression<T>(context, binaryExpression.Right))})";
        }
        else if(expression is UnaryExpression unaryExpression)
        {
            string joiner = unaryExpression.NodeType == ExpressionType.Negate ? "-" : " JOINER ";

            return $"{joiner}{CompileExpression<T>(context, unaryExpression.Operand)}";
        }
        else if(expression is ConstantExpression constantExpression)
        {
            return context.DbIO.WriteValue(constantExpression.Value);
        }
        else if(expression is MemberExpression memberExpression)
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
                if(memberType == null) return context.DbIO.WriteValue(null);

                return context.DbIO.WriteValue(GetMemberValue(memberExpression));
            }
        }

        throw new Exception($"{expression.GetType().FullName} expression is not supported");
        
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
                object instance = GetInstanceObject(memberExp.Expression);
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
