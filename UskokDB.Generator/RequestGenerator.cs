using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UskokDB.Generator;

[Generator]
public class RequestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: ShouldClassBeIncluded,
            transform: (syntaxContext, token) => (ClassDeclarationSyntax)syntaxContext.Node
        );
        
        context.RegisterSourceOutput(provider, (productionContext, classSyntax) =>
        {
        });
    }
    
    

    public bool ShouldClassBeIncluded(SyntaxNode node, CancellationToken token)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax) return false;
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.ToString() == "GenerateRequests")
                {
                    return true;
                }
            }
        }

        return true;
    }
}