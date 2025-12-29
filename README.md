# README

As I'd been going back to study Python, I thought the next step was to integrate it
with .NET.

You know, functional programming ... A scripting language seemed the best way to
implement this. You can do functional programming in .NET, but it has to be done in the
context of an object, so ...

As Python is one of the most widely used languages, I assumed support for integration
with .NET would be mature by now. Unfortunately, it isn't, and I'm not sure I'd recommend
using it in production, especially in Core 9+, as it isn't fully compatible.

Particularly problematic was getting modules to load from a .venv virtual environment.
None of the recommended approaches in forum posts worked. I eventually found a solution.

I should have guessed that there was trouble in paradise in paradise to the paucity of
information and demo code on the pythonNET site.
