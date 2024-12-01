namespace Clio.ClioKit.Collections;

using System.Collections;
using SysDiag = System.Diagnostics;

/// A debugger type proxy for enumerables. Use with <see cref="SysDiag.DebuggerTypeProxyAttribute"/>.
// When troubleshooting why this is not working, try the following:
// - Uncheck "Tools" > "Options" > "Debugging" > "General" > "Show raw structure of objects in variables windows".
// - If working with aspDotNet, make sure you are running with full trust. (Partial trust can cause this to fail.)
// - Make sure this class is not nested. (Must be a public or internal class directly under its namespace.)
public class EnumerableDebugView
{
	readonly IEnumerable enumerable;

	//This constructor will be invoked by the debugger.
	public EnumerableDebugView( IEnumerable enumerable )
	{
		this.enumerable = enumerable;
	}

	[SysDiag.DebuggerBrowsable( SysDiag.DebuggerBrowsableState.RootHidden )]
#pragma warning disable CA1819 // Properties should not return arrays
	//This property will be invoked by the debugger.
	public object[] Items => enumerable.Cast<object>().ToArray();
#pragma warning restore CA1819 // Properties should not return arrays
}
