using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEditor
{
	public class AssetList
	{
		public Type Type;
		public bool HasNone;
		public bool HasCreate;

		public GUIContent[] Names;
		public List<Object> Assets;
		public TypeList Types;

		#region Lookup

		public int GetIndex(Object asset)
		{
			var index = Assets.IndexOf(asset);

			if (HasNone)
			{
				if (index >= 0) index += 2; // skip 'None' and separator
				else index = 0;
			}

			return index;
		}

		public Object GetAsset(int index)
		{
			if (HasNone) index -= 2;  // skip 'None' and separator
			return index >= 0 && index < Assets.Count ? Assets[index] : null;
		}

		public Type GetType(int index)
		{
			if (Types != null)
			{
				if (HasNone) index -= 2; // skip 'None' and separator
				index -= Assets.Count + 1; // skip assets and separator

				return Types.GetType(index);
			}
			else
			{
				return null;
			}
		}

		#endregion
	}

	public class AssetHelper : AssetPostprocessor
	{
		private static Dictionary<string, AssetList> _assetLists = new Dictionary<string, AssetList>();

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			_assetLists.Clear(); // no reason to figure out what actually changed - just clear all the lists so they are rebuilt on next access
		}

		#region Creation

		public static AssetType CreateAsset<AssetType>(string name) where AssetType : ScriptableObject
		{
			return CreateAsset(name, typeof(AssetType)) as AssetType;
		}

		public static AssetType GetOrCreateAsset<AssetType>(string name) where AssetType : ScriptableObject
		{
			var asset = GetAsset<AssetType>();

			if (asset == null)
				asset = CreateAsset<AssetType>(name);

			return asset;
		}

		public static ScriptableObject CreateAsset(string name, Type type)
		{
			var asset = ScriptableObject.CreateInstance(type);
			var path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + name + ".asset");

			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			return asset;
		}

		public static ScriptableObject GetOrCreateAsset(string name, Type type)
		{
			var asset = GetAsset(type) as ScriptableObject;

			if (asset == null)
				asset = CreateAsset(name, type);

			return asset;
		}

		public static ScriptableObject CreateAssetAtPath(string path, Type type)
		{
			if (!path.StartsWith(Application.dataPath))
				return null;

			path = path.Substring(Application.dataPath.Length - 6); // keep 'Assets' as the root folder
			
			var asset = ScriptableObject.CreateInstance(type);

			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			return asset;
		}

		#endregion

		#region Lookup

		public static AssetType GetAsset<AssetType>() where AssetType : Object
		{
			return FindAssets<AssetType>().FirstOrDefault();
		}

		public static AssetType GetAssetWithId<AssetType>(string id) where AssetType : Object
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath<AssetType>(path);
		}

		public static AssetType GetAssetAtPath<AssetType>(string path) where AssetType : Object
		{
			return AssetDatabase.LoadAssetAtPath<AssetType>(path);
		}

		public static Object GetAsset(Type assetType)
		{
			return FindAssets(assetType).FirstOrDefault();
		}

		public static Object GetAssetWithId(string id, Type type)
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath(path, type);
		}

		public static Object GetAssetAtPath(string path, Type type)
		{
			return AssetDatabase.LoadAssetAtPath(path, type) as Object;
		}

		#endregion

		#region Listing

		public static List<AssetType> ListAssets<AssetType>() where AssetType : Object
		{
			return FindAssets<AssetType>().ToList();
		}

		public static IEnumerable<AssetType> FindAssets<AssetType>() where AssetType : Object
		{
			return FindAssets(typeof(AssetType)).Select(asset => asset as AssetType);
		}

		public static List<Object> ListAssets(Type assetType)
		{
			return FindAssets(assetType).ToList();
		}

		public static IEnumerable<Object> FindAssets(Type assetType)
		{
			// This query seems to occassionally miss finding some objects. Renaming, moving, or modifying the missing
			// object seems to fix it but the underlying cause is unknown. It might have something to do with loading
			// objects whose serialized representation has changed in which case AssetDatabase.ForceReserializeAssets
			// might also fix it. That could be exposed as a workaround with a refresh button in the AssetPopup ui.

			var query = string.Format("t:{0}", assetType).Replace("UnityEngine.", "");
			return AssetDatabase.FindAssets(query).Select(id => GetAssetWithId(id, assetType));
		}

		public static AssetList GetAssetList<AssetType>(bool includeNone, bool includeCreate) where AssetType : Object
		{
			return GetAssetList(typeof(AssetType), includeNone, includeCreate);
		}

		public static AssetList GetAssetList(Type assetType, bool includeNone, bool includeCreate)
		{
			// include the settings in the name so lists of the same type can be created with different settings
			var listName = string.Format("{0}-{1}-{2}", includeNone, includeCreate, assetType.AssemblyQualifiedName);

			if (!_assetLists.TryGetValue(listName, out var list))
			{
				list = new AssetList { Type = assetType, HasNone = includeNone, HasCreate = includeCreate };
				_assetLists.Add(listName, list);
			}

			if (list.Assets == null)
			{
				list.Assets = ListAssets(assetType);
				list.Types = includeCreate ? TypeHelper.GetTypeList(assetType, false) : null;

				var index = 0;
				var count = 0;
				var paths = list.Assets.Select(asset => GetPath(asset));
				var prefix = FindCommonPath(paths);

				if (includeNone) count += 2; // 'None' and separator
				count += list.Assets.Count;
				if (includeCreate) count += list.Types.Names.Length + 1; // separator and types

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

					foreach (var name in list.Types.Names)
						list.Names[index++] = new GUIContent("Create/" + name.text);
				}
			}

			return list;
		}

		private static string GetPath(Object asset)
		{
			var path = AssetDatabase.GetAssetPath(asset);
			var slash = path.LastIndexOf('/');

			return path.Substring(0, slash + 1);
		}

		#endregion

		#region Helpers

		public static string FindCommonPath(IEnumerable<string> paths)
		{
			var prefix = paths.FirstOrDefault() ?? "";

			foreach (var path in paths)
			{
				var index = 0;
				var count = Math.Min(prefix.Length, path.Length);

				for (; index < count; index++)
				{
					if (prefix[index] != path[index])
						break;
				}

				prefix = prefix.Substring(0, index);

				var slash = prefix.LastIndexOf('/');
				if (slash != prefix.Length - 1)
					prefix = slash >= 0 ? prefix.Substring(0, slash + 1) : "";
			}

			return prefix;
		}

		#endregion
	}
}
