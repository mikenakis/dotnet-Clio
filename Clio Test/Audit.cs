namespace Clio_Test;

using Sys = System;
using SysIo = System.IO;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;
using SysText = System.Text;

public sealed class Audit
{
	public const string FileExtension = ".audit";
	static readonly Dictionary<string, Audit> auditsByPathName = new();

	public static void With( Sys.Action<Sys.Action<string>> procedure, //
		[SysCompiler.CallerFilePath] string? callerFilePathName = null, //
		[SysCompiler.CallerMemberName] string? callerMemberName = null )
	{
		Assert( callerFilePathName != null );
		Assert( callerMemberName != null );
		string auditFilePathName = SysIo.Path.ChangeExtension( callerFilePathName, FileExtension );
		Audit audit = getOrCreateAuditFile( auditFilePathName );
		using( AuditFile auditFile = audit.newFile() )
		{
			auditFile.WriteLine( new string( '-', 80 ) );
			auditFile.WriteLine( callerMemberName );
			auditFile.WriteLine( "" );
			procedure.Invoke( auditFile.WriteLine );
		}
	}

	readonly string auditFilePathName;
	bool isNew = true;

	Audit( string auditFilePathName )
	{
		this.auditFilePathName = auditFilePathName;
		truncate( auditFilePathName );
	}

	AuditFile newFile()
	{
		AuditFile auditFile = new( auditFilePathName );
		if( isNew )
			isNew = false;
		else
			auditFile.WriteLine( "" );
		return auditFile;
	}

	static void truncate( string pathName ) => SysIo.File.WriteAllText( pathName, "" );

	static Audit getOrCreateAuditFile( string auditFilePathName )
	{
		return getOrCreate( auditsByPathName, auditFilePathName, () => new Audit( auditFilePathName ) );
	}

	static V getOrCreate<K, V>( IDictionary<K, V> dictionary, K key, Sys.Func<V> valueFactory )
	{
		lock( dictionary )
		{
			if( dictionary.TryGetValue( key, out V? existingValue ) )
				return existingValue;
			V newValue = valueFactory.Invoke();
			dictionary.Add( key, newValue );
			return newValue;
		}
	}

	[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
	sealed class AuditFile : Sys.IDisposable
	{
		readonly string outputFilePath;
		readonly string endOfLine;
		readonly SysIo.StreamWriter streamWriter;

		public AuditFile( string outputFilePath, string endOfLine = "\n" )
		{
			this.outputFilePath = outputFilePath;
			this.endOfLine = endOfLine;
			SysIo.FileStreamOptions fileStreamOptions = new();
			fileStreamOptions.Access = SysIo.FileAccess.Write;
			fileStreamOptions.Mode = SysIo.FileMode.Truncate;
			fileStreamOptions.Share = SysIo.FileShare.Read;
			fileStreamOptions.Options = SysIo.FileOptions.WriteThrough | SysIo.FileOptions.SequentialScan;
			SysIo.FileStream stream = new( outputFilePath, SysIo.FileMode.Append, SysIo.FileAccess.Write, SysIo.FileShare.Read, 4096, SysIo.FileOptions.WriteThrough | SysIo.FileOptions.SequentialScan );
			streamWriter = new SysIo.StreamWriter( stream, SysText.Encoding.UTF8 );
		}

		public void WriteLine( string text )
		{
			streamWriter.Write( text );
			streamWriter.Write( endOfLine );
		}

		public void Dispose()
		{
			streamWriter.Dispose();
		}

		public override string ToString() => outputFilePath;
	}
}
