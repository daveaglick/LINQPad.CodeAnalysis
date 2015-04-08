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
    internal class SyntaxTokenWrapper : SyntaxWrapper
    {
        private readonly SyntaxToken _token;

        public SyntaxTokenWrapper(SyntaxToken token)
        {
            _token = token;
        }

        public override object GetSyntaxObject()
        {
            return _token;
        }

        public override bool CanExpand()
        {
            return _token.HasLeadingTrivia || _token.HasTrailingTrivia;
        }

        public override IEnumerable GetChildren()
        {
            return _token.LeadingTrivia
                .Concat(_token.TrailingTrivia)
                .Select(x => SyntaxWrapper.Get(x));
        }

        public override Color GetColor()
        {
            return Color.Green;
        }

        public override string GetKind()
        {
            // C#
            Microsoft.CodeAnalysis.CSharp.SyntaxKind cSharpKind = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind(_token);
            if(cSharpKind != Microsoft.CodeAnalysis.CSharp.SyntaxKind.None)
            {
                return cSharpKind.ToString();
            }

            // Visual Basic
            Microsoft.CodeAnalysis.VisualBasic.SyntaxKind visualBasicKind = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.Kind(_token);
            if(visualBasicKind != Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.None)
            {
                return visualBasicKind.ToString();
            }

            return "None";
        }

        public override string GetSpan()
        {
            return _token.FullSpan.ToString();
        }

        public override string GetText()
        {
            return _token.Text;
        }
    }
}
