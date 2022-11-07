using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
	/// <summary>
	/// The main application for analyzing input files and producing output.
	/// </summary>
	/// <typeparam name="TInspector">The type of <see cref="Inspector"/> to use.</typeparam>
    public class Application<TInspector> : CommandApplication where TInspector : Inspector, new()
    {
        readonly IApplicationEnvironment environment;

		TextWriter writer;

		protected override TextWriter LogWriter => writer;

		protected override int WindowWidth => environment.WindowWidth;

		readonly InspectorOptions options;

		static readonly IEnumerable<string> modeNames = Enum.GetNames(typeof(Mode)).Select(n => n.ToLowerInvariant());

		/// <summary>
		/// Creates a new instance of the application from the supplied environment.
		/// </summary>
		/// <param name="environment">
		/// An instance of <see cref="IApplicationEnvironment"/>
		/// providing manipulation with the environment.
		/// </param>
		public Application(IApplicationEnvironment environment)
        {
            this.environment = environment;
			writer = environment.LogWriter;

			options = new InspectorOptions();
			options.PrettyPrint = true;
			options.DirectOutput = true;
			options.NewLine = environment.NewLine;
			options.HideMetadata = true;
		}

		Mode? mode;
		List<string> inputs = new List<string>();
		List<string> queries = new List<string>();
		List<(bool result, Predicate<string> matcher)> componentMatchers = new List<(bool result, Predicate<string> matcher)>();
		Regex mainHash;
		string output;

		bool quiet;
		bool rootSpecified;
		bool dataOnly;

		/// <summary>
		/// Runs the application with the supplied arguments.
		/// </summary>
		/// <param name="args">The arguments to the application.</param>
		public async ValueTask Run(params string[] args)
        {
			try{
				Parse(args);

				Banner();

				if(mode == null)
				{
					throw new ApplicationException($"Mode must be specified (one of {String.Join(", ", modeNames)})!");
				}

				var inspector = new TInspector();
				inspector.OutputLog = writer;
				inspector.AddDefault();

				// Print the available components
				switch(mode)
				{
					case Mode.List:
						LogWriter.WriteLine("Included components:");
						PrintComponents("analyzer", inspector.Analyzers);
						PrintComponents("data-format", inspector.DataAnalyzer.DataFormats);
						PrintComponents("xml-format", inspector.XmlAnalyzer.XmlFormats);
						PrintComponents("container-format", inspector.ContainerProviders);
						PrintComponents("data-hash", inspector.DataAnalyzer.HashAlgorithms);
						PrintComponents("file-hash", inspector.FileAnalyzer.HashAlgorithms);
						PrintComponents("pixel-hash", inspector.ImageDataHashAlgorithms);
						PrintComponents("image-hash", inspector.ImageHashAlgorithms);
						return;
				}

				if(inputs.Count == 0 || output == null)
				{
					throw new ApplicationException("At least one input and an output must be specified!");
				}

				if(quiet)
				{
					inspector.OutputLog = writer = TextWriter.Null;
				}

				// Filter out the components based on arguments

				int componentCount = 0;
				FilterComponents("analyzer", inspector.Analyzers, ref componentCount);
				FilterComponents("data-format", inspector.DataAnalyzer.DataFormats, ref componentCount);
				FilterComponents("xml-format", inspector.XmlAnalyzer.XmlFormats, ref componentCount);
				FilterComponents("container-format", inspector.ContainerProviders, ref componentCount);
				FilterComponents("data-hash", inspector.DataAnalyzer.HashAlgorithms, ref componentCount);
				FilterComponents("file-hash", inspector.FileAnalyzer.HashAlgorithms, ref componentCount);
				FilterComponents("pixel-hash", inspector.ImageDataHashAlgorithms, ref componentCount);
				FilterComponents("image-hash", inspector.ImageHashAlgorithms, ref componentCount);

				writer.WriteLine($"Included {componentCount} components in total.");

				if(mainHash != null)
                {
					// Set the primary ni: hash
					var hash = inspector.DataAnalyzer.HashAlgorithms.FirstOrDefault(h => mainHash.IsMatch(DataTools.GetUserFriendlyName(h)));
					if(hash == null)
                    {
						throw new ApplicationException("Main hash cannot be found!");
					}
					inspector.DataAnalyzer.ContentUriFormatter = new NiHashedContentUriFormatter(hash);
                }

				// Load the input files from the environment
				var inputFiles = inputs.SelectMany(input => environment.GetFiles(input));
				
				options.Queries = queries.SelectMany(query => environment.GetFiles(query));

				foreach(var analyzer in inspector.Analyzers.OfType<IHasFileOutput>())
                {
					analyzer.OutputFile += OnOutputFile;
				}

				var update = environment.Update();
				if(!update.IsCompleted)
				{
					await update;
					inspector.Updated += environment.Update;
				}

				// Open the output RDF file
				using(var outputStream = environment.CreateFile(output, inspector.OutputMediaType))
                {
					if(dataOnly)
                    {
						// The input should be treated as a byte sequence without file metadata
						await inspector.Archive<IStreamFactory>(inputFiles, outputStream, options);
                    }else{
						await inspector.Archive(inputFiles, outputStream, options);
					}
				}
			}catch(ApplicationExitException)
			{

			}catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
			{
				Log(e.Message);
			}
        }

		/// <summary>
		/// Called from an analyzer when an output file should be created.
		/// </summary>
        private async ValueTask OnOutputFile(string name, bool isBinary, IReadOnlyDictionary<string, object> properties, Func<Stream, ValueTask> writer)
        {
			if(properties.TryGetValue("path", out var pathObject) && pathObject is string path)
            {
				if(path.EndsWith("/"))
                {
					name = path + name;
                }else{
					name = path;
                }
            }
			using(var stream = environment.CreateFile(name, isBinary ? "application/octet-stream" : "text/plain"))
            {
				await writer(stream);
            }
        }

		bool IsIncluded<T>(string prefix, T item, out string name)
		{
			name = $"{prefix}:{DataTools.GetUserFriendlyName(item)}";
			var included = true;
			foreach(var (result, matcher) in componentMatchers)
            {
				included = result ?
					included || matcher(name) :
					included && !matcher(name);
            }
			return included;
		}

		/// <summary>
		/// Outputs a list of configurable components.
		/// </summary>
        void PrintComponents<T>(string prefix, IEnumerable<T> list)
		{
			foreach(var item in list)
			{
				if(IsIncluded(prefix, item, out var name))
				{
					LogWriter.WriteLine($" - {name} ({DataTools.GetUserFriendlyName(item.GetType())})");
				}
            }
        }

		/// <summary>
		/// Filters a list of configurable components, removing everything not matched
		/// by <paramref name="matches"/>.
		/// </summary>
		void FilterComponents<T>(string prefix, ICollection<T> collection, ref int count)
        {
			switch(collection)
            {
				case SortedSet<T> sortedSet:
					sortedSet.RemoveWhere(item => !IsIncluded(prefix, item, out _));
					break;
				case List<T> list:
					list.RemoveAll(item => !IsIncluded(prefix, item, out _));
					break;
				default:
					var filtered = new List<T>();
					foreach(var item in collection)
					{
						if(!IsIncluded(prefix, item, out _))
						{
							filtered.Add(item);
						}
					}
					foreach(var item in filtered)
					{
						collection.Remove(item);
					}
					break;
            }

			count += collection.Count;
        }

        protected override string Usage => $"({String.Join("|", modeNames)}) [options] input... output";

        public override void Description()
		{
			base.Description();
			LogWriter.WriteLine();
			LogWriter.Write(" ");
			OutputWrapPad("This software analyzes the formats of given files and outputs RDF description of their contents.", 1);
		}

		/// <summary>
		/// The operating mode of the application.
		/// </summary>
		enum Mode
		{
			/// <summary>
			/// The application should describe the input files in RDF.
			/// </summary>
			Describe,

			/// <summary>
			/// The application should list all available components.
			/// </summary>
			List,
		}

		public override IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection{
				{"q", "quiet", null, "do not print any additional messages"},
				{"i", "include", "pattern", "include given components"},
				{"x", "exclude", "pattern", "exclude given components"},
				{"c", "compress", null, "perform gzip compression on the output"},
				{"m", "metadata", null, "add annotation metadata to output"},
				{"d", "data-only", null, "do not store input file information"},
				{"h", "hash", "pattern", "set the main binary hash"},
				{"r", "root", "uri", "set the hierarchy root URI prefix"},
				{"s", "sparql-query", "file", "perform a SPARQL query on the result"},
				{"?", "help", null, "displays this help message"},
			};
		}
		
		protected override OperandState OnOperandFound(string operand)
		{
			if(mode == null)
            {
				if(!Enum.TryParse<Mode>(operand, true, out var mode))
                {
					throw new ApplicationException($"Invalid mode (must be one of {String.Join(", ", modeNames)}).");
				}
				this.mode = mode;
				return OperandState.ContinueOptions;
			}
			if(output == null)
			{
				output = operand;
				return OperandState.OnlyOperands;
			}
			inputs.Add(output);
			output = operand;
			return OperandState.OnlyOperands;
		}

		protected override OptionArgument OnOptionFound(string option)
		{
			switch(option)
			{
				case "q":
				case "quiet":
					if(quiet)
					{
						throw OptionAlreadySpecified(option);
					}
					quiet = true;
					return OptionArgument.None;
				case "c":
				case "compress":
					if(options.CompressedOutput)
					{
						throw OptionAlreadySpecified(option);
					}
					options.CompressedOutput = true;
					return OptionArgument.None;
				case "m":
				case "metadata":
					if(!options.HideMetadata)
					{
						throw OptionAlreadySpecified(option);
					}
					options.HideMetadata = false;
					return OptionArgument.None;
				case "d":
				case "data-only":
					if(dataOnly)
					{
						throw OptionAlreadySpecified(option);
					}
					dataOnly = true;
					return OptionArgument.None;
				case "r":
				case "root":
					return OptionArgument.Required;
				case "i":
				case "include":
					return OptionArgument.Required;
				case "x":
				case "exclude":
					return OptionArgument.Required;
				case "h":
				case "hash":
					return OptionArgument.Required;
				case "s":
				case "sparql-query":
					return OptionArgument.Required;
				case "?":
				case "help":
					Help();
					return OptionArgument.None;
				default:
					throw UnrecognizedOption(option);
			}
		}
		
		protected override void OnOptionArgumentFound(string option, string argument)
		{
			switch(option)
			{
				case "r":
				case "root":
					if(rootSpecified)
					{
						throw OptionAlreadySpecified(option);
					}
					options.Root = argument;
					rootSpecified = true;
					break;
				case "i":
				case "include":
					componentMatchers.Add((true, DataTools.ConvertWildcardToRegex(argument).IsMatch));
					break;
				case "x":
				case "exclude":
					componentMatchers.Add((false, DataTools.ConvertWildcardToRegex(argument).IsMatch));
					break;
				case "h":
				case "hash":
					var match = DataTools.ConvertWildcardToRegex(argument);
					if(mainHash == null)
					{
						mainHash = match;
						componentMatchers.Add((true, DataTools.ConvertWildcardToRegex("data-hash:" + argument).IsMatch));
					}else{
						throw OptionAlreadySpecified(option);
					}
					break;
				case "s":
				case "sparql-query":
					queries.Add(argument);
					break;
			}
		}
    }
}
