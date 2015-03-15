using System.Collections;
using System.Drawing;
using BrightIdeasSoftware;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

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

        public override void FormatCell(FormatCellEventArgs format)
        {
            if (format.Column.Text == "Kind")
            {
                format.SubItem.ForeColor = Color.Green;
            }
        }

        public override string GetKind()
        {
            return _token.Kind().ToString();
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
