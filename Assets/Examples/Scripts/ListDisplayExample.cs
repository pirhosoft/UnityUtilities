using PiRhoSoft.UtilityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[Serializable]
	public class StringList : SerializedList<string> { }

	[Serializable]
	public class StringArray : SerializedArray<string> { public StringArray(int count) : base(count) { } }

	[Serializable]
	public class AssetList : SerializedList<ExampleAsset> { }
	
	[AddComponentMenu("PiRho Soft/Examples/List Display")]
	public class ListDisplayExample : MonoBehaviour
	{
		[Serializable]
		public class Class
		{
			public int IntField;
			public string StringField;
		}
		
		[Serializable]
		public class ClassList : SerializedList<Class> { }

		public List<string> Normal = new List<string>();

		[ListDisplay]
		public StringList DefaultStringList = new StringList();

		[ListDisplay]
		public AssetList DefaultAssetList = new AssetList();

		[ListDisplay]
		[AssetDisplay]
		public AssetList AssetList = new AssetList();

		[ListDisplay]
		[ClassDisplay]
		public ClassList IndentedClassList = new ClassList();

		[ListDisplay]
		[ClassDisplay(Type = ClassDisplayType.Inline)]
		public ClassList InlineClassList = new ClassList();

		[ListDisplay]
		[ClassDisplay(Type = ClassDisplayType.Foldout)]
		public ClassList FoldoutClassList = new ClassList();
	}
}
