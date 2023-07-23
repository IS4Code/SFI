using IS4.SFI.Application;
using IS4.SFI.Application.Tools;
using IS4.SFI.Services;
using Microsoft.Extensions.Logging;
using MorseCode.ITask;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// The main application for analyzing input files and producing output.
    /// </summary>
    /// <typeparam name="TInspector">The type of <see cref="Inspector"/> to use.</typeparam>
    public class Application<TInspector> : ConfigurableApplication, IResultFactory<ValueTuple, string>, IResultFactory<bool, string> where TInspector : ExtensibleInspector, new()
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
		public Application(IApplicationEnvironment environment) : base("https://sfi.is4.site/config")
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
        readonly List<string> plugins = new();
        readonly List<Matcher> componentMatchers = new();
		Regex? mainHash;
		string? mainHashName;
		string? output;

		readonly Dictionary<string, Dictionary<string, string>> componentProperties = new(StringComparer.OrdinalIgnoreCase);

		bool quiet;
		bool rootSpecified;
		bool dataOnly;
		bool onlyOnce;

        /// <summary>
        /// Runs the application with the supplied arguments.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        public async ValueTask Run(params string[] args)
        {
			try{
				LoadConfig();
				Parse(args);

				if(!quiet) Banner();

				if(mode == null)
				{
					throw new ApplicationException($"Mode must be specified (one of {String.Join(", ", modeNames)})! Use -? for help.");
				}

                var inspector = new TInspector
                {
                    CacheResults = onlyOnce
                };
                if(!quiet)
                {
					inspector.OutputLog = new ComponentLogger(writer);
                }
				foreach(var plugin in plugins)
				{
					inspector.AdditionalPluginIdentifiers.Add(plugin);
				}
				var logger = inspector.OutputLog;
				await inspector.AddDefault();

				ConfigurationTools.RegisterCustomDescriptors();

				int componentCount = 0;
				async Task ConfigureComponents()
                {
					// Filter out the components based on arguments

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
							if(componentProperties.Count > 0)
							{
								// Check if there are properties for this component
								var id = collection.GetIdentifier(component);
								if(componentProperties.TryGetValue(id, out var dict))
								{
									componentProperties.Remove(id);
									SetProperties(component, id, dict);
								}
							}
						}
					}
                }

				switch(mode)
                {
                    // Print the available components
                    case Mode.List:
						await ConfigureComponents();
						if(!quiet) LogWriter.WriteLine("Included components:");
						foreach(var collection in inspector.ComponentCollections)
						{
							await collection.ForEach(this);
						}
						return;
					// Print the options XML
					case Mode.Options:
						PrintOptionsXml();
                        return;
					case Mode.About:
						if(output != null)
						{
							inputs.Add(output);
						}
						if(inputs.Count == 0)
						{
                            throw new ApplicationException("'about' requires the name of a component.");
                        }
                        await ConfigureComponents();
                        foreach(var collection in inspector.ComponentCollections)
                        {
							aboutBrowsedCollection = collection;
							if(inputs.Contains(collection.Attribute.Prefix))
							{
								AboutCollection(collection);
							}
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
					writer = TextWriter.Null;
				}

				await ConfigureComponents();

				foreach(var matcher in componentMatchers)
                {
					if(!matcher.HadEffect && matcher.Pattern != null)
					{
                        logger?.LogWarning($"Warning: Pattern '{matcher.Pattern}' did not {(matcher.Result ? "include" : "exclude")} any components!");
					}
				}

				foreach(var properties in componentProperties)
				{
					logger?.LogWarning($"Warning: Properties for component {properties.Key} could not be found!");
				}

				logger?.LogInformation($"Included {componentCount} components in total.");

				if(mainHash != null)
                {
					// Set the primary ni: hash
					var hash = inspector.DataAnalyzer.HashAlgorithms.FirstOrDefault(h => mainHash.IsMatch(TextTools.GetUserFriendlyName(h)));
					if(hash == null)
                    {
						throw new ApplicationException($"Main hash '{mainHashName}' cannot be found!");
					}
					inspector.DataAnalyzer.ContentUriFormatter = new NiHashedContentUriFormatter(hash);
                }

				if(options.Format == null)
                {
					options.Format = VDS.RDF.MimeTypesHelper.GetTrueFileExtension(output);
                    if(String.IsNullOrEmpty(options.Format))
                    {
						options.Format = null;
                    }
                }

				// Load the input files from the environment
				var inputFiles = inputs.SelectMany(input => environment.GetFiles(input)).ToList();

				if(inputFiles.Count == 0)
				{
					throw new ApplicationException("No specified input files were found!");
				}
				
				var queryFiles = queries.SelectMany(query => environment.GetFiles(query).SelectMany(f => f.EnumerateFiles())).ToList();

				if(queryFiles.Count == 0 && queries.Count > 0)
				{
					throw new ApplicationException("No specified SPARQL queries were found!");
				}

				if(mode == Mode.Search)
                {
					options.OutputIsSparqlResults = true;
					if(queryFiles.Count == 0)
					{
						throw new ApplicationException("A SPARQL query must be provided via -s for search!");
					}
				}

				options.Queries = queryFiles;

				var update = environment.Update();
				if(!update.Equals(default(ValueTask)))
				{
					// Only subscribe when the operation is asynchronous
					await update;
					inspector.Updated += environment.Update;
				}

				// Open the output RDF file
				var streams = new List<Stream>();
				Stream Factory(string mime)
				{
					var stream = environment.CreateFile(output, mime);
					streams.Add(stream);
					return stream;
				}
				try{
					if(dataOnly)
					{
						// The input should be treated as a byte sequence without file metadata
						await inspector.Inspect<IStreamFactory>(inputFiles.SelectMany(f => f.EnumerateFiles()), Factory, options);
					}else{
						await inspector.Inspect(inputFiles, Factory, options);
					}
                }finally{
					foreach(var stream in streams)
                    {
						stream.Dispose();
                    }
                }
			}catch(ApplicationExitException)
			{

			}catch(InternalApplicationException e) when(GlobalOptions.SuppressNonCriticalExceptions)
			{
				environment.LogWriter.WriteLine(e.InnerException.Message);
			}catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
			{
				environment.LogWriter.WriteLine(e.Message);
			}
        }

		/// <summary>
		/// Called from an analyzer when an output file should be created.
		/// </summary>
        private async ValueTask OnOutputFile(bool isBinary, INodeMatchProperties properties, Func<Stream, ValueTask> writer)
        {
			properties.Name ??= Guid.NewGuid().ToString("N");
			var mediaType = properties.MediaType;
			var path = properties.PathFormat ?? "${name}${extension}";
			properties.PathFormat = null;

			var name = TextTools.SubstituteVariables(path, properties.GetPropertyValues());

			mediaType ??= isBinary ? "application/octet-stream" : "text/plain";
			LogWriter?.WriteLine($"Extracting {mediaType} to '{name}'...");
			using var stream = environment.CreateFile(name, mediaType);
            await writer(stream);
        }

        #region Components
        private void SetProperties(object component, string componentName, IDictionary<string, string> properties)
        {
			ConfigurationTools.SetProperties(component, componentName, properties);

			foreach(var pair in properties)
            {
				LogWriter?.WriteLine($"Warning: Property {componentName}:{pair.Key} was not found!");
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

        async ITask<bool> IResultFactory<bool, string>.Invoke<T>(T value, string name)
        {
            return IsIncluded(value, name);
        }
        #endregion

        #region Listing
        void ListComponent<T>(T component, string id) where T : notnull
		{
			LogWriter?.WriteLine($" - {id} ({TextTools.GetUserFriendlyName(TypeDescriptor.GetReflectionType(component))})");
			if(componentProperties.TryGetValue(id, out var dict))
			{
				componentProperties.Remove(id);
				SetProperties(component, id, dict);
			}
			foreach(var prop in ConfigurationTools.GetConfigurableProperties(component))
			{
				var value = prop.GetValue(component);
				var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
				var converter = prop.Converter;
				string typeDesc;
				if(ConfigurationTools.GetStandardValues(type, converter, out var values))
				{
					const int maxShown = 10;
					typeDesc = String.Join("|", values.Cast<object>().Take(maxShown).Select(converter.ConvertToInvariantString));
					if(!converter.GetStandardValuesExclusive() || values.Count > maxShown)
					{
						typeDesc = $"{TextTools.GetIdentifierFromType(type)}: {typeDesc}...";
					}
				}else{
					typeDesc = TextTools.GetIdentifierFromType(type);
				}
				var line = $"{id}:{TextTools.FormatComponentName(prop.Name)} ({typeDesc})";
				if(value != null)
				{
					LogWriter?.WriteLine($"  - {line} = {converter.ConvertToInvariantString(value)}");
				}else{
					LogWriter?.WriteLine($"  - {line}");
				}
			}
        }

        SFI.Application.ComponentCollection? aboutBrowsedCollection;

        void AboutComponent<T>(T component, string id) where T : notnull
		{
			if(inputs.Contains(id))
			{
                LogWriter?.WriteLine();
                LogWriter?.WriteLine($"# Component `{id}`");
				var type = TypeDescriptor.GetReflectionType(component);
				bool realType = type.Equals(component.GetType());

				if(realType)
                {
                    var elementType = aboutBrowsedCollection!.Attribute.CommonType ?? typeof(T);
                    LogWriter?.WriteLine($"Element type: {elementType.FullName}");
                }

                var assembly = type.Assembly;
                LogWriter?.WriteLine($"Component type: {type.FullName}");
                LogWriter?.WriteLine($"Assembly: {assembly.GetName().Name}");
				PrintAttributes("Assembly ", assembly);

                if(realType)
                {
					// Not a proxy configuration for another type

					var baseType = type;
					do{
						// Look for the nearest base type
						if(!baseType.IsGenericType)
						{
							// Must be generic
							continue;
						}
						var genArgs = baseType.GetGenericArguments();
						if(genArgs.Length != 1)
						{
							// Must have single type argument
							continue;
						}
                        var objType = genArgs[0];
                        LogWriter?.WriteLine($"Argument type: {objType.FullName}");
                        LogWriter?.WriteLine($"Argument assembly: {objType.Assembly.GetName().Name}");
                        PrintAttributes("Argument assembly ", objType.Assembly);
                        break;
					}while((baseType = baseType.BaseType) != null);
                }

                PrintAttributes("", TypeDescriptor.GetAttributes(component));

				var properties = ConfigurationTools.GetConfigurableProperties(component);

				foreach(var prop in properties)
				{
					LogWriter?.WriteLine();
                    LogWriter?.WriteLine($"## Property `{TextTools.FormatComponentName(prop.Name)}`");

					LogWriter?.WriteLine($"Name: {prop.Name}");
					
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    LogWriter?.WriteLine($"Type: {propType.FullName}");

                    var converter = prop.Converter;

                    if(ConfigurationTools.GetStandardValues(propType, converter, out var values))
                    {
                        var valuesText = String.Join("|", values.Cast<object>().Select(converter.ConvertToInvariantString));
						var exclusive = converter.GetStandardValuesExclusive();

                        LogWriter?.WriteLine($"Standard values: {valuesText}{(exclusive ? "" : "...")}");
                    }

					PrintAttributes("", prop.Attributes);

                    var value = prop.GetValue(component);
					if(value != null)
					{
						LogWriter?.WriteLine($"Value: {converter.ConvertToInvariantString(value)}");
					}
				}
            }
		}

		void AboutCollection(Application.ComponentCollection collection)
        {
            LogWriter?.WriteLine();
            LogWriter?.WriteLine($"# Collection `{collection.Attribute.Prefix}`");

			Type? componentType = null;
			if(collection.Property is PropertyDescriptor prop)
            {
				PrintAttributes("", prop.Attributes);
				componentType = prop.ComponentType;
            }

            var elementType = collection.Attribute.CommonType ?? collection.ElementType;
            LogWriter?.WriteLine($"Element type: {elementType.FullName}");
            LogWriter?.WriteLine($"Element assembly: {elementType.Assembly.GetName().Name}");

            if(collection.Component is object component)
            {
                LogWriter?.WriteLine();
                LogWriter?.WriteLine($"Component: {TextTools.GetUserFriendlyName(component)}");
                PrintAttributes("Component ", TypeDescriptor.GetAttributes(component));
				componentType ??= TypeDescriptor.GetReflectionType(component);
            }

			if(componentType != null)
			{
				var assembly = componentType.Assembly;
				LogWriter?.WriteLine($"Component type: {componentType.FullName}");
				LogWriter?.WriteLine($"Assembly: {assembly.GetName().Name}");
				PrintAttributes("Assembly ", assembly);
			}
        }

		void PrintAttributes(string prefix, ICustomAttributeProvider provider)
        {
            foreach(var pair in ConfigurationTools.GetTextAttributes(provider.GetCustomAttributes(false).OfType<Attribute>()))
            {
                LogWriter?.WriteLine($"{prefix}{pair.Key}: {pair.Value}");
            }
        }

		void PrintAttributes(string prefix, AttributeCollection attributes)
		{
			foreach(var pair in ConfigurationTools.GetTextAttributes(attributes.OfType<Attribute>()))
            {
                LogWriter?.WriteLine($"{prefix}{pair.Key}: {pair.Value}");
            }
		}

        async ITask<ValueTuple> IResultFactory<ValueTuple, string>.Invoke<T>(T component, string id)
		{
			if(IsIncluded(component, id))
			{
				switch(mode)
				{
					case Mode.List:
						ListComponent(component, id);
                        break;
					case Mode.About:
						AboutComponent(component, id);
						break;

                }
			}
			return default;
        }
        #endregion

        #region Parameters
        /// <inheritdoc/>
        protected override string Usage => $"({String.Join("|", modeNames)}) [options] input... output";

		/// <inheritdoc/>
		public override void Description()
		{
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
			/// The application should search input files using SPARQL.
			/// </summary>
			Search,

			/// <summary>
			/// The application should list all available components.
			/// </summary>
			List,

            /// <summary>
            /// The application should display information about a particular component.
            /// </summary>
            About,

            /// <summary>
            /// The application should output the provided options in the XML format.
            /// </summary>
            Options,
		}

		/// <inheritdoc/>
		public override IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection{
				{"q", "quiet", null, "do not print any additional messages"},
				{"i", "include", "pattern", "include given components"},
				{"x", "exclude", "pattern", "exclude given components"},
				{"f", "format", "extension|mime", "the RDF serialization format of the output"},
				{"c", "compress", null, "perform gzip compression on the output"},
				{"m", "metadata", null, "add annotation metadata to output"},
				{"d", "data-only", null, "do not store input file information"},
				{"u", "ugly", null, "do not use pretty print"},
                {"o", "only-once", null, "skip processing duplicate entities"},
                {"b", "buffered", null, "buffer all data in a graph before writing"},
				{"h", "hash", "pattern", "set the main binary hash"},
				{"r", "root", "uri", "set the hierarchy root URI prefix"},
				{"s", "sparql-query", "file", "perform a SPARQL query during processing"},
                {"p", "plugin", "id", "loads a plugin with a particular identifier"},
                {"C", "config", "file", "loads additional configuration"},
                {"?", "help", null, "displays this help message"},
				{null, "[component]:[property]", "value", "sets a specific component's property (see list)"}
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
				if(mode == Mode.Options)
				{
                    CaptureOptions();
				}
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

		static readonly Regex componentPropertyRegex = new(@"^([^:]+:[^:]+):(.*)$", RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override string GetCanonicalOption(string option)
        {
			switch(option)
			{
				case "q":
					return "quiet";
				case "c":
					return "compress";
				case "m":
					return "metadata";
				case "d":
					return "data-only";
				case "o":
					return "only-once";
				case "b":
					return "buffered";
				case "u":
					return "ugly";
				case "r":
					return "root";
				case "i":
					return "include";
				case "x":
					return "exclude";
				case "h":
					return "hash";
				case "s":
					return "sparql-query";
				case "f":
					return "format";
				case "p":
					return "plugin";
				case "C":
					return "config";
				case "?":
					return "help";
			}
            return base.GetCanonicalOption(option);
        }

        /// <inheritdoc/>
        protected override OptionArgumentFlags OnOptionFound(string option)
		{
			switch(GetCanonicalOption(option))
			{
				case "quiet":
					quiet = true;
					return OptionArgumentFlags.None;
				case "compress":
					options.CompressedOutput = true;
					return OptionArgumentFlags.None;
				case "metadata":
					options.HideMetadata = false;
					return OptionArgumentFlags.None;
				case "data-only":
					dataOnly = true;
					return OptionArgumentFlags.None;
				case "only-once":
					onlyOnce = true;
					return OptionArgumentFlags.None;
				case "buffered":
					options.DirectOutput = false;
					return OptionArgumentFlags.None;
				case "ugly":
					options.PrettyPrint = false;
					return OptionArgumentFlags.None;
				case "root":
					return OptionArgumentFlags.RequiredArgument;
				case "include":
					return OptionArgumentFlags.RequiredArgument | OptionArgumentFlags.AllowMultiple;
				case "exclude":
					return OptionArgumentFlags.RequiredArgument | OptionArgumentFlags.AllowMultiple;
				case "hash":
					return OptionArgumentFlags.RequiredArgument;
				case "sparql-query":
					return OptionArgumentFlags.RequiredArgument | OptionArgumentFlags.AllowMultiple;
				case "format":
					return OptionArgumentFlags.RequiredArgument;
				case "plugin":
					return OptionArgumentFlags.RequiredArgument | OptionArgumentFlags.AllowMultiple;
				case "config":
					return OptionArgumentFlags.RequiredArgument | OptionArgumentFlags.AllowMultiple;
				case "help":
					Help();
					return OptionArgumentFlags.None;
				default:
					if(componentPropertyRegex.IsMatch(option))
					{
						return OptionArgumentFlags.RequiredArgument;
					}
					throw UnrecognizedOption(option);
			}
		}

		/// <inheritdoc/>
		protected override void OnOptionArgumentFound(string option, string? argument, OptionArgumentFlags flags)
        {
			switch(GetCanonicalOption(option))
			{
				case "root":
					if(!Uri.TryCreate(argument, UriKind.Absolute, out _))
					{
						throw new ApplicationException("The argument to option '" + option + "' must be a well-formed absolute URI.");
					}
					options.Root = argument!;
					rootSpecified = true;
					break;
				case "include":
					componentMatchers.Add(new Matcher(true, argument!));
					break;
				case "exclude":
					componentMatchers.Add(new Matcher(false, argument!));
					break;
				case "hash":
					mainHash = TextTools.ConvertWildcardToRegex(argument!);
					mainHashName = argument;
					componentMatchers.Add(new Matcher(true, "data-hash:" + argument!, true));
					break;
				case "sparql-query":
					queries.Add(argument!);
					break;
				case "format":
					options.Format = argument!;
					break;
				case "plugin":
					plugins.Add(argument!);
					break;
				case "config":
					LoadConfig(argument!);
					break;
				default:
					if(componentPropertyRegex.Match(option) is { Success: true } propMatch)
					{
						var componentId = propMatch.Groups[1].Value.ToLowerInvariant();
						if(!componentProperties.TryGetValue(componentId, out var dict))
						{
							componentProperties[componentId] = dict = new(StringComparer.OrdinalIgnoreCase);
						}
						dict[propMatch.Groups[2].Value.ToLowerInvariant()] = argument!;
					}
					break;
			}
        }

		class Matcher
        {
			public bool Result { get; }
			public string? Pattern { get; }
			public Predicate<string> Predicate { get; }
			public bool HadEffect { get; set; }

			public Matcher(bool result, string pattern, bool forgetPattern = false)
            {
				Result = result;
				Pattern = forgetPattern ? null : pattern;
				Predicate = TextTools.ConvertWildcardToRegex(pattern).IsMatch;
			}
        }
        #endregion

        void LoadConfig(string name = "sfi-config.xml")
        {
            foreach(var config in environment.GetFiles(name).OfType<IFileInfo>())
            {
				LoadConfigXml(name, config);
            }
        }

        void PrintOptionsXml()
		{
            if(output == null)
            {
                throw new ApplicationException("An output must be specified to save the options!");
            }
			using var file = environment.CreateFile(output, "application/xml");
			SaveConfigXml(file);
		}
    }
}
