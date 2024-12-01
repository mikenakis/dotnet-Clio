namespace Clio.ClioKit;

using Sys = System;
using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

///<summary>Frequently used stuff that needs to be conveniently accessible without a type name qualification.</summary>
///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class GlobalStatics
{
	///<summary>Returns <c>true</c>.</summary>
	///<remarks>Same as <c>if( true )</c>, but without danger of receiving warning CS0162: "Unreachable code detected".
	///Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool True => true;

	///<summary>Returns <c>false</c>.</summary>
	///<remarks>Same as <c>if( false )</c>, but without danger of receiving warning CS0162: "Unreachable code detected".
	///Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool False => false;

	///<summary>Returns <c>true</c> if <c>DEBUG</c> is defined, <c>false</c> otherwise.</summary>
	///<remarks>Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
#if DEBUG
	public static bool DebugMode => true;
#else
	public static bool DebugMode => false;
#endif

	public static bool Debugging => DebugMode && SysDiag.Debugger.IsAttached;

	///<summary>Identity function.</summary>
	///<remarks>useful as a no-op lambda and sometimes as a debugging aid.</remarks>
	public static T Identity<T>( T value ) => value;

	/// <summary>Performs an assertion.</summary>
	/// <remarks>Invokes the supplied <paramref name="check" /> function, passing it the supplied <paramref name="value"/>.
	/// If the <paramref name="check"/> function returns <c>false</c>,
	/// then the <paramref name="value"/> is passed to the supplied <paramref name="exceptionFactory"/> function, and the returned <see cref="Sys.Exception"/> is thrown.
	/// (Though the factory may just as well throw the exception instead of returning it.)
	/// This function is only executed (and the supplied <paramref name="value"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert<T>( T value, Sys.Func<T, bool> check, Sys.Func<T, Sys.Exception> exceptionFactory )
	{
		if( check.Invoke( value ) )
			return;
		fail( () => exceptionFactory.Invoke( value ) );
	}

	/// <summary>Performs an assertion.</summary>
	/// <remarks>If the given <paramref name="condition"/> is <c>false</c>, the supplied <paramref name="exceptionFactory"/> is invoked, and the returned <see cref="Sys.Exception"/> is thrown.
	/// (Though the factory may just as well throw the exception instead of returning it.)
	/// This function is only executed (and the supplied <paramref name="condition"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert( [SysCodeAnalysis.DoesNotReturnIf( false )] bool condition, Sys.Func<Sys.Exception> exceptionFactory ) //
	{
		if( condition )
			return;
		fail( exceptionFactory );
	}

	/// <summary>Performs an assertion.</summary>
	/// <remarks>If the given <paramref name="condition"/> is <c>false</c>, an <see cref="AssertionFailureException"/> is thrown.
	/// This function is only executed (and the supplied <paramref name="condition"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert( [SysCodeAnalysis.DoesNotReturnIf( false )] bool condition ) //
	{
		if( condition )
			return;
		fail( () => new AssertionFailureException() );
	}

	public static T WithAssertion<T>( this T self, Sys.Func<bool> assertion )
	{
		Assert( assertion.Invoke() );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<T, bool> assertion )
	{
		Assert( assertion.Invoke( self ) );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<bool> assertion, Sys.Func<Sys.Exception> exceptionFactory )
	{
		Assert( self, _ => assertion.Invoke(), _ => exceptionFactory.Invoke() );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<T, bool> assertion, Sys.Func<T, Sys.Exception> exceptionFactory )
	{
		Assert( self, assertion, exceptionFactory.Invoke );
		return self;
	}

	public static T WithNotNullAssertion<T>( this T? self )
	{
		Assert( self is not null );
		return self!;
	}

	[SysDiag.DebuggerHidden]
	static void fail( Sys.Func<Sys.Exception> exceptionFactory )
	{
		Sys.Exception exception = exceptionFactory.Invoke();
		if( Debugging && !KitHelpers.FailureTesting.Value )
		{
			Breakpoint();
			return;
		}
		throw exception;
	}

	/// <summary>Returns the supplied pointer unchanged, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableReference ) where T : class //
	{
		Assert( nullableReference != null );
		return nullableReference;
	}

	/// <summary>Returns the supplied pointer unchanged, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableReference, Sys.Func<Sys.Exception> exceptionFactory ) where T : class //
	{
		Assert( nullableReference != null, exceptionFactory );
		return nullableReference;
	}

	/// <summary>Converts a nullable value to non-nullable, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableValue ) where T : struct //
	{
		Assert( nullableValue.HasValue );
		return nullableValue.Value;
	}

	/// <summary>Converts a nullable value to non-nullable, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableValue, Sys.Func<Sys.Exception> exceptionFactory ) where T : struct //
	{
		Assert( nullableValue.HasValue, exceptionFactory );
		return nullableValue.Value;
	}

	[SysDiag.DebuggerHidden]
#pragma warning disable CA1021 //CA1021: "Avoid `out` parameters"
	public static void NotNullCast<T, U>( T? input, out U output ) where U : T where T : class => output = (U)NotNull( input );
#pragma warning restore CA1021 //CA1021: "Avoid `out` parameters"

	/// <summary>If a debugger is attached, hits a breakpoint and returns <c>true</c>; otherwise, returns <c>false</c></summary>
	[SysDiag.DebuggerHidden]
	public static void Breakpoint()
	{
		if( SysDiag.Debugger.IsAttached )
			SysDiag.Debugger.Break(); //Note: this is problematic due to some Visual Studio bug: when it hits, you are prevented from setting the next statement either within the calling function or within this function.
	}

	public static Sys.Exception? TryCatch( Sys.Action procedure )
	{
		Assert( !KitHelpers.FailureTesting.Value );
		KitHelpers.FailureTesting.Value = true;
		try
		{
			procedure.Invoke();
			return null;
		}
		catch( Sys.Exception exception )
		{
			return exception;
		}
		finally
		{
			KitHelpers.FailureTesting.Value = false;
		}
	}
}
