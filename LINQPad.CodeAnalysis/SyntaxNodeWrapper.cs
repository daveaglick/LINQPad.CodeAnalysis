using System.Collections;
using System.Drawing;
using BrightIdeasSoftware;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.CodeAnalysis
{
    internal class SyntaxNodeWrapper : SyntaxWrapper
    {
        private readonly SyntaxNode _node;

        public SyntaxNodeWrapper(SyntaxNode node)
        {
            _node = node;
        }

        public override object GetSyntaxObject()
        {
            return _node;
        }

        public override bool CanExpand()
        {
            return _node.ChildNodesAndTokens().Count > 0;
        }

        public override IEnumerable GetChildren()
        {
            return _node.ChildNodesAndTokens().Select(x => SyntaxWrapper.Get(x));
        }

        public override void FormatCell(FormatCellEventArgs format)
        {
            if (format.Column.Text == "Kind")
            {
                format.SubItem.ForeColor = Color.Blue;
            }
        }

        public override string GetKind()
        {
            // C#
            Microsoft.CodeAnalysis.CSharp.SyntaxKind cSharpKind = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind(_node);
            if(cSharpKind != Microsoft.CodeAnalysis.CSharp.SyntaxKind.None)
            {
                return cSharpKind.ToString();
            }

            // Visual Basic
            Microsoft.CodeAnalysis.VisualBasic.SyntaxKind visualBasicKind = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.Kind(_node);
            if(visualBasicKind != Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.None)
            {
                return visualBasicKind.ToString();
            }

            return "None";
        }

        public override string GetSpan()
        {
            return _node.FullSpan.ToString();
        }

        public override string GetText()
        {
            return string.Empty;
        }
    }
}
