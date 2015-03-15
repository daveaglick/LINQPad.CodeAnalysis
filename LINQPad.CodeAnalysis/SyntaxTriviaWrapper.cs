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
    internal class SyntaxTriviaWrapper : SyntaxWrapper
    {
        private readonly SyntaxTrivia _trivia;

        public SyntaxTriviaWrapper(SyntaxTrivia trivia)
        {
            _trivia = trivia;
        }

        public override object GetSyntaxObject()
        {
            return _trivia;
        }

        public override bool CanExpand()
        {
            return false;
        }

        public override IEnumerable GetChildren()
        {
            return null;
        }

        public override void FormatCell(FormatCellEventArgs format)
        {
            if (format.Column.Text == "Kind")
            {
                format.SubItem.ForeColor = Color.Maroon;
            }
        }

        public override string GetKind()
        {
            return (_trivia.Token.LeadingTrivia.Contains(_trivia) ? "Lead: " : "Trail: ") + _trivia.Kind().ToString();
        }

        public override string GetSpan()
        {
            return _trivia.FullSpan.ToString();
        }

        public override string GetText()
        {
            return _trivia.ToString();
        }
    }
}
