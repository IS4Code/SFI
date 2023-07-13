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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI
{
    /// <summary>
    /// The main application for analyzing input files and producing output.
    /// </summary>
    /// <typeparam name="TInspector">The type of <see cref="Inspector"/> to use.</typeparam>
    public class Application<TInspector> : CommandApplication, IResultFactory<ValueTuple, string>, IResultFactory<bool, string> where TInspector : ExtensibleInspector, new()
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

				RegisterCustomDescriptors();

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

		static void RegisterCustomDescriptors()
		{
			var provider = TypeDescriptor.GetProvider(typeof(Encoding));
			TypeDescriptor.AddProvider(new EncodingTypeDescriptionProvider(provider), typeof(Encoding));
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

		/// <summary>
		/// Assigns the values of properties on a component.
		/// </summary>
		/// <param name="component">The component to assign to.</param>
		/// <param name="componentName">The name of the component, for diagnostics.</param>
		/// <param name="properties">The dictionary of property names and their values to assign.</param>
		private void SetProperties(object component, string componentName, IDictionary<string, string> properties)
        {
            var batch = component as ISupportInitialize;
            batch?.BeginInit();

            foreach(var prop in GetConfigurableProperties(component))
            {
				var name = TextTools.FormatComponentName(prop.Name);
				if(properties.TryGetValue(name, out var value))
				{
					// The property is assigned
					properties.Remove(name);
					var converter = prop.Converter;
					object? convertedValue = null;
					Exception? conversionException = null;
					try{
						convertedValue = converter.ConvertFromInvariantString(value);
                    }catch(Exception e)
                    {
						// Conversion failed (for any reason)
						conversionException = e;
					}
					if(convertedValue == null && !(String.IsNullOrEmpty(value) && conversionException == null))
                    {
						throw new ApplicationException($"Cannot convert value '{value}' for property {componentName}:{name} to type {TextTools.GetIdentifierFromType(prop.PropertyType)}!", conversionException);
                    }
                    try{
						prop.SetValue(component, convertedValue);
					}catch(Exception e)
					{
						throw new ApplicationException($"Cannot assign value '{value}' to property {componentName}:{name}: {e.Message}", e);
					}
				}
            }
			foreach(var pair in properties)
            {
				LogWriter?.WriteLine($"Warning: Property {componentName}:{pair.Key} was not found!");
            }

			batch?.EndInit();
        }

		/// <summary>
		/// Returns the collection of all properties on a component
		/// that can be configured from the command line.
		/// </summary>
		/// <remarks>
		/// Configurable properties are those properties that can be set (not read-only),
		/// which do not have [<see cref="BrowsableAttribute"/>(<see langword="false"/>)], and their type
		/// can be converted to and from <see cref="string"/>.
		/// </remarks>
		private IEnumerable<PropertyDescriptor> GetConfigurableProperties(object component)
        {
			return TypeDescriptor.GetProperties(component).Cast<PropertyDescriptor>().Where(
				p =>
					!p.IsReadOnly &&
					p.IsBrowsable &&
					IsStringConvertible(p.Converter)
			);
		}

		static readonly Type stringType = typeof(string);

		/// <summary>
		/// Checks whether <paramref name="converter"/> can be used to convert to and from
		/// <see cref="string"/>.
		/// </summary>
		/// <param name="converter">The converter to check.</param>
		/// <returns><see langword="true"/> if the conversion is permitted, <see langword="false"/> otherwise.</returns>
		private bool IsStringConvertible(TypeConverter converter)
        {
			if(converter != null)
			{
				return converter.CanConvertFrom(stringType) && converter.CanConvertTo(stringType);
			}
			return false;
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

		async ITask<ValueTuple> IResultFactory<ValueTuple, string>.Invoke<T>(T component, string id)
		{
			if(IsIncluded(component, id))
			{
				LogWriter?.WriteLine($" - {id} ({TextTools.GetUserFriendlyName(TypeDescriptor.GetReflectionType(component))})");
				if(componentProperties.TryGetValue(id, out var dict))
				{
					componentProperties.Remove(id);
					SetProperties(component, id, dict);
				}
				foreach(var prop in GetConfigurableProperties(component))
				{
					var value = prop.GetValue(component);
					var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
					var converter = prop.Converter;
					string typeDesc;
					if(!(type.IsPrimitive || (!type.IsEnum && Type.GetTypeCode(type) != TypeCode.Object)) && converter.GetStandardValuesSupported() && converter.GetStandardValues() is { Count: > 0 } values)
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
			/// The application should search input files using SPARQL.
			/// </summary>
			Search,

			/// <summary>
			/// The application should list all available components.
			/// </summary>
			List,

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
				{"b", "buffered", null, "buffer all data in a graph before writing"},
				{"h", "hash", "pattern", "set the main binary hash"},
				{"r", "root", "uri", "set the hierarchy root URI prefix"},
				{"s", "sparql-query", "file", "perform a SPARQL query during processing"},
                {"p", "plugin", "id", "loads a plugin with a particular identifier"},
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
					CreateOptionsXml();
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
		protected override OptionArgument OnOptionFound(string option)
		{
            OptionArgument Inner()
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
					case "o":
					case "only-once":
						if(onlyOnce)
						{
							throw OptionAlreadySpecified(option);
						}
						onlyOnce = true;
						return OptionArgument.None;
					case "b":
					case "buffered":
						if(!options.DirectOutput)
						{
							throw OptionAlreadySpecified(option);
						}
						options.DirectOutput = false;
						return OptionArgument.None;
					case "u":
					case "ugly":
						if(!options.PrettyPrint)
						{
							throw OptionAlreadySpecified(option);
						}
						options.PrettyPrint = false;
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
					case "f":
					case "format":
						return OptionArgument.Required;
					case "p":
					case "plugin":
						return OptionArgument.Required;
					case "C":
					case "config":
						return OptionArgument.Required;
					case "?":
					case "help":
						Help();
						return OptionArgument.None;
					default:
						if(componentPropertyRegex.IsMatch(option))
						{
							if(componentProperties.ContainsKey(option))
							{
								throw OptionAlreadySpecified(option);
							}
							return OptionArgument.Required;
						}
						throw UnrecognizedOption(option);
				}
			}
			var result = Inner();
			if(result == OptionArgument.None)
			{
				StoreOption(option, null);
            }
			return result;
		}

		/// <inheritdoc/>
		protected override void OnOptionArgumentFound(string option, string? argument)
		{
			void Inner()
			{
				switch(option)
				{
					case "r":
					case "root":
						if(rootSpecified)
						{
							throw OptionAlreadySpecified(option);
						}
						if(!Uri.TryCreate(argument, UriKind.Absolute, out _))
						{
							throw new ApplicationException("The argument to option '" + option + "' must be a well-formed absolute URI.");
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
						var match = TextTools.ConvertWildcardToRegex(argument!);
						if(mainHash == null)
						{
							mainHash = match;
							mainHashName = argument;
							componentMatchers.Add(new Matcher(true, "data-hash:" + argument!, true));
						}else{
							throw OptionAlreadySpecified(option);
						}
						break;
					case "s":
					case "sparql-query":
						queries.Add(argument!);
						break;
					case "f":
					case "format":
						if(options.Format != null)
						{
							throw OptionAlreadySpecified(option);
						}
						options.Format = argument!;
						break;
					case "p":
					case "plugin":
						plugins.Add(argument!);
						break;
					case "C":
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
			Inner();
            StoreOption(option, argument);
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

		static readonly XNamespace configNs = "https://sfi.is4.site/config";
		static readonly XName configRoot = configNs + "options";

		static readonly XName nil = XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance");

        void LoadConfig(string name = "sfi-config.xml")
		{
			foreach(var config in environment.GetFiles(name))
			{
				if(config is IFileInfo configFile)
				{
					LogWriter.WriteLine($"Loading configuration from {configFile.Name}...");
					using var stream = configFile.Open();
					var doc = XDocument.Load(stream);
					var root = doc.Root;
					if(root.Name != configRoot)
					{
						throw new ApplicationException($"{name}: expected {configRoot} as root element, found {root.Name}.");
					}
					foreach(var elem in AttributeElements(root).Concat(root.Elements()))
					{
						LoadConfigElement(elem, null);
					}
				}
			}
		}

		void LoadConfigElement(XElement element, string? prefix)
		{
			if(element.Name.Namespace != configNs)
			{
				// Ignore foreign elements
				return;
			}

			var optionName = XmlConvert.DecodeName(element.Name.LocalName);
			if(prefix != null)
			{
				optionName = prefix + optionName;
            }

			bool? isNil = null;
			if(element.Attribute(nil)?.Value is string nilValue)
			{
				isNil = XmlConvert.ToBoolean(nilValue);
			}

			var content = AttributeElements(element).Concat(element.Nodes());

            if(!content.Any(n => n is XText or XElement))
			{
                if((element.IsEmpty && isNil != false) || (isNil == true))
                {
                    // Must be a switch
                    var arg = OnOptionFound(optionName);
                    switch(arg)
                    {
                        case OptionArgument.Optional:
                            OnOptionArgumentFound(optionName, null);
                            break;
                        case OptionArgument.Required:
                            throw ArgumentExpected(optionName);
                    }
				}else{
					// May be a switch or empty string
					var arg = OnOptionFound(optionName);
					switch(arg)
					{
						case OptionArgument.Optional:
						case OptionArgument.Required:
							OnOptionArgumentFound(optionName, "");
							break;
					}
				}
            }else if(isNil == true)
			{
				throw new ApplicationException($"Option {optionName} is set to nil but it has content.");
			}

            var innerPrefix = optionName + ":";

			foreach(var child in content)
			{
				switch(child)
				{
					case XText childText:
						var arg = OnOptionFound(optionName);
                        switch(arg)
                        {
                            case OptionArgument.Optional:
                            case OptionArgument.Required:
                                OnOptionArgumentFound(optionName, childText.Value);
								break;
                            case OptionArgument.None:
								throw ArgumentNotExpected(optionName);
                        }
                        break;
					case XElement childElement:
						LoadConfigElement(childElement, innerPrefix);
						break;
				}
			}
        }

		static IEnumerable<XElement> AttributeElements(XElement element)
		{
			return element.Attributes()
				.Where(a => !a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None)
				.Select(a => new XElement(element.Name.Namespace + a.Name.LocalName, a.Value));
		}

		XDocument? optionsXml;
		XElement? optionsRoot;

		void CreateOptionsXml()
		{
			optionsXml = new XDocument
			(
				optionsRoot = new XElement(configRoot)
			);
		}

		void StoreOption(string option, string? argument)
		{
			if(optionsRoot != null)
			{
				var path = option.Split(':');
				var elem = optionsRoot;
				foreach(var component in path)
				{
					elem.Add(elem = new XElement(configNs + XmlConvert.EncodeLocalName(component)));
				}
				if(argument != null)
                {
                    elem.Value = argument;
					if(String.IsNullOrWhiteSpace(argument))
					{
						elem.Add(new XAttribute(XNamespace.Xml + "space", "preserve"));
					}
                }
			}
		}

		void SimplifyOptions(XElement elem, bool root)
        {
			if(!root)
            {
                while(elem.NextNode is XElement nextElem && nextElem.Name == elem.Name)
                {
					// Merge content with the following element
					foreach(var attr in nextElem.Attributes())
					{
						elem.Add(attr);
					}
					foreach(var elem2 in nextElem.Elements())
					{
						elem.Add(elem2);
					}
					nextElem.Remove();
                }
            }
            foreach(var inner in elem.Elements().ToList())
			{
				var name = inner.Name;
				if(name.Namespace != configNs)
				{
					// In case some non-config elements get added
					continue;
				}
				if(inner.IsEmpty && !inner.Attributes().Any(a => a.Name.NamespaceName == XNamespace.None))
				{
					// Can't simplify switches
					continue;
				}
				var localName = name.LocalName;
				if(!inner.Elements().Any())
				{
                    // This is a normal option
                    if(root)
                    {
                        switch(localName)
                        {
                            case "x":
                            case "exclude":
                            case "i":
                            case "include":
                            case "s":
                            case "sparql-query":
                                // Don't change these to attributes
                                continue;
                        }
                    }
					if(elem.Attribute(localName) == null)
					{
						elem.Add(new XAttribute(localName, inner.Value));
						inner.Remove();
					}
				}else{
					// Container for options
					SimplifyOptions(inner, false);
				}
			}
		}

		void PrintOptionsXml()
		{
			if(optionsXml != null)
            {
                if(output == null)
                {
                    throw new ApplicationException("An output must be specified to save the options!");
                }
				SimplifyOptions(optionsRoot!, true);
				using var file = environment.CreateFile(output, "application/xml");
				var settings = new XmlWriterSettings
				{
					Encoding = new UTF8Encoding(false),
					CloseOutput = false,
					Indent = true,
					OmitXmlDeclaration = true
				};
                using var writer = XmlWriter.Create(file, settings);
                optionsXml.Save(writer);
			}
		}
    }
}
