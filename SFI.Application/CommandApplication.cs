using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IS4.SFI.Application
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
					if(version.Build == 0)
					{
						version = new Version(version.Major, version.Minor);
					}else{
						version = new Version(version.Major, version.Minor, version.Build);
					}
					msg += " v"+version.ToString();
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
		/// One of the values of <see cref="OptionArgumentFlags"/> specifying
		/// the argument handling for this option.
		/// </returns>
		protected abstract OptionArgumentFlags OnOptionFound(string option);

        /// <summary>
        /// Calls <see cref="OnOptionFound(string)"/> when
        /// an option is found.
        /// </summary>
        /// <param name="option">The name of the option, without any delimiter characters.</param>
        /// <returns>
        /// One of the values of <see cref="OptionArgumentFlags"/> specifying
        /// the argument handling for this option.
        /// </returns>
        protected virtual OptionArgumentFlags OptionFound(string option)
		{
			return OnOptionFound(option);
		}

        /// <summary>
        /// Called internally from <see cref="Parse(string[])"/> when
        /// an argument for an option is found.
        /// </summary>
        /// <param name="option">The name of the option, without any delimiter characters.</param>
        /// <param name="argument">The argument of the option.</param>
        /// <param name="flags">The argument handlings flags previously returned by <see cref="OnOperandFound(string)"/>.</param>
        protected abstract void OnOptionArgumentFound(string option, string? argument, OptionArgumentFlags flags);

        /// <summary>
        /// Calls <see cref="OnOptionArgumentFound(string, string?, OptionArgumentFlags)"/>
		/// when an argument for an option is found.
        /// </summary>
        /// <param name="option">The name of the option, without any delimiter characters.</param>
        /// <param name="argument">The argument of the option.</param>
        /// <param name="flags">The argument handlings flags previously returned by <see cref="OnOperandFound(string)"/>.</param>
        protected virtual void OptionArgumentFound(string option, string? argument, OptionArgumentFlags flags)
		{
			OnOptionArgumentFound(option, argument, flags);
		}

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
		/// Modifies the input argument in a desirable way
		/// before it is parsed by <see cref="Parse(string[])"/>.
		/// </summary>
		/// <param name="arg">The input argument.</param>
		/// <returns>The modified value.</returns>
		protected virtual string ProcessArg(string arg)
		{
			return arg;
		}

		/// <summary>
		/// Obtains the canonical representation of an option.
		/// </summary>
		/// <param name="option">The option name.</param>
		/// <returns>The canonical option name for <paramref name="option"/>.</returns>
		protected virtual string GetCanonicalOption(string option)
		{
			return option.Length <= 1 ? option : option.ToLowerInvariant();
        }

        /// <summary>
        /// Parses the arguments provided to the application
        /// and initializes it with the values specified
        /// by the arguments.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <exception cref="ApplicationExitException">
        /// Could be thrown from one of the option or operand handler
        /// to indicate that the application should be terminated.
        /// </exception>
        public void Parse(string[] args)
		{
			var added = new HashSet<string>();

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
					OptionArgumentFlags flags;
					string name;
					string? argument;

					int delim = arg.IndexOf('=');
					if(delim != -1)
					{
						name = arg.Substring(2, delim - 2);
						flags = OptionFound(name);
                        if((flags & OptionArgumentFlags.HasArgument) == 0)
						{
							throw ArgumentNotExpected(name);
						}
						argument = arg.Substring(delim + 1);
					}else{
						name = arg.Substring(2);
                        flags = OptionFound(name);
                        if((flags & OptionArgumentFlags.HasArgument) == 0)
                        {
                            CheckAdded(name, flags);
                            continue;
                        }
                        if((flags & OptionArgumentFlags.RequiredArgument) != 0)
                        {
                            if(++i >= args.Length) throw ArgumentExpected(name);
							argument = ProcessArg(args[i]);
						}else{
							argument = null;
						}
                    }
                    CheckAdded(name, flags);
                    OptionArgumentFound(name, argument, flags);
                }else if(arg.Length > 1 && arg[0] == '-' && IsOptionChar(arg[1]))
				{
					for(int j = 1; j < arg.Length; j++)
					{
						string name = arg[j].ToString();
						string? argument = String.Join("", arg.Skip(j+1).TakeWhile(c => !IsOptionChar(c)));

						var flags = OptionFound(name);
						CheckAdded(name, flags);

                        if((flags & OptionArgumentFlags.HasArgument) == 0)
                        {
							if(argument.Length > 0)
                            {
                                throw ArgumentNotExpected(name);
                            }
							continue;
                        }
						
                        if((flags & OptionArgumentFlags.RequiredArgument) != 0)
                        {
							if(argument.Length == 0)
							{
								if(j+1 < arg.Length)
								{
                                    argument = arg.Substring(j+1);
									j = arg.Length-1;
								}else{
									if(++i >= args.Length) throw ArgumentExpected(name);
									argument = ProcessArg(args[i]);
                                    argument = ProcessArg(args[i]);
								}
							}
						}else{
							if(argument.Length == 0)
                            {
                                argument = null;
                            }
						}

                        OptionArgumentFound(name, argument, flags);

						j += argument?.Length ?? 0;
					}
				}else{
					if(OnOperandFound(arg) == OperandState.OnlyOperands)
					{
						operands = true;
					}
				}
            }

            void CheckAdded(string name, OptionArgumentFlags flags)
            {
                if((flags & OptionArgumentFlags.AllowMultiple) == 0)
                {
                    if(!added!.Add(GetCanonicalOption(name)))
                    {
                        throw OptionAlreadySpecified(name);
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
		public readonly struct OptionInfo
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
	[Flags]
	public enum OptionArgumentFlags
	{
		/// <summary>
		/// The option should not have any argument.
		/// </summary>
		None = 0,

        /// <summary>
        /// The option has an argument.
        /// </summary>
        HasArgument = 0b10,

		/// <summary>
		/// The option has an optional argument.
		/// </summary>
		OptionalArgument = 0b10,

		/// <summary>
		/// The option has a required argument.
		/// </summary>
		RequiredArgument = 0b110,

		/// <summary>
		/// The option permits multiple values.
		/// </summary>
		AllowMultiple = 0b1000
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
