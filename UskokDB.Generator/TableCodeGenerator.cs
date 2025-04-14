using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UskokDB.Generator;

[Generator]
public class TableCodeGenerator : IIncrementalGenerator
{
    private const string GenerateSqlHelperAttributeName = "GenerateSqlTableHelpers";
    private const string GenerateSqlHelperAttributeFullName = nameof(GenerateSqlTableHelpersAttribute);
    public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
    {
        var tableNameClasses = initializationContext.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
            {
                if (node is not ClassDeclarationSyntax classDeclarationSyntax) return false;

                if (!classDeclarationSyntax.AttributeLists.Any()) return false;
                
                var attributeLists = classDeclarationSyntax.AttributeLists;
                return attributeLists.Any(list => list.Attributes.Any(attr => attr.Name.ToString() is GenerateSqlHelperAttributeName or GenerateSqlHelperAttributeFullName));
            },
            transform: static (context, _) => (ClassDeclarationSyntax)context.Node
        ).Where(t => t is not null);
        
        initializationContext.RegisterSourceOutput(tableNameClasses, (context, classDeclaration) =>
        {
            var code = GenerateTableClassCode(classDeclaration);
            context.AddSource($"{classDeclaration.Identifier.ToString()}.g.cs", code);
        });
    }

    private static string GenerateTableClassCode(ClassDeclarationSyntax classDeclaration)
    {
        var className = classDeclaration.Identifier.Text;
        var classNamespace = Helpers.GetNamespaceForClassDeclaration(classDeclaration);
        
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        if (classNamespace != null)
            builder.AppendLine($"namespace {classNamespace}{{");
        
        AttributeSyntax? tableNameAttribute = null;
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if(!attribute.Name.ToString().Contains("TableName"))continue;
                tableNameAttribute = attribute;
                break;
            }

            if (tableNameAttribute != null) break;
        }

        string tableName = classDeclaration.Identifier.ToString();
        if (tableNameAttribute is { ArgumentList: not null })
        {
            var attributeArgumentSyntax = tableNameAttribute.ArgumentList.Arguments.FirstOrDefault();
            if (attributeArgumentSyntax?.Expression is LiteralExpressionSyntax literalExpressionSyntax)
            {
                tableName = literalExpressionSyntax.Token.ValueText;
            }
        }
        
        builder.AppendLine($"public static class {className}Sql");
        builder.AppendLine("{");
        
        builder.AppendLine($"   public const string TableName = \"{tableName}\";");

        foreach (var childNode in classDeclaration.ChildNodes())
        {
            if (childNode is not PropertyDeclarationSyntax propertyDeclaration) continue;
            string? columnName = null;
            bool isIgnored = false;

            foreach (var attributeList in propertyDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attrName = attribute.Name.ToString();
                    if (attrName.Contains("NotMapped"))
                    {
                        isIgnored = true;
                        break;
                    }

                    if (attribute.ArgumentList is not null && attrName.Contains("Column"))
                    {
                        var attributeArgumentSyntax = attribute.ArgumentList.Arguments.FirstOrDefault();
                        if (attributeArgumentSyntax?.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                        {
                            columnName = literalExpressionSyntax.Token.ValueText;
                        }
                    }
                }

                if (isIgnored) break;
            }
            
            if(isIgnored)continue;
            var propertyName = propertyDeclaration.Identifier.ToString();
            columnName ??=  FirstLetterLowerCase(propertyName);
            builder.AppendLine($"   public const string Column_{propertyName} = \"{columnName}\";");
        }
        
        builder.AppendLine("}");
        
        
        if (classNamespace != null)
            builder.AppendLine("}");
        

        return builder.ToString();
    }
    
    
    
    private static string FirstLetterLowerCase(string str)
    {
        var length = str.Length;
        if (length == 0 || !char.IsUpper(str[0])) return str;
        
        if (length == 1) return str.ToLower();
        return char.ToLower(str[0]) + str.Substring(1);

    }
}