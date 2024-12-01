namespace Clio.Exceptions;

using Clio.ClioKit;
using Sys = System;

abstract class UserException( Sys.Exception? cause = null ) : SaneException( cause );

sealed class HelpException( BaseArgumentParser argumentParser ) : UserException
{
	internal BaseArgumentParser ArgumentParser => argumentParser;
}

sealed class ArgumentSuppliedMoreThanOnceException( string argumentName ) : UserException
{
	public override string Message => $"Argument '{argumentName}' supplied more than once.";
}

sealed class UnexpectedCharactersAfterNamedArgumentException( string argumentName, string unexpectedCharacters ) : UserException
{
	public override string Message => $"Unexpected characters found after '{argumentName}' : '{unexpectedCharacters}'.";
}

sealed class OptionRequiresValueException( string argumentName ) : UserException
{
	public override string Message => $"Option '{argumentName}' was supplied without a value, but it requires a value.";
}

sealed class RequiredArgumentNotSuppliedException( string argumentName ) : UserException
{
	public override string Message => $"Required argument '{argumentName}' was not supplied.";
}

sealed class UnexpectedTokenException( string token ) : UserException
{
	public override string Message => $"Unexpected token: '{token}'.";
}

sealed class UnparsableValueException( string argumentName, string token, Sys.Exception? cause = null ) : UserException( cause )
{
	public override string Message => $"'{token}' is not a valid value for argument '{argumentName}'.";
}

sealed class VerbExpectedException( string verbTerm ) : UserException
{
	public override string Message => $"Expected a {verbTerm}.";
}
