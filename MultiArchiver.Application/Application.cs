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
	/// <typeparam name="TArchiver">The type of archiver to use.</typeparam>
    public class Application<TArchiver> : CommandApplication where TArchiver : Archiver, new()
    {
        readonly IApplicationEnvironment environment;

		TextWriter writer;

		protected override TextWriter LogWriter => writer;

		protected override int WindowWidth => environment.WindowWidth;

		readonly ArchiverOptions options;

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

			options = new ArchiverOptions();
			options.PrettyPrint = true;
			options.DirectOutput = true;
			options.NewLine = environment.NewLine;
			options.HideMetadata = true;
		}

		Mode? mode;
		List<string> inputs = new List<string>();
		List<string> queries = new List<string>();
		List<Regex> matches = new List<Regex>();
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

				var archiver = new TArchiver();
				archiver.OutputLog = writer;
				archiver.AddDefault();

				// Print the available components
				switch(mode)
				{
					case Mode.List:
						PrintList("analyzer", archiver.Analyzers, matches);
						PrintList("data-format", archiver.DataAnalyzer.DataFormats, matches);
						PrintList("xml-format", archiver.XmlAnalyzer.XmlFormats, matches);
						PrintList("container-format", archiver.ContainerProviders, matches);
						PrintList("data-hash", archiver.DataAnalyzer.HashAlgorithms, matches);
						PrintList("file-hash", archiver.FileAnalyzer.HashAlgorithms, matches);
						PrintList("pixel-hash", archiver.ImageDataHashAlgorithms, matches);
						PrintList("image-hash", archiver.ImageHashAlgorithms, matches);
						return;
				}

				if(inputs.Count == 0 || output == null)
				{
					throw new ApplicationException("At least one input and an output must be specified!");
				}

				if(quiet)
				{
					archiver.OutputLog = writer = TextWriter.Null;
				}

				// Filter out the components based on arguments

				int componentCount = 0;
				FilterList("analyzer", archiver.Analyzers, matches, ref componentCount);
				FilterList("data-format", archiver.DataAnalyzer.DataFormats, matches, ref componentCount);
				FilterList("xml-format", archiver.XmlAnalyzer.XmlFormats, matches, ref componentCount);
				FilterList("container-format", archiver.ContainerProviders, matches, ref componentCount);
				FilterList("data-hash", archiver.DataAnalyzer.HashAlgorithms, matches, ref componentCount);
				FilterList("file-hash", archiver.FileAnalyzer.HashAlgorithms, matches, ref componentCount);
				FilterList("pixel-hash", archiver.ImageDataHashAlgorithms, matches, ref componentCount);
				FilterList("image-hash", archiver.ImageHashAlgorithms, matches, ref componentCount);

				writer.WriteLine($"Loaded {componentCount} components in total.");

				if(mainHash != null)
                {
					// Set the primary ni: hash
					var hash = archiver.DataAnalyzer.HashAlgorithms.FirstOrDefault(h => mainHash.IsMatch(DataTools.GetUserFriendlyName(h)));
					if(hash == null)
                    {
						throw new ApplicationException("Main hash cannot be found!");
					}
					archiver.DataAnalyzer.ContentUriFormatter = new NiHashedContentUriFormatter(hash);
                }

				// Load the input files from the environment
				var inputFiles = inputs.SelectMany(input => environment.GetFiles(input));
				
				options.Queries = queries.SelectMany(query => environment.GetFiles(query));

				foreach(var analyzer in archiver.Analyzers.OfType<IHasFileOutput>())
                {
					analyzer.OutputFile += OnOutputFile;
				}

				// Open the output RDF file
				using(var outputStream = environment.CreateFile(output, archiver.OutputMediaType))
                {
					if(dataOnly)
                    {
						// The input should be treated as a byte sequence without file metadata
						await archiver.Archive<IStreamFactory>(inputFiles, outputStream, options);
                    }else{
						await archiver.Archive(inputFiles, outputStream, options);
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

		/// <summary>
		/// Outputs the list of configurable components.
		/// </summary>
        void PrintList<T>(string prefix, IEnumerable<T> list, ICollection<Regex> matches)
		{
			if(matches.Count == 0)
			{
				matches.Add(anyRegex);
			}
			bool first = true;
			foreach(var item in list)
			{
				var name = $"{prefix}:{DataTools.GetUserFriendlyName(item)}";
				if(matches.Any(m => m.IsMatch(name)))
				{
					LogWriter.WriteLine(" - " + name);
				}
            }
        }

		static readonly Regex anyRegex = new Regex(".", RegexOptions.Compiled);

		/// <summary>
		/// Filters a list of configurable components, removing everything not matched
		/// by <paramref name="matches"/>.
		/// </summary>
		void FilterList<T>(string prefix, ICollection<T> list, ICollection<Regex> matches, ref int count)
        {
			if(matches.Count == 0)
            {
				matches.Add(anyRegex);
            }
			var filtered = new List<T>();
			foreach(var item in list)
			{
				var name = $"{prefix}:{DataTools.GetUserFriendlyName(item)}";
				if(!matches.Any(m => m.IsMatch(name)))
                {
					filtered.Add(item);
                }
            }

			foreach(var item in filtered)
            {
				list.Remove(item);
            }

			count += list.Count;
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
				{"c", "compress", null, "perform gzip compression on the output"},
				{"m", "metadata", null, "add annotation metadata to output"},
				{"d", "data-only", null, "do not store input file information"},
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
					matches.Add(DataTools.ConvertWildcardToRegex(argument));
					break;
				case "mh":
				case "main-hash":
					var match2 = DataTools.ConvertWildcardToRegex(argument);
					if(mainHash == null)
					{
						mainHash = match2;
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
