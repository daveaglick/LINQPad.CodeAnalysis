using BrightIdeasSoftware;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Color = System.Drawing.Color;
using Graph = Microsoft.Msagl.Drawing.Graph;
using Orientation = System.Windows.Forms.Orientation;

namespace LINQPad.CodeAnalysis
{
    internal class SyntaxTreePanel : TableLayoutPanel
    {
        private readonly SyntaxTree _syntaxTree;
        private SemanticModel _semanticModel;
        private bool _generatedSemanticModel;

        // Controls must be initialized in this order
        private TextBox _textBox;
        private GViewer _graphViewer;
        private TreeListView _treeList;
        private ToolStrip _toolStrip;
        private CheckBox _syntaxTokenCheckBox;
        private CheckBox _syntaxTriviaCheckBox;
        private CheckBox _semanticsCheckBox;
        private ToolStripButton _diagnosticsButton;

        public SyntaxTreePanel(SyntaxTree syntaxTree, string declarationFilter)
        {
            _syntaxTree = syntaxTree;

            // Controls
            CreateTextBox();
            CreateGraphViewer();
            CreateTreeList(declarationFilter);
            CreateToolStrip(declarationFilter);

            // Right-hand splitter
            SplitContainer rightSplitContainer = new SplitContainer()
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };
            rightSplitContainer.Panel1.Controls.Add(_textBox);
            rightSplitContainer.Panel2.Controls.Add(_graphViewer);
            bool rightSplitVisibleChanged = false;
            rightSplitContainer.VisibleChanged += (x, e) =>
            {
                if (!rightSplitVisibleChanged)
                {
                    rightSplitVisibleChanged = true;
                    rightSplitContainer.SplitterDistance = (int)(rightSplitContainer.ClientSize.Height * 0.5);
                }
            };

            // Top-level splitter
            SplitContainer splitContainer = new SplitContainer()
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel1.Controls.Add(_treeList);
            splitContainer.Panel2.Controls.Add(rightSplitContainer);
            bool splitVisibleChanged = false;
            splitContainer.VisibleChanged += (x, e) =>
            {
                if (!splitVisibleChanged)
                {
                    splitVisibleChanged = true;
                    splitContainer.SplitterDistance = (int)(splitContainer.ClientSize.Width * 0.5);
                }
            };
            Controls.Add(_toolStrip, 0, 0);
            Controls.Add(splitContainer, 0, 1);
        }

