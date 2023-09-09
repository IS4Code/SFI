using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Application
{
    /// <summary>
    /// An abstract base application class that supports loading config
    /// to and from XML files using the option mechanism.
    /// </summary>
    public abstract class ConfigurableApplication : CommandApplication
    {
		readonly XNamespace configNs;
		readonly XName configRoot;

		static readonly XName nil = XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance");

		/// <summary>
		/// Creates a new instance of the application.
		/// </summary>
		/// <param name="configNs">The namespace that holds the option elements.</param>
		public ConfigurableApplication(XNamespace configNs)
		{
			this.configNs = configNs;
			configRoot = configNs + "options";
		}

		/// <inheritdoc/>
        protected override OptionArgumentFlags OptionFound(string option)
        {
            var flags = base.OptionFound(option);
            if((flags & OptionArgumentFlags.HasArgument) == 0)
            {
                StoreOption(GetCanonicalOption(option), null, flags);
            }
            return flags;
        }

        /// <inheritdoc/>
        protected override void OptionArgumentFound(string option, string? argument, OptionArgumentFlags flags)
        {
            base.OptionArgumentFound(option, argument, flags);
            StoreOption(GetCanonicalOption(option), argument, flags);
        }

        /// <summary>
        /// Loadfs configuration from a file.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="configFile">The file instance.</param>
        /// <exception cref="ApplicationException">The configuration cannot be loaded.</exception>
        public void LoadConfigXml(string name, IFileInfo configFile)
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

		void LoadConfigElement(XElement element, string? prefix)
		{
			if(element.Name.Namespace != configNs)
			{
				// Ignore foreign elements
				return;
			}

			var optionName = DecodeName(element.Name.LocalName);
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
                var flags = OnOptionFound(optionName);
                if((element.IsEmpty && isNil != false) || (isNil == true))
                {
                    // Must be a switch or null optional argument
					if((flags & OptionArgumentFlags.HasArgument) != 0)
					{
						if((flags & OptionArgumentFlags.RequiredArgument) != 0)
                        {
                            throw ArgumentExpected(optionName);
                        }
                        OnOptionArgumentFound(optionName, null, flags);
                    }
				}else{
					// May be a switch or empty string
					if((flags & OptionArgumentFlags.HasArgument) != 0)
                    {
                        OnOptionArgumentFound(optionName, "", flags);
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
						var flags = OnOptionFound(optionName);
                        if((flags & OptionArgumentFlags.HasArgument) == 0)
                        {
                            throw ArgumentNotExpected(optionName);
                        }
                        OnOptionArgumentFound(optionName, childText.Value, flags);
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

		CapturedOptions? capturedOptions;

        class CapturedOptions
        {
            public XDocument Document { get; }
            public XElement Root { get; }
            public ConditionalWeakTable<XElement, object?> MustRemainElements { get; }

			public CapturedOptions(XName rootName)
            {
                Document = new XDocument
                (
                    Root = new XElement(rootName)
                );
                MustRemainElements = new();
            }
        }

        /// <summary>
        /// Call to start capturing provided options to save as XML.
        /// </summary>
        protected void CaptureOptions()
		{
            capturedOptions = new(configRoot);
        }

		static string EncodeName(string name)
		{
			var encoded = XmlConvert.EncodeLocalName(name);
			if(encoded.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
			{
				// These names are reserved in XML
				return $"_x{(int)encoded[0]:X4}_{encoded.Substring(1)}";
			}
			return encoded;
        }

		static string DecodeName(string name)
		{
			return XmlConvert.DecodeName(name);
		}

		void StoreOption(string option, string? argument, OptionArgumentFlags flags)
		{
			if(capturedOptions != null)
			{
				var path = option.Split(':');
				var elem = capturedOptions.Root;
				foreach(var component in path)
				{
					elem.Add(elem = new XElement(configNs + EncodeName(component)));
				}
				if((flags & OptionArgumentFlags.AllowMultiple) != 0)
				{
					capturedOptions.MustRemainElements.Add(elem, null);
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
                    if(capturedOptions!.MustRemainElements.TryGetValue(inner, out _))
                    {
                        // Don't change this to an attribute
                        continue;
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

		/// <summary>
		/// Serializase the current options into a stream.
		/// </summary>
		/// <param name="stream">The stream to write the XML to.</param>
		/// <exception cref="InvalidOperationException">
		/// There are no options to save to the stream.
		/// </exception>
		public void SaveConfigXml(Stream stream)
		{
			if(capturedOptions == null)
			{
				throw new InvalidOperationException("No options were stored.");
			}
			SimplifyOptions(capturedOptions.Root, true);
			var settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				CloseOutput = false,
				Indent = true,
				OmitXmlDeclaration = true
			};
            using var writer = XmlWriter.Create(stream, settings);
            capturedOptions.Document.Save(writer);
		}
    }
}
