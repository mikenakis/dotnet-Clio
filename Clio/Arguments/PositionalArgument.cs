namespace Clio.Arguments;

using Sys = System;
using SysDiag = System.Diagnostics;
using Clio.Exceptions;
using Clio.Codecs;
using Clio.ClioKit;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
abstract class PositionalArgument : Argument
{
	bool supplied;
	public override bool IsSupplied => supplied;
	internal sealed override string ShortUsage => "<" + Name + ">";
	private protected abstract string TypeName { get; }

	internal PositionalArgument( BaseArgumentParser argumentParser, string name, string? description, bool isRequired )
		: base( argumentParser, name, description, isRequired )
	{
		Assert( Helpers.ParameterNameIsValidAssertion( name ) );
		//TODO: revise the usefulness of this.
		Assert( Helpers.ArgumentMustPrecedeVerbAssertion( argumentParser, name, argumentParser.GetRootArgumentParser().VerbTerm ) );
		//TODO: revise the usefulness of this.
		if( !IsOptional )
			Assert( argumentParser.Arguments.OfType<PositionalArgument>().Where( positionalArgument => positionalArgument.IsOptional ).FirstOrDefault(), //
				optionalPositionalArgument => optionalPositionalArgument == null, //
				optionalPositionalArgument => throw new InvalidArgumentOrderingException( ArgumentOrderingRule.RequiredPositionalMustPrecedeOptionalPositional, Name, optionalPositionalArgument!.Name ) );
	}

	public sealed override int TryParse( int tokenIndex, IReadOnlyList<string> tokens )
	{
		if( supplied )
			return tokenIndex;
		string valueToken = tokens[tokenIndex];
		if( valueToken[0] == '-' )
			return tokenIndex;
		supplied = true;
		try
		{
			RealizeValue( valueToken );
		}
		catch( Sys.Exception exception )
		{
			throw new UnparsableValueException( Name, valueToken, exception );
		}
		return tokenIndex + 1;
	}

	private protected abstract void RealizeValue( string valueToken );
}

sealed class NullableClassPositionalArgument<T> : PositionalArgument, IPositionalArgument<T?> where T : class
{
	readonly ClassCodec<T> codec;
	public override object? RawValue => Value;
	private protected override string TypeName => codec.Name;
	public T? Value => value.WithAssertion( HasBeenParsedAssertion );
	T? value;
	private protected override void RealizeValue( string valueToken ) => value = codec.ValueFromText( valueToken );

	internal NullableClassPositionalArgument( BaseArgumentParser argumentParser, string name, ClassCodec<T> codec, string? description )
		: base( argumentParser, name, description, isRequired: false )
	{
		this.codec = codec;
	}
}

sealed class NullableStructPositionalArgument<T> : PositionalArgument, IPositionalArgument<T?> where T : struct
{
	readonly StructCodec<T> codec;
	public override object? RawValue => Value;
	private protected override string TypeName => codec.Name;
	public T? Value => value.WithAssertion( HasBeenParsedAssertion );
	T? value;
	private protected override void RealizeValue( string valueToken ) => value = codec.ValueFromText( valueToken );

	internal NullableStructPositionalArgument( BaseArgumentParser argumentParser, string name, StructCodec<T> codec, string? description )
		: base( argumentParser, name, description, isRequired: false )
	{
		this.codec = codec;
	}
}

sealed class NonNullableClassPositionalArgument<T> : PositionalArgument, IPositionalArgument<T> where T : class
{
	readonly ClassCodec<T> codec;
	public override object? RawValue => Value;
	private protected override string TypeName => codec.Name;
	public T Value => (IsSupplied ? value : defaultValue).WithAssertion( HasBeenParsedAssertion ) ?? throw new Sys.InvalidOperationException();
	readonly T? defaultValue;
	T? value;
	private protected override void RealizeValue( string valueToken ) => value = codec.ValueFromText( valueToken );

	public NonNullableClassPositionalArgument( BaseArgumentParser argumentParser, string name, ClassCodec<T> codec, string? description, T? defaultValue )
		: base( argumentParser, name, description, isRequired: defaultValue is null )
	{
		this.codec = codec;
		this.defaultValue = defaultValue;
	}
}

sealed class NonNullableStructPositionalArgument<T> : PositionalArgument, IPositionalArgument<T> where T : struct
{
	readonly StructCodec<T> codec;
	public override object? RawValue => Value;
	private protected override string TypeName => codec.Name;
	public T Value => (IsSupplied ? value : defaultValue).WithAssertion( HasBeenParsedAssertion ) ?? throw new Sys.InvalidOperationException();
	readonly T? defaultValue;
	T? value;
	private protected override void RealizeValue( string valueToken ) => value = codec.ValueFromText( valueToken );

	public NonNullableStructPositionalArgument( BaseArgumentParser argumentParser, string name, StructCodec<T> codec, string? description, T? defaultValue )
		: base( argumentParser, name, description, isRequired: defaultValue is null )
	{
		this.codec = codec;
		this.defaultValue = defaultValue;
	}
}
