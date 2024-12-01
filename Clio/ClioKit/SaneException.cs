namespace Clio.ClioKit;

using System.Collections;
using Clio.ClioKit.Collections;
using Sys = System;
using SysDiag = System.Diagnostics;
using SysReflect = System.Reflection;

[SysDiag.DebuggerDisplay( "{GetType().Name,nq}: {Message,nq}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public abstract class SaneException : Sys.Exception, IEnumerable<(string, object?)>
{
	public override string Message => buildMessage();

	string buildMessage()
	{
		return getPublicDeclaredPropertyNamesAndValues( this ) //
				.Select( tuple => $"{tuple.name} = {KitHelpers.SafeToString( tuple.value )}" )
				.MakeString( "; " );
	}

	static IEnumerable<(string name, object? value)> getPublicDeclaredPropertyNamesAndValues( object self )
	{
		return self.GetType() //
			.GetMembers( SysReflect.BindingFlags.Public | SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.DeclaredOnly )
			.OfType<SysReflect.PropertyInfo>()
			.Select( propertyInfo => (propertyInfo.Name, propertyInfo.GetValue( self )) );
	}

	protected SaneException( Sys.Exception? cause )
			: base( null, cause )
	{ }

	protected SaneException()
			: base( null, null )
	{ }

	public IEnumerator<(string, object?)> GetEnumerator() => getPublicDeclaredPropertyNamesAndValues( this ).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
