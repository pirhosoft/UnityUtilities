using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Asset Display")]
	public class AssetDisplayExample : MonoBehaviour
	{
		[AssetDisplay] public ExampleAsset Pick;
		[AssetDisplay(ShowNoneOption = false)] public ExampleAsset PickRequired;
		[AssetDisplay(ShowEditButton = true)] public ExampleAsset PickAndEdit;
		[AssetDisplay(SaveLocation = AssetDisplaySaveLocation.AssetRoot)] public ExampleAsset PickAndCreate;
		[AssetDisplay(SaveLocation = AssetDisplaySaveLocation.Selectable)] public ExampleAsset PickAndSave;

		public ExampleAsset Default;
	}
}
