using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace AspireAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PostgresDatabaseNamingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ASPIRECA0001";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ASPCA001_Title), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ASPCA001_MessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ASPCA001_Description), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeContainerAndDbName, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeContainerAndDbName(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        _ = context.CancellationToken;

        // Check if the node is an invocation expression
        if (node is InvocationExpressionSyntax invocationExpression)
        {
            // Check if the invocation expression is a call to AddDatabase
            if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                               && memberAccessExpression.Name.Identifier.ValueText == "AddDatabase")
            {
                // Check if the invocation expression is a call to AddPostgresContainer
                if (memberAccessExpression.Expression is InvocationExpressionSyntax invocationExpression2
                                       && invocationExpression2.Expression is MemberAccessExpressionSyntax memberAccessExpression2
                                                          && memberAccessExpression2.Name.Identifier.ValueText == "AddPostgresContainer")
                {
                    // Check if the first argument of AddDatabase is a string literal
                    if (invocationExpression.ArgumentList.Arguments.Count > 0
                                               && invocationExpression.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalExpression
                                                                      && literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        // Check if the first argument of AddPostgresContainer is a string literal
                        if (invocationExpression2.ArgumentList.Arguments.Count > 0
                                                       && invocationExpression2.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalExpression2
                                                                                  && literalExpression2.Kind() == SyntaxKind.StringLiteralExpression)
                        {
                            // Check if the first argument of AddDatabase is the same as the first argument of AddPostgresContainer
                            if (literalExpression.Token.ValueText.ToLowerInvariant() == literalExpression2.Token.ValueText.ToLowerInvariant())
                            {
                                // Report diagnostic
                                var diagnostic = Diagnostic.Create(Rule, literalExpression.GetLocation(), literalExpression.Token.ValueText, literalExpression2.Token.Value);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }
    }
}
