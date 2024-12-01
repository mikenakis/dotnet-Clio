namespace Clio;

using System.Collections.Generic;
using Clio.Arguments;
using Clio.Exceptions;
using Clio.Codecs;
using Sys = System;

/// <summary>Common base class for command-line parsers.</summary>
public abstract class BaseArgumentParser
{
	internal abstract BaseArgumentParser? Parent { get; }
	internal abstract ArgumentParser GetRootArgumentParser();
	internal bool HasBeenParsed { get; private set; }
	internal string GetFullName( char delimiter ) => Parent == null ? Name : $"{Parent.GetFullName( delimiter )}{delimiter}{Name}";

	/// <summary>The name of the program or verb.</summary>
	public string Name { get; }

	readonly List<Argument> arguments = new();
	internal IReadOnlyList<Argument> Arguments => arguments;

	protected BaseArgumentParser( string name )
	{
		Name = name;
	}

	///<summary>Adds a switch.</summary>
	///<remarks>A switch is a named argument without a parameter, e.g. <c>AcmeCli --verbose</c>. The value of a
	/// switch is of type <c>bool</c>, indicating whether the switch was supplied in the command-line or not.</remarks>
	///<param name="name">The name of the switch.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the switch.</param>
	///<param name="description">The description of the switch, for use when displaying help.</param>
	public ISwitchArgument AddSwitch( string name, char? singleLetterName = null, string? description = null )
	{
		Assert( Helpers.SwitchNameIsValidAssertion( name ) );
		return new SwitchArgument( this, name, singleLetterName, description );
	}

	///<summary>Adds an option.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between string and
	///the actual type of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is specified in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T?> AddOption<T>( string name, StructCodec<T> codec, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : struct
	{
		return new NullableStructOption<T>( this, name, singleLetterName, parameterName, description, codec, presetValue );
	}

	///<summary>Adds an option.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between string and
	///the actual type of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is specified in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T?> AddOption<T>( string name, ClassCodec<T> codec, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : class
	{
		return new NullableClassOption<T>( this, name, singleLetterName, parameterName, description, codec, presetValue );
	}

	///<summary>Adds an option with a default value.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between
	///<c>string</c> and the actual type of the option.</param>
	///<param name="defaultValue">The default value for the option, which will be the value of the option if the option
	///is not supplied in the command-line.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T> AddOptionWithDefault<T>( string name, StructCodec<T> codec, T defaultValue, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : struct
	{
		return new NonNullableStructOption<T>( this, name, singleLetterName, parameterName, codec, description, presetValue, defaultValue );
	}

	///<summary>Adds an option with a default value.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between
	///<c>string</c> and the actual type of the option.</param>
	///<param name="defaultValue">The default value for the option, which will be the value of the option if the option
	///is not supplied in the command-line.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T> AddOptionWithDefault<T>( string name, ClassCodec<T> codec, T defaultValue, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : class
	{
		return new NonNullableClassOption<T>( this, name, singleLetterName, parameterName, codec, description, presetValue, defaultValue );
	}

	///<summary>Adds a required option.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between
	///<c>string</c> and the actual type of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T> AddRequiredOption<T>( string name, StructCodec<T> codec, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : struct
	{
		return new NonNullableStructOption<T>( this, name, singleLetterName, parameterName, codec, description, presetValue, default );
	}

	///<summary>Adds a required option.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the option; specifies how to convert between
	///<c>string</c> and the actual type of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<T> AddRequiredOption<T>( string name, ClassCodec<T> codec, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, T? presetValue = default ) where T : class
	{
		return new NonNullableClassOption<T>( this, name, singleLetterName, parameterName, codec, description, presetValue, default );
	}

	///<summary>Adds an option of type <c>string</c>.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is specified in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<string?> AddStringOption( string name, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, string? presetValue = null )
	{
		return AddOption( name, StringCodec.Instance, singleLetterName, description, parameterName, presetValue );
	}

