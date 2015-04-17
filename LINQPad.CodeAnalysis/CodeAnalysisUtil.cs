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

    public static void DumpCSharpSyntaxTree(string text)
    {
        DumpCSharpSyntaxTree(text, null);
    }

    public static void DumpCSharpSyntaxTree(string text, string description)
    {
        SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text);
        DumpSyntaxTree(syntaxTree, null, description);
    }

    public static void DumpCSharpSyntaxTree(string text, SourceCodeKind kind)
    {
        DumpCSharpSyntaxTree(text, kind, null);
    }

    public static void DumpCSharpSyntaxTree(string text, SourceCodeKind kind, string description)
    {
        SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text,
            new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: kind));
        DumpSyntaxTree(syntaxTree, null, description);
    }

    public static void DumpVisualBasicSyntaxTree(string text)
    {
        DumpVisualBasicSyntaxTree(text, null);
    }

    public static void DumpVisualBasicSyntaxTree(string text, string description)
    {
        SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(text);
        DumpSyntaxTree(syntaxTree, null, description);
    }

    public static void DumpVisualBasicSyntaxTree(string text, SourceCodeKind kind)
    {
        DumpVisualBasicSyntaxTree(text, kind, null);
    }

    public static void DumpVisualBasicSyntaxTree(string text, SourceCodeKind kind, string description)
    {
        SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(text,
            new Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions(kind: kind));
        DumpSyntaxTree(syntaxTree, null, description);
    }

    internal static SyntaxTree GetSyntaxTree(Query query)
    {
        if (query != null)
        {
            switch(query.Language)
            {
                case QueryLanguage.Expression:
                    return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Interactive));
                case QueryLanguage.Statements:
                    return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Script));
                case QueryLanguage.Program:
                    return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Script));
                case QueryLanguage.VBExpression:
                    return Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions(kind: SourceCodeKind.Interactive));
                case QueryLanguage.VBStatements:
                    return Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions(kind: SourceCodeKind.Script));
                case QueryLanguage.VBProgram:
                    return Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(query.Text,
                        new Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions(kind: SourceCodeKind.Script));
                default:
                    return null;
            }
        }
        return null;
    }
}
