using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax;

namespace LINQPad.CodeAnalysis.Tests
{
    [TestFixture]
    public class InternalsHelperFixture
    {
        [Test]
        public void GetUserDataFolderReturnsString()
        {
            // Given

            // When
            string userDataFolder = InternalsHelper.GetUserDataFolder();

            // Then
            Assert.IsNotNullOrEmpty(userDataFolder);
        }

        [Test]
        public void GetDefaultFontReturnsAFont()
        {
            // Given
            
            // When
            Font font = InternalsHelper.GetDefaultFont();

            // Then
            Assert.IsNotNull(font);
        }

        [Test]
        public void GetIdentifierTokenValueTextReturnsTokenValueTextForValidNode()
        {
            // Given
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("int c = 0;",
                new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Script));
            SyntaxToken syntaxToken = syntaxTree.GetRoot()
                .DescendantNodesAndTokens()
                .Where(x => x.IsToken)
                .Select(x => x.AsToken())
                .First(x => Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind(x) == Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken);
            SyntaxNode syntaxNode = syntaxToken.Parent;

            // When
            string tokenValueText = InternalsHelper.GetIdentifierTokenValueText(syntaxNode);

            // Then
            Assert.AreEqual("c", tokenValueText);
        }

        [Test]
        public void GetIdentifierTokenValueTextReturnsNullForInvalidNode()
        {
            // Given
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("int c = 0;",
                new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(kind: SourceCodeKind.Script));
            SyntaxNode syntaxNode = syntaxTree.GetRoot();  // The root will never have an identifier token

            // When
            string tokenValueText = InternalsHelper.GetIdentifierTokenValueText(syntaxNode);

            // Then
            Assert.IsNull(tokenValueText);
        }
    }
}
