using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace LINQPad.CodeAnalysis
{
    internal class SyntaxTreeDeclarationFilter
    {
        private readonly string _declarationFilter;

        public SyntaxTreeDeclarationFilter(string declarationFilter)
        {
            _declarationFilter = declarationFilter;
        }

        public IEnumerable<SyntaxNode> GetMatchingSyntaxNodes(SyntaxTree syntaxTree)
        {
            SyntaxNode root;
            if (syntaxTree.TryGetRoot(out root))
            {
                // Just return the root if not filtering by declaration
                if (string.IsNullOrWhiteSpace(_declarationFilter))
                {
                    return new[] { root };
                }
                else
                {
                    // Filter by declaration
                    return root
                        .DescendantNodes(x => !SyntaxNodeMatchesDeclaration(x))
                        .Where(x => SyntaxNodeMatchesDeclaration(x));
                }
            }
            return new SyntaxNode[] { };
        }

        public bool SyntaxNodeMatchesDeclaration(SyntaxNode syntaxNode)
        {
            if (string.IsNullOrWhiteSpace(_declarationFilter))
            {
                return true;
            }

            // The semantics of CSharp and Visual Basic appear to be different (I.e., VB puts everything inside a "Block")
            if (syntaxNode.SyntaxTree is Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree)
            {
                if (SyntaxNodeWrapper.Get(syntaxNode).GetKind().EndsWith("Declaration"))
                {
                    return GetIdentifierTokenValueText(syntaxNode) == _declarationFilter;
                }
            }
            else if (syntaxNode.SyntaxTree is Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree)
            {
                if (SyntaxNodeWrapper.Get(syntaxNode).GetKind().EndsWith("Block"))
                {
                    SyntaxNode firstChild = syntaxNode.ChildNodes().FirstOrDefault();
                    if (firstChild != null && SyntaxNodeWrapper.Get(firstChild).GetKind().EndsWith("Statement"))
                    {
                        return GetIdentifierTokenValueText(firstChild) == _declarationFilter;
                    }
                }
            }

            return false;
        }

        // Using reflection to get .Identifier is a hack, but don't know any other way to check for identifiers across all syntax node types - YOLO!
        public static string GetIdentifierTokenValueText(SyntaxNode syntaxNode)
        {
            PropertyInfo identifierProperty = syntaxNode.GetType().GetProperty("Identifier", BindingFlags.Public | BindingFlags.Instance);
            if (identifierProperty != null)
            {
                object identifierToken = identifierProperty.GetValue(syntaxNode);
                if (identifierToken != null && identifierToken is SyntaxToken)
                {
                    return ((SyntaxToken)identifierToken).ValueText;
                }
            }
            return null;
        }
    }
}
