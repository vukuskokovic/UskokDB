using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        else if(expression is MemberExpression parameterExpression)
        {
            var memberName = parameterExpression.Member.Name;
            var sqlName = TypeMetadata<T>.NameToPropertyMap[memberName].PropertyName;
            return sqlName;
        }

        throw new Exception($"{expression.GetType().FullName} is not supported");
        
    }
}
