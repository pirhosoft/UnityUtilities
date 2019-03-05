using PiRhoSoft.UtilityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PiRhoSoft.DocGenEditor
{
	[Serializable]
	public class TableOfContents
	{
		[Serializable] public class SectionList : SerializedList<string> { }

		[Serializable]
		public class TemplateSet
		{
			[TextArea(2, 8)] public string File = "{Categories}";
			[TextArea(2, 8)] public string Category = "{CategoryName}/{CategoryNiceName}/{CategoryId}\n{Sections}";
			[TextArea(2, 8)] public string Section = "\t{SectionName}/{SectionNiceName}/{SectionId}\n{Types}\n";
			[TextArea(2, 8)] public string Type = "\t\t{TypeName}/{TypeNiceName}/{TypeId}: {TypeFilename}\n";
		}

		public string OutputFile = "toc.txt";
		public string CodePath = "Scripts";

		[ListDisplay] public SectionList Sections = new SectionList();

		public TemplateSet Templates = new TemplateSet();

		private const string _categoriesTag = "{Categories}";
		private const string _sectionsTag = "{Sections}";
		private const string _typesTag = "{Types}";

		private class Category
		{
			public string Name;
			public string NiceName;
			public string Id;
			public DocumentationCategory Documentation;
			public List<Section> Sections;
		}

		private class Section
		{
			public string Name;
			public string NiceName;
			public string Id;
			public List<DocumentationType> Types;
		}

		public void Generate(string applicationPath, IEnumerable<DocumentationCategory> categories, string outputFolder)
		{
			if (!string.IsNullOrEmpty(CodePath))
			{
				var root = Path.Combine(applicationPath, CodePath);
				var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories);
				var file = new StringBuilder();

				var roots = categories.Where(category => category.IncludeInTableOfContents).Select(category => new Category
				{
					Name = category.Name,
					NiceName = DocumentationGenerator.GetNiceName(category.Name),
					Id = DocumentationGenerator.GetId(category.Name),
					Documentation = category,
					Sections = Sections
						.Select(sectionName => new Section
						{
							Name = sectionName,
							NiceName = DocumentationGenerator.GetNiceName(sectionName),
							Id = DocumentationGenerator.GetId(sectionName),
							Types = GetTypes(category, sectionName, files)
						})
						.ToList()
				});

				foreach (var category in roots)
				{
					var contents = GenerateCategory(category);
					file.Append(contents);
				}

				var tableOfContents = Templates.File.Replace(_categoriesTag, file.ToString());
				DocumentationGenerator.WriteFile(outputFolder, OutputFile, tableOfContents);
			}
		}

		private List<DocumentationType> GetTypes(DocumentationCategory category, string sectionName, IEnumerable<string> files)
		{
			var sectionTypes = new List<DocumentationType>();

			if (category != null)
			{
				var types = category.GetTypes();

				foreach (var file in files)
				{
					var section = GetSection(file);

					if (section == sectionName)
					{
						var contents = File.ReadAllText(file);

						foreach (var type in types)
						{
							if (IsTypeInFile(type.Type, contents))
								sectionTypes.Add(type);
						}
					}
				}
			}

			return sectionTypes;
		}

		private bool IsTypeInFile(Type type, string contents)
		{
			if (type.IsNested)
			{
				if (!IsTypeInFile(type.DeclaringType, contents))
					return false;
			}

			var expression = new Regex(@"(class|enum)\s" + type.Name + @"\s");
			return expression.IsMatch(contents);
		}

		private string GetSection(string file)
		{
			var info = new FileInfo(file);
			var directory = info.Directory;
			var name = directory.Name;

			while (directory != null && directory.Name != "Engine" && directory.Name != "Editor" && directory.Name != "Scripts" && directory.Name != "Assets")
			{
				name = directory.Name;
				directory = directory.Parent;
			}

			return name;
		}

		private string GenerateCategory(Category category)
		{
			var sections = new StringBuilder();

			foreach (var section in category.Sections.Where(s => s.Types.Count > 0))
			{
				var content = GenerateSection(category, section);
				sections.Append(content);
			}

			return Templates.Category
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(_sectionsTag, sections.ToString());
		}

		private string GenerateSection(Category category, Section section)
		{
			var builder = new StringBuilder();
			var types = section.Types.OrderBy(type => type.Name);

			foreach (var type in types)
			{
				var content = GenerateType(category, section, type);
				builder.Append(content);
			}

			return Templates.Section
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(DocumentationGenerator.SectionNameTag, section.Name)
				.Replace(DocumentationGenerator.SectionNiceNameTag, section.NiceName)
				.Replace(DocumentationGenerator.SectionIdTag, section.Id)
				.Replace(_typesTag, builder.ToString());
		}

		private string GenerateType(Category category, Section section, DocumentationType type)
		{
			return Templates.Type
				.Replace(DocumentationGenerator.CategoryNameTag, category.Name)
				.Replace(DocumentationGenerator.CategoryNiceNameTag, category.NiceName)
				.Replace(DocumentationGenerator.CategoryIdTag, category.Id)
				.Replace(DocumentationGenerator.SectionNameTag, section.Name)
				.Replace(DocumentationGenerator.SectionNiceNameTag, section.NiceName)
				.Replace(DocumentationGenerator.SectionIdTag, section.Id)
				.Replace(DocumentationGenerator.TypeNameTag, type.Name)
				.Replace(DocumentationGenerator.TypeNiceNameTag, type.NiceName)
				.Replace(DocumentationGenerator.TypeIdTag, type.Id)
				.Replace(DocumentationGenerator.TypeFilenameTag, type.Filename);
		}
	}
}
