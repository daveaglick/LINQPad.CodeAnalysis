# LINQPad.CodeAnalysis
Because it is so low ceremony but also has advanced functionality like debugging, data source connections, and advanced output and visulaization, LINQPad provides an ideal platform for quickly experimenting, exploring, and working with the .NET Compiler Platform. This library adds various capabilities to LINQPad that make working with the .NET Compiler Platform easier.

## Installation

**Please note that at this time, `LINQPad.CodeAnalysis` requires LINQPad version 4.56.04 or better, which is a beta release.**

### Install via NuGet

If you want to use LINQPad.CodeAnalysis on a query-by-query basis, it can be installed via NuGet:
- Open the LINQPad NuGet Manager
  - Right-click your query and select `Query Properties...`
  - Select `Add NuGet...`
- Perform a search for `LINQPad.CodeAnalysis`
  - Note that the `LINQPad.CodeAnalysis` package is prerelease because the `Microsoft.CodeAnalysis` packages on which it depends are also prerelease.
- Install it to the LINQPad NuGet cache and add it to your query by selecting `Add To Query`
- The capabilities of `LINQPad.CodeAnalysis` will now be available to your query.

Note that NuGet support is only available in LINQPad Developer and LINQPad Premium editions.

### Install as a Plugin

LINQPad also supports plugins, which are typically installed to `My Documents\LINQPad Plugins`. To install `LINQPad.CodeAnalysis` as a plugin and make it available to any query from your installation of LINQPad:
- Download the latest release as a .zip file from the GitHub Releases at https://github.com/somedave/LINQPad.CodeAnalysis/releases
- Unzip the archive into your `My Documents\LINQPad Plugins` folder.

## Syntax Tree

The first feature is a syntax tree visualizer similar to the one available for Visual Studio 2015. It allows you to dump a syntax tree for your current query, other queries, or generated directly via the .NET Compiler Platform.

### From Your Query

Dumping a .NET Compiler Platform syntax tree from your current query is easy. Use `CodeAnalysisUtil.DumpSyntaxTree()` inside your query. This will get the text of the currently executing query, generate an appropriate .NET Compiler Platform `SyntaxTree`, and dump it to an output tab.

### From Another Query

You can output a syntax tree from any other query as well (for example, a query returned by a call to `Util.GetMyQueries()`). Use the static method `CodeAnalysisUtil.DumpSyntaxTree(query)` or the extension method `query.DumpSyntaxTree()`.

### From a `SyntaxTree`

If you construct a `SyntaxTree` directly by using the .NET Compiler Platform, it can also be dumped to an output tab. Use the static method `CodeAnalysisUtil.DumpSyntaxTree(syntaxTree)` or the extension method `syntaxTree.DumpSyntaxTree()`.

### Dumping a Syntax Tree Node

The syntax tree visualization shows all of the nodes in the syntax tree. Double-clicking on one of the nodes will result in that node being dumped as a class in a seperate output tab. This makes it very easy to explore the various properties of the nodes in the syntax tree.

### Filtering by Node Type

The syntax tree shows `SyntaxNode` nodes (in blue), `SyntaxToken` nodes (in green), and `SyntaxTrivia` nodes (in maroon). Because they sometimes add noise when you're looking for specific patterns or syntax, you can toggle filtering out `SyntaxToken` nodes, `SyntaxTrivia` nodes, or both. To do so, click the check boxes next to these node types in the syntax tree tool bar.

### Declaration Filter

It may be difficult to find the exact syntax tree node you're looking for (be it a class, method, property, etc.) Decalaration filtering allows you to limit the syntax tree to just those nodes that represent a declaration with a given name. For example, consider the following code.

```
public class A
{
  public int X { get; set; }
}

public class B
{
  public int Y { get; set; }
  public int A { get; set; }
}
```

If you set a declaration filter of `B`, the syntax tree will contain the node for class `B`. If you set a declaration filter of `A`, the syntax tree will contain two root nodes, one for class `A` and another for property `B.A`.

This can be useful when you want to isolate the syntax tree for a particular portion of your query. For example, you may put the bit that you care about in a seperate method and then set a declaration filter that only outputs the syntax tree for that method.

All of the `DumpSyntaxTree()` methods accept an initial declaration filter. You can also specify one within the interface in the tool bar.

## What's Next?

To a large extent, that's up to you! If you have any ideas for helpful tools to make working with the .NET Compiler Platform from LINQPad easier, just let me know by opening a new issue. Some ideas I have:
- Even more syntax tree functionality such as round-trip code review and graph visualization.
- Some kind of support for writing and/or applying diagnostics and code fixes.
- Support for the new .NET Compiler Platform scripting capabilities (such as outputting code that packages a LINQPad query as a script).
