using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IS4.SFI
{
	/// <summary>
	/// An abstract command-based application.
	/// </summary>
    public abstract class CommandApplication
	{
		/// <summary>
		/// The assembly of the current program.
		/// </summary>
		protected Assembly Assembly => Assembly.GetExecutingAssembly();

		/// <summary>
		/// The name of the application.
		/// </summary>
		protected virtual string ApplicationName => Assembly.GetName().Name;

		/// <summary>
		/// The name of the executable.
		/// </summary>
		protected virtual string ExecutableName => Path.GetFileNameWithoutExtension(Assembly.Location);

		/// <summary>
		/// The writer to use for writing messages for the user.
		/// </summary>
		protected abstract TextWriter LogWriter { get; }

		/// <summary>
		/// The expected window width.
		/// </summary>
		protected abstract int WindowWidth { get; }

		/// <summary>
		/// Writes a short text about the application.
		/// </summary>
		public virtual void Banner()
		{
			var asm = Assembly;
			string msg = "";
			
			var copyright = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if(copyright.Length > 0)
			{
				msg = ((AssemblyCopyrightAttribute)copyright[0]).Copyright;
			}
			var title = asm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if(title.Length > 0)
			{
				msg += " "+((AssemblyTitleAttribute)title[0]).Title;
				
				var version = asm.GetName().Version;
				if(version != null)
				{
					msg += " v"+version.ToString(2);
				}
			}
			var author = asm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
			if(author.Length > 0)
			{
				msg += " by "+((AssemblyCompanyAttribute)author[0]).Company;
			}
			LogWriter.WriteLine(msg);
		}
		
		/// <summary>
		/// Writes the description of the application.
		/// </summary>
		public virtual void Description()
		{
			foreach(AssemblyDescriptionAttribute desc in Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false))
			{
				LogWriter.WriteLine(desc.Description);
			}
		}
		
		/// <summary>
		/// Returns the list of available options, for example
		/// as <see cref="OptionInfoCollection"/>.
		/// </summary>
		/// <returns>The list of options available to use.</returns>
		public virtual IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection();
		}

		/// <summary>
		/// The command usage of the application.
		/// </summary>
		protected virtual string Usage => "";

		/// <summary>
		/// Writes the help screen and terminates the application.
		/// </summary>
		/// <exception cref="ApplicationExitException">
		/// Thrown at the end of the method.
		/// </exception>
		protected void Help()
		{
			Banner();
			Description();
			LogWriter.WriteLine();
			if(String.IsNullOrWhiteSpace(ExecutableName))
			{
				LogWriter.WriteLine("Usage: {0}", Usage);
			}else{
				LogWriter.WriteLine("Usage: {0} {1}", ExecutableName, Usage);
			}
			LogWriter.WriteLine();

			const string optFormat = " -{0} [ --{1} ] {2} ";
			const string optFormatLong = " --{1} {2} ";
			var options = GetOptions();
			
			int colLength = options.Max(o => String.Format(o.ShortName == null ? optFormatLong : optFormat, o.ShortName, o.LongName, o.ArgumentText).Length);
			
			foreach(var opt in options)
			{
				string usage = String.Format(opt.ShortName == null ? optFormatLong : optFormat, opt.ShortName, opt.LongName, opt.ArgumentText);
				LogWriter.Write(usage);
				LogWriter.Write(new string(' ', colLength-usage.Length));
				OutputWrapPad(opt.Description, colLength, WindowWidth - colLength);
			}
			
			Notes();

			throw new ApplicationExitException();
		}
		
		/// <summary>
		/// Writes additional notes about the application or its usage.
		/// </summary>
		protected virtual void Notes()
		{
			LogWriter.WriteLine();
			LogWriter.WriteLine("Examples:");

			Args("describe dir/* out.ttl");
			Desc("Describes all files in 'dir' using the default components, and saves the RDF output to 'out.ttl'.");

			Args("describe -d -h sha1 dir out.ttl");
			Desc("Same as above, but only loads the files in the directory as data ('-d'), without storing their names or other metadata.");
			Desc("In addition to that, the SHA-1 hash algorithm is used to produce 'ni:' URIs for content.");

			Args("describe -f rdf dir -");
			Desc("As above, but writes the RDF description as RDF/XML to the standard output.");

			Args("describe -b -f jsonld dir -");
			Desc("Writes the RDF description in JSON-LD instead. This requires buffering the output ('-b').");

			Args("describe -r urn:uuid: dir -");
			Desc("Does not use blank nodes to identify entities, instead using URIs starting with 'urn:uuid:'.");

			Args("describe -x *-hash:* -i data-hash:sha1 dir -");
			Desc("Does not use any of the supported hash algorithms, with the exception of SHA-1, to describe data.");

			Args("list -x *-format:* -i *-format:image/*");
			Desc("Excludes all file formats from the list of components, but keeps specific image formats.");

			Args("list -x * -i analyzer:stream-factory -i analyzer:data-object");
			Desc("Only allows the analysis of actual data, not files.");

			Args("list --analyzer:stream-factory:max-depth-for-formats \"\"");
			Desc("Sets this property value to null, disabling depth checks.");

			void Args(string str)
			{
				LogWriter.WriteLine();
				LogWriter.WriteLine(str);
			}

			void Desc(string str)
            {
				LogWriter.Write("  ");
				OutputWrapPad(str, 2, WindowWidth - 2);
			}
		}
		
		/// <summary>
		/// Writes text to the output, padded by <paramref name="pad"/> spaces
		/// from both sides of the window, wrapping it if necessary.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="pad">The number of spaces to pad with.</param>
		public void OutputWrapPad(string text, int pad)
		{
			OutputWrapPad(text, pad, WindowWidth - pad);
		}

		/// <summary>
		/// Writes text to the output, padded by <paramref name="padLeft"/> spaces
		/// from both sides of the window, wrapping it if necessary.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="padLeft">The number of spaces to pad with.</param>
		/// <param name="textWidth">The maximum characters allowed on a line.</param>
		public void OutputWrapPad(string text, int padLeft, int textWidth)
		{
			int totalLength = 0;
			foreach(string s in text.Split(' '))
			{
				bool first = totalLength == 0;
				totalLength += s.Length;
				if(totalLength >= textWidth-1)
				{
					LogWriter.WriteLine();
					if(padLeft > 0)
					{
						LogWriter.Write(new string(' ', padLeft));
					}
					totalLength = s.Length;
					first = true;
				}
				if(!first)
				{
					LogWriter.Write(" ");
					totalLength += 1;
				}
				LogWriter.Write(s);
			}
			if(totalLength > 0)
			{
				LogWriter.WriteLine();
			}
		}
		
		/// <summary>
		/// Called internally from <see cref="Parse(string[])"/> when
		/// an option is found.
		/// </summary>
		/// <param name="option">The name of the option, without any delimiter characters.</param>
		/// <returns>
		/// One of the values of <see cref="OptionArgument"/> specifying
		/// the argument handling for this option.
		/// </returns>
		protected abstract OptionArgument OnOptionFound(string option);

		/// <summary>
		/// Called internally from <see cref="Parse(string[])"/> when
		/// an argument for an option is found.
		/// </summary>
		/// <param name="option">The name of the option, without any delimiter characters.</param>
		/// <param name="argument">The argument of the option.</param>
		protected abstract void OnOptionArgumentFound(string option, string? argument);

		/// <summary>
		/// Called internally from <see cref="Parse(string[])"/> when
		/// the command's operand is found.
		/// </summary>
		/// <param name="operand">The value of the operand.</param>
		/// <returns>
		/// One of the values of <see cref="OperandState"/> specifying
		/// the state of the parser after this operand.
		/// </returns>
		protected abstract OperandState OnOperandFound(string operand);

		/// <summary>
		/// Called internally from <see cref="Parse(string[])"/> when
		/// a short option is found. The default implementation
		/// calls <see cref="OnOptionFound(string)"/>.
		/// </summary>
		/// <param name="option">The name of the option, without any delimiter characters.</param>
		/// <returns>
		/// One of the values of <see cref="OptionArgument"/> specifying
		/// the argument handling for this option.
		/// </returns>
		protected virtual OptionArgument OnShortOptionFound(char option)
		{
			return OnOptionFound(option.ToString());
		}

		/// <summary>
		/// Called internally from <see cref="Parse(string[])"/> when
		/// an argument for a short option is found. The default implementation
		/// calls <see cref="OnOptionArgumentFound(string, string)"/>.
		/// </summary>
		/// <param name="option">The name of the option, without any delimiter characters.</param>
		/// <param name="argument">The argument of the option.</param>
		protected virtual void OnShortOptionArgumentFound(char option, string? argument)
		{
			OnOptionArgumentFound(option.ToString(), argument);
		}
		
		/// <summary>
		/// Modifies the input argument in a desirable way
		/// before it is parsed by <see cref="Parse(string[])"/>.
		/// </summary>
		/// <param name="arg">The input argument.</param>
		/// <returns>The modified value.</returns>
		public virtual string ProcessArg(string arg)
		{
			return arg;
		}

		/// <summary>
		/// Parses the arguments provided to the application
		/// and initializes it with the values specified
		/// by the arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="ApplicationExitException">
		/// Could be thrown from one of the option or operand handler
		/// to indicate that the application should be terminated.
		/// </exception>
		public void Parse(string[] args)
		{
			bool operands = false;
			for(int i = 0; i < args.Length; i++)
			{
				string arg = ProcessArg(args[i]);
				if(operands)
				{
					OnOperandFound(arg);
				}else if(arg == "--")
				{
					operands = true;
				}else if(arg.StartsWith("--"))
				{
					int delim = arg.IndexOf('=');
					if(delim != -1)
					{
						string name = arg.Substring(2, delim-2);
						if(OnOptionFound(name) == OptionArgument.None)
						{
							throw ArgumentNotExpected(name);
						}
						string argument = arg.Substring(delim+1);
						OnOptionArgumentFound(name, argument);
					}else{
						string name = arg.Substring(2);
						switch(OnOptionFound(name))
						{
							case OptionArgument.Optional:
								OnOptionArgumentFound(name, null);
								break;
							case OptionArgument.Required:
								if(++i >= args.Length) throw ArgumentExpected(name);
								OnOptionArgumentFound(name, ProcessArg(args[i]));
								break;
						}
					}
				}else if(arg.Length > 1 && arg[0] == '-' && IsOptionChar(arg[1]))
				{
					for(int j = 1; j < arg.Length; j++)
					{
						char opt = arg[j];
						string argument = String.Join("", arg.Skip(j+1).TakeWhile(c => !IsOptionChar(c)));
						
						switch(OnShortOptionFound(opt))
						{
							case OptionArgument.None:
								if(argument.Length > 0) throw ArgumentNotExpected(opt);
								break;
							case OptionArgument.Optional:
								if(argument.Length > 0)
								{
									OnShortOptionArgumentFound(opt, argument);
								}else{
									OnShortOptionArgumentFound(opt, null);
								}
								break;
							case OptionArgument.Required:
								if(argument.Length > 0)
								{
									OnShortOptionArgumentFound(opt, argument);
								}else{
									if(j+1 < arg.Length)
									{
										OnShortOptionArgumentFound(opt, arg.Substring(j+1));
										j = arg.Length-1;
									}else if(++i >= args.Length)
									{
										throw ArgumentExpected(opt);
									}else{
										OnShortOptionArgumentFound(opt, ProcessArg(args[i]));
									}
								}
								break;
						}
						j += argument.Length;
					}
				}else{
					if(OnOperandFound(arg) == OperandState.OnlyOperands)
					{
						operands = true;
					}
				}
			}
		}
		
		private static bool IsOptionChar(char c)
		{
			return c == '?' || Char.IsLetter(c);
		}
		
		/// <summary>
		/// Produces an exception when an unrecognized option is found.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException UnrecognizedOption(char option)
		{
			return UnrecognizedOption(option.ToString());
		}

		/// <summary>
		/// Produces an exception when an unrecognized option is found.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException UnrecognizedOption(string option)
		{
			return new ApplicationException("Unrecognized option '"+option+"'.");
		}

		/// <summary>
		/// Produces an exception when an option should
		/// have an argument, but none is found in the input.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentExpected(char option)
		{
			return ArgumentExpected(option.ToString());
		}

		/// <summary>
		/// Produces an exception when an option should
		/// have an argument, but none is found in the input.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentExpected(string option)
		{
			return new ApplicationException("Argument expected for option '"+option+"'.");
		}

		/// <summary>
		/// Produces an exception when an option should not
		/// have an argument, but one is assigned to it.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentNotExpected(char option)
		{
			return ArgumentNotExpected(option.ToString());
		}

		/// <summary>
		/// Produces an exception when an option should not
		/// have an argument, but one is assigned to it.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentNotExpected(string option)
		{
			return new ApplicationException("Argument not expected for option '"+option+"'.");
		}

		/// <summary>
		/// Produces an exception when an option has
		/// an argument in an invalid form.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <param name="expected">The expected form of the argument.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentInvalid(char option, string expected)
		{
			return ArgumentInvalid(option.ToString(), expected);
		}

		/// <summary>
		/// Produces an exception when an option has
		/// an argument in an invalid form.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <param name="expected">The expected form of the argument.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException ArgumentInvalid(string option, string expected)
		{
			return new ApplicationException("Invalid argument provided for option '"+option+"', "+expected+" expected.");
		}

		/// <summary>
		/// Produces an exception when an option should
		/// be specified only once, but it was used multiple times.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException OptionAlreadySpecified(char option)
		{
			return OptionAlreadySpecified(option.ToString());
		}

		/// <summary>
		/// Produces an exception when an option should
		/// be specified only once, but it was used multiple times.
		/// </summary>
		/// <param name="option">The name of the option.</param>
		/// <returns>The exception for this situation.</returns>
		public ApplicationException OptionAlreadySpecified(string option)
		{
			return new ApplicationException("Option '"+option+"' has been already specified.");
		}
		
		/// <summary>
		/// Provides a collection of instances of <see cref="OptionInfo"/>
		/// with a convenient <see cref="Add(string, string, string, string)"/>
		/// method.
		/// </summary>
		public class OptionInfoCollection : List<OptionInfo>
		{
			/// <summary>
			/// Creates a new instance of the collection.
			/// </summary>
			public OptionInfoCollection()
			{
				
			}
			
			/// <summary>
			/// Creates a new instance of the collection with a given capacity.
			/// </summary>
			/// <param name="capacity">The capacity of the collection.</param>
			public OptionInfoCollection(int capacity) : base(capacity)
			{
				
			}
			
			/// <summary>
			/// Creates a new instance of the collection from existing members.
			/// </summary>
			/// <param name="collection">The members of the collection.</param>
			public OptionInfoCollection(IEnumerable<OptionInfo> collection) : base(collection)
			{
				
			}
			
			/// <summary>
			/// Adds a new option to the collection.
			/// </summary>
			/// <param name="shortName">The short name of the option.</param>
			/// <param name="longName">The long name of the option.</param>
			/// <param name="argument">The type of the argument for the option.</param>
			/// <param name="description">The description of the option.</param>
			public void Add(string? shortName, string longName, string? argument, string description)
			{
				Add(new OptionInfo(shortName, longName, argument, description));
			}
		}
		
		/// <summary>
		/// Represents a single option.
		/// </summary>
		public struct OptionInfo
		{
			/// <summary>
			/// The short name of the option.
			/// </summary>
			public string? ShortName { get; }

			/// <summary>
			/// The long name of the option.
			/// </summary>
			public string LongName { get; }

			/// <summary>
			/// The type of the argument for the option.
			/// </summary>
			public string? Argument { get; }

			/// <summary>
			/// The description of the option.
			/// </summary>
			public string Description { get; }
			
			/// <summary>
			/// A text when the argument should be displayed.
			/// </summary>
			public string ArgumentText => Argument == null ? "" : Argument+" ";

			/// <summary>
			/// Creates a new instance of the option.
			/// </summary>
			/// <param name="shortName">The value of <see cref="ShortName"/>.</param>
			/// <param name="longName">The value of <see cref="LongName"/>.</param>
			/// <param name="argument">The value of <see cref="Argument"/>.</param>
			/// <param name="description">The value of <see cref="Description"/>.</param>
			public OptionInfo(string? shortName, string longName, string? argument, string description) : this()
			{
				ShortName = shortName;
				LongName = longName;
				Argument = argument;
				Description = description;
			}
		}

		/// <summary>
		/// Thrown from within one of the methods to indicate that the
		/// application should be terminated.
		/// </summary>
		public class ApplicationExitException : Exception
        {

        }
	}
	
	/// <summary>
	/// Specifies the argument handling for an encountered option.
	/// </summary>
	public enum OptionArgument
	{
		/// <summary>
		/// The option should not have any argument.
		/// </summary>
		None,

		/// <summary>
		/// The option has an optional argument.
		/// </summary>
		Optional,

		/// <summary>
		/// The option has a required argument.
		/// </summary>
		Required
	}
	
	/// <summary>
	/// Specifies the state of the parser after an operand is encountered.
	/// </summary>
	public enum OperandState
	{
		/// <summary>
		/// The options may still be provided.
		/// </summary>
		ContinueOptions,
		
		/// <summary>
		/// Only operands are accepted after this point.
		/// </summary>
		OnlyOperands
	}
}
