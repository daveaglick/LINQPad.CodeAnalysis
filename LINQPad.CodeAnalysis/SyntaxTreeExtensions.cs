using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using LINQPad;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using LINQPad.CodeAnalysis;
using LINQPad.ObjectModel;
using Microsoft.CodeAnalysis.VisualBasic;

static class SyntaxTreeExtensions
{
    public static void DumpSyntaxTree(this SyntaxTree syntaxTree)
    {
        DumpSyntaxTree(syntaxTree, null, null);
    }

    public static void DumpSyntaxTree(this SyntaxTree syntaxTree, string declarationFilter)
    {
        DumpSyntaxTree(syntaxTree, declarationFilter, null);
    }

    public static void DumpSyntaxTree(this SyntaxTree syntaxTree, string declarationFilter, string description)
    {
        if (syntaxTree != null)
        {
            PanelManager.DisplayControl(new SyntaxTreePanel(syntaxTree, declarationFilter), description ?? "Syntax Tree");
        }
    }

    public static void DumpSyntaxTree(this Query query)
    {
        DumpSyntaxTree(query, null, null);
    }

    public static void DumpSyntaxTree(this Query query, string declarationFilter)
    {
        DumpSyntaxTree(query, declarationFilter, null);
    }

    public static void DumpSyntaxTree(this Query query, string declarationFilter, string description)
    {
        if (query != null)
        {
            SyntaxTree syntaxTree = null;
            if (query.Language == QueryLanguage.Expression
                || query.Language == QueryLanguage.Statements
                || query.Language == QueryLanguage.Program)
            {
                syntaxTree = CSharpSyntaxTree.ParseText(query.Text);
            }
            else if (query.Language == QueryLanguage.VBExpression
                || query.Language == QueryLanguage.VBStatements
                || query.Language == QueryLanguage.VBProgram)
            {
                syntaxTree = VisualBasicSyntaxTree.ParseText(query.Text);
            }
            if (syntaxTree != null)
            {
                DumpSyntaxTree(syntaxTree, declarationFilter, description);
            }
        }
    }
}
