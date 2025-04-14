using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UskokDB.Generator;

[Generator]
public class LinqToSqlGeneration : IIncrementalGenerator
{
    private const string GenerateLinqToSqlAttributeName = "GenerateSqlTableHelpers";
    private const string GenerateLinqToSqlAttributeFullName = "GenerateSqlTableHelpersAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classesToGenerateLinq = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
            {
                if (node is not ClassDeclarationSyntax classDeclarationSyntax) return false;
                //check if static
                if (!classDeclarationSyntax.Modifiers
                        .Any(mod => mod.IsKind(SyntaxKind.StaticKeyword))) return false;

                return true;
                if (!classDeclarationSyntax.AttributeLists.Any()) return false;
                
                var attributeLists = classDeclarationSyntax.AttributeLists;
                return attributeLists.Any(list => list.Attributes.Any(attr => attr.Name.ToString() is GenerateLinqToSqlAttributeName or GenerateLinqToSqlAttributeFullName));
            },
            transform: static (context, _) => (ClassDeclarationSyntax)context.Node
        ).Where(t => t is not null);
        
        context.RegisterSourceOutput(classesToGenerateLinq, (c, classDeclaration) =>
        {
            var code = GenerateCodeForClass(classDeclaration);
            c.AddSource($"{classDeclaration.Identifier.ToString()}.g.cs", code);
        });
    }

    private static string GenerateCodeForClass(ClassDeclarationSyntax classDeclarationSyntax)
    {
        StringBuilder builder = new("using System;\n");
        builder.Append("namespace ");
        builder.Append(Helpers.GetNamespaceForClassDeclaration(classDeclarationSyntax));
        builder.AppendLine("{");
        builder.AppendLine($"public static class Compiled{classDeclarationSyntax.Identifier.ToString()}{{");
        foreach (var childNode in classDeclarationSyntax.ChildNodes())
        {
            if (childNode is not MethodDeclarationSyntax methodDeclaration) continue;
            var returnType = methodDeclaration.ReturnType;
            
            
            var parameters = methodDeclaration.ParameterList.Parameters;
            
            builder.AppendLine("/*");
            builder.AppendLine($"{methodDeclaration.ReturnType.IsUnmanaged.ToString()} {parameters.Count}");
            builder.AppendLine($"{returnType.ToString()}");
            builder.AppendLine($"{returnType.ChildNodes().Count()}");
            builder.AppendLine("*/");
            builder.AppendLine($"public const int {methodDeclaration.Identifier.ToString()} = 3;");
        }
        

        builder.Append("}}");
        return builder.ToString();
    }
}