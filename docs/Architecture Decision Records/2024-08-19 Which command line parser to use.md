# Architecture Decision Record: Which command line parser to use?

## Problem Statement

In another ADR document, [How users should input the configuration parameters.md](./2024-08-19 How users should input the configuration parameters.md), I decided to design the toolkit using the CLI model.

There's no point in re-inventing the wheel and implementing a code to parse command line arguments by myself, so let's just choose the best library for the job I can find.  

## Options considered

I found the following libraries:

- [Command Line Parser](https://github.com/commandlineparser/commandline). I used this one before. It has gained popularity (over 4500 stars on GitHub), it is straightforward to use. Unfortunately, the last traces of development or maintenance stopped around May 2022 so the project looks like it's dead.
- [System.CommandLine](https://github.com/dotnet/command-line-api). This is a **preview** of a library maintained by Microsoft ([docs here](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)). It looks like it started with a hackathon project, then got developed into a library. Unfortunately, the project also shows little activity. There is no stable version of the package ever released, and the last pre-release package is dated Feb 2022. Also, I tried it, and I find design non-intuitive.
- [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) were recommended to me by the Gemini AI ;) It has 2.2k GitHub stars, looks easy to use, and... unfortunately, is also declared as being in maintenance mode, with no future releases planned. 
- [Spectre.Console.CLI](https://spectreconsole.net/). I tried `Spectre.Console` before (really cool library!), and was happy to found they also develop `Spectre.Console.CLI` to solve the argument parsing problem.
- Lastly, I could just give up on using any library and parse the arguments from `Main(string[] args)` manually. It could be done, but it adds code on the project side that can be handled better by a generic library.

## Decision and rationale

I chose `Spectre.Console.CLI`. As of 2024, it's beautifully maintained, the issues are looked at regularly. It has more than 100 contributors listed and 9.1k GitHub stars, so it's most likely to still work when .NET 9 and 10 are released, and hopefully will continue into the future. 