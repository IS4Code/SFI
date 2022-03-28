using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class Application<TArchiver> : CommandApplication where TArchiver : Archiver, new()
    {
        readonly IApplicationEnvironment environment;

		TextWriter writer;

		protected override TextWriter LogWriter => writer;

		protected override int WindowWidth => environment.WindowWidth;

		readonly ArchiverOptions options;

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
		List<Regex> analyzerMatches = new List<Regex>();
		List<Regex> formatMatches = new List<Regex>();
		List<Regex> hashMatches = new List<Regex>();
		string output;

		bool quiet;
		bool rootSpecified;
		bool dataOnly;

		public async ValueTask Run(params string[] args)
        {
			try{
				Parse(args);

				Banner();

				if(mode == null)
				{
					var modeNames = Enum.GetNames(typeof(Mode)).Select(n => n.ToLowerInvariant());
					throw new ApplicationException($"Mode must be specified (one of {String.Join(", ", modeNames)})!");
				}

				var archiver = new TArchiver();
				archiver.OutputLog = writer;
				archiver.AddDefault();

				switch(mode)
				{
					case Mode.Analyzers:
						PrintList("analyzers", archiver.Analyzers);
						return;
					case Mode.Formats:
						PrintList("data formats", archiver.DataAnalyzer.DataFormats);
						PrintList("XML formats", archiver.XmlAnalyzer.XmlFormats);
						PrintList("container formats", archiver.ContainerProviders);
						return;
					case Mode.Hashes:
						PrintList("data hashes", archiver.DataAnalyzer.HashAlgorithms);
						PrintList("file hashes", archiver.FileAnalyzer.HashAlgorithms);
						PrintList("image data hashes", archiver.ImageDataHashAlgorithms);
						return;
				}

				if(inputs.Count == 0 || output == null)
				{
					throw new ApplicationException("At least one input and an output must be specified!");
				}

				FilterList(archiver.Analyzers, analyzerMatches);
				FilterList(archiver.DataAnalyzer.DataFormats, formatMatches);
				FilterList(archiver.XmlAnalyzer.XmlFormats, formatMatches);
				FilterList(archiver.ContainerProviders, formatMatches);
				FilterList(archiver.DataAnalyzer.HashAlgorithms, hashMatches);
				FilterList(archiver.FileAnalyzer.HashAlgorithms, hashMatches);
				FilterList(archiver.ImageDataHashAlgorithms, hashMatches);

				if(quiet)
				{
					writer = TextWriter.Null;
				}

				var inputFiles = inputs.SelectMany(input => environment.GetFiles(input));

				using(var outputStream = environment.CreateFile(output, archiver.OutputMediaType))
                {
					if(dataOnly)
                    {
						await archiver.Archive<IStreamFactory>(inputFiles, outputStream, options);
                    }else{
						await archiver.Archive(inputFiles, outputStream, options);
					}
				}
			}catch(Exception e) when(!Debugger.IsAttached)
			{
				Log(e.Message);
			}
        }

		void PrintList<T>(string type, IEnumerable<T> values)
        {
			bool first = true;
			foreach(var value in values)
            {
				if(first)
                {
					LogWriter.WriteLine();
					LogWriter.WriteLine($"Available {type}:");
					first = false;
				}
				LogWriter.WriteLine(" - " + DataTools.GetUserFriendlyName(value));
            }
        }

		void FilterList<T>(ICollection<T> list, IEnumerable<Regex> matches)
        {
			var filtered = new List<T>();
			foreach(var item in list)
			{
				var name = DataTools.GetUserFriendlyName(item);
				if(!matches.Any(m => m.IsMatch(name)))
                {
					filtered.Add(item);
                }
            }

			foreach(var item in filtered)
            {
				list.Remove(item);
            }
        }

        protected override string Usage => "mode [options] input... output";

        public override void Description()
		{
			base.Description();
			LogWriter.WriteLine();
			LogWriter.Write(" ");
			OutputWrapPad("This software analyzes the formats of given files and outputs RDF description of their contents.", 1);
		}

		enum Mode
		{
			Describe,
			Formats,
			Analyzers,
			Hashes
		}

		public override IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection{
				{"q", "quiet", null, "do not print any additional messages"},
				{"a", "analyzer", "pattern", "enable given analyzers"},
				{"f", "format", "pattern", "enable given formats"},
				{"h", "hash", "pattern", "enable given hashes"},
				{"c", "compress", null, "perform gzip compression on the output"},
				{"m", "metadata", null, "add annotation metadata to output"},
				{"d", "data-only", null, "do not store input file information"},
				{"?", "help", null, "displays this help message"},
			};
		}
		
		protected override void Notes()
		{

		}

		protected override OperandState OnOperandFound(string operand)
		{
			if(mode == null)
            {
				if(!Enum.TryParse<Mode>(operand, true, out var mode))
                {
					throw new ApplicationException("Invalid mode.");
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
				case "h":
				case "hash":
					return OptionArgument.Required;
				case "a":
				case "analyzer":
					return OptionArgument.Required;
				case "f":
				case "format":
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
				case "a":
				case "analyzer":
					analyzerMatches.Add(DataTools.ConvertWildcardToRegex(argument));
					break;
				case "f":
				case "format":
					formatMatches.Add(DataTools.ConvertWildcardToRegex(argument));
					break;
				case "h":
				case "hash":
					hashMatches.Add(DataTools.ConvertWildcardToRegex(argument));
					break;
			}
		}
    }
}
