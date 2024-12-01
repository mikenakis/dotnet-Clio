namespace Clio.Exceptions;

using Clio.ClioKit;
using Sys = System;

/// <summary>Represent a mistake made by the programmer.</summary>
public abstract class ProgrammerException : SaneException
{
	private protected ProgrammerException()
	{ }

	private protected ProgrammerException( Sys.Exception cause )
		: base( cause )
	{ }
}

/// <summary>Thrown when an attempt is made to add an argument after the command-line has been parsed.</summary>
public sealed class CommandLineHasAlreadyBeenParsedException() : ProgrammerException;

/// <summary>Thrown when an attempt is made to read the value of an argument without first having parsed the command-line.</summary>
public sealed class CommandLineHasNotBeenParsedException() : ProgrammerException;

/// <summary>Thrown when an attempt is made to add an argument with the same name as an already-added argument.</summary>
public sealed class DuplicateArgumentNameException( string argumentName ) : ProgrammerException
{
	public string ArgumentName => argumentName;
}

/// <summary>Thrown when an attempt is made to add an argument with the same single-letter name as an already-added argument.</summary>
public sealed class DuplicateArgumentSingleLetterNameException( char argumentShortFormName ) : ProgrammerException
{
	public char ArgumentShortFormName => argumentShortFormName;
}

/// <summary>Thrown when an attempt is made to add an argument with an invalid name.</summary>
public sealed class InvalidArgumentNameException( string argumentName ) : ProgrammerException
{
	public string ArgumentName => argumentName;
}

public enum ArgumentOrderingRule
{
	NamedArgumentMustPrecedePositional, //TODO: revise the usefulness of this.
	RequiredPositionalMustPrecedeOptionalPositional, //TODO: revise the usefulness of this.
	ArgumentMustPrecedeVerb, //TODO: revise the usefulness of this.
	VerbMayNotBePrecededByOptionalPositional
}

/// <summary>Thrown when an attempt is made to add arguments in the wrong order.</summary>
public sealed class InvalidArgumentOrderingException( ArgumentOrderingRule argumentOrderingRule, string violatingArgumentName, string precedingArgumentName ) : ProgrammerException
{
	public ArgumentOrderingRule ArgumentOrderingRule => argumentOrderingRule;
	public string ViolatingArgumentName => violatingArgumentName;
	public string PrecedingArgumentName => precedingArgumentName;
}

/// <summary>Thrown when an attempt is made to re-specify the single-letter name of an argument.</summary>
public sealed class SingleLetterNameAlreadyGivenException( string argumentName, char existingShortFormName, char newShortFormName ) : ProgrammerException
{
	public string ArgumentName => argumentName;
	public char ExistingSingleLetterName => existingShortFormName;
	public char NewSingleLetterName => newShortFormName;
}

/// <summary>Thrown when an attempt is made to re-specify the parameter name of an option.</summary>
public sealed class OptionParameterNameAlreadyGivenException( string argumentName, string existingParameterName, string newParameterName ) : ProgrammerException
{
	public string ArgumentName => argumentName;
	public string ExistingParameterName => existingParameterName;
	public string NewParameterName => newParameterName;
}

/// <summary>Thrown when an attempt is made to re-specify the description of an argument.</summary>
public sealed class DescriptionAlreadyGivenException( string argumentName, string existingDescription, string newDescription ) : ProgrammerException
{
	public string ArgumentName => argumentName;
	public string ExistingDescription => existingDescription;
	public string NewDescription => newDescription;
}

public sealed class TryParseWasNotInvokedException( string verbName ) : ProgrammerException
{
	public string VerbName => verbName;
}

public sealed class TryParseInvokedMoreThanOnceException( string verbName ) : ProgrammerException
{
	public string VerbName => verbName;
}
