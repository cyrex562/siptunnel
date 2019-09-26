using System;
using System.Globalization;
using COL = System.Collections.Generic;
using TXT = System.Text;

namespace SipTunnel
{
	/// <summary>
	/// Used to parse command line
	/// </summary>
	internal class CommandLineParser
		: COL.ICollection<CommandLineParser.Parameter>
#if PLAT_WINDOWS || PLAT_MONO
		,ICommandLineParser
#endif
	{
		public struct Parameter
#if PLAT_WINDOWS || PLAT_MONO
			: IParameter
#endif
		{
			private string m_Name;
			private COL.List<string> m_Values;
			private byte m_Count;

			public Parameter(string name)
			{
				m_Name = name;
				m_Count = 0;
				m_Values = null;
			}

			public void AddValue(string value)
			{
				if (null == m_Values)
					m_Values = new COL.List<string>();

				m_Values.Add(value);
			}

			public static Parameter operator++(Parameter p)
			{
				p.m_Count++;
				return p;
			}

			public string Name
			{
				get
				{
					return m_Name;
				}
			}

			public COL.IList<string> Values
			{
				get
				{
					if (null == m_Values)
						return null;

					return m_Values.AsReadOnly();
				}
			}

			public byte Count
			{
				get
				{
					return m_Count;
				}
			}
		}

		private COL.Dictionary<string, Parameter> m_Params = new COL.Dictionary<string, Parameter>(16);

		public CommandLineParser(string[] args)
		{
			if (null == args)
				throw new ArgumentNullException("args");

			TXT.StringBuilder sb = new TXT.StringBuilder(32);
			foreach (string s in args)
				sb.Append(s + ' ');

			ParseCmdLine(sb.ToString());
		}

		public CommandLineParser(string cmdLine)
		{
			if (null == cmdLine)
				throw new ArgumentNullException("cmdLine");

			ParseCmdLine(cmdLine);
		}

		private void ParseCmdLine(string cmdLine)
		{
			for (int i = 0; i < cmdLine.Length; i++)
			{
				if (cmdLine[i] == '-')
					i += ParseParameter(cmdLine, i);
				else if (cmdLine[i] != ' ')
					throw new CommandLineParserException("Unrecognized token '" + cmdLine[i] + "'.");
			}
		}

		private int ParseParameter(string cmdLine, int index)
		{
			int startIndex = index;
			while (cmdLine[index] == '-' && index < cmdLine.Length)
				index++;

			if (cmdLine.Length == index || cmdLine[index] == ' ')
				throw new CommandLineParserException("Parameter without name.");
			if (!char.IsLetter(cmdLine[index]))
				throw new CommandLineParserException("Parameter name must start with letter.");

			int nameStartIndex = index;
			while (index < cmdLine.Length && (char.IsLetterOrDigit(cmdLine[index]) || cmdLine[index] == '-'))
				index++;

			Parameter newParam = CreateNewParameter(cmdLine.Substring(nameStartIndex, index - nameStartIndex));
			if (cmdLine.Length <= index || cmdLine[index] == ' ')
				return (index - startIndex);

			if (cmdLine[index] != ':' && cmdLine[index] != '=')
				throw new CommandLineParserException("Parameter name contains invalid character '" + cmdLine[index] + "'.");

			index++;
			if (cmdLine[index] == ' ')
			{
				newParam.AddValue(string.Empty);
				m_Params[newParam.Name] = newParam;
				return (index - startIndex) + 1;
			}

			int valStartIndex = index;
			char quote = cmdLine[index];
			if ('\'' == quote || '"' == quote)
				valStartIndex = ++index;

			while (index < cmdLine.Length)
			{
				if ('\'' == quote || '"' == quote)
				{
					if (cmdLine[index] == quote)
						break;
				}
				else if (' ' == cmdLine[index])
					break;

				index++;
			}
			if (cmdLine.Length == index && ('\'' == quote || '"' == quote))
				throw new CommandLineParserException("Missing quote.");

			newParam.AddValue(cmdLine.Substring(valStartIndex, index - valStartIndex));
			m_Params[newParam.Name] = newParam;
			return (index - startIndex);
		}

		private Parameter CreateNewParameter(string name)
		{
			Parameter retVal;
			name = name.ToLower(CultureInfo.CurrentUICulture);

			if (m_Params.TryGetValue(name, out retVal))
			{
				retVal++;
				return retVal;
			}

			retVal = new Parameter(name);
			m_Params.Add(name, retVal);
			return retVal;
		}

		public Parameter this[string name]
		{
			get
			{
				if (null == name)
					throw new ArgumentNullException("name");

				return m_Params[name.ToLower(CultureInfo.CurrentUICulture)];
			}
		}

#if PLAT_WINDOWS || PLAT_MONO
		IParameter ICommandLineParser.this[string name]
		{
			get
			{
				return this[name];
			}
		}
#endif

		public bool ContainsParameter(string name)
		{
			if (null == name)
				throw new ArgumentNullException("name");

			return m_Params.ContainsKey(name.ToLower(CultureInfo.CurrentUICulture));
		}

		#region ICollection<Parameter> Members

		public void Add(CommandLineParser.Parameter item)
		{
			throw new InvalidOperationException();
		}

		public void Clear()
		{
			throw new InvalidOperationException();
		}

		public bool Contains(CommandLineParser.Parameter item)
		{
			throw new InvalidOperationException();
		}

		public void CopyTo(CommandLineParser.Parameter[] array, int arrayIndex)
		{
			m_Params.Values.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get
			{
				return m_Params.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public bool Remove(CommandLineParser.Parameter item)
		{
			throw new InvalidOperationException();
		}

		#endregion

		#region IEnumerable<Parameter> Members

		public System.Collections.Generic.IEnumerator<CommandLineParser.Parameter> GetEnumerator()
		{
			return m_Params.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return m_Params.Values.GetEnumerator();
		}

		#endregion
	}
}
