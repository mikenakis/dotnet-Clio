namespace Clio;
using Sys = System;

public sealed class TestingOptions
{
	public Sys.Action<string> LineOutputConsumer { get; }
	public string? ProgramName { get; }
	public int? ScreenWidth { get; }

	public TestingOptions( Sys.Action<string> lineOutput, string? programName, int? screenWidth )
	{
		LineOutputConsumer = lineOutput;
		ProgramName = programName;
		ScreenWidth = screenWidth;
	}
}
