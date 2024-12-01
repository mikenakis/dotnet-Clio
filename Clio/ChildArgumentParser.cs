namespace Clio;

/// <summary>Parses command-line arguments for a verb.</summary>
public abstract class ChildArgumentParser : BaseArgumentParser
{
	internal override BaseArgumentParser Parent { get; }
	internal override ArgumentParser GetRootArgumentParser() => Parent.GetRootArgumentParser();

	internal ChildArgumentParser( BaseArgumentParser parent, string name )
		: base( name )
	{
		Parent = parent;
	}

	public abstract bool TryParse();
}

