using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		}

		Mode? mode;
		List<string> inputs = new List<string>();
		string output;

		bool quiet;
		bool rootSpecified;

		public async ValueTask Run(params string[] args)
        {
			try{
				Parse(args);

				Banner();

				if(mode == null)
				{
					throw new ApplicationException("Mode must be specified!");
				}
				if(inputs.Count == 0)
				{
					throw new ApplicationException("Input must be specified!");
				}
				if(output == null)
				{
					throw new ApplicationException("Output must be specified!");
				}

				if(quiet)
				{
					writer = TextWriter.Null;
				}

				var archiver = new TArchiver();
				archiver.OutputLog = writer;
				archiver.AddDefault();

				var inputFiles = inputs.Select(input => new InputFile(environment, input));

				using(var outputStream = environment.OpenOutputFile(output))
                {
					await archiver.Archive(inputFiles, outputStream, options);
				}
			}catch(Exception e)
			{
				Log(e.Message);
			}
        }

        class InputFile : IStreamFactory
        {
			readonly IApplicationEnvironment environment;
			readonly string path;

			public InputFile(IApplicationEnvironment environment, string path)
            {
				this.environment = environment;
				this.path = path;
            }

            public long Length {
				get {
					using(var file = environment.OpenInputFile(path))
                    {
						return file.Length;
                    }
				}
			}

			public StreamFactoryAccess Access => StreamFactoryAccess.Reentrant;

            public object ReferenceKey => this;

            public object DataKey => null;

            public Stream Open()
            {
				return environment.OpenInputFile(path);
            }
        }

        protected override string Usage => "mode [options] input output";

        public override void Description()
		{
			base.Description();
			LogWriter.WriteLine();
			LogWriter.Write(" ");
			OutputWrapPad("This software extracts metadata from a specific file.", 1);
		}

		enum Mode
		{
			Describe
		}

		public override IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection{
				{"q", "quiet", null, "do not print any additional messages"},
				{"c", "compress", null, "perform gzip compression on the output"},
				{"s", "stable", null, "produce the same output every time"},
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
				case "s":
				case "stable":
					if(options.HideMetadata)
					{
						throw OptionAlreadySpecified(option);
					}
					options.HideMetadata = true;
					return OptionArgument.None;
				case "r":
				case "root":
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
			}
		}
    }
}