	///<summary>Adds an option of type <c>string</c> with a default value.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="defaultValue">The default value for the option, which will be the value of the option if the option
	///is not supplied in the command-line.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<string> AddStringOptionWithDefault( string name, string defaultValue, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, string? presetValue = null )
	{
		return AddOptionWithDefault( name, StringCodec.Instance, defaultValue, singleLetterName, description, parameterName, presetValue );
	}

	///<summary>Adds a required option of type <c>string</c>.</summary>
	///<param name="name">The name of the option.</param>
	///<param name="singleLetterName">The (optional) single-letter name for the option.</param>
	///<param name="description">The description of the option, for use when displaying help.</param>
	///<param name="parameterName">The name of the parameter of the option, for use when displaying help.</param>
	///<param name="presetValue">The (optional) preset value of the option, which will be the value of the option if the
	///option is supplied in the command-line without an equals-sign and a value.</param>
	public IOptionArgument<string> AddRequiredStringOption( string name, char? singleLetterName = null, //
		string? description = null, string? parameterName = null, string? presetValue = null )
	{
		return AddRequiredOption( name, StringCodec.Instance, singleLetterName, description, parameterName, presetValue );
	}

	///<summary>Adds a positional argument.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	public IPositionalArgument<T?> AddPositional<T>( string name, StructCodec<T> codec, string? description = null ) where T : struct
	{
		return new NullableStructPositionalArgument<T>( this, name, codec, description );
	}

	///<summary>Adds a positional argument.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	public IPositionalArgument<T?> AddPositional<T>( string name, ClassCodec<T> codec, string? description = null ) where T : class
	{
		return new NullableClassPositionalArgument<T>( this, name, codec, description );
	}

	///<summary>Adds a positional argument with a default value.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	///<param name="defaultValue">The default value for the positional argument, which will be the value of the argument
	///if the argument is not supplied in the command-line.</param>
	public IPositionalArgument<T> AddPositionalWithDefault<T>( string name, StructCodec<T> codec, T defaultValue, string? description = null ) where T : struct
	{
		return new NonNullableStructPositionalArgument<T>( this, name, codec, description, defaultValue );
	}

	///<summary>Adds a positional argument with a default value.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	///<param name="defaultValue">The default value for the positional argument, which will be the value of the argument
	///if the argument is not supplied in the command-line.</param>
	public IPositionalArgument<T> AddPositionalWithDefault<T>( string name, ClassCodec<T> codec, T defaultValue, string? description = null ) where T : class
	{
		return new NonNullableClassPositionalArgument<T>( this, name, codec, description, defaultValue );
	}

	///<summary>Adds a required positional argument.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the parameter, for use when displaying help.</param>
	public IPositionalArgument<T> AddRequiredPositional<T>( string name, StructCodec<T> codec, string? description = null ) where T : struct
	{
		return new NonNullableStructPositionalArgument<T>( this, name, codec, description, default );
	}

	///<summary>Adds a required positional argument.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="codec">The <see cref="Codec{T}"/> of the positional argument; provides conversions between
	///<c>string</c> and the type of the argument.</param>
	///<param name="description">The description of the parameter, for use when displaying help.</param>
	public IPositionalArgument<T> AddRequiredPositional<T>( string name, ClassCodec<T> codec, string? description = null ) where T : class
	{
		return new NonNullableClassPositionalArgument<T>( this, name, codec, description, default );
	}

	///<summary>Adds a positional argument of type <c>string</c>.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	public IPositionalArgument<string?> AddStringPositional( string name, string? description = null )
	{
		return AddPositional( name, StringCodec.Instance, description );
	}

	///<summary>Adds a positional argument of type <c>string</c> with a default value.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="defaultValue">The default value for the positional argument, which will be the value of the argument
	///if the argument is not supplied in the command-line.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	public IPositionalArgument<string> AddStringPositionalWithDefault( string name, string defaultValue, string? description = null )
	{
		return AddPositionalWithDefault( name, StringCodec.Instance, defaultValue, description );
	}

