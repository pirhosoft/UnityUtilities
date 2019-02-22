using System;
using System.Collections.Generic;
using System.IO;
using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[Flags]
	public enum DocumentationTypeCategory
	{
		Behaviour = 0x1,
		Asset = 0x2,
		Class = 0x4,
		Enum = 0x8,
		Abstract = 0x10,
		All = ~0
	}

	[Serializable] public class DocumentationNamespaceList : SerializedList<string> { }
	[Serializable] public class DocumentationSectionList : SerializedList<DocumentationSection> { }

	public class DocumentationGenerator : ScriptableObject
	{
		public string OutputDirectory = "Documentation/Generated";
		public List<DocumentationCategory> Categories = new List<DocumentationCategory>(); // making this a ListDisplay gets awkward, might want to do some custom drawing at some point, though
		public TableOfContents TableOfContents = new TableOfContents();
		public LogDescriptions LogDescriptions = new LogDescriptions();
		public HelpUrlValidator HelpUrls = new HelpUrlValidator();

		#region Tags

		public const string CategoryNameTag = "{CategoryName}";
		public const string CategoryNiceNameTag = "{CategoryNiceName}";
		public const string CategoryIdTag = "{CategoryId}";

		public const string TypeNameTag = "{TypeName}";
		public const string TypeRawNameTag = "{TypeRawName}";
		public const string TypeNiceNameTag = "{TypeNiceName}";
		public const string TypeIdTag = "{TypeId}";
		public const string TypeNamespaceTag = "{TypeNamespace}";
		public const string TypeFilenameTag = "{TypeFilename}";

		public const string SectionNameTag = "{SectionName}";
		public const string SectionNiceNameTag = "{SectionNiceName}";
		public const string SectionIdTag = "{SectionId}";

		#endregion

		#region Names

		private static Dictionary<string, string> _typeNameMap = new Dictionary<string, string>()
		{
			{ "Void", "void" },
			{ "Boolean", "bool" },
			{ "Int32", "int" },
			{ "Single", "float" },
			{ "String", "string" }
		};

		public static string GetCleanName(Type type)
		{
			var index = type.Name.IndexOf('`'); // generics have this tilde appended followed by the generic list
			var name = index < 0 ? type.Name : type.Name.Substring(0, index);

			name = name.TrimEnd('&'); // refs and outs have this appended

			return _typeNameMap.TryGetValue(name, out string mappedName) ? mappedName : name; // system types use the full name (Int32, Single, etc)
		}

		public static string GetNiceName(string name)
		{
			// for some reason NicifyVariableName can't be called from a background thread, so this is more or less a
			// re-implementation of that

			var nice = name[0].ToString();
			var space = true;

			for (var i = 1; i < name.Length; i++)
			{
				if (space && char.IsUpper(name[i]))
					nice += " ";

				nice += name[i];

				space = name[i] != ' ' && name[i] != '<';
			}

			return nice;
		}

		public static string GetTypeId(Type type)
		{
			var id = GetId(type.Name);

			if (type.IsNested)
				id = GetTypeId(type.DeclaringType) + "-" + id;

			return id;
		}

		public static string GetId(string name)
		{
			var id = char.ToLowerInvariant(name[0]).ToString();

			for (var i = 1; i < name.Length; i++)
			{
				if (name[i] == '[' || name[i] == ']')
					continue;

				if (char.IsUpper(name[i]))
					id += "-";

				if (name[i] == '`')
					id += '-';
				else
					id += char.ToLowerInvariant(name[i]);
			}

			return id;
		}

		#endregion

		#region Type Inclusion

		public static bool IsTypeIncluded(Type type, DocumentationTypeCategory includedTypes, IList<string> includedNamespaces)
		{
			var includeAbstract = includedTypes.HasFlag(DocumentationTypeCategory.Abstract);
			var includeClasses = includedTypes.HasFlag(DocumentationTypeCategory.Class);
			var includeEnums = includedTypes.HasFlag(DocumentationTypeCategory.Enum);
			var includeBehaviours = includedTypes.HasFlag(DocumentationTypeCategory.Behaviour);
			var includeAssets = includedTypes.HasFlag(DocumentationTypeCategory.Asset);

			var isBehavior = typeof(MonoBehaviour).IsAssignableFrom(type);
			var isAsset = typeof(ScriptableObject).IsAssignableFrom(type);

			if (!type.IsVisible || (type.IsAbstract && !includeAbstract) || (type.IsEnum && !includeEnums) || (!includeBehaviours && isBehavior) || (!includeAssets && isAsset))
				return false;

			if (!includeClasses && !isBehavior && !isAsset && !type.IsEnum)
				return false;

			return IsTypeIncluded(type, includedNamespaces);
		}

		public static bool IsTypeIncluded(Type type, IList<string> namespaces)
		{
			foreach (var ns in namespaces)
			{
				if (type.Namespace != null && type.Namespace.StartsWith(ns))
					return true;
			}

			return false;
		}

		#endregion

		#region File I/O

		public static string RootPath { get; private set; } = new DirectoryInfo(Application.dataPath).Parent.FullName;

		public static void Initialize()
		{
			// Call this from the main thread if DocumentationGenerator is potentially first used from a background
			// thread. This is necessary because Application.dataPath will throw an exception if called from a thread
		}

		public static bool WriteFile(string folder, string filename, string content)
		{
			var outputFile = new FileInfo(Path.Combine(RootPath, folder, filename));

			try
			{
				Directory.CreateDirectory(outputFile.Directory.FullName);
				File.WriteAllText(outputFile.FullName, content);
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}

		#endregion

		#region Default Settings

		public void SetDefaults()
		{
			Categories = new List<DocumentationCategory>
			{
				new DocumentationCategory
				{
					Name = "Manual",
					CategoryFilename = "{CategoryId}.adoc",
					TypeFilename = "{CategoryId}/{TypeId}.adoc",
					IncludeInTableOfContents = true,
					IncludedTypes = DocumentationTypeCategory.Behaviour | DocumentationTypeCategory.Asset | DocumentationTypeCategory.Abstract,
					IncludedNamespaces = new DocumentationNamespaceList(),
					ExternalNamespaces = new DocumentationCategory.ExternalNamespaceList
					{
						new DocumentationCategory.ExternalNamespace
						{
							Namespace = "System.Collections",
							LinkTemplate = _defaultSystemLink
						},
						new DocumentationCategory.ExternalNamespace
						{
							Namespace = "Unity",
							LinkTemplate = _defaultUnityLink
						}
					},
					Templates = new DocumentationCategory.TemplateSet
					{
						CategoryFile = _manualDefaultCategoryFile,
						Type = _manualDefaultType,
						TypeSeparator = _manualDefaultTypeSeparator,
						TypeFile = _manualDefaultTypeFile,
						BaseOpener = _manualDefaultBaseOpener,
						BaseSeparator = _manualDefaultBaseSeparator,
						SectionSeparator = _manualDefaultSectionSeparator,
						Generic = _manualDefaultGeneric,
						GenericOpener = _manualDefaultGenericOpener,
						GenericCloser = _manualDefaultGenericCloser,
						GenericSeparator = _manualDefaultGenericSeparator,
						Section = _manualDefaultSection,
						Constructor = _manualDefaultConstructor,
						Field = _manualDefaultField,
						Property = _manualDefaultProperty,
						Method = _manualDefaultMethod,
						MemberSeparator = _manualDefaultMemberSeparator,
						Decorator = _manualDefaultDecorator,
						DecoratorSeparator = _manualDefaultDecoratorSeparator,
						Parameter = _manualDefaultParameter,
						ParameterSeparator = _manualDefaultParameterSeparator,
						InternalLink = _defaultInternalLink,
						UnknownLink = _defaultUnknownLink
					},
					Sections = new DocumentationSectionList
					{
						new DocumentationSection
						{
							Name = "Fields",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Serializable,
							IncludedMemberTypes = DocumentationSection.MemberType.Field
						}
					}
				},
				new DocumentationCategory
				{
					Name = "Reference",
					CategoryFilename = "{CategoryId}.adoc",
					TypeFilename = "{CategoryId}/{TypeId}.adoc",
					IncludeInTableOfContents = true,
					IncludedTypes = DocumentationTypeCategory.All,
					IncludedNamespaces = new DocumentationNamespaceList(),
					ExternalNamespaces = new DocumentationCategory.ExternalNamespaceList
					{
						new DocumentationCategory.ExternalNamespace
						{
							Namespace = "System.Collections",
							LinkTemplate = _defaultSystemLink
						},
						new DocumentationCategory.ExternalNamespace
						{
							Namespace = "Unity",
							LinkTemplate = _defaultUnityLink
						}
					},
					Templates = new DocumentationCategory.TemplateSet
					{
						CategoryFile = _referenceDefaultCategoryFile,
						Type = _referenceDefaultType,
						TypeSeparator = _referenceDefaultTypeSeparator,
						TypeFile = _referenceDefaultTypeFile,
						BaseOpener = _referenceDefaultBaseOpener,
						BaseSeparator = _referenceDefaultBaseSeparator,
						SectionSeparator = _referenceDefaultSectionSeparator,
						Generic = _referenceDefaultGeneric,
						GenericOpener = _referenceDefaultGenericOpener,
						GenericCloser = _referenceDefaultGenericCloser,
						GenericSeparator = _referenceDefaultGenericSeparator,
						Section = _referenceDefaultSection,
						Constructor = _referenceDefaultConstructor,
						Field = _referenceDefaultField,
						Property = _referenceDefaultProperty,
						Method = _referenceDefaultMethod,
						MemberSeparator = _referenceDefaultMemberSeparator,
						Decorator = _referenceDefaultDecorator,
						DecoratorSeparator = _referenceDefaultDecoratorSeparator,
						Parameter = _referenceDefaultParameter,
						ParameterSeparator = _referenceDefaultParameterSeparator,
						InternalLink = _defaultInternalLink,
						UnknownLink = _defaultUnknownLink
					},
					Sections = new DocumentationSectionList
					{
						new DocumentationSection
						{
							Name = "Values",
							IncludedDeclarations = DocumentationSection.DeclarationType.Static,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.EnumValue
						},
						new DocumentationSection
						{
							Name = "Static Fields",
							IncludedDeclarations = DocumentationSection.DeclarationType.Static,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Field
						},
						new DocumentationSection
						{
							Name = "Static Properties",
							IncludedDeclarations = DocumentationSection.DeclarationType.Static,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Property
						},
						new DocumentationSection
						{
							Name = "Static Methods",
							IncludedDeclarations = DocumentationSection.DeclarationType.Static,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Method
						},
						new DocumentationSection
						{
							Name = "Constructors",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Constructor
						},
						new DocumentationSection
						{
							Name = "Public Fields",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Field
						},
						new DocumentationSection
						{
							Name = "Public Properties",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Property
						},
						new DocumentationSection
						{
							Name = "Public Methods",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Public,
							IncludedMemberTypes = DocumentationSection.MemberType.Method
						},
						new DocumentationSection
						{
							Name = "Protected Constructors",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Protected,
							IncludedMemberTypes = DocumentationSection.MemberType.Constructor
						},
						new DocumentationSection
						{
							Name = "Protected Fields",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Protected,
							IncludedMemberTypes = DocumentationSection.MemberType.Field
						},
						new DocumentationSection
						{
							Name = "Protected Properties",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Protected,
							IncludedMemberTypes = DocumentationSection.MemberType.Property
						},
						new DocumentationSection
						{
							Name = "Protected Methods",
							IncludedDeclarations = DocumentationSection.DeclarationType.Instance,
							IncludedAccessLevels = DocumentationSection.AccessLevel.Protected,
							IncludedMemberTypes = DocumentationSection.MemberType.Method
						}
					}
				}
			};

			LogDescriptions = new LogDescriptions
			{
				OutputFile = "log-descriptions.adoc",
				IncludedTypes = DocumentationTypeCategory.All,
				IncludedNamespaces = new DocumentationNamespaceList(),
				DocumentTemplate = _logDefaultDocumentTemplate,
				MessageTemplate = _logDefaultMessageTemplate
			};

			HelpUrls = new HelpUrlValidator
			{
				UrlRoot = "",
				IncludedNamespaces = new DocumentationNamespaceList(),
			};

			TableOfContents = new TableOfContents
			{
				OutputFile = "table-of-contents.html",
				CodePath = "Scripts",
				Sections = new TableOfContents.SectionList(),
				Templates = new TableOfContents.TemplateSet
				{
					File = _tocDefaultFileTemplate,
					Category = _tocDefaultCategoryTemplate,
					Section = _tocDefaultSectionTemplate,
					Type = _tocDefaultTypeTemplate
				}
			};
		}

		#region Default Link Templates

		private const string _defaultSystemLink = "https://docs.microsoft.com/en-us/dotnet/api/{TypeNamespace}.{TypeRawName}[{TypeName}^]";
		private const string _defaultUnityLink = "https://docs.unity3d.com/ScriptReference/{TypeName}.html[{TypeName}^]";
		private const string _defaultInternalLink = "<<{CategoryId}/{TypeId},{TypeNiceName}>>";
		private const string _defaultCrossLink = "link:{CategoryId}/{TypeId}.html[{TypeName}^]";
		private const string _defaultUnknownLink = "{TypeName}";

		#endregion

		#region Default Manual Templates

		private const string _manualDefaultCategoryFile = ":imagesdir: {CategoryId}/\n\n{Types}";
		private const string _manualDefaultType = "include::{TypeFilename}[]";
		private const string _manualDefaultTypeSeparator = "\n\n<<<\n\n";
		private const string _manualDefaultTypeFile = "[#{CategoryId}/{TypeId}]\n\n## {TypeNiceName}\n\n{Sections}\n\nifdef::backend-multipage_html5[]\nlink:reference/{TypeId}.html[Reference]\nendif::[]";
		private const string _manualDefaultBaseOpener = "";
		private const string _manualDefaultBaseSeparator = "";
		private const string _manualDefaultGeneric = "_{Name}_";
		private const string _manualDefaultGenericOpener = "";
		private const string _manualDefaultGenericCloser = "";
		private const string _manualDefaultGenericSeparator = "";
		private const string _manualDefaultSectionSeparator = "";
		private const string _manualDefaultSection = "### {SectionName}\n\n{Members}";
		private const string _manualDefaultConstructor = "";
		private const string _manualDefaultField = "{Type} _{NiceName}_::";
		private const string _manualDefaultProperty = "";
		private const string _manualDefaultMethod = "";
		private const string _manualDefaultMemberSeparator = "\n\n";
		private const string _manualDefaultDecorator = "";
		private const string _manualDefaultDecoratorSeparator = "";
		private const string _manualDefaultParameter = "";
		private const string _manualDefaultParameterSeparator = "";

		#endregion

		#region Default Reference Templates

		private const string _referenceDefaultCategoryFile = ":imagesdir: {CategoryId}/\n\n{Types}";
		private const string _referenceDefaultType = "include::{TypeFilename}[]";
		private const string _referenceDefaultTypeSeparator = "\n\n<<<\n\n";
		private const string _referenceDefaultTypeFile = "[#{CategoryId}/{TypeId}]\n\n## {TypeName}\n\n{TypeNamespace}.{TypeName}{Generics}{Bases}\n\n### Description\n\n{Sections}";
		private const string _referenceDefaultBaseOpener = " : ";
		private const string _referenceDefaultBaseSeparator = ", ";
		private const string _referenceDefaultGeneric = "_{Name}_";
		private const string _referenceDefaultGenericOpener = "<";
		private const string _referenceDefaultGenericCloser = ">";
		private const string _referenceDefaultGenericSeparator = ", ";
		private const string _referenceDefaultSectionSeparator = "\n\n";
		private const string _referenceDefaultSection = "### {SectionName}\n\n{Members}";
		private const string _referenceDefaultConstructor = "{Name}({Parameters})::";
		private const string _referenceDefaultField = "{Type} _{Name}_::";
		private const string _referenceDefaultProperty = "{Type} _{Name}_{Decorators}::";
		private const string _referenceDefaultMethod = "{Type} {Name}({Parameters}){Decorators}::";
		private const string _referenceDefaultMemberSeparator = "\n\n";
		private const string _referenceDefaultDecorator = " _({Name})_";
		private const string _referenceDefaultDecoratorSeparator = "";
		private const string _referenceDefaultParameter = "{Type} {Name}{Decorators}";
		private const string _referenceDefaultParameterSeparator = ", ";

		#endregion

		#region Default Log Templates

		private const string _logDefaultDocumentTemplate = @"[#manual/log-descriptions]
## Log Descriptions

### Warnings
{Warnings}
### Errors
{Errors}";

		private const string _logDefaultMessageTemplate = @"
{Id}:: {Message}
--
--
";

		#endregion

		#region Default Table of Contents Templates

		private const string _tocDefaultFileTemplate = "<ul>\n{Categories}</ul>";
		private const string _tocDefaultCategoryTemplate = "\t<li>\n\t\t<span class='menu-header'>{CategoryName}</span>\n\t\t<ul>\n{Sections}\t\t</ul>\n\t</li>\n";
		private const string _tocDefaultSectionTemplate = "\t\t\t<li>\n\t\t\t\t<a class='menu-section' data-section='{CategoryId}' href>{SectionName}</a>\n\t\t\t\t<ul class='submenu'>\n{Types}\t\t\t\t</ul>\n\t\t\t</li>\n";
		private const string _tocDefaultTypeTemplate = "\t\t\t\t\t<li><a class='menu-link' href='{CategoryId}/{TypeId}.html'>{TypeName}</a></li>\n";

		#endregion

		#endregion
	}
}
