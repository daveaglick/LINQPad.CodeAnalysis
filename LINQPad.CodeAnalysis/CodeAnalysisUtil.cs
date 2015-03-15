using LINQPad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CodeAnalysisUtil
{
    public static void DumpSyntaxTree()
    {
        DumpSyntaxTree(null);
    }

    public static void DumpSyntaxTree(string description)
    {
        Util.CurrentQuery.DumpSyntaxTree(description);
    }
}
