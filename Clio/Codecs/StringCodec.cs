namespace Clio.Codecs;

public sealed class StringCodec : ClassCodec<string>
{
	public static readonly ClassCodec<string> Instance = new StringCodec();

	StringCodec()
	{ }

	public override string Name => "string";
	public override string ValueFromText( string text ) => text;
	public override string TextFromValue( string value ) => value;
}
