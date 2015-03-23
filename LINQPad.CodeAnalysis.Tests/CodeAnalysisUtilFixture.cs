using System.Reflection;
using LINQPad.ObjectModel;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.CodeAnalysis.Tests
{
    [TestFixture(Category = "ExcludeFromAppVeyor")]
    public class CodeAnalysisUtilFixture
    {
        private Query GetQuery(string queryFile)
        {
            string queryPath = Path.Combine(Path.GetDirectoryName(typeof(CodeAnalysisUtilFixture).Assembly.Location), "LINQPadQueries", queryFile);
            Assembly linqPadAssembly = typeof(Query).Assembly;
            Type fileQueryType = linqPadAssembly.GetType("LINQPad.ObjectModel.FileQuery");
            MethodInfo fileQueryFromPathMethod = fileQueryType.GetMethod("FromPath", BindingFlags.Static | BindingFlags.NonPublic);
            Query query = (Query)fileQueryFromPathMethod.Invoke(null, new [] { queryPath });
            return query;
        }

        [Test]
        public void GetSyntaxTreeForCSharpExpressionQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("CSharpExpression.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = 
                Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForCSharpStatementsQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("CSharpStatements.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation =
                Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForCSharpProgramQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("CSharpProgram.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation =
                Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForVbExpressionQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("VbExpression.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation compilation =
                Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.VisualBasic.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForVbStatementsQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("VbStatements.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation compilation =
                Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.VisualBasic.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForVbProgramQueryReturnsCorrectSyntaxTree()
        {
            // Given
            Query query = GetQuery("VbProgram.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation compilation =
                Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.Create("TestCompilation", new[] { syntaxTree });
            IEnumerable<Diagnostic> diagnostics = compilation.GetParseDiagnostics();

            // Then
            Assert.IsInstanceOf<Microsoft.CodeAnalysis.VisualBasic.Syntax.CompilationUnitSyntax>(syntaxTree.GetRoot());
            CollectionAssert.IsEmpty(diagnostics);
        }

        [Test]
        public void GetSyntaxTreeForUnsupportedQueryReturnsNull()
        {
            // Given
            Query query = GetQuery("FSharpProgram.linq");

            // When
            SyntaxTree syntaxTree = CodeAnalysisUtil.GetSyntaxTree(query);

            // Then
            Assert.IsNull(syntaxTree);
        }
    }
}
