using Microsoft.CodeAnalysis;
using System.IO;
using System.Text;

namespace IS4.SFI
{
    [Generator]
    public class ImportSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            var prefix = Path.GetFileNameWithoutExtension(context.Compilation.AssemblyName) + ".";

            var sb = new StringBuilder();
            sb.AppendLine("using Visible = System.Runtime.CompilerServices.InternalsVisibleToAttribute;");
            sb.AppendLine("using Forward = System.Runtime.CompilerServices.TypeForwardedToAttribute;");
            foreach(var assembly in context.Compilation.SourceModule.ReferencedAssemblySymbols)
            {
                if(assembly.Name.StartsWith(prefix))
                {
                    sb.AppendLine($"[assembly: Visible(\"{assembly.Name}\")]");
                    foreach(var symbol in assembly.GlobalNamespace.GetMembers())
                    {
                        var name = symbol.Name;
                        if(name == "IS4")
                        {
                            Visit(symbol, name);
                        }
                    }
                    void Visit(INamespaceOrTypeSymbol symbol, string name)
                    {
                        switch(symbol)
                        {
                            case INamedTypeSymbol typeSymbol:
                                if(typeSymbol.CanBeReferencedByName && typeSymbol.DeclaredAccessibility == Accessibility.Public)
                                {
                                    if(typeSymbol.IsGenericType)
                                    {
                                        name += $"<{new string(',', typeSymbol.Arity - 1)}>";
                                    }
                                    sb.AppendLine($"[assembly: Forward(typeof({name}))]");
                                }
                                break;
                            case INamespaceSymbol namespaceSymbol:
                                foreach(var innerSymbol in namespaceSymbol.GetMembers())
                                {
                                    Visit(innerSymbol, name + "." + innerSymbol.Name);
                                }
                                break;
                        }
                    }
                }
            }
            context.AddSource($"Referenced.g.cs", sb.ToString());

            sb.Clear();
            sb.AppendLine($"namespace {context.Compilation.AssemblyName}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class Provider");
            sb.AppendLine("    {");
            sb.AppendLine("        public static System.Reflection.Assembly Assembly { get; } = typeof(Provider).Assembly;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            context.AddSource($"Provider.g.cs", sb.ToString());
        }
    }
}
