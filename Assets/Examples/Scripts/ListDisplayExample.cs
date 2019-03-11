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
		public class Data
		{
			public int Intfield;
			public string StringField;
		}
		
		[Serializable]
		public class DataList : SerializedList<Data> { }

		public List<string> Normal = new List<string>();

		[ListDisplay]
		public StringList DefaultStringList = new StringList();

		[ListDisplay]
		public AssetList DefaultAssetList = new AssetList();

		[ListDisplay]
		[AssetDisplay]
		public AssetList AssetList = new AssetList();
	}
}
