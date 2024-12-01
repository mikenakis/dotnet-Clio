namespace Clio_Test;

using Sys = System;
using Clio;
using Clio.Arguments;
using Clio.Codecs;
using Clio.Exceptions;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T100_ClioTests
{
	static ArgumentParser newArgumentParser()
	{
		TestingOptions testingOptions = new( s => Assert( false ), "TestApp", screenWidth: null );
		return new ArgumentParser( "subcommand", testingOptions );
	}

	enum Enum1
	{
		Value1,
		Value2,
		Value3
	}

	static readonly Sys.Action<ChildArgumentParser> emptyVerbHandler = argumentParser => //
			{
				argumentParser.TryParse(); //must be invoked because by design, failure to invoke causes exception.
			};

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Sunny day tests

	[VSTesting.TestMethod]
	public void T101_Empty_Clio_Works()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.TryParse( "" );
	}

	[VSTesting.TestMethod]
	public void T102_Switch_Receives_False_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( !alpha.Value );
	}

	[VSTesting.TestMethod]
	public void T103_Switch_Receives_True_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		bool ok = argumentParser.TryParse( "--alpha" );
		Assert( ok );
		Assert( alpha.Value );
	}

	[VSTesting.TestMethod]
	public void T104_Optional_String_Option_Receives_Null_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == null );
	}

	[VSTesting.TestMethod]
	public void T105_Optional_String_Option_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha" );
		bool ok = argumentParser.TryParse( "--alpha=alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T106_Defaulted_String_Option_Receives_Default_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddStringOptionWithDefault( "alpha", "alpha-default" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == "alpha-default" );
	}

	[VSTesting.TestMethod]
	public void T107_Defaulted_String_Option_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddStringOptionWithDefault( "alpha", "alpha-default" );
		bool ok = argumentParser.TryParse( "--alpha=alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T108_Optional_String_Option_With_Preset_Receives_Null_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha", presetValue: "alpha-preset" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == null );
	}

	[VSTesting.TestMethod]
	public void T109_Optional_String_Option_With_Preset_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha", presetValue: "alpha-preset" );
		bool ok = argumentParser.TryParse( "--alpha=alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T110_Optional_String_Option_With_Preset_Receives_Preset_When_Supplied_Without_Value()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha", presetValue: "alpha-preset" );
		bool ok = argumentParser.TryParse( "--alpha" );
		Assert( ok );
		Assert( alpha.Value == "alpha-preset" );
	}

	[VSTesting.TestMethod]
	public void T111_Required_String_Option_With_Preset_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddRequiredStringOption( "alpha", presetValue: "alpha-preset" );
		bool ok = argumentParser.TryParse( "--alpha=alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T112_Required_String_Option_With_Preset_Receives_Preset_When_Supplied_Without_Value()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddRequiredStringOption( "alpha", presetValue: "alpha-preset" );
		bool ok = argumentParser.TryParse( "--alpha" );
		Assert( ok );
		Assert( alpha.Value == "alpha-preset" );
	}

	[VSTesting.TestMethod]
	public void T113_Enum_Option_Works()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1?> alpha = argumentParser.AddOption( "alpha", EnumCodec<Enum1>.Instance );
		bool ok = argumentParser.TryParse( $"--alpha={nameof( Enum1.Value2 )}" );
		Assert( ok );
		Assert( alpha.Value == Enum1.Value2 );
	}

	[VSTesting.TestMethod]
	public void T114_Int_Option_Works()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<int> alpha = argumentParser.AddOptionWithDefault( "alpha", IntCodec.Instance, 42 );
		bool ok = argumentParser.TryParse( $"--alpha=5" );
		Assert( ok );
		Assert( alpha.Value == 5 );
	}

	[VSTesting.TestMethod]
	public void T115_Optional_String_Positional_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringPositional( "alpha" );
		bool ok = argumentParser.TryParse( "alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T116_Optional_String_Positional_Receives_Null_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringPositional( "alpha" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == null );
	}

	[VSTesting.TestMethod]
	public void T117_Defaulted_String_Positional_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddStringPositionalWithDefault( "alpha", "alpha-default" );
		bool ok = argumentParser.TryParse( "alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T118_Defaulted_String_Positional_Receives_Default_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddStringPositionalWithDefault( "alpha", "alpha-default" );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == "alpha-default" );
	}

	[VSTesting.TestMethod]
	public void T119_Required_String_Positional_Receives_Value()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddRequiredStringPositional( "alpha" );
		bool ok = argumentParser.TryParse( "alpha-value" );
		Assert( ok );
		Assert( alpha.Value == "alpha-value" );
	}

	[VSTesting.TestMethod]
	public void T120_Optional_Struct_Positional_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1?> alpha = argumentParser.AddPositional( "alpha", EnumCodec<Enum1>.Instance );
		bool ok = argumentParser.TryParse( nameof( Enum1.Value3 ) );
		Assert( ok );
		Assert( alpha.Value == Enum1.Value3 );
	}

	[VSTesting.TestMethod]
	public void T121_Optional_Struct_Positional_Receives_Null_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1?> alpha = argumentParser.AddPositional( "alpha", EnumCodec<Enum1>.Instance );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == null );
	}

	[VSTesting.TestMethod]
	public void T122_Defaulted_Struct_Positional_Receives_Value_When_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1> alpha = argumentParser.AddPositionalWithDefault( "alpha", EnumCodec<Enum1>.Instance, Enum1.Value2 );
		bool ok = argumentParser.TryParse( nameof( Enum1.Value3 ) );
		Assert( ok );
		Assert( alpha.Value == Enum1.Value3 );
	}

	[VSTesting.TestMethod]
	public void T123_Defaulted_Struct_Positional_Receives_Default_When_Not_Supplied()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1> alpha = argumentParser.AddPositionalWithDefault( "alpha", EnumCodec<Enum1>.Instance, Enum1.Value2 );
		bool ok = argumentParser.TryParse( "" );
		Assert( ok );
		Assert( alpha.Value == Enum1.Value2 );
	}

	[VSTesting.TestMethod]
	public void T124_Required_Struct_Positional_Receives_Value()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<Enum1> alpha = argumentParser.AddRequiredPositional( "alpha", EnumCodec<Enum1>.Instance );
		bool ok = argumentParser.TryParse( nameof( Enum1.Value3 ) );
		Assert( ok );
		Assert( alpha.Value == Enum1.Value3 );
	}

	[VSTesting.TestMethod]
	public void T125_Short_Form_Name_Gets_Parsed()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a' );
		bool ok = argumentParser.TryParse( "-a" );
		Assert( ok );
		Assert( alpha.Value );
	}

	[VSTesting.TestMethod]
	public void T126_Combined_Short_Form_Names_Get_Parsed()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a' );
		IArgument<bool> bravo = argumentParser.AddSwitch( "bravo", 'b' );
		bool ok = argumentParser.TryParse( "-ab" );
		Assert( ok );
		Assert( alpha.Value );
		Assert( bravo.Value );
	}

	[VSTesting.TestMethod]
	public void T127_Parameter_Supplied_Before_Named_Argument_Works()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		IArgument<string?> bravo = argumentParser.AddStringPositional( "bravo" );
		bool ok = argumentParser.TryParse( "bravo-value --alpha" );
		Assert( ok );
		Assert( alpha.Value );
		Assert( bravo.Value == "bravo-value" );
	}

	[VSTesting.TestMethod]
	public void T128_Optional_Positional_Before_Defaulted_Positional_Works()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> mike = argumentParser.AddStringPositional( "mike", "mike-description" );
		IArgument<string> papa = argumentParser.AddStringPositionalWithDefault( "papa", "papa-default", description: "papa-description" );
		bool ok = argumentParser.TryParse( "mike-value" );
		Assert( ok );
		Assert( mike.Value == "mike-value" );
		Assert( papa.Value == "papa-default" );
	}

	[VSTesting.TestMethod]
	public void T129_Argument_Names_May_Overlap()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alphaBravo = argumentParser.AddSwitch( "alphaBravo" );
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		IArgument<bool> alphaBravoCharlie = argumentParser.AddSwitch( "alphaBravoCharlie" );
		bool ok = argumentParser.TryParse( "--alpha --alphaBravo --alphaBravoCharlie" );
		Assert( ok );
		Assert( alpha.Value == true );
		Assert( alphaBravo.Value == true );
		Assert( alphaBravoCharlie.Value == true );
	}

	[VSTesting.TestMethod]
	public void T130_String_Option_Value_Can_Be_Empty()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string?> alpha = argumentParser.AddStringOption( "alpha" );
		bool ok = argumentParser.TryParse( "--alpha=" );
		Assert( ok );
		Assert( alpha.Value == "" );
	}

	[VSTesting.TestMethod]
	public void T131_Verbs_Work()
	{
		ArgumentParser argumentParser = newArgumentParser();
		bool verbHandlerInvoked = false;
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a', "alpha-description" );
		argumentParser.AddVerb( "juliet", "juliet-description", argumentParser => //
			{
				IArgument<bool> lima = argumentParser.AddSwitch( "lima", description: "lima-description" );
				IArgument<string> india = argumentParser.AddRequiredStringPositional( "india", "india-description" );
				if( !argumentParser.TryParse() )
					return;
				Assert( alpha.Value == true );
				Assert( lima.Value == true );
				Assert( india.Value == "india-value" );
				verbHandlerInvoked = true;
			} );
		argumentParser.AddVerb( "kilo", "kilo-description", emptyVerbHandler );
		bool ok = argumentParser.TryParse( "-a juliet --lima india-value" );
		Assert( ok );
		Assert( verbHandlerInvoked );
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Tests for programmer mistakes

	[VSTesting.TestMethod]
	public void T201_Switch_Names_Must_Be_Unique()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddSwitch( "alpha" ) );
		NotNullCast( caughtException, out DuplicateArgumentNameException exception );
		Assert( exception.ArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T202_Option_Names_Must_Be_Unique()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringOption( "alpha" ) );
		NotNullCast( caughtException, out DuplicateArgumentNameException exception );
		Assert( exception.ArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T203_Parameter_Names_Must_Be_Unique()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringPositional( "alpha" ) );
		NotNullCast( caughtException, out DuplicateArgumentNameException exception );
		Assert( exception.ArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T204_Switch_Single_Letter_Names_Must_Be_Unique()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a' );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddSwitch( "bravo", 'a' ) );
		NotNullCast( caughtException, out DuplicateArgumentSingleLetterNameException exception );
		Assert( exception.ArgumentShortFormName == 'a' );
	}

	[VSTesting.TestMethod]
	public void T205_Option_Single_Letter_Names_Must_Be_Unique()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a' );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringOption( "bravo", 'a' ) );
		NotNullCast( caughtException, out DuplicateArgumentSingleLetterNameException exception );
		Assert( exception.ArgumentShortFormName == 'a' );
	}

	[VSTesting.TestMethod]
	public void T206_Positional_Value_Cannot_Be_Accessed_Before_Parsing()
	{
		ArgumentParser argumentParser = newArgumentParser();
		IArgument<string> alpha = argumentParser.AddRequiredStringPositional( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
			_ = alpha.Value );
		NotNullCast( caughtException, out CommandLineHasNotBeenParsedException _ );
	}

	[VSTesting.TestMethod]
	public void T207_Argument_Cannot_Be_Added_After_Parsing()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.TryParse( "" );
		Sys.Exception? caughtException = TryCatch( () => //
						argumentParser.AddRequiredStringPositional( "alpha" ) );
		NotNullCast( caughtException, out CommandLineHasAlreadyBeenParsedException _ );
	}

	[VSTesting.TestMethod]
	public void T208_Named_Argument_Name_Must_Be_Valid()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
						argumentParser.AddSwitch( "-" ) );
		NotNullCast( caughtException, out InvalidArgumentNameException exception );
		Assert( exception.ArgumentName == "-" );
	}

	[VSTesting.TestMethod]
	public void T209_Positional_Argument_Name_Must_Be_Valid()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringPositional( "-invalid" ) );
		NotNullCast( caughtException, out InvalidArgumentNameException exception );
		Assert( exception.ArgumentName == "-invalid" );
	}

	[VSTesting.TestMethod]
	public void T210_Required_Positional_Must_Precede_Optional_Positional()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddStringPositional( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddRequiredStringPositional( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.RequiredPositionalMustPrecedeOptionalPositional );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T211_Required_Positional_Must_Precede_Positional_With_Default()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddStringPositionalWithDefault( "alpha", "alpha-default" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddRequiredStringPositional( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.RequiredPositionalMustPrecedeOptionalPositional );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T212_Named_Argument_Must_Precede_Positional()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddStringPositional( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddSwitch( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.NamedArgumentMustPrecedePositional );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T213_Switch_May_Not_Be_Added_After_Verb()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddVerb( "alpha", "alpha-description", emptyVerbHandler );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddSwitch( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.ArgumentMustPrecedeVerb );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T214_Option_May_Not_Be_Added_After_Verb()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddVerb( "alpha", "alpha-description", emptyVerbHandler );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringOption( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.ArgumentMustPrecedeVerb );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T215_Positional_May_Not_Be_Added_After_Verb()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddVerb( "alpha", "alpha-description", emptyVerbHandler );
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringPositional( "bravo" ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.ArgumentMustPrecedeVerb );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T216_Verb_May_Not_Be_Preceded_By_Optional_Positional()
	{
		ArgumentParser argumentParser = newArgumentParser();
		argumentParser.AddStringPositional( "alpha" );
		Sys.Exception? caughtException = TryCatch( () => //
						argumentParser.AddVerb( "bravo", "bravo-description", emptyVerbHandler ) );
		NotNullCast( caughtException, out InvalidArgumentOrderingException exception );
		Assert( exception.ArgumentOrderingRule == ArgumentOrderingRule.VerbMayNotBePrecededByOptionalPositional );
		Assert( exception.ViolatingArgumentName == "bravo" );
		Assert( exception.PrecedingArgumentName == "alpha" );
	}

	[VSTesting.TestMethod]
	public void T217_Switch_Name_Must_Be_Longer_Than_One_Character()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddSwitch( "a" ) );
		NotNullCast( caughtException, out InvalidArgumentNameException exception );
		Assert( exception.ArgumentName == "a" );
	}

	[VSTesting.TestMethod]
	public void T218_Option_Name_Must_Be_Longer_Than_One_Character()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddStringOption( "a" ) );
		NotNullCast( caughtException, out InvalidArgumentNameException exception );
		Assert( exception.ArgumentName == "a" );
	}

	[VSTesting.TestMethod]
	public void T219_Verb_Handler_Must_Invoke_TryParse()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddVerb( "juliet", "juliet-description", argumentParser => //
					{
					} )
				);
		NotNullCast( caughtException, out TryParseWasNotInvokedException exception );
		Assert( exception.VerbName == "juliet" );
	}

	[VSTesting.TestMethod]
	public void T220_Verb_Handler_Must_Not_Invoke_TryParse_More_Than_Once()
	{
		ArgumentParser argumentParser = newArgumentParser();
		Sys.Exception? caughtException = TryCatch( () => //
				argumentParser.AddVerb( "juliet", "juliet-description", argumentParser => //
					{
						argumentParser.TryParse();
						argumentParser.TryParse();
					} )
				);
		NotNullCast( caughtException, out TryParseInvokedMoreThanOnceException exception );
		Assert( exception.VerbName == "juliet" );
	}
}

static class Extensions
{
	public static bool TryParse( this ArgumentParser self, string commandLine )
	{
		string[] tokens = commandLine.Split( ' ', Sys.StringSplitOptions.RemoveEmptyEntries | Sys.StringSplitOptions.TrimEntries );
		return self.TryParse( tokens );
	}
}
