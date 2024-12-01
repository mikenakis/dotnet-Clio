namespace Clio;

using System.Collections.Generic;
using Clio.Arguments;
using Clio.Exceptions;
using Sys = System;

///<summary>Parses command-line arguments for a program.</summary>
public sealed class ArgumentParser : BaseArgumentParser
{
	public string VerbTerm { get; }
	internal override BaseArgumentParser? Parent => null;
	internal override ArgumentParser GetRootArgumentParser() => this;
	internal Sys.Action<string> LineOutputConsumer { get; }
	readonly int screenWidth;
	public bool OutputArgumentNamesAndValues { get; set; }

	/// <summary>Constructor.</summary>
	/// <param name="testingOptions" >Supplies options used for testing.</param>
	public ArgumentParser( string? verbTerm = null, TestingOptions? testingOptions = null )
		: base( testingOptions?.ProgramName ?? Sys.AppDomain.CurrentDomain.FriendlyName )
	{
		VerbTerm = verbTerm ?? "verb";
		LineOutputConsumer = testingOptions?.LineOutputConsumer ?? Sys.Console.Error.Write;
		screenWidth = testingOptions?.ScreenWidth ?? 120;
	}

	///<summary>Adds a verb.</summary>
	///<param name="name">The name of the verb.</param>
	///<param name="description">The description of the verb, for use when displaying help.</param>
	///<param name="handler">The handler of the verb.</param>
	public IVerbArgument AddVerb( string name, string description, Sys.Action<ChildArgumentParser> handler )
	{
		return new VerbArgument( this, name, description, handler );
	}

	/// <summary>Parses an array of command-line tokens, stores values in arguments, invokes verb handlers, etc.</summary>
	/// <remarks>If something goes wrong, (or if the `--help` option is supplied,) it displays all necessary messages
	/// and terminates the current process.</remarks>
	/// <param name="arrayOfToken">The command-line tokens to parse.</param>
	public void Parse( string[] arrayOfToken )
	{
		if( !TryParse( arrayOfToken ) )
			Sys.Environment.Exit( 1 );
	}

	/// <summary>Parses an array of command-line tokens, stores values in arguments, invokes verb handlers, etc.</summary>
	/// <remarks>If something goes wrong, (or if the `--help` option is supplied,) it displays all necessary messages
	/// and returns <c>false</c>, meaning that the current process should terminate.</remarks>
	/// <param name="arrayOfToken">The command-line tokens to parse.</param>
	/// <returns><c>true</c> if successful; <c>false</c> otherwise.</returns>
	public bool TryParse( string[] arrayOfToken )
	{
		IReadOnlyList<string> tokens = splitCombinedSingleLetterArguments( arrayOfToken );
		try
		{
			ParseRemainingTokens( 0, tokens );
		}
		catch( HelpException helpException )
		{
			helpException.ArgumentParser.OutputHelp( screenWidth );
			return false;
		}
		catch( UserException userException )
		{
			LineOutputConsumer.Invoke( userException.Message );
			for( Sys.Exception? innerException = userException.InnerException; innerException != null; innerException = innerException.InnerException )
				LineOutputConsumer.Invoke( "Because: " + innerException.Message );
			string fullName = GetFullName( ' ' );
			LineOutputConsumer.Invoke( $"Try '{fullName} --help' for more information." );
			return false;
		}
		return true;

		static IReadOnlyList<string> splitCombinedSingleLetterArguments( IReadOnlyList<string> tokens )
		{
			List<string> mutableTokens = new();
			foreach( string token in tokens )
				if( isCombined( token ) )
					foreach( char c in token.Skip( 1 ) )
						mutableTokens.Add( $"-{c}" );
				else
					mutableTokens.Add( token );
			return mutableTokens;

			static bool isCombined( string token ) => token[0] == '-' && token.Length > 2 && token[1] != '-' /* && token[1..].All( Helpers.ShortFormNameIsValid ) */ ;
		}
	}
}
