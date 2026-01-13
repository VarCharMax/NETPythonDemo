# README

The aim of this project is to provide a manager class for pythonnet.

It has two main aims:

- Take care of the initialisation of the pythonet environment.
- Make pythonnet compatible with Python virtual environments.

Additionally, it addresses a deficiency in Python virtual environment utilies such as venv, which cache the specified Python runtime exe, but do not cache the dll runtime.

The Initialiser can run in two modes:

- default - it locates and assigns the highest system-wide Python installation that is compatible with pythonnet, using techniques specific to each operating system.
- ve - it locates and caches the specified Python dll version according to the ve configuration file. It updates filepaths, and also runs the associated activate and deactivate batch files.

In both cases, it verifies that the specified Python version is compatible with the pythonnet library.

The copying and caching is performed by a Grunt file. This can be set to run according to specified triggers, or simply run once when the project is being created, as per the venv utility.
