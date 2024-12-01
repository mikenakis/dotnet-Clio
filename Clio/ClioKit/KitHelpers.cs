namespace Clio.ClioKit;

using Sys = System;
using SysText = System.Text;
using SysThreading = System.Threading;
using LegacyCollections = System.Collections;
using SysCompiler = System.Runtime.CompilerServices;

public static class KitHelpers
{
	public static readonly SysThreading.ThreadLocal<bool> FailureTesting = new( false );

	public static string SafeToString( object? value )
	{
		SysText.StringBuilder stringBuilder = new();
		SafeToString( value, s => stringBuilder.Append( s ) );
		return stringBuilder.ToString();
	}

	public static void SafeToString( object? value, Sys.Action<string> textConsumer )
	{
		ISet<object> visitedObjects = new HashSet<object>( ReferenceEqualityComparer.Instance );
		recurse( value );
		return;

		void recurse( object? value )
		{
			if( value == null )
				textConsumer.Invoke( "null" );
			else if( value is char c )
				EscapeForCSharp( c, textConsumer );
			else if( value is string s )
				EscapeForCSharp( s, textConsumer );
			else if( value is LegacyCollections.IEnumerable enumerable )
				enumerable.Cast<object>().MakeString( recurse, "[ ", ", ", " ]", "[]", textConsumer );
			else if( value.GetType().IsValueType || visitedObjects.Add( value ) )
				textConsumer.Invoke( value.ToString() ?? "\u2620" ); //U+2620 Skull and Crossbones Unicode Character
			else
				textConsumer.Invoke( $"{value.GetType()}@{SysCompiler.RuntimeHelpers.GetHashCode( value )}" );
		}
	}

	public static void EscapeForCSharp( string content, Sys.Action<string> textConsumer ) => EscapeForCSharp( content, '"', textConsumer );

	public static void EscapeForCSharp( char content, Sys.Action<string> textConsumer ) => EscapeForCSharp( content.ToString(), '\'', textConsumer );

	public static string EscapeForCSharp( string content, char quote )
	{
		SysText.StringBuilder stringBuilder = new();
		EscapeForCSharp( content, quote, stringBuilder );
		return stringBuilder.ToString();
	}

	public static void EscapeForCSharp( string content, char quote, SysText.StringBuilder stringBuilder )
	{
		ScribeStringLiteral( quote, content, s => stringBuilder.Append( s ) );
	}

	public static void EscapeForCSharp( string content, char quote, Sys.Action<string> textConsumer )
	{
		ScribeStringLiteral( quote, content, textConsumer );
	}

	public static void ScribeStringLiteral( char quoteCharacter, string instance, Sys.Action<string> textConsumer )
	{
		textConsumer.Invoke( $"{quoteCharacter}" );
		foreach( char c in instance )
			if( c == quoteCharacter )
				textConsumer.Invoke( $"\\{c}" );
			else
				switch( c )
				{
					case '\t':
						textConsumer.Invoke( @"\t" );
						break;
					case '\r':
						textConsumer.Invoke( @"\r" );
						break;
					case '\n':
						textConsumer.Invoke( @"\n" );
						break;
					case '\\':
						textConsumer.Invoke( @"\\" );
						break;
					default:
						emitOtherCharacter( textConsumer, c );
						break;
				}
		textConsumer.Invoke( $"{quoteCharacter}" );
		return;

		static void emitOtherCharacter( Sys.Action<string> textConsumer, char c )
		{
			if( isPrintable( c ) )
				textConsumer.Invoke( c.ToString() );
			else if( c < 256 ) // no need to check for >= 0 because char is unsigned.
			{
				char c1 = digitFromNibble( c >> 4 );
				char c2 = digitFromNibble( c & 0x0f );
				textConsumer.Invoke( $"\\x{c1}{c2}" );
			}
			else
			{
				char c1 = digitFromNibble( c >> 12 & 0x0f );
				char c2 = digitFromNibble( c >> 8 & 0x0f );
				char c3 = digitFromNibble( c >> 4 & 0x0f );
				char c4 = digitFromNibble( c & 0x0f );
				textConsumer.Invoke( $"\\u{c1}{c2}{c3}{c4}" );
			}

			static bool isPrintable( char c ) => c is >= (char)32 and < (char)127;

			static char digitFromNibble( int nibble )
			{
				Assert( nibble is >= 0 and < 16 );
				return (char)((nibble >= 10 ? 'a' - 10 : '0') + nibble);
			}
		}
	}
}
