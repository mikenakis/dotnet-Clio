namespace Clio_Test;

using Sys = System;
using Clio;
using Clio.Arguments;
using Clio.Codecs;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;
using SysCompiler = System.Runtime.CompilerServices;

[VSTesting.TestClass]
public sealed class T101_ClioAuditTests
{
	const string verbTerm = "subcommand";

	static TestingOptions getTestingOptions( Sys.Action<string> textConsumer, int? screenWidth = null, [SysCompiler.CallerMemberName] string? callerMemberName = null )
	{
		return new TestingOptions( textConsumer, "TestApp", screenWidth );
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
	// Audits

	[VSTesting.TestMethod]
	public void T101_No_Optional_Named_Arguments_Help_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddRequiredStringPositional( "india", "This is the description of india" );
			bool ok = argumentParser.TryParse( "--help" );
			Assert( !ok ); //because help was requested.
		} );
	}

	[VSTesting.TestMethod]
	public void T102_Simple_Help_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddSwitch( "alpha", 'a', "This is the description of alpha" );
			argumentParser.AddSwitch( "bravo", 'b' );
			argumentParser.AddStringOptionWithDefault( "charlie", "charlie-default", singleLetterName: 'c', description: "This is the description of charlie", parameterName: "charlie-parameter" );
			argumentParser.AddStringOption( "delta", 'd', "This is the description of delta", parameterName: "delta-parameter" );
			argumentParser.AddRequiredStringOption( "echo", description: "This is the description of echo", parameterName: "echo-parameter" );
			argumentParser.AddOptionWithDefault( "foxtrot", EnumCodec<Enum1>.Instance, Enum1.Value2, description: "This is the description of foxtrot", parameterName: "foxtrot-parameter" );
			argumentParser.AddOption( "golf", EnumCodec<Enum1>.Instance, description: "This is the description of golf", parameterName: "golf-parameter" );
			argumentParser.AddRequiredOption( "hotel", EnumCodec<Enum1>.Instance, description: "This is the description of hotel", parameterName: "hotel-parameter" );
			argumentParser.AddStringOptionWithDefault( "mike", "mike-default", description: "This is the description of mike", presetValue: "mike-preset" );
			argumentParser.AddStringOptionWithDefault( "november", "november-default", description: "This is the description of november", presetValue: "november-preset" );
			argumentParser.AddStringOptionWithDefault( "oscar", "oscar-default", description: "This is the description of oscar", presetValue: "oscar-preset" );
			argumentParser.AddRequiredStringPositional( "india", "This is the description of india" );
			argumentParser.AddStringPositional( "juliet", "This is the description of juliet" );
			argumentParser.AddStringPositionalWithDefault( "papa", "papa-default", description: "This is the description of papa" );
			bool ok = argumentParser.TryParse( "--help" );
			Assert( !ok ); //because help was requested.
		} );
	}

	[VSTesting.TestMethod]
	public void T103_Option_With_Long_Usage_Help_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer, screenWidth: 80 );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddSwitch( "alpha", 'a', "This is the description of alpha" );
			argumentParser.AddRequiredStringOption( "echo", description: "This is the description of echo", parameterName: "rather-lengthy-echo-parameter" );
			argumentParser.AddRequiredOption( "hotel", EnumCodec<Enum1>.Instance, description: "This is the description of hotel", parameterName: "rather-lengthy-hotel-parameter" );
			argumentParser.AddStringOptionWithDefault( "mike", "this is mike-default", description: "This is the description of mike, which is a very long description in order to test word-breaking", presetValue: "this is mike-preset" );
			bool ok = argumentParser.TryParse( "--help" );
			Assert( !ok ); //because help was requested.
		} );
	}

	[VSTesting.TestMethod]
	public void T104_Root_With_Verbs_Help_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			setupAndParse( testingOptions, "--help" );
		} );
	}

	[VSTesting.TestMethod]
	public void T105_Verb_Help_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			setupAndParse( testingOptions, "juliet --help" );
		} );
	}

	static void setupAndParse( TestingOptions testingOptions, string commandLine )
	{
		ArgumentParser argumentParser = new( verbTerm, testingOptions );
		IArgument<bool> alpha = argumentParser.AddSwitch( "alpha", 'a', "This is the description of alpha" );
		IArgument<string> echo = argumentParser.AddRequiredStringOption( "echo", description: "This is the description of echo", parameterName: "echo-parameter" );
		IArgument juliet = argumentParser.AddVerb( "juliet", "This is the description of juliet", argumentParser => //
			{
				IArgument<bool> lima = argumentParser.AddSwitch( "lima", description: "This is the description of lima" );
				IArgument<string> papa = argumentParser.AddStringPositionalWithDefault( "papa", "papa-default", description: "This is the description of papa" );
				if( !argumentParser.TryParse() )
					return;
			} );
		IArgument kilo = argumentParser.AddVerb( "kilo", "This is the description of kilo", argumentParser =>
			{
				IArgument<bool> lima = argumentParser.AddSwitch( "lima", description: "This is the description of lima" );
				IArgument<string?> mike = argumentParser.AddStringPositional( "mike", "This is the description of mike" );
				if( !argumentParser.TryParse() )
					return;
				Assert( lima.Value == false );
				Assert( mike.Value == null );
			} );
		bool ok = argumentParser.TryParse( commandLine );
		Assert( !ok ); //because help was requested.
	}

	[VSTesting.TestMethod]
	public void T106_Argument_Dump_Audit()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			ISwitchArgument alpha = argumentParser.AddSwitch( "alpha", 'a' );
			ISwitchArgument bravo = argumentParser.AddSwitch( "bravo", 'b' );
			IOptionArgument<string> charlie = argumentParser.AddStringOptionWithDefault( "charlie", "charlie-default", singleLetterName: 'c' );
			IOptionArgument<string?> delta = argumentParser.AddStringOption( "delta", 'd' );
			IOptionArgument<string> echo = argumentParser.AddRequiredStringOption( "echo" );
			IOptionArgument<Enum1> foxtrot = argumentParser.AddOptionWithDefault( "foxtrot", EnumCodec<Enum1>.Instance, Enum1.Value3 );
			IOptionArgument<Enum1?> golf = argumentParser.AddOption( "golf", EnumCodec<Enum1>.Instance );
			IOptionArgument<Enum1> hotel = argumentParser.AddRequiredOption( "hotel", EnumCodec<Enum1>.Instance );
			IOptionArgument<string> india = argumentParser.AddStringOptionWithDefault( "india", "india-default", presetValue: "india-preset" );
			IOptionArgument<string> juliet = argumentParser.AddStringOptionWithDefault( "juliet", "juliet-default", presetValue: "juliet-preset" );
			IOptionArgument<string> kilo = argumentParser.AddStringOptionWithDefault( "kilo", "kilo-default", presetValue: "kilo-preset" );
			IPositionalArgument<string> lima = argumentParser.AddRequiredStringPositional( "lima" );
			bool mikeHandlerWasInvoked = false;
			IVerbArgument mike = argumentParser.AddVerb( "mike", "This is the description of mike", argumentParser => //
				{
					IArgument<bool> november = argumentParser.AddSwitch( "november" );
					IArgument<bool> oscar = argumentParser.AddSwitch( "oscar" );
					IPositionalArgument<string?> papa = argumentParser.AddStringPositional( "papa" );
					IPositionalArgument<string> quebec = argumentParser.AddStringPositionalWithDefault( "quebec", "quebec-default" );
					IPositionalArgument<string?> romeo = argumentParser.AddStringPositional( "romeo" );
					if( !argumentParser.TryParse() )
						return;
					Assert( alpha.Value == true );
					Assert( bravo.Value == false );
					Assert( charlie.Value == "charlie-default" );
					Assert( delta.Value == null );
					Assert( echo.Value == "echo-value" );
					Assert( foxtrot.Value == Enum1.Value3 );
					Assert( golf.Value == null );
					Assert( hotel.Value == Enum1.Value2 );
					Assert( india.Value == "india-preset" );
					Assert( juliet.Value == "juliet-default" );
					Assert( kilo.Value == "kilo-value" );
					Assert( lima.Value == "lima-value" );
					Assert( november.Value == true );
					Assert( oscar.Value == false );
					Assert( papa.Value == "papa-value" );
					Assert( quebec.Value == "quebec-default" );
					Assert( romeo.Value == null );
					mikeHandlerWasInvoked = true;
				} );
			argumentParser.OutputArgumentNamesAndValues = true;
			bool ok = argumentParser.TryParse( "-a --echo=echo-value --hotel=Value2 --india --kilo=kilo-value lima-value mike --november papa-value" );
			Assert( ok );
			Assert( alpha.Value == true );
			Assert( bravo.Value == false );
			Assert( charlie.Value == "charlie-default" );
			Assert( delta.Value == null );
			Assert( echo.Value == "echo-value" );
			Assert( foxtrot.Value == Enum1.Value3 );
			Assert( golf.Value == null );
			Assert( hotel.Value == Enum1.Value2 );
			Assert( india.Value == "india-preset" );
			Assert( juliet.Value == "juliet-default" );
			Assert( kilo.Value == "kilo-value" );
			Assert( lima.Value == "lima-value" );
			Assert( mike.Value == true );
			Assert( mikeHandlerWasInvoked );
		} );
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Tests for user mistakes

	[VSTesting.TestMethod]
	public void T107_Argument_Supplied_More_Than_Once_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddSwitch( "alpha" );
			bool ok = argumentParser.TryParse( "--alpha --alpha" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T108_Option_Value_Not_Supplied_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddStringOption( "alpha" );
			bool ok = argumentParser.TryParse( "--alpha" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T109_Required_Option_Not_Supplied_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddRequiredStringOption( "alpha" );
			bool ok = argumentParser.TryParse( "" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T110_Required_Positional_Not_Supplied_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddRequiredStringPositional( "alpha" );
			bool ok = argumentParser.TryParse( "" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T111_Unparsable_Option_Value_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddOption( "alpha", EnumCodec<Enum1>.Instance );
			bool ok = argumentParser.TryParse( "--alpha=Unparsable" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T112_Unparsable_Positional_Value_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddPositional( "alpha", IntCodec.Instance );
			bool ok = argumentParser.TryParse( "X" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T113_Unexpected_Token_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			bool ok = argumentParser.TryParse( "unexpected" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T114_Unknown_Single_Letter_Name_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddSwitch( "alpha", 'a' );
			argumentParser.AddSwitch( "bravo", 'b' );
			bool ok = argumentParser.TryParse( "-abc" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T115_Non_String_Option_Value_Cannot_Be_Empty()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddOption( "alpha", IntCodec.Instance );
			bool ok = argumentParser.TryParse( "--alpha=" );
			Assert( !ok );
		} );
	}

	[VSTesting.TestMethod]
	public void T116_Missing_Verb_Is_Caught()
	{
		Audit.With( lineConsumer =>
		{
			TestingOptions testingOptions = getTestingOptions( lineConsumer );
			ArgumentParser argumentParser = new( verbTerm, testingOptions );
			argumentParser.AddVerb( "alpha", "This is the description of alpha", emptyVerbHandler );
			bool ok = argumentParser.TryParse( "" );
			Assert( !ok );
		} );
	}
}
