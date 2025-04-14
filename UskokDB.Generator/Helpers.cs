using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UskokDB.Generator;

public static class Helpers
{
    public static string? GetNamespaceForClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxNode? current = classDeclaration.Parent;

        while (current != null)
        {
            if (current is BaseNamespaceDeclarationSyntax namespaceDecl)
                return namespaceDecl.Name.ToString();
            current = current.Parent;
        }

        return null;
    }
}