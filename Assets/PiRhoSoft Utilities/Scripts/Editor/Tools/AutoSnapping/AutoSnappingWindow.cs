using UnityEngine;
using UnityEditor;

namespace PiRhoSoft.UtilityEditor
{
	[InitializeOnLoad]
	public static class AutoSnapping
	{
		public static class Preferences
		{
			public static BoolPreference Enabled = new BoolPreference("PiRhoSoft.AutoSnapping.Enabled", false);
			public static BoolPreference SnapPosition = new BoolPreference("PiRhoSoft.AutoSnapping.SnapPosition", true);
			public static BoolPreference SnapScale = new BoolPreference("PiRhoSoft.AutoSnapping.SnapScale", true);
			public static BoolPreference SnapRotation = new BoolPreference("PiRhoSoft.AutoSnapping.SnapRotation", true);
			public static FloatPreference XPositionIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.XPositionIncrement", 0.1f);
			public static FloatPreference YPositionIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.YPositionIncrement", 0.1f);
			public static FloatPreference ZPositionIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.ZPositionIncrement", 0.1f);
			public static FloatPreference XScaleIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.XScaleIncrement", 0.1f);
			public static FloatPreference YScaleIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.YScaleIncrement", 0.1f);
			public static FloatPreference ZScaleIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.ZScaleIncrement", 0.1f);
			public static FloatPreference RotationIncrement = new FloatPreference("PiRhoSoft.AutoSnapping.RotationIncrement", 15.0f);
		}

		private static bool _active = false;

		static AutoSnapping()
		{
			SetEnabled(Preferences.Enabled.Value);
		}

		public static void SetEnabled(bool enabled)
		{
			Preferences.Enabled.Value = enabled;

			if (enabled && !_active)
			{
				EditorApplication.update += Update;
				_active = true;
			}
			else if (!enabled && _active)
			{
				EditorApplication.update -= Update;
				_active = false;
			}
		}

		private static void Update()
		{
			if (!EditorApplication.isPlaying && Selection.transforms.Length > 0)
			{
				if (Preferences.SnapPosition.Value)
					SnapPosition();

				if (Preferences.SnapScale.Value)
					SnapScale();

				if (Preferences.SnapRotation.Value)
					SnapRotation();
			}
		}

		private static void SnapPosition()
		{
			foreach (var transform in Selection.transforms)
			{
				if (!(transform is RectTransform))
				{
					var position = transform.position;
					position.x = Snap(position.x, Preferences.XPositionIncrement.Value);
					position.y = Snap(position.y, Preferences.YPositionIncrement.Value);
					position.z = Snap(position.z, Preferences.ZPositionIncrement.Value);
					transform.transform.position = position;
				}
			}
		}

		private static void SnapScale()
		{
			foreach (var transform in Selection.transforms)
			{
				if (!(transform is RectTransform))
				{
					var scale = transform.localScale;
					scale.x = Snap(scale.x, Preferences.XScaleIncrement.Value);
					scale.y = Snap(scale.y, Preferences.YScaleIncrement.Value);
					scale.z = Snap(scale.z, Preferences.ZScaleIncrement.Value);
					transform.transform.localScale = scale;

					var renderer = transform.GetComponent<SpriteRenderer>();
					if (renderer)
					{
						var x = Snap(renderer.size.x, Preferences.XScaleIncrement.Value);
						var y = Snap(renderer.size.y, Preferences.YScaleIncrement.Value);

						renderer.size = new Vector2(x, y);
					}
				}
			}
		}

		private static void SnapRotation()
		{
			foreach (var transform in Selection.transforms)
			{
				if (!(transform is RectTransform))
				{
					var rotation = transform.eulerAngles;
					rotation.x = Snap(rotation.x, Preferences.RotationIncrement.Value);
					rotation.y = Snap(rotation.y, Preferences.RotationIncrement.Value);
					rotation.z = Snap(rotation.z, Preferences.RotationIncrement.Value);
					transform.transform.eulerAngles = rotation;
				}
			}
		}

