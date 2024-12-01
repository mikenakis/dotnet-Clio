namespace Clio.Arguments;

using Sys = System;
using SysText = System.Text;
using Clio.Codecs;
using Clio.Exceptions;
using Clio.ClioKit;

abstract class OptionArgument : NamedArgument
{
	internal OptionArgument( BaseArgumentParser argumentParser, string name, char? shortFormName, string? parameterName, string? description, bool isRequired )
			: base( argumentParser, name, shortFormName, description, isRequired )
	{
		Assert( Helpers.OptionNameIsValidAssertion( name ) );
		this.parameterName = parameterName;
	}

	readonly string? parameterName;
	string effectiveParameterName => $"<{parameterName ?? TypeName}>";
	private protected abstract string TypeName { get; }
	internal sealed override string ShortUsage => buildShortUsage();
	private protected abstract object? RawDefaultValue { get; }
	internal bool HasDefault => RawDefaultValue != null;
	private protected abstract object? RawPresetValue { get; }
	bool hasPreset => RawPresetValue != null;

	string buildShortUsage()
	{
		SysText.StringBuilder stringBuilder = new();
		if( SingleLetterName != null )
			stringBuilder.Append( '-' ).Append( SingleLetterName ).Append( ", " );
		stringBuilder.Append( "--" ).Append( Name );
		if( hasPreset )
			stringBuilder.Append( '[' );
		stringBuilder.Append( '=' ).Append( effectiveParameterName );
		if( hasPreset )
			stringBuilder.Append( ']' );
		return stringBuilder.ToString();
	}

	internal override void CollectLongUsageLines( Sys.Action<string> lineConsumer )
	{
		base.CollectLongUsageLines( lineConsumer );
		if( HasDefault )
			lineConsumer.Invoke( $"If omitted, the default is {KitHelpers.SafeToString( RawDefaultValue )}." );
		if( hasPreset )
			lineConsumer.Invoke( $"If supplied without a value, the preset is {KitHelpers.SafeToString( RawPresetValue )}." );
	}

	public sealed override int TryParse( int tokenIndex, IReadOnlyList<string> tokens )
	{
		string? remainder = TryParseNameAndGetRemainder( tokens[tokenIndex] );
		if( remainder == null )
			return tokenIndex;
		if( remainder.Length == 0 )
		{
			if( !hasPreset )
				throw new OptionRequiresValueException( Name );
			RealizePreset(); //value = presetValue;
		}
		else
		{
			if( remainder[0] != '=' )
				throw new UnexpectedCharactersAfterNamedArgumentException( Name, remainder );
			try
			{
				RealizeValue( remainder[1..] ); //value = codec.ValueFromString( stringValue );
			}
			catch( Sys.Exception exception )
			{
				throw new UnparsableValueException( Name, remainder[1..], exception );
			}
		}
		return tokenIndex + 1;
	}

	private protected abstract void RealizePreset();
	private protected abstract void RealizeValue( string stringValue );
}

sealed class NullableStructOption<T> : OptionArgument, IOptionArgument<T?> where T : struct
{
	readonly StructCodec<T> codec;
	private protected override string TypeName => codec.Name;
	public override object? RawValue => Value;
	private protected override object? RawDefaultValue => null;
	private protected override object? RawPresetValue => presetValue;
	public T? Value => (IsSupplied ? value ?? presetValue : default).WithAssertion( HasBeenParsedAssertion );
	readonly T? presetValue;
	T? value;
	private protected override void RealizePreset() => value = presetValue ?? throw new Sys.InvalidOperationException();
	private protected override void RealizeValue( string stringValue ) => value = codec.ValueFromText( stringValue );

	internal NullableStructOption( BaseArgumentParser argumentParser, string name, char? shortFormName, string? parameterName, //
		string? description, StructCodec<T> codec, T? presetValue )
			: base( argumentParser, name, shortFormName, parameterName, description, isRequired: false )
	{
		this.codec = codec;
		this.presetValue = presetValue;
	}
}

sealed class NullableClassOption<T> : OptionArgument, IOptionArgument<T?> where T : class
{
	readonly ClassCodec<T> codec;
	private protected override string TypeName => codec.Name;
	public override object? RawValue => Value;
	private protected override object? RawDefaultValue => null;
	private protected override object? RawPresetValue => presetValue;
	public T? Value => (IsSupplied ? value ?? presetValue : default).WithAssertion( HasBeenParsedAssertion );
	readonly T? presetValue;
	T? value;
	private protected override void RealizePreset() => value = presetValue ?? throw new Sys.InvalidOperationException();
	private protected override void RealizeValue( string stringValue ) => value = codec.ValueFromText( stringValue );

	internal NullableClassOption( BaseArgumentParser argumentParser, string name, char? shortFormName, string? parameterName, //
		string? description, ClassCodec<T> codec, T? presetValue )
			: base( argumentParser, name, shortFormName, parameterName, description, isRequired: false )
	{
		this.codec = codec;
		this.presetValue = presetValue;
	}
}

sealed class NonNullableStructOption<T> : OptionArgument, IOptionArgument<T> where T : struct
{
	readonly StructCodec<T> codec;
	private protected override string TypeName => codec.Name;
	public override object? RawValue => Value;
	private protected override object? RawDefaultValue => defaultValue;
	private protected override object? RawPresetValue => presetValue;
	public T Value => (value ?? defaultValue).WithAssertion( HasBeenParsedAssertion ) ?? throw new Sys.InvalidOperationException();
	readonly T? presetValue;
	readonly T? defaultValue;
	T? value;
	private protected override void RealizePreset() => value = presetValue ?? throw new Sys.InvalidOperationException();
	private protected override void RealizeValue( string stringValue ) => value = codec.ValueFromText( stringValue );

	public NonNullableStructOption( BaseArgumentParser argumentParser, string name, char? shortFormName, string? parameterName, StructCodec<T> codec, string? description, T? presetValue, T? defaultValue )
		: base( argumentParser, name, shortFormName, parameterName, description, isRequired: defaultValue is null )
	{
		this.codec = codec;
		this.presetValue = presetValue;
		this.defaultValue = defaultValue;
	}
}

sealed class NonNullableClassOption<T> : OptionArgument, IOptionArgument<T> where T : class
{
	readonly ClassCodec<T> codec;
	private protected override string TypeName => codec.Name;
	public override object? RawValue => Value;
	private protected override object? RawDefaultValue => defaultValue;
	private protected override object? RawPresetValue => presetValue;
	public T Value => (value ?? defaultValue).WithAssertion( HasBeenParsedAssertion ) ?? throw new Sys.InvalidOperationException();
	readonly T? presetValue;
	readonly T? defaultValue;
	T? value;
	private protected override void RealizePreset() => value = presetValue ?? throw new Sys.InvalidOperationException();
	private protected override void RealizeValue( string stringValue ) => value = codec.ValueFromText( stringValue );

	public NonNullableClassOption( BaseArgumentParser argumentParser, string name, char? shortFormName, string? parameterName, ClassCodec<T> codec, string? description, T? presetValue, T? defaultValue )
		: base( argumentParser, name, shortFormName, parameterName, description, isRequired: defaultValue is null )
	{
		this.codec = codec;
		this.presetValue = presetValue;
		this.defaultValue = defaultValue;
	}
}
