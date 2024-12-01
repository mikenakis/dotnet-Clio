namespace Clio.Codecs;

using SysGlob = System.Globalization;
using Sys = System;

public sealed class IntCodec : StructCodec<int>
{
	public static readonly StructCodec<int> Instance = new IntCodec();

	IntCodec()
	{ }

	public override string Name => "integer";

	public override int ValueFromText( string text )
	{
		if( !int.TryParse( text, SysGlob.CultureInfo.InvariantCulture, out int value ) )
			throw new Sys.FormatException( $"Expected an integer, found '{text}'" );
		return value;
	}

	public override string TextFromValue( int value )
	{
		return value.ToString( SysGlob.CultureInfo.InvariantCulture );
	}
}
