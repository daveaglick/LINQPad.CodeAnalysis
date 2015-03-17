using LINQPad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQPad.CodeAnalysis;
using LINQPad.ObjectModel;
using Microsoft.CodeAnalysis;

public static class CodeAnalysisUtil
{
    public static void DumpSyntaxTree()
    {
        DumpSyntaxTree(Util.CurrentQuery, null, null);
    }

    public static void DumpSyntaxTree(string declarationFilter)
    {
        DumpSyntaxTree(Util.CurrentQuery, declarationFilter, null);
    }

    public static void DumpSyntaxTree(string declarationFilter, string description)
    {
        DumpSyntaxTree(Util.CurrentQuery, declarationFilter, description);
    }

    public static void DumpSyntaxTree(Query query)
    {
        DumpSyntaxTree(query, null, null);
    }

    public static void DumpSyntaxTree(Query query, string declarationFilter)
    {
        DumpSyntaxTree(query, declarationFilter, null);
    }

    public static void DumpSyntaxTree(Query query, string declarationFilter, string description)
    {
        DumpSyntaxTree(GetSyntaxTree(query), declarationFilter, description);
    }

    public static void DumpSyntaxTree(SyntaxTree syntaxTree)
    {
        DumpSyntaxTree(syntaxTree, null, null);
    }

    public static void DumpSyntaxTree(SyntaxTree syntaxTree, string declarationFilter)
    {
        DumpSyntaxTree(syntaxTree, declarationFilter, null);
    }

    public static void DumpSyntaxTree(SyntaxTree syntaxTree, string declarationFilter, string description)
    {
        if (syntaxTree != null)
        {
            PanelManager.DisplayControl(new SyntaxTreePanel(syntaxTree, declarationFilter), description ?? "Syntax Tree");
        }
    }

    internal static SyntaxTree GetSyntaxTree(Query query)
    {
        if (query != null)
        {
            if (query.Language == QueryLanguage.Program)
            {
                return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(query.Text);
            }
            else if (query.Language == QueryLanguage.Expression
                || query.Language == QueryLanguage.Statements)
            {
                return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(query.Text,
                    new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Script));
            }
            else if (query.Language == QueryLanguage.VBProgram)
            {
                return Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(query.Text);
            }
            else if (query.Language == QueryLanguage.VBExpression
                || query.Language == QueryLanguage.VBStatements)
            {
                return Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(query.Text,
                    new Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions(kind: SourceCodeKind.Script));
            }
        }
        return null;
    }
}