		private static float Snap(float value, float snap)
		{
			return snap > 0.0f ? Mathf.Round(value / snap) * snap : value;
		}
	}

	public class AutoSnappingWindow : EditorWindow
	{
		private static readonly GUIContent _enabledContent = new GUIContent("Enabled Snapping", "Enable automatic snapping of transforms to a selected grid");
		private static readonly GUIContent _snapPositionContent = new GUIContent("Snap Positions", "Whether positions should be snapped to increments automatically when moved");
		private static readonly GUIContent _snapScaleContent = new GUIContent("Snap Scales", "Whether scales should be snapped to the increments automatically when scaled");
		private static readonly GUIContent _snapRotationContent = new GUIContent("Snap Rotations", "Whether rotations should be snapped to the increments when rotated");
		private static readonly GUIContent _xPositionContent = new GUIContent("X Position Increment", "The increment to snap the x positions to");
		private static readonly GUIContent _yPositionContent = new GUIContent("Y Position Increment", "The increment to snap the y positions to");
		private static readonly GUIContent _zPositionContent = new GUIContent("Z Position Increment", "The increment to snap the z positions to");
		private static readonly GUIContent _xScaleContent = new GUIContent("X Scale Increment", "The increment to snap the x scales to");
		private static readonly GUIContent _yScaleContent = new GUIContent("Y Scale Increment", "The increment to snap the y scales to");
		private static readonly GUIContent _zScaleContent = new GUIContent("Z Scale Increment", "The increment to snap the z scales to");
		private static readonly GUIContent _rotationContent = new GUIContent("Rotation Increment", "The increment to snap rotations to");

		[MenuItem("Window/PiRhoSoft Utility/Auto Snapping")]
		public static void Open()
		{
			GetWindow<AutoSnappingWindow>("Auto Snapping").Show();
		}

		public void OnGUI()
		{
			var enabled = EditorGUILayout.Toggle(_enabledContent, AutoSnapping.Preferences.Enabled.Value);

			if (enabled != AutoSnapping.Preferences.Enabled.Value)
				AutoSnapping.SetEnabled(enabled);

			if (AutoSnapping.Preferences.Enabled.Value)
			{
				AutoSnapping.Preferences.SnapPosition.Value = EditorGUILayout.Toggle(_snapPositionContent, AutoSnapping.Preferences.SnapPosition.Value);

				if (AutoSnapping.Preferences.SnapPosition.Value)
				{
					AutoSnapping.Preferences.XPositionIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_xPositionContent, AutoSnapping.Preferences.XPositionIncrement.Value));
					AutoSnapping.Preferences.YPositionIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_yPositionContent, AutoSnapping.Preferences.YPositionIncrement.Value));
					AutoSnapping.Preferences.ZPositionIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_zPositionContent, AutoSnapping.Preferences.ZPositionIncrement.Value));
				}

				AutoSnapping.Preferences.SnapScale.Value = EditorGUILayout.Toggle(_snapScaleContent, AutoSnapping.Preferences.SnapScale.Value);

				if (AutoSnapping.Preferences.SnapScale.Value)
				{
					AutoSnapping.Preferences.XScaleIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_xScaleContent, AutoSnapping.Preferences.XScaleIncrement.Value));
					AutoSnapping.Preferences.YScaleIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_yScaleContent, AutoSnapping.Preferences.YScaleIncrement.Value));
					AutoSnapping.Preferences.ZScaleIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_zScaleContent, AutoSnapping.Preferences.ZScaleIncrement.Value));
				}

				AutoSnapping.Preferences.SnapRotation.Value = EditorGUILayout.Toggle(_snapRotationContent, AutoSnapping.Preferences.SnapRotation.Value);

				if (AutoSnapping.Preferences.SnapRotation.Value)
					AutoSnapping.Preferences.RotationIncrement.Value = Mathf.Max(0.001f, EditorGUILayout.FloatField(_rotationContent, AutoSnapping.Preferences.RotationIncrement.Value));
			}
		}
	}
}