	///<summary>Adds a required positional argument of type <c>string</c>.</summary>
	///<param name="name">The name of the positional argument, for use when displaying help.</param>
	///<param name="description">The description of the positional argument, for use when displaying help.</param>
	public IPositionalArgument<string> AddRequiredStringPositional( string name, string? description = null )
	{
		return AddRequiredPositional( name, StringCodec.Instance, description );
	}

	internal void AddArgument( Argument argument )
	{
		Assert( !HasBeenParsed, () => throw new CommandLineHasAlreadyBeenParsedException() );
		if( helpSwitch == null && argument is not NamedArgument )
			addHelpSwitch();
		arguments.Add( argument );
	}

	internal void OutputHelp( int screenWidth )
	{
		Sys.Action<string> lineOutputConsumer = GetRootArgumentParser().LineOutputConsumer;
		string fullName = GetFullName( ' ' );
		HelpGenerator.OutputHelp( lineOutputConsumer, screenWidth, fullName, arguments, GetRootArgumentParser().VerbTerm );
		return;
	}

	ISwitchArgument? helpSwitch;

	void addHelpSwitch()
	{
		Assert( helpSwitch == null );
		helpSwitch = AddSwitch( "help", 'h', "Display this help" );
	}

	private protected void ParseRemainingTokens( int tokenIndex, IReadOnlyList<string> tokens )
	{
		Assert( !HasBeenParsed );
		if( helpSwitch == null )
			addHelpSwitch();
		HasBeenParsed = true;
		while( tokenIndex < tokens.Count )
		{
			int newTokenIndex = tryParseArgument( tokenIndex, tokens );
			if( newTokenIndex == tokenIndex )
				throw new UnexpectedTokenException( tokens[tokenIndex] );
			tokenIndex = newTokenIndex;
		}
		if( helpSwitch!.Value )
			throw new HelpException( this );
		reportAnyMissingRequiredArguments();
		reportMissingVerb();
		if( GetRootArgumentParser().OutputArgumentNamesAndValues && !arguments.Where( a => a is VerbArgument ).Any() )
			DumpArguments();
		return;

		int tryParseArgument( int tokenIndex, IReadOnlyList<string> tokens )
		{
			IEnumerable<Argument> orderedArguments = arguments.Where( a => a is NamedArgument ).Concat( arguments.Where( a => a is PositionalArgument ) ).Concat( arguments.Where( a => a is VerbArgument ) );
			Assert( orderedArguments.Count() == arguments.Count );
			foreach( Argument argument in orderedArguments )
			{
				int newTokenIndex = argument.TryParse( tokenIndex, tokens );
				if( newTokenIndex > tokenIndex )
					return newTokenIndex;
			}
			return tokenIndex;
		}

		void reportAnyMissingRequiredArguments()
		{
			foreach( Argument argument in arguments.Where( argument => argument.IsRequired && !argument.IsSupplied ) )
				throw new RequiredArgumentNotSuppliedException( argument.Name );
		}

		void reportMissingVerb()
		{
			Assert( arguments.OfType<VerbArgument>().ToArray(),
				verb => verb.Length == 0 || verb.Where( verb => verb.IsSupplied ).Any(),
				verb => throw new VerbExpectedException( GetRootArgumentParser().VerbTerm ) );
		}
	}

	internal void DumpArguments()
	{
		Parent?.DumpArguments();
		Sys.Action<string> lineOutputConsumer = GetRootArgumentParser().LineOutputConsumer;
		string parserName = getParserName( this );
		foreach( Argument argument in arguments )
			lineOutputConsumer.Invoke( $"{dotSeparated( parserName, argument.Name )} = {argument.ValueToString()}" );

		static string getParserName( BaseArgumentParser argumentParser )
		{
			if( argumentParser.Parent == null )
				return "";
			string parentName = getParserName( argumentParser.Parent );
			return dotSeparated( parentName, argumentParser.Name );
		}

		static string dotSeparated( string a, string b ) => a == "" ? b : $"{a}.{b}";
	}
}
