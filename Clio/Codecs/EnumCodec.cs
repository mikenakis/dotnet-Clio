namespace Clio.Codecs;

using Sys = System;

public sealed class EnumCodec<T> : StructCodec<T> where T : struct, Sys.Enum
{
	public static readonly StructCodec<T> Instance = new EnumCodec<T>();

	readonly Sys.Type enumType;

	EnumCodec()
			: this( typeof( T ) )
	{ }

	EnumCodec( Sys.Type enumType )
	{
		this.enumType = enumType;
	}

	public override string Name => enumType.Name;

	public override T ValueFromText( string text )
	{
		if( !Sys.Enum.TryParse( text, out T value ) )
			throw new Sys.FormatException( $"Expected one of ({string.Join( ", ", enumType.GetEnumNames() )}), found '{text}'" );
		return value;
	}

	public override string TextFromValue( T value ) => NotNull( enumType.GetEnumName( value ) );
}
