namespace Clio;

using RegEx = System.Text.RegularExpressions;
using Clio.Exceptions;
using Clio.Arguments;

static partial class Helpers
{
	static readonly RegEx.Regex namedArgumentNameValidationRegex = new( "^[a-zA-Z][a-zA-Z0-9-]+$", RegEx.RegexOptions.CultureInvariant );
	static readonly RegEx.Regex shortFormNameValidationRegex = new( "^[a-zA-Z0-9]+$", RegEx.RegexOptions.CultureInvariant );
	static readonly RegEx.Regex optionParameterNameValidationRegex = new( "^[a-zA-Z0-9-]+$", RegEx.RegexOptions.CultureInvariant );
	static readonly RegEx.Regex parameterNameValidationRegex = new( "^[a-zA-Z][a-zA-Z0-9-]+$", RegEx.RegexOptions.CultureInvariant );
	static readonly RegEx.Regex verbNameValidationRegex = new( "^[a-zA-Z0-9-]+$", RegEx.RegexOptions.CultureInvariant );

	internal const string DefaultDescription = "See user's manual";

	internal static bool SwitchNameIsValidAssertion( string name )
	{
		Assert( nameIsValidAssertion( name, namedArgumentNameValidationRegex ) );
		return true;
	}

	internal static bool OptionNameIsValidAssertion( string name )
	{
		Assert( nameIsValidAssertion( name, namedArgumentNameValidationRegex ) );
		return true;
	}

	internal static bool ParameterNameIsValidAssertion( string name )
	{
		Assert( nameIsValidAssertion( name, parameterNameValidationRegex ) );
		return true;
	}

	internal static bool VerbNameIsValidAssertion( string name )
	{
		Assert( nameIsValidAssertion( name, verbNameValidationRegex ) );
		return true;
	}

	internal static bool ShortFormNameIsValid( char shortFormName )
	{
		return nameIsValid( new string( shortFormName, 1 ), shortFormNameValidationRegex );
	}

	internal static bool ShortFormNameIsValidAssertion( char shortFormName )
	{
		Assert( nameIsValidAssertion( new string( shortFormName, 1 ), shortFormNameValidationRegex ) );
		return true;
	}

	internal static bool OptionParameterNameIsValidAssertion( string name )
	{
		Assert( nameIsValidAssertion( name, optionParameterNameValidationRegex ) );
		return true;
	}

	static bool nameIsValidAssertion( string name, RegEx.Regex regex )
	{
		Assert( nameIsValid( name, regex ), () => throw new InvalidArgumentNameException( name ) );
		return true;
	}

	static bool nameIsValid( string shortFormName, RegEx.Regex regex )
	{
		return regex.IsMatch( shortFormName );
	}

	internal static bool IsTerminator( char c ) => !shortFormNameValidationRegex.IsMatch( new string( c, 1 ) );

	//TODO: revise the usefulness of this.
	internal static bool ArgumentMustPrecedeVerbAssertion( BaseArgumentParser argumentParser, string name, string verbTerm )
	{
		Assert( argumentParser.Arguments.OfType<VerbArgument>().FirstOrDefault(), //
			verb => verb == null, //
			verb => throw new InvalidArgumentOrderingException( ArgumentOrderingRule.ArgumentMustPrecedeVerb, name, verb!.Name ) );
		return true;
	}
}
