using System;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace IS4.SFI
{
    /// <summary>
    /// Provides an implementation of <see cref="ISparqlResultsWriter"/> that produces
    /// a SPARQL query from the results. The query uses DISTINCT, and does not store
    /// results with blank nodes.
    /// </summary>
    internal class SparqlValuesQueryWriter : ISparqlResultsWriter
    {
        event SparqlWarning ISparqlResultsWriter.Warning {
            add { }
            remove { }
        }

        readonly SparqlFormatter formatter = new();

        public void Save(SparqlResultSet results, string filename)
        {
            using var writer = File.CreateText(filename);
            Save(results, writer);
        }

        public void Save(SparqlResultSet results, TextWriter output)
        {
            switch(results.ResultsType)
            {
                case SparqlResultsType.Boolean:
                    output.Write("ASK WHERE { FILTER (");
                    output.Write(formatter.FormatBooleanResult(results.Result));
                    output.WriteLine(") }");
                    break;

                case SparqlResultsType.VariableBindings:
                    var variables = String.Join(" ", results.Variables.Select(v => "?" + v).DefaultIfEmpty("*"));
                    output.Write("SELECT DISTINCT ");
                    output.Write(variables);
                    output.WriteLine(" WHERE {");
                    if(variables != "*")
                    {
                        output.Write("  VALUES (");
                        output.Write(variables);
                        output.WriteLine(") {");
                        foreach(var result in results.Results)
                        {
                            if(!result.Any(p => p.Value.NodeType == NodeType.Blank))
                            {
                                output.Write("    (");
                                output.Write(String.Join(" ", results.Variables.Select(
                                    v => result.TryGetValue(v, out var node)
                                        ? formatter.Format(node).Trim()
                                        : "UNDEF"
                                )));
                                output.WriteLine(")");
                            }
                        }
                        output.WriteLine("  }");
                    }
                    output.WriteLine("}");
                    break;

                default:
                    throw new ArgumentException("Unknown results type.", nameof(results));
            }
        }
    }
}
