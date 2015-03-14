<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference Prerelease="true">Microsoft.CodeAnalysis.Compilers</NuGetReference>
  <NuGetReference>ObjectListView.Official</NuGetReference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>BrightIdeasSoftware</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
	SyntaxTree st = CSharpSyntaxTree.ParseText(
@"using System;
using System.Collections;
using System.Linq;
using System.Text;
namespace HelloWorld
{
	// This is the program
	class Program
	{
		static void Main(string[] args)
		{
			// Write the line
			Console.WriteLine(""Hello, World!"");
		}
	}
}");
	//st.DumpSyntaxTree();
	SyntaxTree queryTree = CSharpSyntaxTree.ParseText(Util.CurrentQuery.Text);
	queryTree.DumpSyntaxTree();
}

abstract class SyntaxWrapper
{
	public static SyntaxWrapper Get(object syntax)
	{
		if(syntax is SyntaxNode)
		{
			return new SyntaxNodeWrapper((SyntaxNode)syntax);
		}
		if(syntax is SyntaxToken)
		{
			return new SyntaxTokenWrapper((SyntaxToken)syntax);
		}
		if(syntax is SyntaxTrivia)
		{
			return new SyntaxTriviaWrapper((SyntaxTrivia)syntax);
		}
		if(syntax is SyntaxNodeOrToken)
		{
			if(((SyntaxNodeOrToken)syntax).IsNode)
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
	
	public abstract void FormatCell(FormatCellEventArgs format);
	public abstract string GetKind();
	public abstract string GetSpan();
	public abstract string GetText();
}

class SyntaxNodeWrapper : SyntaxWrapper
{
	private readonly SyntaxNode _node;
	
	public SyntaxNodeWrapper(SyntaxNode node)
	{
		_node = node;
	}
	
	public override object GetSyntaxObject()
	{
		return _node;
	}
	
	public override bool CanExpand()
	{
		return _node.ChildNodesAndTokens().Count > 0;
	}
	
	public override IEnumerable GetChildren()
	{
		return _node.ChildNodesAndTokens().Select(x => SyntaxWrapper.Get(x));
	}
	
	public override void FormatCell(FormatCellEventArgs format)
	{
		if(format.Column.Text == "Kind")
		{
			format.SubItem.ForeColor = Color.Blue;
		}
	}
	
	public override string GetKind()
	{
		return _node.Kind().ToString();
	}
	
	public override string GetSpan()
	{
		return _node.FullSpan.ToString();
	}
	
	public override string GetText()
	{
		return string.Empty;
	}
}

class SyntaxTokenWrapper : SyntaxWrapper
{
	private readonly SyntaxToken _token;
	
	public SyntaxTokenWrapper(SyntaxToken token)
	{
		_token = token;
	}
	
	public override object GetSyntaxObject()
	{
		return _token;
	}
	
	public override bool CanExpand()
	{
		return _token.HasLeadingTrivia || _token.HasTrailingTrivia;
	}
	
	public override IEnumerable GetChildren()
	{
		return _token.LeadingTrivia
			.Concat(_token.TrailingTrivia)
			.Select(x => SyntaxWrapper.Get(x));
	}
	
	public override void FormatCell(FormatCellEventArgs format)
	{
		if(format.Column.Text == "Kind")
		{
			format.SubItem.ForeColor = Color.Green;
		}
	}
	
	public override string GetKind()
	{
		return _token.Kind().ToString();
	}
	
	public override string GetSpan()
	{
		return _token.FullSpan.ToString();
	}
	
	public override string GetText()
	{
		return _token.Text;
	}
}

class SyntaxTriviaWrapper : SyntaxWrapper
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
		if(format.Column.Text == "Kind")
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

// Define other methods and classes here
static class SyntaxTreeExtensions
{
	public static void DumpSyntaxTree(this SyntaxTree syntaxTree)
	{
		DumpSyntaxTree(syntaxTree, null);
	}
	
	public static void DumpSyntaxTree(this SyntaxTree syntaxTree, string description)
	{		
		// Create the tree view
		TreeListView treeList = new TreeListView()
		{
			CanExpandGetter = x => ((SyntaxWrapper)x).CanExpand(),
			ChildrenGetter = x => ((SyntaxWrapper)x).GetChildren(),
			UseCellFormatEvents = true
		};
		treeList.FormatCell += (x, e) => ((SyntaxWrapper)e.CellValue).FormatCell(e);
		treeList.BeforeSorting += (x, e) => e.Canceled = true;
		
		// Handle activate (dump the syntax object)
		treeList.ItemActivate += (x, e) => {
			if(treeList.SelectedItem != null)
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
		if(syntaxTree.TryGetRoot(out root))
		{
			treeList.Roots = new [] { SyntaxWrapper.Get(root) };	
			depth = GetDepth(root);
			treeList.ExpandAll();
		}
		
		// Calculate control width
		AutoSizeColumns(treeList, depth, true);
		int lastWidth = treeList.Width;
		treeList.Layout += (x, e) => {
			if(treeList.Width != lastWidth)
			{
				lastWidth = treeList.Width;
				AutoSizeColumns(treeList, depth, false);
			}
		};
		
		// Create the panel
		OutputPanel panel = PanelManager.DisplayControl(treeList, description ?? "Syntax Tree");
		
		// Keep query running so I can debug
		// TODO: Remove this
		//Util.KeepRunning();
	}
	
	private static int GetDepth(SyntaxNodeOrToken syntax, int depth = 0)
	{
		if(syntax.IsNode)
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
			if(recalculate)
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