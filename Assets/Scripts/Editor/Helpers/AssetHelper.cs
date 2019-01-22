using System;
using System.Collections.Generic;
using System.Linq;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class AssetList
	{
		public Type Type;
		public bool HasNone;
		public bool HasCreate;
		public int PrependedSlots;
		public int AppendedSlots;

		public GUIContent[] Names;
		public List<ScriptableObject> Assets;
		public List<Type> Types;

		public int GetIndex(ScriptableObject asset)
		{
			var index = Assets.IndexOf(asset) + PrependedSlots;

			if (HasNone)
			{
				if (index >= 0)
					index += 2;
				else
					index = 0;
			}

			return index;
		}

		public int GetIndex(Type type)
		{
			if (Types != null)
			{
				var index = Types.IndexOf(type);

				if (index >= 0)
				{
					if (HasNone)
						index += 2;

					index += Assets.Count + 1 + PrependedSlots;
				}

				return index;
			}
			else
			{
				return -1;
			}
		}

		public ScriptableObject GetAsset(int index)
		{
			if (HasNone)
				index -= 2;

			index -= PrependedSlots;

			return index >= 0 && index < Assets.Count ? Assets[index] : null;
		}

		public Type GetType(int index)
		{
			if (Types != null)
			{
				if (HasNone)
					index -= 2;

				index -= Assets.Count + 1 + PrependedSlots;

				return index >= 0 && index < Types.Count ? Types[index] : null;
			}
			else
			{
				return null;
			}
		}
	}

	public class AssetHelper : AssetPostprocessor
	{
		private static Dictionary<string, AssetList> _assetLists = new Dictionary<string, AssetList>();

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (var list in _assetLists)
			{
				list.Value.Names = null;
				list.Value.Assets = null;
				list.Value.Types = null;
			}
		}

		public static AssetType GetOrCreateAsset<AssetType>(string name) where AssetType : ScriptableObject
		{
			return GetAsset<AssetType>() ?? CreateAsset<AssetType>(name);
		}

		public static AssetType CreateAsset<AssetType>(string name) where AssetType : ScriptableObject
		{
			return CreateAsset<AssetType>(name, typeof(AssetType));
		}

		public static AssetType CreateAsset<AssetType>(string name, Type type) where AssetType : ScriptableObject
		{
			var asset = ScriptableObject.CreateInstance(type) as AssetType;
			var path = "Assets/" + name + ".asset";

			asset.name = name;

			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			return asset;
		}

		public static AssetType CreateChildAssetWithUndo<AssetType>(ScriptableObject parent, IList<AssetType> assets, string name) where AssetType : ScriptableObject
		{
			return CreateChildAssetWithUndo(parent, assets, typeof(AssetType), name);
		}

		public static AssetType CreateChildAssetWithUndo<AssetType>(ScriptableObject parent, IList<AssetType> assets, Type assetType, string name) where AssetType : ScriptableObject
		{
			var asset = ScriptableObject.CreateInstance(assetType) as AssetType;
			asset.name = name;
			asset.hideFlags = HideFlags.HideInHierarchy;

			using (new UndoScope(parent, true))
			{
				assets.Add(asset);

				Undo.RegisterCreatedObjectUndo(asset, "Undo create");
				AssetDatabase.AddObjectToAsset(asset, parent);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parent));
			}

			return asset;
		}

		public static void DestroyChildAssetWithUndo<AssetType>(ScriptableObject parent, List<AssetType> assets, int index) where AssetType : ScriptableObject
		{
			var asset = assets[index];
			assets.RemoveAt(index);
			Undo.DestroyObjectImmediate(asset);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parent));
		}

		public static void RenameChildAssetWithUndo(ScriptableObject parent, ScriptableObject asset, string newName)
		{
			using (new UndoScope(asset, true))
				asset.name = newName;
		}

		public static string GetPath(ScriptableObject asset)
		{
			var path = AssetDatabase.GetAssetPath(asset);
			var slash = path.LastIndexOf('/');

			return path.Substring(0, slash + 1);
		}

		public static string FindCommonPath<AssetType>(List<AssetType> assets) where AssetType : ScriptableObject
		{
			return StringHelper.FindCommonPath(assets.Select(asset => GetPath(asset)));
		}

		public static AssetList GetAssetList<AssetType>(bool includeNone, bool includeCreate, int prepended = 0, int appended = 0) where AssetType : ScriptableObject
		{
			return GetAssetList(typeof(AssetType), includeNone, includeCreate, prepended, appended);
		}

		public static AssetList GetAssetList(Type assetType, bool includeNone, bool includeCreate, int prepended = 0, int appended = 0)
		{
			var listName = string.Format("{0}-{1}-{2}-{3}-{4}", includeNone, includeCreate, prepended, appended, assetType.AssemblyQualifiedName);

			AssetList list;
			if (!_assetLists.TryGetValue(listName, out list))
			{
				list = new AssetList { Type = assetType, HasNone = includeNone, HasCreate = includeCreate, PrependedSlots = prepended, AppendedSlots = appended };
				_assetLists.Add(listName, list);
			}

			if (list.Assets == null)
			{
				list.Assets = ListAssets(assetType);
				list.Types = includeCreate ? TypeHelper.ListDerivedTypes(assetType) : null;

				var index = prepended;
				var count = prepended + appended;
				var prefix = FindCommonPath(list.Assets);

				if (includeNone)
					count += 2;

				count += list.Assets.Count;

				if (includeCreate)
					count += list.Types.Count + 1;

				list.Names = new GUIContent[count];

				if (includeNone)
				{
					list.Names[index++] = new GUIContent("None");
					list.Names[index++] = new GUIContent("");
				}

				for (var i = 0; i < list.Assets.Count; i++)
				{
					var path = GetPath(list.Assets[i]).Substring(prefix.Length);
					var name = path.Length > 0 ? path + list.Assets[i].name : list.Assets[i].name;
					list.Names[index++] = new GUIContent(name);
				}

				if (includeCreate)
				{
					list.Names[index++] = new GUIContent("");
					list.Types = list.Types.OrderBy(type => type.Name).ToList();

					foreach (var type in list.Types)
					{
						var name = "Create/" + ObjectNames.NicifyVariableName(type.Name);
						list.Names[index++] = new GUIContent(name);
					}
				}
			}

			return list;
		}

		public static AssetType GetAsset<AssetType>() where AssetType : ScriptableObject
		{
			return FindAssets<AssetType>().FirstOrDefault();
		}

		public static List<AssetType> ListAssets<AssetType>() where AssetType : ScriptableObject
		{
			return FindAssets<AssetType>().ToList();
		}

		public static IEnumerable<AssetType> FindAssets<AssetType>() where AssetType : ScriptableObject
		{
			return FindAssets(typeof(AssetType)).Select(asset => asset as AssetType);
		}

		public static List<ScriptableObject> ListAssets(Type assetType)
		{
			return FindAssets(assetType).ToList();
		}

		public static IEnumerable<ScriptableObject> FindAssets(Type assetType)
		{
			var query = string.Format("t:{0}", assetType);
			return AssetDatabase.FindAssets(query).Select(id => GetAssetWithId(id, assetType));
		}

		public static AssetType GetAssetWithId<AssetType>(string id) where AssetType : ScriptableObject
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath<AssetType>(path);
		}

		public static AssetType GetAssetAtPath<AssetType>(string path) where AssetType : ScriptableObject
		{
			return AssetDatabase.LoadAssetAtPath<AssetType>(path);
		}

		public static ScriptableObject GetAssetWithId(string id, Type type)
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath(path, type);
		}

		public static ScriptableObject GetAssetAtPath(string path, Type type)
		{
			return AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;
		}
	}
}
