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
        public SyntaxTreePanel(SyntaxTree syntaxTree, string declarationFilter)
        {
            // Create the tree view
            TreeListView treeList = new TreeListView()
            {
                CanExpandGetter = x => ((SyntaxWrapper)x).CanExpand(),
                ChildrenGetter = x => ((SyntaxWrapper)x).GetChildren(),
                UseCellFormatEvents = true,
                Dock = DockStyle.Fill,
                UseFiltering = true
            };
            treeList.FormatCell += (x, e) => ((SyntaxWrapper)e.CellValue).FormatCell(e);
            treeList.BeforeSorting += (x, e) => e.Canceled = true;

            // Handle activate (dump the syntax object)
            treeList.ItemActivate += (x, e) =>
            {
                if (treeList.SelectedItem != null)
                {
                    SyntaxWrapper wrapper = (SyntaxWrapper)treeList.SelectedItem.RowObject;
                    wrapper.GetSyntaxObject().Dump(wrapper.GetKind() + " " + wrapper.GetSpan());
                }
            };

            // Create columns
            treeList.Columns.Add(new OLVColumn("Kind", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetKind()
            });
            treeList.Columns.Add(new OLVColumn("Span", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetSpan()
            });
            treeList.Columns.Add(new OLVColumn("Text", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetText()
            });

            // Set the root
            SyntaxNode root;
            int depth = 0;
            if (syntaxTree.TryGetRoot(out root))
            {
                treeList.Roots = GetRoots(syntaxTree, declarationFilter);
                depth = GetDepth(root);
                treeList.ExpandAll();
            }

            // Calculate control width
            AutoSizeColumns(treeList, depth, true);
            treeList.Layout += (x, e) => AutoSizeColumns(treeList, depth, false);            

            // Layout
            Controls.Add(CreateToolStrip(treeList), 0, 0);
            Controls.Add(treeList, 0, 1);
        }

        public object[] GetRoots(SyntaxTree syntaxTree, string declarationFilter)
        {
            SyntaxNode root;
            if (syntaxTree.TryGetRoot(out root))
            {
                // Just return the root if not filtering by declaration
                if(declarationFilter == null)
                {
                    return new[] { SyntaxWrapper.Get(root) };
                }

                // Filter by declaration
                return root.DescendantNodes(x => !SyntaxNodeMatchesDeclaration(x, declarationFilter))
                    .Where(x => SyntaxNodeMatchesDeclaration(x, declarationFilter))
                    .Select(x => SyntaxWrapper.Get(x))
                    .ToArray();
            }
            return null;
        }

        private bool SyntaxNodeMatchesDeclaration(SyntaxNode syntaxNode, string declarationFilter)
        {
            if(declarationFilter == null)
            {
                return true;
            }
            
            // This is a hack, but don't know any other way to check for identifiers across all syntax node types - YOLO!
            PropertyInfo identifierProperty = syntaxNode.GetType().GetProperty("Identifier", BindingFlags.Public | BindingFlags.Instance);
            if(identifierProperty != null)
            {
                object identifierToken = identifierProperty.GetValue(syntaxNode);
                return identifierToken != null && identifierToken is SyntaxToken && ((SyntaxToken)identifierToken).ValueText == declarationFilter;
            }

            return false;
        }

        public ToolStrip CreateToolStrip(TreeListView treeList)
        {
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
                        treeList.ModelFilter = null;
                    }
                    treeList.ModelFilter = new SyntaxFilter(syntaxTokenCheckBox.Checked, syntaxTriviaCheckBox.Checked);
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
                        treeList.ModelFilter = null;
                    }
                    treeList.ModelFilter = new SyntaxFilter(syntaxTokenCheckBox.Checked, syntaxTriviaCheckBox.Checked);
                }
            };
            ToolStrip toolStrip = new ToolStrip(
                new ToolStripButton("Expand All", null, (x, e) => treeList.ExpandAll()),
                new ToolStripButton("Collapse All", null, (x, e) => treeList.CollapseAll()),
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
                new ToolStripLabel("(Double-Click A Node To Dump)"))
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Renderer = new BorderlessToolStripRenderer(),
                Padding = new Padding(4)
            };
            toolStrip.Layout += (x, e) => toolStrip.Width = toolStrip.Parent.Width;

            return toolStrip;
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
