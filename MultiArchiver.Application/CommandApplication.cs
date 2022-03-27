using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IS4.MultiArchiver
{
    public abstract class CommandApplication
	{
		protected Assembly Assembly => Assembly.GetExecutingAssembly();

		protected virtual string ApplicationName => Assembly.GetName().Name;

		protected virtual string ExecutableName => Path.GetFileNameWithoutExtension(Assembly.Location);

		protected abstract TextWriter LogWriter { get; }

		protected abstract int WindowWidth { get; }

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
		
		public virtual void Description()
		{
			foreach(AssemblyDescriptionAttribute desc in Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false))
			{
				LogWriter.WriteLine(desc.Description);
			}
		}
		
		public virtual IList<OptionInfo> GetOptions()
		{
			return new OptionInfoCollection();
		}
		
		protected virtual string Usage{
			get{
				return "";
			}
		}
		
		protected void Help()
		{
			Banner();
			Description();
			LogWriter.WriteLine();
			LogWriter.WriteLine("Usage: {0} {1}", ExecutableName, Usage);
			LogWriter.WriteLine();
			
			string optFormat = " -{0} [ --{1} ] {2} ";
			var options = GetOptions();
			
			int colLength = options.Max(o => String.Format(optFormat, o.ShortName, o.LongName, o.ArgumentText).Length);
			
			foreach(var opt in options)
			{
				string usage = String.Format(optFormat, opt.ShortName, opt.LongName, opt.ArgumentText);
				LogWriter.Write(usage);
				LogWriter.Write(new string(' ', colLength-usage.Length));
				OutputWrapPad(opt.Description, colLength, WindowWidth - colLength);
			}
			
			Notes();
			
			Environment.Exit(0);
		}
		
		protected virtual void Notes()
		{
			
		}
		
		public void OutputWrapPad(string text, int padLeft)
		{
			OutputWrapPad(text, padLeft, WindowWidth - padLeft);
		}
		
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
		
		public void Log(string message)
		{
			LogWriter.WriteLine("[{0}] {1}", ApplicationName, message);
		}
		
		protected abstract OptionArgument OnOptionFound(string option);
		protected abstract void OnOptionArgumentFound(string option, string argument);
		protected abstract OperandState OnOperandFound(string operand);
		
		protected virtual OptionArgument OnShortOptionFound(char option)
		{
			return OnOptionFound(option.ToString());
		}
		
		protected virtual void OnShortOptionArgumentFound(char option, string argument)
		{
			OnOptionArgumentFound(option.ToString(), argument);
		}
		
		
		public CommandApplication()
		{
			
		}
		
		public virtual string ProcessArg(string arg)
		{
			return arg;
		}
		
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
		
		public ApplicationException UnrecognizedOption(char option)
		{
			return UnrecognizedOption(option.ToString());
		}
		
		public ApplicationException UnrecognizedOption(string option)
		{
			return new ApplicationException("Unrecognized option '"+option+"'.");
		}
		
		public ApplicationException ArgumentExpected(char option)
		{
			return ArgumentExpected(option.ToString());
		}
		
		public ApplicationException ArgumentExpected(string option)
		{
			return new ApplicationException("Argument expected for option '"+option+"'.");
		}
		
		public ApplicationException ArgumentNotExpected(char option)
		{
			return ArgumentNotExpected(option.ToString());
		}
		
		public ApplicationException ArgumentNotExpected(string option)
		{
			return new ApplicationException("Argument not expected for option '"+option+"'.");
		}
		
		public ApplicationException ArgumentInvalid(char option, string expected)
		{
			return ArgumentInvalid(option.ToString(), expected);
		}
		
		public ApplicationException ArgumentInvalid(string option, string expected)
		{
			return new ApplicationException("Invalid argument provided for option '"+option+"', "+expected+" expected.");
		}
		
		public ApplicationException OptionAlreadySpecified(char option)
		{
			return OptionAlreadySpecified(option.ToString());
		}
		
		public ApplicationException OptionAlreadySpecified(string option)
		{
			return new ApplicationException("Option '"+option+"' has been already specified.");
		}
		
		public class OptionInfoCollection : List<OptionInfo>
		{
			public OptionInfoCollection()
			{
				
			}
			
			public OptionInfoCollection(int capacity) : base(capacity)
			{
				
			}
			
			public OptionInfoCollection(IEnumerable<OptionInfo> collection) : base(collection)
			{
				
			}
			
			public void Add(string shortName, string longName, string argument, string description)
			{
				Add(new OptionInfo(shortName, longName, argument, description));
			}
		}
		
		public struct OptionInfo
		{
			public string ShortName{get; private set;}
			public string LongName{get; private set;}
			public string Argument{get; private set;}
			public string Description{get; private set;}
			
			public string ArgumentText{
				get{
					return Argument == null ? "" : Argument+" ";
				}
			}
			
			public OptionInfo(string shortName, string longName, string argument, string description) : this()
			{
				ShortName = shortName;
				LongName = longName;
				Argument = argument;
				Description = description;
			}
		}
	}
	
	public enum OptionArgument
	{
		None, Optional, Required
	}
	
	public enum OperandState
	{
		ContinueOptions, OnlyOperands
	}
}
