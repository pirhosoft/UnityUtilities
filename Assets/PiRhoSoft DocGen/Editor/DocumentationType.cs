using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiRhoSoft.DocGenEditor
{
	public class GenericClass<T>
	{
		public void Method(T parameter)
		{
		}
	}

	public class DocumentationType
	{
		public Type Type;
		public string Name;
		public string RawName;
		public string NiceName;
		public string Id;
		public string Filename;
		public string Link;

		private const string _namespaceTag = "{Namespace}";
		private const string _genericsTag = "{Generics}";
		private const string _basesTag = "{Bases}";
		private const string _sectionsTag = "{Sections}";

		private static List<Type> _ignoredBases = new List<Type>
		{
			typeof(object),
			typeof(Enum)
		};

		private static List<Type> _ignoredInterfaces = new List<Type>
		{
			typeof(ISerializationCallbackReceiver)
		};

		public DocumentationType(Type type, DocumentationCategory category)
		{
			Type = type;
			Name = DocumentationGenerator.GetCleanName(type);
			RawName = type.Name;
			NiceName = DocumentationGenerator.GetNiceName(Name);
			Id = DocumentationGenerator.GetTypeId(type);
			Filename = GetFilename(category);
			Link = category.GetLink(type);
		}

		public string GenerateIndex(DocumentationCategory category)
		{
			return category.Templates.Type
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(DocumentationGenerator.TypeNameTag, Name)
				.Replace(DocumentationGenerator.TypeRawNameTag, RawName)
				.Replace(DocumentationGenerator.TypeNiceNameTag, NiceName)
				.Replace(DocumentationGenerator.TypeIdTag, Id)
				.Replace(DocumentationGenerator.TypeFilenameTag, Filename);
		}

		public string GenerateFile(DocumentationCategory category)
		{
			var generics = GenerateGenerics(category);
			var bases = GenerateBases(category);
			var sections = GenerateSections(category);

			return category.Templates.TypeFile
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(DocumentationGenerator.TypeIdTag, Id)
				.Replace(DocumentationGenerator.TypeNameTag, Name)
				.Replace(DocumentationGenerator.TypeRawNameTag, RawName)
				.Replace(DocumentationGenerator.TypeNiceNameTag, NiceName)
				.Replace(DocumentationGenerator.TypeNamespaceTag, Type.Namespace)
				.Replace(DocumentationGenerator.TypeFilenameTag, Filename)
				.Replace(_genericsTag, generics)
				.Replace(_basesTag, bases)
				.Replace(_sectionsTag, sections);
		}

		private string GenerateGenerics(DocumentationCategory category)
		{
			return Type.IsGenericType
				? category.GetGenerics(Type)
				: "";
		}

		private string GenerateBases(DocumentationCategory category)
		{
			var builder = new StringBuilder();
			var hasParent = Type.BaseType != null && !_ignoredBases.Contains(Type.BaseType);
			var interfaces = Type.GetInterfaces().Where(t => !_ignoredInterfaces.Contains(t) && (Type.BaseType == null || !Type.BaseType.GetInterfaces().Contains(t)));

			if (hasParent || interfaces.Any())
				builder.Append(category.Templates.BaseOpener);

			if (hasParent)
				builder.Append(category.GetLink(Type.BaseType));

			var first = !hasParent;

			if (interfaces.Any())
			{
				foreach (var i in interfaces)
				{
					if (!first)
						builder.Append(category.Templates.BaseSeparator);

					builder.Append(category.GetLink(i));
					first = false;
				}
			}

			return builder.ToString();
		}

		private string GenerateSections(DocumentationCategory category)
		{
			var builder = new StringBuilder();
			var first = true;

			foreach (var section in category.Sections)
			{
				var contents = section.Generate(this, category);

				if (!string.IsNullOrEmpty(contents))
				{
					if (!first)
						builder.Append(category.Templates.SectionSeparator);

					builder.Append(contents);
					first = false;
				}
			}

			return builder.ToString();
		}

		private string GetFilename(DocumentationCategory category)
		{
			return category.TypeFilename
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(DocumentationGenerator.TypeNameTag, Name)
				.Replace(DocumentationGenerator.TypeNiceNameTag, NiceName)
				.Replace(DocumentationGenerator.TypeIdTag, Id);
		}
	}
}