        private void CreateTextBox()
        {
            _textBox = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = InternalsHelper.GetDefaultFont()
            };
        }

        private void CreateGraphViewer()
        {
            _graphViewer = new GViewer
            {
                Dock = DockStyle.Fill, 
                OutsideAreaBrush = Brushes.White,
                LayoutEditingEnabled = false, 
                LayoutAlgorithmSettingsButtonVisible = false, 
                EdgeInsertButtonVisible = false,
                SaveAsMsaglEnabled = false,
                UndoRedoButtonsVisible = false
            };
        }

        private void CreateTreeList(string declarationFilter)
        {
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

            // Select - show the text
            _treeList.SelectedIndexChanged += (x, e) =>
            {
                if (_treeList.SelectedItem != null)
                {
                    SyntaxWrapper wrapper = (SyntaxWrapper)_treeList.SelectedItem.RowObject;
                    _textBox.Text = wrapper.GetSyntaxObject().ToString();
                    PopulateGraph(wrapper);
                }
            };

            // Activate - dump the syntax object
            _treeList.ItemActivate += (x, e) =>
            {
                if (_treeList.SelectedItem != null)
                {
                    SyntaxWrapper wrapper = (SyntaxWrapper)_treeList.SelectedItem.RowObject;
                    wrapper.GetSyntaxObject().Dump(wrapper.GetKind() + " " + wrapper.GetSpan());
                }
            };

            // Context menus - dump semantic nodes
            _treeList.CellRightClick += (x, e) =>
            {
                e.MenuStrip = null;
                if (_treeList.SelectedItem != null)
                {
                    SyntaxNodeWrapper wrapper = _treeList.SelectedItem.RowObject as SyntaxNodeWrapper;
                    if (wrapper != null)
                    {
                        ContextMenuStrip menuStrip = new ContextMenuStrip();
                        SyntaxNode syntaxNode = (SyntaxNode) wrapper.GetSyntaxObject();

                        // Dump Symbol
                        ISymbol symbol = GetSymbol(syntaxNode);
                        if (symbol != null)
                        {
                            menuStrip.Items.Add("Dump Symbol").Click += 
                                (x2, e2) => symbol.Dump(symbol.Kind + " " + wrapper.GetSpan());
                        }

                        // Dump TypeSymbol
                        ITypeSymbol typeSymbol = GetTypeSymbol(syntaxNode);
                        if (typeSymbol != null)
                        {
                            menuStrip.Items.Add("Dump TypeSymbol").Click +=
                                (x2, e2) => typeSymbol.Dump(typeSymbol.Kind + " " + wrapper.GetSpan());
                        }

                        // Dump Converted TypeSymbol
                        ITypeSymbol convertedTypeSymbol = GetConvertedTypeSymbol(syntaxNode);
                        if (convertedTypeSymbol != null)
                        {
                            menuStrip.Items.Add("Dump Converted TypeSymbol").Click +=
                                (x2, e2) => convertedTypeSymbol.Dump(convertedTypeSymbol.Kind + " " + wrapper.GetSpan());
                        }

                        // Dump AliasSymbol
                        IAliasSymbol aliasSymbol = GetAliasSymbol(syntaxNode);
                        if (aliasSymbol != null)
                        {
                            menuStrip.Items.Add("Dump AliasSymbol").Click +=
                                (x2, e2) => aliasSymbol.Dump(aliasSymbol.Kind + " " + wrapper.GetSpan());
                        }

                        // Dump ContantValue
                        object constantValue;
                        if (GetConstantValue(syntaxNode, out constantValue))
                        {
                            menuStrip.Items.Add("Show Constant Value").Click +=
                                (x2, e2) => MessageBox.Show(constantValue.ToString(), constantValue.GetType().Name + " " + wrapper.GetSpan(), MessageBoxButtons.OK);
                        }

                        // Add the menu strip if any items were added
                        if (menuStrip.Items.Count > 0)
                        {
                            e.MenuStrip = menuStrip;
                        }
                    }
                }
            };

            // Create columns
            _treeList.Columns.Add(new OLVColumn("Kind", null)
            {
                AspectGetter = x => x,
                AspectToStringConverter = x => ((SyntaxWrapper)x).GetTreeText()
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
            if (_syntaxTree.TryGetRoot(out root))
            {
                SetRoots(declarationFilter);
                depth = GetDepth(root);
            }

            // Calculate control width
            AutoSizeColumns(_treeList, depth, true);
            _treeList.Layout += (x, e) => AutoSizeColumns(_treeList, depth, false);
        }

        private void CreateToolStrip(string declarationFilter)
        {
            // Syntax and trivia toggles
            _syntaxTokenCheckBox = new CheckBox()
            {
                BackColor = Color.Transparent,
                Checked = true,
            };
            _syntaxTriviaCheckBox = new CheckBox()
            {
                BackColor = Color.Transparent,
                Checked = true,
            };

            bool handleChecked = true;	// Prevent double handling from adjustments during another handler
            _syntaxTokenCheckBox.CheckedChanged += (x, e) =>
            {
                if (handleChecked)
                {
                    if (!_syntaxTokenCheckBox.Checked)
                    {
                        handleChecked = false;
                        _syntaxTriviaCheckBox.Checked = false;
                        handleChecked = true;
                    }
                    if (_syntaxTokenCheckBox.Checked && _syntaxTriviaCheckBox.Checked)
                    {
                        _treeList.ModelFilter = null;
                    }
                    _treeList.ModelFilter = new SyntaxFilter(_syntaxTokenCheckBox.Checked, _syntaxTriviaCheckBox.Checked);
                    PopulateGraph();
                }
            };
            _syntaxTriviaCheckBox.CheckedChanged += (x, e) =>
            {
                if (handleChecked)
                {
                    if (!_syntaxTokenCheckBox.Checked)
                    {
                        handleChecked = false;
                        _syntaxTokenCheckBox.Checked = true;
                        handleChecked = true;
                    }
                    if (_syntaxTokenCheckBox.Checked && _syntaxTriviaCheckBox.Checked)
                    {
                        _treeList.ModelFilter = null;
                    }
                    _treeList.ModelFilter = new SyntaxFilter(_syntaxTokenCheckBox.Checked, _syntaxTriviaCheckBox.Checked);
                    PopulateGraph();
                }
            };

            // Semantics toggle and diagnostics button
            _semanticsCheckBox = new CheckBox()
            {
                BackColor = Color.Transparent
            };
            _semanticsCheckBox.CheckedChanged += (x, e) =>
            {
                GetSemanticModel();
            };
            _diagnosticsButton = new ToolStripButton("Dump Diagnostics", null, (x, e) =>
            {
                SemanticModel semanticModel = GetSemanticModel();
                if (semanticModel != null)
                {
                    // Exclude duplicate mscorlib reference warnings (referenced in LINQPad.Codeanalysis because ObjectListView is .NET 2.0)
                    semanticModel.GetDiagnostics()
                        .Where(y => y.Id != "CS1701" && !y.Descriptor.Description.ToString().Contains("mscorlib"))
                        .Dump();
                }
            });

            // Declaration filter
            ToolStripTextBox declarationFilterTextBox = new ToolStripTextBox();
            if(!string.IsNullOrWhiteSpace(declarationFilter))
            {
                declarationFilterTextBox.Text = declarationFilter;
            }
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
            _toolStrip = new ToolStrip(
                new ToolStripButton("Expand All", null, (x, e) => _treeList.ExpandAll()),
                new ToolStripButton("Collapse All", null, (x, e) => _treeList.CollapseAll()),
                new ToolStripSeparator(),
                new ToolStripLabel("SyntaxNode  ")
                {
                    ForeColor = Color.Blue
                },
                new ToolStripControlHost(_syntaxTokenCheckBox),
                new ToolStripLabel("SyntaxToken  ")
                {
                    ForeColor = Color.Green
                },
                new ToolStripControlHost(_syntaxTriviaCheckBox),
                new ToolStripLabel("SyntaxTrivia  ")
                {
                    ForeColor = Color.Maroon
                },
                new ToolStripSeparator(),
                new ToolStripLabel("Declaration Filter"),
                declarationFilterTextBox,
                new ToolStripSeparator(),
                new ToolStripLabel(" "),
                new ToolStripControlHost(_semanticsCheckBox),
                new ToolStripLabel("Semantics  "),
                _diagnosticsButton)
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Renderer = new BorderlessToolStripRenderer(),
                Padding = new Padding(4)
            };
            _toolStrip.Layout += (x, e) => _toolStrip.Width = _toolStrip.Parent.Width;
        }

        public void SetRoots(string declarationFilter)
        {
            _textBox.Text = string.Empty;
            PopulateGraph(null);
            SyntaxWrapper[] roots = new SyntaxTreeDeclarationFilter(declarationFilter)
                .GetMatchingSyntaxNodes(_syntaxTree)
                .Select(x => SyntaxWrapper.Get(x))
                .ToArray();
            _treeList.Roots = roots;
            if(roots.Length != 0)
            {
                _treeList.ExpandAll();
                _textBox.Text = string.Join(
                    Environment.NewLine + Environment.NewLine + "-" + Environment.NewLine + Environment.NewLine,
                    roots.Select(x => x.GetSyntaxObject().ToString()));
                PopulateGraph();
            }
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

        // This populates the graph using the current roots of the tree list
        private void PopulateGraph()
        {
            SyntaxWrapper[] roots = _treeList.Roots == null ? null : _treeList.Roots.Cast<SyntaxWrapper>().ToArray();
            if (roots == null || roots.Length == 0)
            {
                PopulateGraph(null);
                return;
            }

            _graphViewer.SuspendLayout();
            Graph graph = new Graph();
            int id = 0;
            foreach (SyntaxWrapper root in roots)
            {
                id = PopulateGraph(graph, root, id, null) + 1;
            }
            _graphViewer.Graph = graph;
            _graphViewer.ResumeLayout();
        }

        // This populates the graph using a given root of the tree
        // If the root is null, then the graph is cleared
        private void PopulateGraph(SyntaxWrapper wrapper)
        {
            _graphViewer.SuspendLayout();
            Graph graph = new Graph();
            if (wrapper != null)
            {
                PopulateGraph(graph, wrapper, 0, null);
            }
            _graphViewer.Graph = graph;
            _graphViewer.ResumeLayout();
        }

        // Returns the last used id
        private int PopulateGraph(Graph graph, SyntaxWrapper wrapper, int id, string parentId)
        {
            // Check if filtering
            if ((wrapper.GetSyntaxObject() is SyntaxToken && _syntaxTokenCheckBox != null && !_syntaxTokenCheckBox.Checked)
                  || (wrapper.GetSyntaxObject() is SyntaxTrivia && _syntaxTriviaCheckBox != null && !_syntaxTriviaCheckBox.Checked))
            {
                return id;
            }

            // Create the node
            string nodeId = id.ToString();
            Node node = new Node(nodeId);
            Color color = wrapper.GetColor();
            node.Attr.FillColor = new Microsoft.Msagl.Drawing.Color(color.R, color.G, color.B);
            node.Attr.LabelMargin = 10;
            node.LabelText = wrapper.GetGraphText();
            node.Label.FontColor = Microsoft.Msagl.Drawing.Color.White;
            node.Label.FontStyle = (Microsoft.Msagl.Drawing.FontStyle)(int) wrapper.GetGraphFontStyle();
            graph.AddNode(node);

            // Add the edge
            if (parentId != null)
            {
                graph.AddEdge(parentId, nodeId);
            }

            // Descend
            IEnumerable children = wrapper.GetChildren();
            if (children != null)
            {
                // Note that we have to iterate children in reverse order to get them to layout in the correct order
                foreach (SyntaxWrapper childWrapper in children.Cast<SyntaxWrapper>().Reverse())
                {
                    id = PopulateGraph(graph, childWrapper, id + 1, nodeId);
                }
            }
            return id;
        }

        // Gets the semantic model and caches it
        // TODO: Write tests to check getting a semantic model against all the different query types - make sure the diagnostics generated are appropriate
        private SemanticModel GetSemanticModel()
        {
            if (!_generatedSemanticModel)
            {
                // Generate the semantic model
                Compilation compilation = null;
                if (_syntaxTree is CSharpSyntaxTree)
                {
                    CSharpCompilationOptions options =
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                    compilation = CSharpCompilation.Create("QueryCompilation").WithOptions(options);
                }
                else if (_syntaxTree is VisualBasicSyntaxTree)
                {
                    VisualBasicCompilationOptions options =
                        new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                    compilation =
                        VisualBasicCompilation.Create("QueryCompilation").WithOptions(options);
                }
                if (compilation != null)
                {
                    // Get assembly references from the current AppDomain (which will be the domain of the currently running query)
                    // Make sure to exclude empty locations (created by in-memory assemblies, specifically AsyncBridge)
                    // See http://stackoverflow.com/questions/28503569/roslyn-create-metadatareference-from-in-memory-assembly
                    AppDomain appDomain = AppDomain.CurrentDomain;  
                    compilation = compilation
                        .AddReferences(appDomain.GetAssemblies()
                            .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                            .Select(MetadataReference.CreateFromAssembly))
                        .AddSyntaxTrees(_syntaxTree);
                    _semanticModel = compilation.GetSemanticModel(_syntaxTree);
                }
                _generatedSemanticModel = true;

                // Update the UI
                _semanticsCheckBox.Checked = true;
                _semanticsCheckBox.Enabled = false;
            }
            return _semanticModel;
        }

        private ISymbol GetSymbol(SyntaxNode syntaxNode)
        {
            ISymbol symbol = null;
            SemanticModel semanticModel = GetSemanticModel();
            if (semanticModel != null)
            {
                symbol = semanticModel.GetSymbolInfo(syntaxNode).Symbol;
                if (symbol == null)
                {
                    symbol = semanticModel.GetDeclaredSymbol(syntaxNode);
                }
                if (symbol == null)
                {
                    symbol = semanticModel.GetPreprocessingSymbolInfo(syntaxNode).Symbol;
                }
            }
            return symbol;
        }

        private ITypeSymbol GetTypeSymbol(SyntaxNode syntaxNode)
        {
            SemanticModel semanticModel = GetSemanticModel();
            return semanticModel != null ? semanticModel.GetTypeInfo(syntaxNode).Type : null;
        }

        private ITypeSymbol GetConvertedTypeSymbol(SyntaxNode syntaxNode)
        {
            SemanticModel semanticModel = GetSemanticModel();
            return semanticModel != null ? semanticModel.GetTypeInfo(syntaxNode).ConvertedType : null;
        }

        private IAliasSymbol GetAliasSymbol(SyntaxNode syntaxNode)
        {
            SemanticModel semanticModel = GetSemanticModel();
            return semanticModel != null ? semanticModel.GetAliasInfo(syntaxNode) : null;
        }

        // Returns if a constant value is available
        private bool GetConstantValue(SyntaxNode syntaxNode, out object constantValue)
        {
            constantValue = null;
            SemanticModel semanticModel = GetSemanticModel();
            if (semanticModel != null)
            {
                Microsoft.CodeAnalysis.Optional<object> optional = semanticModel.GetConstantValue(syntaxNode);
                if (optional.HasValue)
                {
                    constantValue = optional.Value;
                    return true;
                }
            }
            return false;
        }
    }
}
