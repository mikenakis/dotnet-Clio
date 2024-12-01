# Clio<br><sub><sup>A dotnet library for parsing command-line arguments.</sub></sup>

<p align="center">
  <img title="MikeNakis.Clio logo" src="MikeNakis.Clio-Logo.svg" width="256" />
</p

<sup>Note: The above image looks fine in most markdown renderers, but not in Visual Studio, whose built-in markdown renderer is broken nowadays.
[Someone has brought it to their attention](https://developercommunity.visualstudio.com/t/10774870), and last I checked they were "investigating".</sup>

## Introduction

The main thing to know about Clio is that you can always start with `new Clio.ArgumentParser()`, 
and let auto-completion and inline documentation guide you through the rest.

Note that the terminology around command-line argument parsing is not standardized,
so if some term is unclear, be sure to look it up in the glossary below.

## Example 1

```csharp
namespace Program1;

class Program
{
    internal static void Main( string[] tokens )
    {
        Clio.ArgumentParser parser = new();
        Clio.IArgument<string> name = parser.AddStringOption( "name", defaultValue: "world" );
        parser.Parse( tokens );
        System.Console.WriteLine( $"Hello, {name.Value}!" );
    }
}
```

Here is what happens when you run the above:

```
C:\>Program1
Hello, world!

C:\>Program1 --name=John
Hello, John!
```

## Example 2

```csharp
namespace Program1;

class Program
{
    internal static void Main( string[] tokens )
    {
        Clio.ArgumentParser parser = new();
        Clio.IArgument<bool> verbose = parser.AddSwitch( "verbose" );
        parser.AddVerb( "new", "create a new item", parser =>
            {
                Clio.IArgument<string> name = parser.AddRequiredStringPositional( "name" );
                if( !parser.TryParse() )
                    return;
                if( verbose.Value )
                    System.Console.WriteLine( $"Creating new item {name.Value}..." );
                ...
            } );
        parser.AddVerb( "list", "list items", parser =>
            {
                if( !parser.TryParse() )
                    return;
                if( verbose.Value )
                    System.Console.WriteLine( $"Listing all items..." );
                ...
            } );
        parser.Parse( tokens );
    }
}
```

## Features

- Kinds of arguments supported:
  - named arguments:
    - switches (e.g. `Program1 --verbose`)
    - options (e.g. `Program1 --verbosity=quiet`)
  - positionals (e.g. `Program1 inputfile.txt outputfile.txt`)
  - verbs (e.g. `Program1 new ...` or `Program1 list ...`)
- Single letter names.
  - Besides a long name, each named argument can also have a single-letter name.
- Single-letter argument grouping.
  - Multiple single-letter argument names can be grouped into one.
  - For example `-latr` is the same as `-l -a -t -r`.
- Argument data types make sense from a programmatic point of view.
  - The value of a switch is of type `bool`, indicating whether the switch was supplied.
  - The value of an option or positional is commonly of type `string`, but it can be of any type, even some `enum` defined by the programmer.
  - Required arguments are always of a non-nullable type.
  - Optional arguments typically are of a nullable type, where `null` indicates that the argument was not supplied; however:
  - The programmer may create an optional argument with a default value; in this case, the argument is still optional from the user's point of view, but from the programmer's point of view the argument is of a non-nullable type.
- Preset values for options.
  - By default, the user must follow an option with an equals-sign and then a value. 
  - The programmer can specify that an option has a preset value, in which case the user may supply the option without an equals-sign and a value. In this case, the option will receive its preset value.
- Fully automated usage help.
  - Clio has a built-in `-h`, `--help` switch which displays extensive automatically generated usage help text, using descriptions supplied by the programmer.
  - If using verbs, then each verb has its own `--help` switch which displays help specifically for that verb.
- Adjustable help output width.
  - When showing usage help, Clio performs line-wrapping with word-break on the 120th column by default, but the programmer can specify a different column number to wrap at.
- Fully taken-care-of failure.
  - When Clio determines that there is something wrong with the formulation of the command-line, (or if the `--help` option is supplied,) Clio shows all necessary messages to the user; then:
	- If the `TryParse()` method was invoked, it returns `false`, in which case all the programmer needs to do is terminate the program.
	- If the `Parse()` method was invoked, it terminates the current process, so there is nothing else for the programmer to do.
- Custom term for "verb".
  - The programmer can specify a custom term to appear in usage help instead of "verb". (e.g. "command" or "subcommand".)
- Extensive error checking.
  - If the programmer makes any mistake in specifying the syntax of the command-line, Clio throws an exception which clearly identifies the mistake.
  - When running a debug build, Clio also invokes each verb handler once during startup, allowing it to specify the syntax of the arguments of the verb. Thus, all mistakes are caught during startup, even for verbs that were not supplied.
- ...and more.

## Limitations

### Limitations without a workaround

- No support (yet) for the end-of-named-arguments argument (`--`) to signify that everything after that point in the command line is a positional argument.
  - Support for the end-of-named-arguments argument will be added soon.
- No support (yet) for the more-arguments-in-file argument (`@file`) to specify a file containing additional command-line arguments, one argument per line.
  - Support for the more-arguments-in-file argument will be added soon.

### Limitations with a workaround

- No support for repeatable switches, as in `Program1 --switch1 --switch1`. Every switch can be supplied at most once.
  - **Workaround:** Turn the switch into an option of type `int`, with a default of `0` so that it can be omitted, and a preset of `1` so that `--switch1` can be used instead of `--switch1=1`.
  - Support for repeatable switches will be added in the future.
- No support for repeatable options, as in `Program1 --option1=<value1> --option1=<value2>`. Every option can be supplied at most once.
  - **Workaround:** In the "description" of the option, explain that the value of the option is a `;`-separated list of values, and do the splitting yourself.
  - Support for repeatable options will be added in the future.
- No support for meta-options, for example `--option1=<name>=<value>`.
  - **Workaround:** In the "description" of the option, explain that the value of the option is a `;`-separated list of `name=value` pairs, and do the splitting and the parsing yourself.
  - Support for meta-options will be added in the future. (After adding support for repeatable options.)
- No support for invertable switches. For example, `git config` has options like `--[no-]global`, which means that either `--global` or `--no-global` can be used. Other utilities allow prefixing switch names with either `-` or `+`, which inverts the effect of the switch.
  - **Workaround:** Specify each option and its inverse option separately.
  - Support for invertable switches and options will be added in the future.
- No support for a greedy positional, which consumes all remaining non-option tokens in the command-line.
  - **Workaround:** Use a regular positional, explain in the description that it is a `;`-delimited list of values, and do the parsing yourself.
  - Support for a greedy positional will be added in the future.
- No support for a number-only option. For example, `grep` supports `-NUM` and says that it is the same as `--context=NUM`.
  - **Workaround:** Use normal, `--option=N` syntax.
  - Support for a number-only option will be added in the future.
- No support for mutually exclusive options. For example, in the usage of `git` we see `[--paginate | --no-pager]` which means that either `--paginate` or `--no-pager` can be specified, but not both.
  - **Workaround:** Define each option separately, add help footnotes explaining mutual exclusions, and implement the mutual exclusion logic yourself.
  - Support for mutually exclusive options might be added in the future.
- No support for supplying partial long option names. Some utilities allow supplying fewer characters than the full option name, as long as enough characters are supplied as to be inambiguous.
  - **Workaround:** If you do not want to require the user to always supply the full long name of an option, add a single-letter name to that option.
  - Support for supplying partial long option names is not planned, because it defeats the purpose of having long option names in the first place, which is to allow writing self-explanatory scripts by fully spelling out each option. For those who want convenience when typing commands at the prompt, that is what single-letter option names are for.

### Limitations that are so by design.

- No support for space as the separator between a named argument and its value: only the equals sign (`=`) can be used.
  - Space as a separator is error-prone, and when an error occurs, it is difficult to give a useful error message.
- No support for fault tolerance: if the slightest thing goes wrong with the formulation of the command-line, an error message gets printed, and any attempt to read the value of any argument after that results in an exception.
  - There is no such thing as "fault tolerance"; it is just a euphemism for _**silent failure**_. I do not engage in, endorse, or facilitate, any form of silent failure.
- No support for case-insensitive argument names: all argument names must be supplied exactly as specified.
  - This is so by design.

## License

This will probably change soon, but for now, this is what it is:

_**All rights reserved.**_

For more information, see [LICENSE.md](LICENSE.md)

## Coding style

This project uses _**my very own™**_ coding style.

More information: [michael.gr - On Coding Style](https://blog.michael.gr/2018/04/on-coding-style.html)

## Glossary

_**Argument**_: A programmatic construct that describes part of the syntax of the command-line. After successful parsing of a command-line, an argument typically contains a value that has been parsed from the command-line and has been converted to a form that is readily usable by the programmer.

_**Default (value)**_: A value that will be used for an option or a positional if the user omits supplying that argument. Note that a default value can be specified only for optional arguments.

_**Named argument**_: An argument that is identified in the command-line by either a dash followed by a single-letter name, or a double dash followed by a (long) name. For example: `-h`, `--help`.

_**Nullable**_: An argument which is of a nullable type. A value of `null` indicates that the argument was not supplied. Required arguments are of course non-nullable, but note that optional arguments with a default, are also non-nullable. Thus, the programmatic term _**nullable**_ is not synonymous with _**optional**_, which is a user-experience term.

_**Option**_: A named argument which accepts a parameter. For example: `Program1 --verbosity=quiet`. 

_**Optional (argument)**_: An argument that the user may omit supplying. Switches are optional by definition; options and positionals can be either optional or required.

_**Positional (argument)**_: An argument which is identified by its position relative to other positional arguments in the command-line. For example, `Program1 inputfile.txt outputfile.txt`. Note that positionals also have a name, but this name is only used for referring to the argument when displaying usage help.

_**Preset (value)**_: A value that will be used for an option if the user supplies the option without following it with an equals-sign and a value.

_**Programmer**_: The person who writes the program that makes use of `Clio`.  The programmer uses `Clio` to describe the syntax of the command-line, and to parse a command-line supplied by the user, according to that syntax.

_**Required (argument)**_: An argument that the user must supply, or else there will be an error. If verbs are defined, then one verb must be supplied.

_**Supplied (argument)**_: An argument that has been included, by the user, in the command-line.

_**Switch**_: A named argument which accepts no parameters. For example: `Program1 --verbose`.

_**Token**_: One of the individual strings that make up a command-line. Traditionally this is called an "argument", but in the context of Clio the term "argument" is used for a different purpose.

_**User**_: The person who uses the program written by the programmer. The user supplies the command-line that gets parsed by Clio according to the syntax specified by the programmer.

_**Verb**_: A special kind of argument which is identified by a word and has an argument list of its own; for example `Program1 new ...` or `Program1 list ...`.

## To do:

- TODO: Add support for including some free text in usage help: a short help prefix and a number of help footnotes.
- TODO: Remove all strictly-speaking-unnecessary functionality from the Clio interface, move it to extension methods.
- TODO: Add support for the end-of-options switch (`--`).
- TODO: Add support for the more-arguments-in-file argument.
- TODO: Add more functionality for creating enum arguments.
- TODO: Clarify the message about combining short-form options into one argument by mentioning that it applies to switches and to options with a preset. Give a correct example by searching for either switches or options with a preset.  (As it stands, the example will include any named options with a short-form name, and this may include an option without a preset, which cannot really be combined.)
- TODO: Improve user-exception messages.
- TODO: Rewrite the argument name validation functions to _not_ work with regular expressions and add more exceptions that give more detailed explanations as to what is wrong with an invalid argument name.
- TODO: Introduce invertable options, as in `--[no-]option` and as in `-o|+o` or possibly `-o-|-o+`.
- TODO: Introduce repeatable switches, whose type is `int` instead of `bool`.
- TODO: Introduce repeatable options, whose type is `IReadOnlyList<T>` instead of `<T>`.
- TODO: Introduce meta-options, as in `--option=<name>=<value>`, whose type is `IReadOnlyDictionary<K,V>`.
- TODO: Introduce greedy parameters. A greedy parameter consumes all remaining non-named arguments in the command-line, and its value is of type `IReadOnlyList<T>` instead of `T`.
- TODO: Introduce a number-only option, as per `grep` option `-NUM` which is the same as `--context=NUM`.
- TODO: Introduce hidden arguments. (Arguments which are excluded from help.)
- TODO: Introduce argument categories, so that they can be grouped by category when displaying usage help, as grep does.
- TODO: Introduce verb categories, so that they can be grouped by category when displaying usage help, as git does.

## Will not do:

- NOT-TODO: When a usage error occurs, show the full argument list and indicate the precise location of the error the way compilers do.
  - This is not really necessary.
- NOT-TODO: When a usage error occurs, show short usage help after the error message and before "use --h for more".
  - This is not really necessary. The error message and the suggestion to use --help is enough.
- NOT-TODO: Add the ability to specify a verb as the default verb.
  - This is probably not a good idea.

## Notes

- `Clio` was created before `System.CommandLine` was made available by Microsoft. `Clio` is more powerful and easier to use than `System.CommandLine`.
- According to "Build Awesome Command-Line Applications in Ruby 2" a switch doesn't take arguments, while a flag does. Their idea of both switch and flag is retarded. By long-established practice, a flag is boolean, so it is synonymous to a switch. Furthermore, a boolean is either true or false, and this can be signified by the presence or absence of the argument in the command-line, so it does not make sense to say that a switch or a flag takes arguments.

---------------

Cover image: The Clio logo, by michael.gr; based on detail of "Woman reading a scroll", (very possibly [Clio, the Muse of History](https://en.wikipedia.org/wiki/Clio),) Louvre Museum CA2220: Attic red-figure lekythos, ca. 430 BC; public domain image obtained from [Wikimedia Commons](https://commons.wikimedia.org/wiki/File:Muse_reading_Louvre_CA2220.jpg).
