# README

As I'd been going back to study Python, I thought the next step was to integrate it
with .NET.

You know, functional programming ... A scripting language seemed the best way to
implement this. You can do functional programming in .NET, but it has to be done in the
context of an object, so ...

As Python is one of the most widely used languages, I assumed support for integration
with .NET would be mature by now. Unfortunately, it isn't, and I'm not sure I'd recommend
using it in production, especially in Core 9+, as it isn't fully compatible. The mandatory
ShutDown() method has a dependency on BinaryFormatter, which has been removed from the
platform. The only practical option is to trap an exception and hope for the best. (There
was a work-around, but it no longer works. I have kept a reference to it in the code but
it caused an untrappable memory corruption exception in Core 8.)

Particularly problematic was getting modules to load from a .venv virtual environment.
None of the recommended approaches in forum posts worked. I eventually found a solution.

I should have guessed that there was trouble in paradise on account of the paucity of
information and demo code on the pythonNET site.

The aim in making the Python component self-maintaining is to intelligently parse the
Python environment .venv config file to loate the correct references for the Python
library. Exaclt how this is done depends on how the Python runtimes were installed. It
has to be consistent for it to be automatically upgradable. For Windows, I used the
provided installers. For Mac OS, I might have used Homebrew, but I'm not entirely sure -
possibly I used a different utility which in turn used brew. But the important thing is
to keep track of the process and always do it the same way. At the moment, this is just
a demo of how you can make a cross-platform .NET-to-Python implementation.
