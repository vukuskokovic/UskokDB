using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace UskokDB.CodeGeneration;

[Generator]
public class ColumnNameGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AttributeSyntax?> attributeProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (SyntaxNode node, CancellationToken _) => node is AttributeSyntax,
            transform: static (context, token) => {
                var attributeSyntax = (AttributeSyntax)context.Node;

                var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken: token).Symbol as IMethodSymbol;

                if (symbol?.ContainingType.ToDisplayString() == "UskokDB.Attributes.ColumnAttribute")
                {
                    return attributeSyntax;
                }

                return null;
            }
        ).Where(attr => attr is not null);

        context.RegisterSourceOutput(attributeProvider, (sourceContext, argumentSyntax) => Execute(argumentSyntax, sourceContext));
    }

    public void Execute(AttributeSyntax attributeSyntax, SourceProductionContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("CLN001", "Column Name", $"Found column name", "SourceGen", DiagnosticSeverity.Warning, true),
                attributeSyntax.GetLocation()
            ));
        var arg = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
        if (arg?.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            string value = literal.Token.ValueText;

            // Example: emit source or debug
            

            // You can now use `value` in code generation
        }
    }
}
