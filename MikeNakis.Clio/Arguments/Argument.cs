namespace Clio.Arguments;

using Sys = System;
using Clio.Exceptions;
using SysDiag = System.Diagnostics;
using static Clio.ClioKit.GlobalStatics;
using Clio.ClioKit;

/// <summary>Represents a command-line argument.</summary>
public interface IArgument
{
	///<summary>The name of the argument.</summary>
	string Name { get; }

	///<summary>The description of the argument.</summary>
	string? Description { get; }

	///<summary><c>true</c> if the argument is required; <c>false</c> otherwise.</summary>
	bool IsRequired { get; }

	///<summary><c>true</c> if the argument was supplied in the command-line; <c>false</c> otherwise.</summary>
	bool IsSupplied { get; }

	///<summary>The value of the argument as a nullable object.</summary>
	object? RawValue { get; }
}

/// <summary>Represents a command-line argument with a value.</summary>
public interface IArgument<T> : IArgument
{
	///<summary>The value of the argument.</summary>
	T Value { get; }
}

///<summary>Represents a switch argument.</summary>
///<remarks>A switch is a named argument without a parameter, e.g. <c>AcmeCli --verbose</c>. The value of a switch
///argument is of type <c>bool</c>, and it is <c>true</c> if the switch was supplied in the command-line, <c>false</c>
///otherwise.</remarks>
public interface ISwitchArgument : IArgument<bool>
{
	char? SingleLetterName { get; }
}

///<summary>Represents an option argument.</summary>
///<remarks>An option is a named argument with a parameter, e.g. <c>AcmeCli --verbosity quiet</c>.</remarks>
public interface IOptionArgument<T> : IArgument<T>
{
}

///<summary>Represents a positional argument.</summary>
///<remarks>A positional argument is a free-standing value in the command-line, identified by its position relative
///to other positional arguments. For example: <c>AcmeCli InputFile.txt</c>.</remarks>
public interface IPositionalArgument<T> : IArgument<T>
{
}

///<summary>Represents a verb argument.</summary>
///<remarks>A verb is a word followed by a new set of arguments. For example: <c>AcmeCli create ...</c>
///or <c>AcmeCli list ...</c>.</remarks>
public interface IVerbArgument : IArgument<bool>
{
}

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
abstract class Argument : IArgument
{
	public string Name { get; }
	private protected BaseArgumentParser ArgumentParser { get; }
	public abstract object? RawValue { get; }
	public string? Description { get; }
	public bool IsRequired { get; }
	internal bool IsOptional => !IsRequired;
	public abstract bool IsSupplied { get; }
	internal abstract string ShortUsage { get; }
	public abstract int TryParse( int tokenIndex, IReadOnlyList<string> tokens );
	public sealed override string ToString() => $"'{Name}' = {ValueToString()}";

	private protected Argument( BaseArgumentParser argumentParser, string name, string? description, bool isRequired )
	{
		Assert( !argumentParser.Arguments.Where( argument => argument.Name == name ).Any(), () => throw new DuplicateArgumentNameException( name ) );
		ArgumentParser = argumentParser;
		Name = name;
		Description = description;
		IsRequired = isRequired;
		ArgumentParser.AddArgument( this );
	}

	internal string ValueToString()
	{
		if( !ArgumentParser.HasBeenParsed )
			return "not yet parsed";
		return KitHelpers.SafeToString( RawValue );
	}

	internal virtual void CollectLongUsageLines( Sys.Action<string> lineConsumer )
	{
		lineConsumer.Invoke( $"{Description ?? Helpers.DefaultDescription}." );
	}

	private protected bool HasBeenParsedAssertion()
	{
		Assert( ArgumentParser.HasBeenParsed, () => throw new CommandLineHasNotBeenParsedException() );
		return true;
	}
}

abstract class NamedArgument : Argument
{
	public char? SingleLetterName { get; }
	bool supplied;
	public override bool IsSupplied => supplied;

	private protected NamedArgument( BaseArgumentParser argumentParser, string name, char? singleLetterName, string? description, bool isRequired )
			: base( argumentParser, name, description, isRequired )
	{
		//TODO: revise the usefulness of this.
		Assert( Helpers.ArgumentMustPrecedeVerbAssertion( argumentParser, name, argumentParser.GetRootArgumentParser().VerbTerm ) );
		//TODO: revise the usefulness of this.
		Assert( argumentParser.Arguments.OfType<PositionalArgument>().FirstOrDefault(), //
			positionalArgument => positionalArgument == null, //
			positionalArgument => throw new InvalidArgumentOrderingException( ArgumentOrderingRule.NamedArgumentMustPrecedePositional, name, positionalArgument!.Name ) );
		if( singleLetterName != null )
			Assert( argumentParser.Arguments.OfType<NamedArgument>().FirstOrDefault( existingArgument => existingArgument.SingleLetterName == singleLetterName ), //
				existingArgument => existingArgument == null, //
				existingArgument => throw new DuplicateArgumentSingleLetterNameException( singleLetterName.Value ) );
		SingleLetterName = singleLetterName;
	}

	protected string? TryParseNameAndGetRemainder( string token )
	{
		int skip = shortFormNameMatch( token, SingleLetterName );
		if( skip == 0 )
			skip = longFormNameMatch( token, Name );
		if( skip == 0 )
			return null;
		if( supplied )
			throw new ArgumentSuppliedMoreThanOnceException( Name );
		supplied = true;
		return token[skip..];

		static int shortFormNameMatch( string token, char? shortFormName )
		{
			if( !shortFormName.HasValue )
				return 0;
			if( token.Length != 2 )
				return 0;
			if( token[0] != '-' )
				return 0;
			if( token[1] != shortFormName.Value )
				return 0;
			return 2;
		}

		static int longFormNameMatch( string token, string name )
		{
			if( token.Length < 2 + name.Length )
				return 0;
			if( !(token[0] == '-' && token[1] == '-') )
				return 0;
			if( !token[2..].StartsWith( name, Sys.StringComparison.Ordinal ) )
				return 0;
			if( token.Length > 2 + name.Length && !Helpers.IsTerminator( token[2 + name.Length] ) )
				return 0;
			return 2 + name.Length;
		}
	}
}
