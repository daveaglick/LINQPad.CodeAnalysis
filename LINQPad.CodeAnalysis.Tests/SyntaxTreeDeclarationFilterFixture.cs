using LINQPad.CodeAnalysis;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.CodeAnalysis.Tests
{
    [TestFixture]
    public class SyntaxTreeDeclarationFilterFixture
    {
        [Test]
        public void GetMatchingSyntaxNodesForCSharpNullFilterReturnsRoot()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter(null);
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.AreEqual(matchingNodes.First(), syntaxTree.GetRoot());
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbNullFilterReturnsRoot()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter(null);
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.AreEqual(matchingNodes.First(), syntaxTree.GetRoot());
        }

        [Test]
        public void GetMatchingSyntaxNodesForCSharpEmptyFilterReturnsRoot()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter(string.Empty);
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.AreEqual(matchingNodes.First(), syntaxTree.GetRoot());
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbEmptyFilterReturnsRoot()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter(string.Empty);
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.AreEqual(matchingNodes.First(), syntaxTree.GetRoot());
        }

        [Test]
        public void GetMatchingSyntaxNodesForCSharpNonMatchingFilterReturnsNoMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("NoMatch");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            CollectionAssert.IsEmpty(matchingNodes);
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbNonMatchingFilterReturnsNoMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("NoMatch");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            CollectionAssert.IsEmpty(matchingNodes);
        }

        [Test]
        public void GetMatchingSyntaxNodesForCSharpClassFilterReturnsClassMatch()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("TestClass");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>(matchingNodes.First());
            Assert.AreEqual(((Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)matchingNodes.First()).Identifier.ValueText, "TestClass");
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbClassFilterReturnsClassMatch()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("TestClass");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(1, matchingNodes.Count());
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.VisualBasic.Syntax.ClassBlockSyntax>(matchingNodes.First());
            Assert.AreEqual(((Microsoft.CodeAnalysis.VisualBasic.Syntax.ClassBlockSyntax)matchingNodes.First()).ClassStatement.Identifier.ValueText, "TestClass");
        }

        [Test]
        public void GetMatchingSyntaxNodesForCSharpPropertyFilterReturnsPropertyMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("Number");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(2, matchingNodes.Count());
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax>(matchingNodes.First());
            Assert.AreEqual(((Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)matchingNodes.First()).Identifier.ValueText, "Number");
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbPropertyFilterReturnsPropertyMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("Number");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            Assert.AreEqual(2, matchingNodes.Count());
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.VisualBasic.Syntax.PropertyBlockSyntax>(matchingNodes.First());
            Assert.AreEqual(((Microsoft.CodeAnalysis.VisualBasic.Syntax.PropertyBlockSyntax)matchingNodes.First()).PropertyStatement.Identifier.ValueText, "Number");
        }

        [Test]
        public void GetMatchingSyntaxNodesForCSharpVariableFilterReturnsNoMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("number");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            CollectionAssert.IsEmpty(matchingNodes);
        }

        [Test]
        public void GetMatchingSyntaxNodesForVbVariableFilterReturnsNoMatches()
        {
            // Given
            SyntaxTreeDeclarationFilter filter = new SyntaxTreeDeclarationFilter("number");
            SyntaxTree syntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(VisualBasicTestCode);

            // When
            IEnumerable<SyntaxNode> matchingNodes = filter.GetMatchingSyntaxNodes(syntaxTree);

            // Then
            CollectionAssert.IsEmpty(matchingNodes);
        }

        public static readonly string CSharpTestCode = @"
            public class TestClass
            {
	            private int _count;
	            public int Number {
		            get { return _count; }
		            set { _count = value; }
	            }
            }

            public class TestClass2
            {
	            private int _count;
	            public int Number {
		            get { return _count; }
		            set { _count = value; }
	            }
            }

            public void Main()
            {
	            int Number = 3;
                int number = 3;
	            TestClass TestClass = new TestClass();
	            TestClass.Number = 1;
            }";

        public static readonly string VisualBasicTestCode = @"
            Class TestClass
                Private _count As Integer
                Public Property Number() As Integer
	            Get
	                Return _count
	            End Get
	            Set(ByVal value As Integer)
	                _count = value
	            End Set
                End Property
            End Class

            Class TestClass2
                Private _count As Integer
                Public Property Number() As Integer
	            Get
	                Return _count
	            End Get
	            Set(ByVal value As Integer)
	                _count = value
	            End Set
                End Property
            End Class

            Sub Main()
	            Dim Number As Integer = 3
	            Dim number As Integer = 3
	            Dim TestClass As TestClass = New TestClass()
	            TestClass.Number = 1
            End Sub";
    }
}
