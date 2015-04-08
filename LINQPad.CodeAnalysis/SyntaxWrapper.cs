using BrightIdeasSoftware;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.CodeAnalysis
{
    abstract internal class SyntaxWrapper
    {
        public static SyntaxWrapper Get(object syntax)
        {
            if (syntax is SyntaxNode)
            {
                return new SyntaxNodeWrapper((SyntaxNode)syntax);
            }
            if (syntax is SyntaxToken)
            {
                return new SyntaxTokenWrapper((SyntaxToken)syntax);
            }
            if (syntax is SyntaxTrivia)
            {
                return new SyntaxTriviaWrapper((SyntaxTrivia)syntax);
            }
            if (syntax is SyntaxNodeOrToken)
            {
                if (((SyntaxNodeOrToken)syntax).IsNode)
                {
                    return new SyntaxNodeWrapper(((SyntaxNodeOrToken)syntax).AsNode());
                }
                return new SyntaxTokenWrapper(((SyntaxNodeOrToken)syntax).AsToken());
            }
            return null;
        }

        public abstract object GetSyntaxObject();

        public abstract bool CanExpand();
        public abstract IEnumerable GetChildren();

        public virtual void FormatCell(FormatCellEventArgs format)
        {
            if (format.Column.Text == "Kind")
            {
                format.SubItem.ForeColor = GetColor();
            }
        }

        public abstract Color GetColor();

        public abstract string GetKind();

        public virtual string GetTreeText()
        {
            return GetKind();
        }

        public virtual string GetGraphText()
        {
            return GetKind();
        }

        public virtual FontStyle GetGraphFontStyle()
        {
            return FontStyle.Regular;
        }

        public abstract string GetSpan();

        public abstract string GetText();
    }
}
