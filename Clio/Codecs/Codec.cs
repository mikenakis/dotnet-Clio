namespace Clio.Codecs;

public abstract class RawCodec
{
	public abstract string Name { get; }
	public abstract object RawValueFromText( string text );
	public abstract string TextFromRawValue( object value );
}

public abstract class ClassCodec<T> : RawCodec where T : class
{
	public sealed override object RawValueFromText( string text ) => ValueFromText( text );
	public sealed override string TextFromRawValue( object value ) => TextFromValue( (T)value );
	public abstract T ValueFromText( string text );
	public abstract string TextFromValue( T value );
}

public abstract class StructCodec<T> : RawCodec where T : struct
{
	public sealed override object RawValueFromText( string text ) => ValueFromText( text );
	public sealed override string TextFromRawValue( object value ) => TextFromValue( (T)value );
	public abstract T ValueFromText( string text );
	public abstract string TextFromValue( T value );
}

