using PiRhoSoft.UtilityEditor;
using PiRhoSoft.UtilityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiRhoSoft.DocGenEditor
{
	[Serializable]
	public class DocumentationSection
	{
		[Flags]
		public enum DeclarationType
		{
			Static = 0x1,
			Instance = 0x2,
			Inherited = 0x4
		}

		[Flags]
		public enum AccessLevel
		{
			Serializable = 0,
			Public = 0x1,
			Protected = 0x2,
			Private = 0x4,
			Internal = 0x8
		}

		[Flags]
		public enum MemberType
		{
			Constructor = 0x1,
			Field = 0x2,
			Property = 0x4,
			Method = 0x8,
			EnumValue = 0x10,
			All = ~0
		}

		public string Name = "";

		[EnumButtons(MinimumWidth = 80)] public DeclarationType IncludedDeclarations = DeclarationType.Instance;
		[EnumButtons(MinimumWidth = 80)] public AccessLevel IncludedAccessLevels = AccessLevel.Public | AccessLevel.Protected;
		[EnumButtons(MinimumWidth = 80)] public MemberType IncludedMemberTypes = MemberType.Field;

		private const string _membersTag = "{Members}";

		private const string _memberNameTag = "{Name}";
		private const string _memberNiceNameTag = "{NiceName}";
		private const string _memberTypeTag = "{Type}";
		private const string _memberDecoratorsTag = "{Decorators}";
		private const string _memberGenericsTag = "{Generics}";
		private const string _memberParametersTag = "{Parameters}";

		private const string _parameterDecoratorsTag = "{Decorators}";
		private const string _parameterTypeTag = "{Type}";
		private const string _parameterNameTag = "{Name}";
		private const string _decoratorNameTag = "{Name}";

		private const string _genericOpener = "<";
		private const string _genericCloser = ">";
		private const string _parameterOpener = "(";
		private const string _parameterCloser = ")";

		private const string _refDecorator = "ref";
		private const string _outDecorator = "out";
		private const string _virtualDecorator = "virtual";
		private const string _abstractDecorator = "abstract";
		private const string _readOnlyDecorator = "read only";

		private static List<string> _ignoredMethods = new List<string>
		{
			"OnAfterDeserialize",
			"OnBeforeSerialize"
		};

		public string Generate(DocumentationType type, DocumentationCategory category)
		{
			var niceName = DocumentationGenerator.GetNiceName(Name);
			var id = DocumentationGenerator.GetId(Name);

			var members = type.Type.IsEnum
				? GenerateValues(type, category)
				: GenerateMembers(type, category);

			return !string.IsNullOrEmpty(members)
				? category.Templates.Section
					.Replace(DocumentationGenerator.SectionNameTag, Name)
					.Replace(DocumentationGenerator.SectionNiceNameTag, niceName)
					.Replace(DocumentationGenerator.SectionIdTag, id)
					.Replace(DocumentationGenerator.TypeIdTag, type.Id)
					.Replace(DocumentationGenerator.TypeIdTag, type.Name)
					.Replace(DocumentationGenerator.TypeIdTag, type.NiceName)
					.Replace(_membersTag, members)
				: "";
		}

		private string GenerateValues(DocumentationType type, DocumentationCategory category)
		{
			var binding = GetBindingFlags();
			var builder = new StringBuilder();

			if (IncludedMemberTypes.HasFlag(MemberType.EnumValue))
			{
				var values = type.Type.GetFields(binding);

				if (values.Length > 0)
				{
					var content = GetMembers(values, GenerateField, category, category.Templates.MemberSeparator);
					builder.Append(content);
				}
			}

			return builder.ToString();
		}

		private string GenerateMembers(DocumentationType type, DocumentationCategory category)
		{
			var binding = GetBindingFlags();
			var builder = new StringBuilder();
			var first = true;

			if (IncludedMemberTypes.HasFlag(MemberType.Constructor))
			{
				var constructors = type.Type.GetConstructors(binding)
					.Where(constructor => IsIncluded(constructor, IncludedAccessLevels))
					.ToArray();

				if (constructors.Length > 0)
				{
					var content = GetMembers(constructors, GenerateConstructor, category, category.Templates.MemberSeparator);
					builder.Append(content);
					first = false;
				}
			}

			if (IncludedMemberTypes.HasFlag(MemberType.Field))
			{
				var fields = type.Type.GetFields(binding)
					.Where(field => IsIncluded(field, IncludedAccessLevels))
					.ToArray();

				if (fields.Length > 0)
				{
					var content = GetMembers(fields, GenerateField, category, category.Templates.MemberSeparator);

					if (!first) builder.Append(category.Templates.MemberSeparator);
					builder.Append(content);
					first = false;
				}
			}

			if (IncludedMemberTypes.HasFlag(MemberType.Property))
			{
				// setter access is determined later and specified via a read only decorator - set only is not supported

				var properties = type.Type.GetProperties(binding)
					.Where(property => IsIncluded(property.GetMethod, IncludedAccessLevels))
					.ToArray();

				if (properties.Length > 0)
				{
					var content = GetMembers(properties, GenerateProperty, category, category.Templates.MemberSeparator);

					if (!first) builder.Append(category.Templates.MemberSeparator);
					builder.Append(content);
					first = false;
				}
			}

			if (IncludedMemberTypes.HasFlag(MemberType.Method))
			{
				var methods = type.Type.GetMethods(binding)
					.Where(method => IsIncluded(method, IncludedAccessLevels))
					.Where(method => !_ignoredMethods.Contains(method.Name))
					.Where(method => !method.IsSpecialName)
					.ToArray();

				if (methods.Length > 0)
				{
					var content = GetMembers(methods, GenerateMethod, category, category.Templates.MemberSeparator);

					if (!first) builder.Append(category.Templates.MemberSeparator);
					builder.Append(content);
					first = false;
				}
			}

			return builder.ToString();
		}

		private bool IsIncluded(MethodBase method, AccessLevel access)
		{
			if (method == null) return false;
			if (method.IsPublic) return access.HasFlag(AccessLevel.Public);
			if (method.IsFamily) return access.HasFlag(AccessLevel.Protected);
			if (method.IsPrivate) return access.HasFlag(AccessLevel.Private);
			if (method.IsFamilyOrAssembly) return access.HasFlag(AccessLevel.Protected) || access.HasFlag(AccessLevel.Internal);
			if (method.IsFamilyAndAssembly) return access.HasFlag(AccessLevel.Protected) || (access.HasFlag(AccessLevel.Protected) && access.HasFlag(AccessLevel.Internal));

			return false;
		}

		private bool IsIncluded(FieldInfo field, AccessLevel access)
		{
			if (access == AccessLevel.Serializable)
			{
				return TypeHelper.IsSerializable(field);
			}
			else
			{
				if (field.IsPublic) return access.HasFlag(AccessLevel.Public);
				if (field.IsFamily) return access.HasFlag(AccessLevel.Protected);
				if (field.IsPrivate) return access.HasFlag(AccessLevel.Private);
				if (field.IsFamilyOrAssembly) return access.HasFlag(AccessLevel.Protected) || access.HasFlag(AccessLevel.Internal);
				if (field.IsFamilyAndAssembly) return access.HasFlag(AccessLevel.Protected) || (access.HasFlag(AccessLevel.Protected) && access.HasFlag(AccessLevel.Internal));
			}

			return false;
		}

		private BindingFlags GetBindingFlags()
		{
			// can't get more specific than public or not public so access level is filtered manually

			BindingFlags binding = BindingFlags.Public | BindingFlags.NonPublic;

			if (IncludedDeclarations.HasFlag(DeclarationType.Static)) binding |= BindingFlags.Static;
			if (IncludedDeclarations.HasFlag(DeclarationType.Instance)) binding |= BindingFlags.Instance;

			if (IncludedDeclarations.HasFlag(DeclarationType.Inherited)) binding |= BindingFlags.FlattenHierarchy;
			else binding |= BindingFlags.DeclaredOnly;

			return binding;
		}

		private string GetMembers<T>(T[] members, Func<T, DocumentationCategory, string> generator, DocumentationCategory category, string separator)
		{
			var builder = new StringBuilder();

			if (members.Length > 0)
			{
				for (var i = 0; i < members.Length; i++)
				{
					if (i != 0)
						builder.Append(separator);

					var member = generator(members[i], category);
					builder.Append(member);
				}
			}

			return builder.ToString();
		}

		private string GenerateConstructor(ConstructorInfo constructor, DocumentationCategory category)
		{
			var name = DocumentationGenerator.GetCleanName(constructor.DeclaringType);
			var parameters = GenerateParameters(constructor.GetParameters(), category);

			return category.Templates.Constructor
				.Replace(_memberNameTag, name)
				.Replace(_memberParametersTag, parameters);
		}

		private string GenerateField(FieldInfo field, DocumentationCategory category)
		{
			var type = category.GetLink(field.FieldType);
			var niceName = DocumentationGenerator.GetNiceName(field.Name);

			return category.Templates.Field
				.Replace(_memberTypeTag, type)
				.Replace(_memberNameTag, field.Name)
				.Replace(_memberNiceNameTag, niceName);
		}

		private string GenerateProperty(PropertyInfo property, DocumentationCategory category)
		{
			var method = GetAbstractDecorator(property.GetMethod);
			var readOnly = !IsIncluded(property.SetMethod, IncludedAccessLevels) ? _readOnlyDecorator : "";

			var decorators = GenerateDecorators(category, readOnly, method);
			var type = category.GetLink(property.PropertyType);

			return category.Templates.Property
				.Replace(_memberDecoratorsTag, decorators)
				.Replace(_memberTypeTag, type)
				.Replace(_memberNameTag, property.Name);
		}

		private string GenerateMethod(MethodInfo method, DocumentationCategory category)
		{
			var decorator = GetAbstractDecorator(method);

			var decorators = GenerateDecorators(category, decorator);
			var type = category.GetLink(method.ReturnType);
			var generics = GenerateGenerics(method.GetGenericArguments(), category);
			var parameters = GenerateParameters(method.GetParameters(), category);

			return category.Templates.Method
				.Replace(_memberDecoratorsTag, decorators)
				.Replace(_memberTypeTag, type)
				.Replace(_memberNameTag, method.Name + generics)
				.Replace(_memberParametersTag, parameters);
		}

		private string GetAbstractDecorator(MethodInfo method)
		{
			return method.IsAbstract
				? _abstractDecorator
				: (method.IsVirtual ? _virtualDecorator : "");
		}

		private string GenerateDecorators(DocumentationCategory category, params string[] decorators)
		{
			var filtered = decorators.Where(decorator => !string.IsNullOrEmpty(decorator)).ToArray();
			return GetMembers(filtered, GenerateDecorator, category, category.Templates.DecoratorSeparator);
		}

		private string GenerateDecorator(string decorator, DocumentationCategory category)
		{
			return category.Templates.Decorator
				.Replace(_decoratorNameTag, decorator);
		}

		private string GenerateParameters(ParameterInfo[] parameters, DocumentationCategory category)
		{
			return GetMembers(parameters, GenerateParameter, category, category.Templates.ParameterSeparator);
		}

		private string GenerateParameter(ParameterInfo parameter, DocumentationCategory category)
		{
			var decorator = parameter.ParameterType.IsByRef ? (parameter.IsOut ? _outDecorator : _refDecorator) : "";

			var decorators = GenerateDecorators(category, decorator);
			var type = category.GetLink(parameter.ParameterType);

			return category.Templates.Parameter
				.Replace(_parameterDecoratorsTag, decorators)
				.Replace(_parameterNameTag, parameter.Name)
				.Replace(_parameterTypeTag, type);
		}

		private string GenerateGenerics(Type[] generics, DocumentationCategory category)
		{
			var members = GetMembers(generics, GenerateGenericParameter, category, category.Templates.GenericSeparator);

			return !string.IsNullOrEmpty(members)
				? string.Format("{0}{1}{2}", _genericOpener, members, _genericCloser)
				: "";
		}

		private string GenerateGenericParameter(Type type, DocumentationCategory category)
		{
			return category.GetLink(type);
		}
	}
}
