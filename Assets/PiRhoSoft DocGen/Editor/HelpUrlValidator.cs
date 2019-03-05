using PiRhoSoft.UtilityEditor;
using PiRhoSoft.UtilityEngine;
using System;
using UnityEngine;

namespace PiRhoSoft.DocGenEditor
{
	[Serializable]
	public class HelpUrlValidator
	{
		private const string _missingHelpUrlWarning = "(UHUVMHU) {0} does not have a HelpURL attribute";
		private const string _invalidHelpUrlWarning = "(UHUVIHU) {0}'s HelpURL attribute is {1} and should be {2}";

		public string UrlRoot; // TODO: expose this as a regex or tag format or something
		[ListDisplay] public DocumentationNamespaceList IncludedNamespaces = new DocumentationNamespaceList();
		
		public void Validate()
		{
			var types = TypeHelper.FindTypes(type => DocumentationGenerator.IsTypeIncluded(type, DocumentationTypeCategory.Asset | DocumentationTypeCategory.Behaviour, IncludedNamespaces));

			foreach (var type in types)
			{
				var id = DocumentationGenerator.GetTypeId(type);
				var url = UrlRoot + id;
				var attribute = TypeHelper.GetAttribute<HelpURLAttribute>(type);

				if (attribute == null)
					Debug.LogWarningFormat(_missingHelpUrlWarning, type.Name);
				else if (attribute.URL != url)
					Debug.LogWarningFormat(_invalidHelpUrlWarning, type.Name, attribute.URL, url);
			}
		}
	}
}
