using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Asset Display")]
	public class AssetDisplayExample : MonoBehaviour
	{
		public ExampleAsset Normal;

		[AssetDisplay(ShowNoneOption = false, ShowEditButton = false, SaveLocation = AssetDisplaySaveLocation.None)] public ExampleAsset Pick;
		[AssetDisplay(ShowNoneOption = false, ShowEditButton = false, SaveLocation = AssetDisplaySaveLocation.Selectable)] public ExampleAsset Create;
		[AssetDisplay(ShowNoneOption = false, ShowEditButton = true, SaveLocation = AssetDisplaySaveLocation.Selectable)] public ExampleAsset CreateAndEdit;
		[AssetDisplay(ShowNoneOption = true, ShowEditButton = true, SaveLocation = AssetDisplaySaveLocation.Selectable)] public ExampleAsset NoneCreateAndEdit;

		[AssetDisplay(SaveLocation = AssetDisplaySaveLocation.AssetRoot)] public ExampleAsset DefaultCreate;
		[AssetDisplay(SaveLocation = AssetDisplaySaveLocation.AssetRoot, DefaultName = "My Example Asset")] public ExampleAsset CreateWithName;
	}
}
