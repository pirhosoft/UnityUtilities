using PiRhoSoft.UtilityEngine;
using System;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[Serializable]
	public class TestClass
	{
		[Tooltip("The int field")] public int IntField;
		[Tooltip("The string field")] public string StringField;
	};

	[AddComponentMenu("PiRho Soft/Examples/Class Display")]
	public class ClassDisplayExample : MonoBehaviour
	{
		[Tooltip("The indented class")] [ClassDisplay] public TestClass IndentedClass;
		[Header("Inline Class")] [ClassDisplay(Type = ClassDisplayType.Inline)] public TestClass InlineClass;
		[Tooltip("The propogated class")] [ClassDisplay(Type = ClassDisplayType.Propogated)] public TestClass PropogatedClass;
		[Tooltip("The foldout class")] [ClassDisplay(Type = ClassDisplayType.Foldout)] public TestClass FoldoutClass;
		[Tooltip("The default class")] public TestClass DefaultClass;
	}
}
