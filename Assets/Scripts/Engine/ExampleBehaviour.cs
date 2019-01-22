using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[Serializable]
	public class StringList : SerializedList<string> { }

	[Serializable]
	public class IntArray : SerializedArray<int> { public IntArray(int count) : base(count) { } }

	[Serializable]
	public class StringDictionary : SerializedDictionary<string, string> { }

	[Flags]
	public enum Buttons
	{
		None = 0,
		Left = 1 << 0,
		Right = 1 << 1,
		Up = 1 << 2,
		Down = 1 << 3,
		AllAround = ~0
	}

	public enum Toggles
	{
		Left,
		Right,
		Up,
		Down
	}

	public class ExampleBehaviour : MonoBehaviour
	{
		[Header("Inline Children")]
		[InlineDisplay] public Vector2 InlineVector;
		[Space]

		[Minimum(0.0f)] public float MinimumFloat;
		[Minimum(0)] public float MinimumInt;
		[Maximum(100.0f)] public float MaximumFloat;
		[Maximum(100)] public float MaximumInt;

		public bool ShowConditional = true;
		[ConditionalDisplay(nameof(ShowConditional), BoolValue = true)] public string ConditionalString;

		[AssetPopup] public ScriptableObject Asset;

		[MinMaxSlider(0, 10, 2)] public int IntMinMaxSlider;
		[HideInInspector] public int IntMinMaxSliderMax;

		[MinMaxSlider(0.0f, 10.0f, 0.5f)] public float FloatMinMaxSlider;
		[HideInInspector] public float FloatMinMaxSliderMax;

		[StringPopup(new string[] { "Yes", "No" })] public string StringPopup;
		[IntPopup(new int[] { 0, 5, 10 }, new string[] { "Zero", "Five", "Ten" })] public int IntPopup;

		[ListDisplay] public StringList StringList = new StringList();
		[DictionaryDisplay] public StringDictionary StringDictionary = new StringDictionary();
		[ListDisplay] public IntArray ArrayList = new IntArray(5);

		[DisableInInspector] public float Disabled;

		[FlagsPopup] public Buttons PopupFlags;
		[EnumButtons] public Buttons ButtonFlags;
		[EnumButtons] public Toggles Buttons;

		[Snap(5)] public int SnapInt;
		[Snap(0.5f)] public float SnapFloat;

		[Slider(0, 100, 25)] public int IntSlider;
		[Slider(0.0f, 10.0f, 1.125f)] public float FloatSlider;
	}
}