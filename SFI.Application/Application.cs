using IS4.SFI.Application;
using IS4.SFI.Services;
using MorseCode.ITask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.SFI
{
	/// <summary>
	/// The main application for analyzing input files and producing output.
	/// </summary>
	/// <typeparam name="TInspector">The type of <see cref="Inspector"/> to use.</typeparam>
    public class Application<TInspector> : CommandApplication, IResultFactory<ValueTuple, string>, IResultFactory<bool, string> where TInspector : ComponentInspector, new()
    {
        readonly IApplicationEnvironment environment;

		TextWriter writer;

		/// <inheritdoc/>
		protected override TextWriter LogWriter => writer;

		/// <inheritdoc/>
		protected override int WindowWidth => environment.WindowWidth;

		/// <inheritdoc/>
		protected override string ExecutableName => environment.ExecutableName ?? base.ExecutableName;

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

            options = new InspectorOptions
            {
                PrettyPrint = true,
                DirectOutput = true,
                NewLine = environment.NewLine,
                HideMetadata = true
            };
        }

		Mode? mode;
		readonly List<string> inputs = new();
		readonly List<string> queries = new();
		readonly List<Matcher> componentMatchers = new();
		Regex? mainHash;
		string? output;

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
				await inspector.AddDefault();

				// Print the available components
				switch(mode)
				{
					case Mode.List:
						LogWriter.WriteLine("Included components:");
						foreach(var collection in inspector.ComponentCollections)
						{
							await collection.ForEach(this);
						}
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
				foreach(var collection in inspector.ComponentCollections)
				{
					componentCount += await collection.Filter(this);
					foreach(var component in collection.Collection)
					{
						// Subscribe to the OutputFile event
						if(component is IHasFileOutput fileOutput)
						{
							fileOutput.OutputFile += OnOutputFile;
						}
					}
				}

				foreach(var matcher in componentMatchers)
                {
					if(!matcher.HadEffect && matcher.Pattern != null)
					{
						writer.WriteLine($"Warning: Pattern '{matcher.Pattern}' did not {(matcher.Result ? "include" : "exclude")} any components!");
					}
                }

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

				var update = environment.Update();
				if(!update.IsCompleted)
				{
					// Only subscribe when the operation is asynchronous
					await update;
					inspector.Updated += environment.Update;
				}

				// Open the output RDF file
				using(var outputStream = environment.CreateFile(output, inspector.OutputMediaType))
                {
					if(dataOnly)
                    {
						// The input should be treated as a byte sequence without file metadata
						await inspector.Inspect<IStreamFactory>(inputFiles, outputStream, options);
                    }else{
						await inspector.Inspect(inputFiles, outputStream, options);
					}
				}
			}catch(ApplicationExitException)
			{

			}catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
			{
				LogWriter.WriteLine(e.Message);
			}
        }

		/// <summary>
		/// Called from an analyzer when an output file should be created.
		/// </summary>
        private async ValueTask OnOutputFile(string? name, bool isBinary, IReadOnlyDictionary<string, object>? properties, Func<Stream, ValueTask> writer)
        {
			name ??= Guid.NewGuid().ToString("N");
			if(properties != null && properties.TryGetValue("path", out var pathObject) && pathObject is string path)
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
		/// Checks whether a component is matched by the inner list of matchers.
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <param name="component">The component object.</param>
		/// <param name="name">The name of the component.</param>
		/// <returns>Whether the component should be included.</returns>
		bool IsIncluded<T>(T component, string name)
		{
			var included = true;
			for(int i = 0; i < componentMatchers.Count; i++)
            {
				var matcher = componentMatchers[i];
				if(included != matcher.Result && matcher.Predicate(name))
				{
					included = matcher.Result;
					matcher.HadEffect = true;
				}
            }
			return included;
		}

		async ITask<ValueTuple> IResultFactory<ValueTuple, string>.Invoke<T>(T component, string name)
		{
			if(IsIncluded(component, name))
			{
				LogWriter?.WriteLine($" - {name} ({DataTools.GetUserFriendlyName(component.GetType())})");
			}
			return default;
		}

		async ITask<bool> IResultFactory<bool, string>.Invoke<T>(T value, string name)
		{
			return IsIncluded(value, name);
		}

		/// <inheritdoc/>
		protected override string Usage => $"({String.Join("|", modeNames)}) [options] input... output";

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		protected override void OnOptionArgumentFound(string option, string? argument)
		{
			switch(option)
			{
				case "r":
				case "root":
					if(rootSpecified)
					{
						throw OptionAlreadySpecified(option);
					}
					options.Root = argument!;
					rootSpecified = true;
					break;
				case "i":
				case "include":
					componentMatchers.Add(new Matcher(true, argument!));
					break;
				case "x":
				case "exclude":
					componentMatchers.Add(new Matcher(false, argument!));
					break;
				case "h":
				case "hash":
					var match = DataTools.ConvertWildcardToRegex(argument!);
					if(mainHash == null)
					{
						mainHash = match;
						componentMatchers.Add(new Matcher(true, "data-hash:" + argument!, true));
					}else{
						throw OptionAlreadySpecified(option);
					}
					break;
				case "s":
				case "sparql-query":
					queries.Add(argument!);
					break;
			}
		}

		class Matcher
        {
			public bool Result { get; }
			public string Pattern { get; }
			public Predicate<string> Predicate { get; }
			public bool HadEffect { get; set; }

			public Matcher(bool result, string pattern, bool forgetPattern = false)
            {
				Result = result;
				Pattern = forgetPattern ? null : pattern;
				Predicate = DataTools.ConvertWildcardToRegex(pattern).IsMatch;
			}
        }
    }
}
