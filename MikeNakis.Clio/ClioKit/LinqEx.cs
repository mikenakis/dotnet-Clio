namespace Clio.ClioKit;

using Sys = System;
using SysText = System.Text;

public static class LinqEx
{
	public static string MakeString( this IEnumerable<string> self, string delimiter ) => self.MakeString( "", delimiter, "", "" );

	public static string MakeString( this IEnumerable<string> self, string prefix, string delimiter, string suffix, string ifEmpty )
	{
		SysText.StringBuilder stringBuilder = new();
		Sys.Action<string> textConsumer = s => stringBuilder.Append( s );
		self.MakeString( textConsumer, prefix, delimiter, suffix, ifEmpty, textConsumer );
		return stringBuilder.ToString();
	}

	public static void MakeString<T>( this IEnumerable<T> self, Sys.Action<T> elementConsumer, string prefix, string delimiter, string suffix, string ifEmpty, Sys.Action<string> textConsumer )
	{
		bool first = true;
		foreach( T element in self )
		{
			if( first )
			{
				textConsumer.Invoke( prefix );
				first = false;
			}
			else
				textConsumer.Invoke( delimiter );
			elementConsumer.Invoke( element );
		}
		textConsumer.Invoke( first ? ifEmpty : suffix );
	}
}
