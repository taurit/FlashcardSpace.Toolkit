# Architecture Decision Record: how users should input the configuration parameters? 

## Problem Statement

The purpose of the `FlashcardSpace.Toolkit` software is to take an input file (like text, movie subtitles, etc.) and create useful flashcards from it.

Some parameters of this process can be auto-detected, for example:

- The **input file type** can be detected by filename extension conventions or the file content. 
- The **input content language** can be quite reliably detected among the supported languages (like English, Spanish, Polish, ...).

But some parameters must be explicitly set by the user to fit their intention:

- The **output language** displayed on the reverse side of the flashcards.
- The **API keys** to online services like OpenAI API or Text-to-speech API can’t be distributed with the source code, and every developer or user must provide their own values.
- Steering the general flow (e.g., skipping generation of flashcard images) might be desired for some use cases.

## Options considered

### 1. Use a JSON configuration file allowing describing the flow and its parameters.

One way to solve this issue is to separate the configuration into a configuration file like:

```jsonc
{
  "InputLanguage": "Spanish",
  "OutputLanguage": "English"
  // other options...
}
```

The program could then be distributed as something like:

```Text
fstoolkit.exe                 # main executable 
fstoolkit.config.json         # configuration file with some placeholder values
fstoolkit.config.schema.json  # schema would help with config validation in IDEs
```

Users could then just change the configuration file and run the toolkit without the need to modify and compile source code. Most users would likely prefer that, at least as their first experience when they find the project.   

### 2. Hardcode settings within a C# project

Configuration could also be hardcoded within the C# file and compiled into the application: 

```c#
var settings = new Settings(
                    inputLanguage: Languages.Spanish,
                    outputLanguage: Languages.English,
                    ... // other options
               ); 
```

This approach allows quick development and refactoring. This is because every typo or missing option instantly manifests as a compilation error in the IDE.

It's tempting to use this form of configuration while software is in alpha/beta stages because it helps me be productive, and there are no other users at this point anyway ;)  

### 3. The command-line interface (CLI)

A sort of the middle ground is to move the configuration outside the compiled application but have it passed as command line arguments:

```ps1
fstoolkit.exe --inputLanguage "Spanish" --outputLanguage "English" --inputFile ebook.txt
```

With the use of a library to handle parsing command line arguments, this can be implemented with few lines of code and provide a good experience for users who can use the command line. Also, argument validation should be easier to maintain (less code) than in the case of JSON input.

### 4. Interactive GUI (windows or just an interactive console)

The easiest variant for the user is to provide an interactive GUI application. Every time the application was run, the user would see a list of parameters and modify them accordingly before generating flashcards.

This could be either a terminal-based "text GUI" or a desktop GUI. A good-looking text-based GUI could be implemented with a library like [Spectre.Console](https://spectreconsole.net/) and be cross-platform. A desktop GUI could be implemented in many ways, with frameworks like WPF (Windows only), Avalonia (cross-platform), MAUI, web app, web app wrapped in a desktop frame like Electron etc.

## Decision and rationale

While all options have their pros and cons, I'll start with **the command-line interface (CLI)** approach.

**Pros:**

- users can run it without the need to touch the source code (even though the lack of GUI limits it to power users)
- relatively quick to implement
- no user interaction is assumed, so automating e2e tests is a bit easier
- works cross-platform

**Cons:**

- Users who can’t use the console will get discouraged from trying the application