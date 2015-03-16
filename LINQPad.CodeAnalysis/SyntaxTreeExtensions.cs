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
using LINQPad.CodeAnalysis;
using LINQPad.ObjectModel;

static class SyntaxTreeExtensions
{
    public static void DumpSyntaxTree(this SyntaxTree syntaxTree)
    {
        CodeAnalysisUtil.DumpSyntaxTree(syntaxTree, null, null);
    }

    public static void DumpSyntaxTree(this SyntaxTree syntaxTree, string declarationFilter)
    {
        CodeAnalysisUtil.DumpSyntaxTree(syntaxTree, declarationFilter, null);
    }

    public static void DumpSyntaxTree(this SyntaxTree syntaxTree, string declarationFilter, string description)
    {
        CodeAnalysisUtil.DumpSyntaxTree(syntaxTree, declarationFilter, description);
    }

    public static void DumpSyntaxTree(this Query query)
    {
        CodeAnalysisUtil.DumpSyntaxTree(query, null, null);
    }

    public static void DumpSyntaxTree(this Query query, string declarationFilter)
    {
        CodeAnalysisUtil.DumpSyntaxTree(query, declarationFilter, null);
    }

    public static void DumpSyntaxTree(this Query query, string declarationFilter, string description)
    {
        CodeAnalysisUtil.DumpSyntaxTree(query, declarationFilter, description);
    }
}
