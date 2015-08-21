using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace LINQPad.CodeAnalysis
{
    // This encapsulates all of the hacky reflection I have to do to get access to internal stuff in LINQPad and Roslyn
    internal static class InternalsHelper
    {
        public static string GetUserDataFolder()
        {
            Assembly linqPadAssembly = typeof(LINQPad.ObjectModel.Query).Assembly;
            Type programType = linqPadAssembly.GetType("LINQPad.Program");
            FieldInfo userDataFolderField = programType.GetField("UserDataFolder");
            return (string)userDataFolderField.GetValue(null);
        }

        public static Font GetDefaultFont()
        {
            string str = Path.Combine(GetUserDataFolder(), "queryfont.txt");
            if (File.Exists(str))
            {
                try
                {
                    return new Font(File.ReadAllText(str), GetQueryEditorDefaultQueryFont().SizeInPoints);
                }
                catch
                {
                }
            }
               
            return GetQueryEditorDefaultQueryFont();
        }

        // I can't actually reflect on QueryEditor because it's derived from a third-party control
        // So this replicates QueryEditor.DefaultQueryFont
        private static Font GetQueryEditorDefaultQueryFont()
        {
            Font defaultQueryFont = null;
            try
            {
                defaultQueryFont = Control.DefaultFont;
                defaultQueryFont = new Font(FontFamily.GenericMonospace, 10f);
            }
            catch
            {
            }
            try
            {
                defaultQueryFont = new Font("Consolas", 10f);
                if (defaultQueryFont.Name != "Consolas")
                {
                    defaultQueryFont = new Font("Courier New", 10f);
                }
            }
            catch
            {
            }
            return defaultQueryFont;
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

        // This is a little workaround for the NotSupportedException the CSharpParseOptions ctor throws for interactive or script
        // It can be removed once Roslyn supports scripting again
        // Huge thanks to Joe Albahari for pointing out that this works to get around it
        public static CSharpParseOptions GetCSharpParseOptionsForSourceCodeKind(SourceCodeKind sourceCodeKind)
        {
            CSharpParseOptions options = new CSharpParseOptions();
            options.GetType().GetProperty("Kind").SetValue(options, sourceCodeKind);
            return options;
        }

        public static VisualBasicParseOptions GetVisualBasicParseOptionsForSourceCodeKind(SourceCodeKind sourceCodeKind)
        {
            VisualBasicParseOptions options = new VisualBasicParseOptions();
            options.GetType().GetProperty("Kind").SetValue(options, sourceCodeKind);
            return options;
        }
    }
}
