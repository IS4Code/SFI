[Semantic File Inspector](https://github.com/IS4Code/SFI/) Application
==========

This is the primary project to use when wishing to run
the [Semantic File Inspector](https://github.com/IS4Code/SFI/) as an embedded component.

The project contains a hierarchy of classes derived from `Inspector`, capable
of easily setting up the process of file format inspection. This class is inherited
by `ComponentInspector`, which is capable of storing configurable collections of
components, and itself inherited by `ExtensibleInspector`, adding
support for plugins.

When intending to use one of these classes, the user should derive from the one
of them covering the user's needs, adding the desired components (such as by
calling the `ComponentInspector.LoadAssembly` method), and either use
the inspector by calling methods on the instance, or through the `Application`
class which can be controlled using command-line arguments (see
[SFI.ConsoleApp](https://github.com/IS4Code/SFI/tree/HEAD/SFI.ConsoleApp)
for an example how to set up the environment).

Documentation for the command-line API, configuration, and plugin mechanism,
can be found in the [wiki](https://github.com/IS4Code/SFI/wiki).
