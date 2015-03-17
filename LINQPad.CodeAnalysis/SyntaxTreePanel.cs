using System.Drawing;
using BrightIdeasSoftware;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace LINQPad.CodeAnalysis
{
    internal class SyntaxTreePanel : TableLayoutPanel
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly TreeListView _treeList;
        private readonly ToolStrip _toolStrip;

        public SyntaxTreePanel(SyntaxTree syntaxTree, string declarationFilter)
        {
            _syntaxTree = syntaxTree;

            // Create the tree view
            _treeList = new TreeListView()
            {
                CanExpandGetter = x => ((SyntaxWrapper)x).CanExpand(),
                ChildrenGetter = x => ((SyntaxWrapper)x).GetChildren(),
                UseCellFormatEvents = true,
                Dock = DockStyle.Fill,
                UseFiltering = true
            };
            _treeList.FormatCell += (x, e) => ((SyntaxWrapper)e.CellValue).FormatCell(e);
            _treeList.BeforeSorting += (x, e) => e.Canceled = true;

            // Handle activate (dump the syntax object)
            _treeList.ItemActivate += (x, e) =>
            {
                if (_treeList.SelectedItem != null)
                {
                    SyntaxWrapper wrapper = (SyntaxWrapper)_treeList.SelectedItem.RowObject;
                    wrapper.GetSyntaxObject().Dump(wrapper.GetKind() + " " + wrapper.GetSpan());
                }
            };

            // Create columns
            _treeList.Columns.Add(new OLVColumn("Kind", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetKind()
            });
            _treeList.Columns.Add(new OLVColumn("Span", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetSpan()
            });
            _treeList.Columns.Add(new OLVColumn("Text", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetText()
            });

            // Set the root
            SyntaxNode root;
            int depth = 0;
            if (syntaxTree.TryGetRoot(out root))
            {
                SetRoots(declarationFilter);
                depth = GetDepth(root);
            }

            // Calculate control width
            AutoSizeColumns(_treeList, depth, true);
            _treeList.Layout += (x, e) => AutoSizeColumns(_treeList, depth, false);            

            // Toolstrip
            _toolStrip = CreateToolStrip();

            // Layout
            Controls.Add(_toolStrip, 0, 0);
            Controls.Add(_treeList, 0, 1);
        }

        private ToolStrip CreateToolStrip()
        {
            // Syntax and trivia toggles
            CheckBox syntaxTokenCheckBox = new CheckBox()
            {
                BackColor = Color.Transparent,
                Checked = true,
            };
            CheckBox syntaxTriviaCheckBox = new CheckBox()
            {
                BackColor = Color.Transparent,
                Checked = true,
            };
            bool handleChecked = true;	// Prevent double handling from adjustments during another handler
            syntaxTokenCheckBox.CheckedChanged += (x, e) =>
            {
                if (handleChecked)
                {
                    if (!syntaxTokenCheckBox.Checked)
                    {
                        handleChecked = false;
                        syntaxTriviaCheckBox.Checked = false;
                        handleChecked = true;
                    }
                    if (syntaxTokenCheckBox.Checked && syntaxTriviaCheckBox.Checked)
                    {
                        _treeList.ModelFilter = null;
                    }
                    _treeList.ModelFilter = new SyntaxFilter(syntaxTokenCheckBox.Checked, syntaxTriviaCheckBox.Checked);
                }
            };
            syntaxTriviaCheckBox.CheckedChanged += (x, e) =>
            {
                if (handleChecked)
                {
                    if (!syntaxTokenCheckBox.Checked)
                    {
                        handleChecked = false;
                        syntaxTokenCheckBox.Checked = true;
                        handleChecked = true;
                    }
                    if (syntaxTokenCheckBox.Checked && syntaxTriviaCheckBox.Checked)
                    {
                        _treeList.ModelFilter = null;
                    }
                    _treeList.ModelFilter = new SyntaxFilter(syntaxTokenCheckBox.Checked, syntaxTriviaCheckBox.Checked);
                }
            };

            // Declaration filter
            ToolStripTextBox declarationFilterTextBox = new ToolStripTextBox();
            declarationFilterTextBox.KeyDown += (x, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SetRoots(declarationFilterTextBox.Text);
                }
            };
            declarationFilterTextBox.LostFocus += (x, e) =>
            {
                SetRoots(declarationFilterTextBox.Text);
            };

            // Layout
            ToolStrip toolStrip = new ToolStrip(
                new ToolStripButton("Expand All", null, (x, e) => _treeList.ExpandAll()),
                new ToolStripButton("Collapse All", null, (x, e) => _treeList.CollapseAll()),
                new ToolStripSeparator(),
                new ToolStripLabel("SyntaxNode  ")
                {
                    ForeColor = Color.Blue
                },
                new ToolStripControlHost(syntaxTokenCheckBox),
                new ToolStripLabel("SyntaxToken  ")
                {
                    ForeColor = Color.Green
                },
                new ToolStripControlHost(syntaxTriviaCheckBox),
                new ToolStripLabel("SyntaxTrivia  ")
                {
                    ForeColor = Color.Maroon
                },
                new ToolStripLabel("(Double-Click A Node To Dump)"),
                new ToolStripSeparator(),
                new ToolStripLabel("Declaration Filter"),
                declarationFilterTextBox)
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Renderer = new BorderlessToolStripRenderer(),
                Padding = new Padding(4)
            };
            toolStrip.Layout += (x, e) => toolStrip.Width = toolStrip.Parent.Width;

            return toolStrip;
        }

        public void SetRoots(string declarationFilter)
        {
            SyntaxNode root;
            if (_syntaxTree.TryGetRoot(out root))
            {
                // Just return the root if not filtering by declaration
                if (string.IsNullOrWhiteSpace(declarationFilter))
                {
                    _treeList.Roots = new[] { SyntaxWrapper.Get(root) };
                }
                else
                {
                    // Filter by declaration
                    _treeList.Roots = root.DescendantNodes(x => !SyntaxNodeMatchesDeclaration(x, declarationFilter))
                        .Where(x => SyntaxNodeMatchesDeclaration(x, declarationFilter))
                        .Select(x => SyntaxWrapper.Get(x))
                        .ToArray();
                }
            }
            _treeList.ExpandAll();
        }

        private bool SyntaxNodeMatchesDeclaration(SyntaxNode syntaxNode, string declarationFilter)
        {
            if (string.IsNullOrWhiteSpace(declarationFilter))
            {
                return true;
            }

            // Using reflection to get .Identifier is a hack, but don't know any other way to check for identifiers across all syntax node types - YOLO!
            // Also, the semantics of CSharp and Visual Basic appear to be different (I.e., VB puts everything inside a "Block")
            if (_syntaxTree is Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree)
            {
                if (SyntaxNodeWrapper.Get(syntaxNode).GetKind().EndsWith("Declaration"))
                {
                    return GetIdentifierTokenValueText(syntaxNode) == declarationFilter;
                }
            }
            else if(_syntaxTree is Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree)
            {
                if (SyntaxNodeWrapper.Get(syntaxNode).GetKind().EndsWith("Block"))
                {
                    SyntaxNode firstChild = syntaxNode.ChildNodes().FirstOrDefault();
                    if (firstChild != null && SyntaxNodeWrapper.Get(firstChild).GetKind().EndsWith("Statement"))
                    {
                        return GetIdentifierTokenValueText(firstChild) == declarationFilter;
                    }
                }
            }

            return false;
        }

        private static string GetIdentifierTokenValueText(SyntaxNode syntaxNode)
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

        private class SyntaxFilter : IModelFilter
        {
            private readonly bool _tokens;
            private readonly bool _trivia;

            public SyntaxFilter(bool tokens, bool trivia)
            {
                _tokens = tokens;
                _trivia = trivia;
            }

            public bool Filter(object model)
            {
                SyntaxWrapper wrapper = (SyntaxWrapper)model;
                return !((wrapper.GetSyntaxObject() is SyntaxToken && !_tokens)
                    || (wrapper.GetSyntaxObject() is SyntaxTrivia && !_trivia));
            }
        }

        private class BorderlessToolStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Do nothing
            }
        }

        private static int GetDepth(SyntaxNodeOrToken syntax, int depth = 0)
        {
            if (syntax.IsNode)
            {
                return syntax.ChildNodesAndTokens().Count == 0 ? depth : syntax.ChildNodesAndTokens().Max(x => GetDepth(x, depth + 1));
            }
            return (syntax.AsToken().HasLeadingTrivia || syntax.AsToken().HasTrailingTrivia) ? depth + 1 : depth;
        }

        // From http://sourceforge.net/p/objectlistview/discussion/812922/thread/d2a643c1/?limit=25
        private static void AutoSizeColumns(TreeListView treeList, int depth, bool recalculate)
        {
            int totalWidth = 0;
            foreach (ColumnHeader col in treeList.Columns)
            {
                if (recalculate)
                {
                    col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    col.Width += 16;
                    if (col.Index == 0)
                    {
                        // We have to manually take care of tree structure, checkbox and image
                        col.Width += 16 * depth;
                    }
                }
                if (col.Index == treeList.Columns.Count - 1)
                {
                    // Fill in the rest
                    col.Width = treeList.ClientSize.Width - totalWidth;
                }
                totalWidth += col.Width;
            }
        }
    }
}
