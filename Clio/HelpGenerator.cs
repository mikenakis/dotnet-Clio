namespace Clio;

using System.Collections.Generic;
using Clio.Arguments;
using Sys = System;
using SysText = System.Text;

static class HelpGenerator
{
	const string indentation = "    ";

	internal static void OutputHelp( Sys.Action<string> lineOutputConsumer, int screenWidth, string fullName, IEnumerable<Argument> arguments, string verbTerm )
	{
		outputShortUsage( lineOutputConsumer, screenWidth, fullName, arguments, verbTerm );
		outputLongUsages( lineOutputConsumer, screenWidth, fullName, arguments, verbTerm );
		outputInformationalMessageAboutCombiningSingleLetters( lineOutputConsumer, arguments );
		return;

		static void outputShortUsage( Sys.Action<string> lineOutputConsumer, int screenWidth, string fullName, IEnumerable<Argument> arguments, string verbTerm )
		{
			lineOutputConsumer.Invoke( "Usage:" );
			string prefix = $"{indentation}{fullName} ";
			int availableWidth = screenWidth - prefix.Length;
			string shortUsageString = buildShortUsageString( arguments, verbTerm );
			foreach( string line in wordBreak( shortUsageString, availableWidth ) )
			{
				SysText.StringBuilder stringBuilder = new();
				stringBuilder.Append( prefix );
				stringBuilder.Append( line );
				lineOutputConsumer.Invoke( stringBuilder.ToString() );
				prefix = new( ' ', prefix.Length );
			}
		}

		static string buildShortUsageString( IEnumerable<Argument> arguments, string verbTerm )
		{
			SysText.StringBuilder stringBuilder = new();
			IReadOnlyList<NamedArgument> optionalNamedArguments = arguments.OfType<NamedArgument>().Where( argument => argument.IsOptional ).ToArray();
			switch( optionalNamedArguments.Count )
			{
				case 0:
					break;
				case 1:
					stringBuilder.Append( " [" ).Append( optionalNamedArguments[0].ShortUsage ).Append( ']' );
					break;
				default:
					stringBuilder.Append( " [<options>]" );
					break;
			}
			foreach( Argument argument in arguments.OfType<NamedArgument>().Where( argument => argument.IsRequired ) )
				stringBuilder.Append( ' ' ).Append( argument.ShortUsage );
			foreach( Argument argument in arguments.Where( argument => argument is not NamedArgument ).Where( argument => argument.IsRequired ) )
				stringBuilder.Append( ' ' ).Append( argument.ShortUsage );
			foreach( Argument argument in arguments.Where( argument => argument is not NamedArgument ).Where( argument => argument.IsOptional ) )
				stringBuilder.Append( " [" ).Append( argument.ShortUsage );
			foreach( Argument argument in arguments.Where( argument => argument is not NamedArgument ).Where( argument => argument.IsOptional ) )
				stringBuilder.Append( ']' );
			if( arguments.OfType<VerbArgument>().Any() )
				stringBuilder.Append( " <" ).Append( verbTerm ).Append( "> ..." );
			return stringBuilder.ToString();
		}

		static void outputLongUsages( Sys.Action<string> lineOutputConsumer, int screenWidth, string programName, IEnumerable<Argument> arguments, string verbTerm )
		{
			lineOutputConsumer.Invoke( "Options:" );
			int maxShortUsageLength = arguments.Select( argument => argument.ShortUsage.Length ).Max();
			foreach( Argument argument in arguments.Where( argument => argument is not VerbArgument ) )
				outputLongUsage( lineOutputConsumer, screenWidth, maxShortUsageLength, argument );
			if( arguments.OfType<VerbArgument>().Any() )
				lineOutputConsumer.Invoke( $"Where <{verbTerm}> is one of:" );
			foreach( Argument argument in arguments.OfType<VerbArgument>() )
				outputLongUsage( lineOutputConsumer, screenWidth, maxShortUsageLength, argument );
			if( arguments.OfType<VerbArgument>().Any() )
				lineOutputConsumer.Invoke( $"Try '{programName} <{verbTerm}> --help' for more information on a specific {verbTerm}." );
			return;

			static void outputLongUsage( Sys.Action<string> lineOutputConsumer, int screenWidth, int maxShortUsageLength, Argument argument )
			{
				List<string> longUsageLines = new();
				argument.CollectLongUsageLines( longUsageLines.Add );
				string prefix = argument.ShortUsage;
				int availableWidth = screenWidth - (indentation.Length + maxShortUsageLength + 1);
				foreach( string longUsageLine in longUsageLines.SelectMany( line => wordBreak( line, availableWidth ) ) )
				{
					SysText.StringBuilder stringBuilder = new();
					stringBuilder.Append( indentation ).Append( prefix ).Append( ' ', maxShortUsageLength - prefix.Length + 1 );
					stringBuilder.Append( longUsageLine );
					lineOutputConsumer.Invoke( stringBuilder.ToString() );
					prefix = "";
				}
				return;
			}
		}

		static void outputInformationalMessageAboutCombiningSingleLetters( Sys.Action<string> lineOutputConsumer, IEnumerable<Argument> arguments )
		{
			//TODO: recursively search for single-letter switches in verbs
			IReadOnlyList<NamedArgument> singleLetterSwitches = arguments.OfType<NamedArgument>().Where( argument => argument.SingleLetterName is not null and not 'h' ).ToArray();
			if( singleLetterSwitches.Count > 1 )
			{
				char a = singleLetterSwitches[0].SingleLetterName!.Value;
				char b = singleLetterSwitches[1].SingleLetterName!.Value;
				lineOutputConsumer.Invoke( $"Single-letter arguments can be combined. For example, -{a} -{b} can be replaced with -{a}{b}." );
			}
		}
	}

	static IReadOnlyList<string> wordBreak( string input, int width )
	{
		List<string> lines = new();
		string[] words = input.Split( ' ', Sys.StringSplitOptions.TrimEntries | Sys.StringSplitOptions.TrimEntries );
		SysText.StringBuilder stringBuilder = new();
		foreach( string word in words )
		{
			if( stringBuilder.Length + word.Length + (stringBuilder.Length > 0 ? 1 : 0) > width )
			{
				lines.Add( stringBuilder.ToString() );
				stringBuilder.Clear();
			}
			else
			{
				if( stringBuilder.Length > 0 )
					stringBuilder.Append( ' ' );
			}
			stringBuilder.Append( word );
		}
		if( stringBuilder.Length > 0 )
			lines.Add( stringBuilder.ToString() );
		return lines;
	}
}
