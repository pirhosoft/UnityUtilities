using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[Serializable]
	public class LogDescriptions
	{
		public string OutputFile = "log.txt";

		[EnumButtons] public DocumentationTypeCategory IncludedTypes = DocumentationTypeCategory.All;
		[ListDisplay] public DocumentationNamespaceList IncludedNamespaces = new DocumentationNamespaceList();

		[TextArea(2, 8)] public string DocumentTemplate = "{Warnings}\n\n{Errors}";
		[TextArea(2, 8)] public string MessageTemplate = "{Id} {Message}";

		private const string _missingIdWarning = "(ULDMI) {0} '{1}' on type '{2}' does not have an id";
		private const string _invalidIdWarning = "(ULDII) {0} '{1}' on type '{2}' has duplicate id '{3}'";

		// TODO: do these and id as regex and expose them as properties
		private const string _warningFieldSuffix = "Warning";
		private const string _errorFieldSuffix = "Error";

		private const string _logWarningsTag = "{Warnings}";
		private const string _logErrorsTag = "{Errors}";
		private const string _logIdTag = "{Id}";
		private const string _logMessageTag = "{Message}";

		#region Log Descriptions

		public void Generate(string outputFolder)
		{
			var types = TypeHelper.FindTypes(type => !type.IsEnum && DocumentationGenerator.IsTypeIncluded(type, IncludedTypes, IncludedNamespaces));

			var warnings = GenerateLogDescriptions(types, _warningFieldSuffix, MessageTemplate);
			var errors = GenerateLogDescriptions(types, _errorFieldSuffix, MessageTemplate);

			var content = DocumentTemplate
				.Replace(_logWarningsTag, warnings)
				.Replace(_logErrorsTag, errors);

			DocumentationGenerator.WriteFile(outputFolder, OutputFile, content);
		}

		private class MessageDescription
		{
			public string Id;
			public string Name;
			public string Message;
		}

		private static string GenerateLogDescriptions(IEnumerable<Type> types, string section, string messageTemplate)
		{
			var descriptions = new List<MessageDescription>();

			foreach (var type in types)
			{
				var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

				foreach (var field in fields)
				{
					if (field.Name.EndsWith(section))
					{
						var message = field.GetValue(null) as string;
						var id = ParseId(ref message);

						if (string.IsNullOrEmpty(id))
							Debug.LogWarningFormat(_missingIdWarning, section, field.Name, type.Name);
						else if (descriptions.Any(description => description.Id == id))
							Debug.LogWarningFormat(_invalidIdWarning, section, field.Name, type.Name, id);
						else
							descriptions.Add(new MessageDescription { Id = id, Name = field.Name, Message = message });
					}
				}
			}

			return WriteLogDescription(descriptions, messageTemplate);
		}

		private static string WriteLogDescription(List<MessageDescription> descriptions, string messageTemplate)
		{
			var builder = new StringBuilder();
			var ordered = descriptions.OrderBy(description => description.Id);

			foreach (var description in ordered)
			{
				var message = messageTemplate
					.Replace(_logIdTag, description.Id)
					.Replace(_logMessageTag, description.Message);

				builder.Append(message);
			}

			return builder.ToString();
		}

		private static string ParseId(ref string message)
		{
			var open = message.IndexOf('(');
			var close = message.IndexOf(')');
			var id = "";

			if (open == 0 && close > 1)
			{
				id = message.Substring(open + 1, close - 1);
				message = message.Substring(close + 2);
			}

			return id;
		}

		#endregion
	}
}
