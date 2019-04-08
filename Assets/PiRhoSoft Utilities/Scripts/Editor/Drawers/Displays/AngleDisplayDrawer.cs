using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(AngleDisplayAttribute))]
	public class AngleDisplayDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(UANDDIT) Invalid type for AngleDisplay of field {0}: AngleDisplay can only be used with Quaternion fields";

		#region Static Property Interface

		private static float[] _rawValues = new float[4];
		private static float[] _eulerValues = new float[3];
		private static float[] _axisAngleValues = new float[4];
		private static float[] _lookForwardValues = new float[3];
		private static float[] _lookUpValues = new float[3];

		public static float GetHeight(GUIContent label, AngleDisplayType type)
		{
			var height = EditorGUIUtility.singleLineHeight;

			if (type == AngleDisplayType.Look)
				return height + EditorGUIUtility.standardVerticalSpacing + height;

			return height;
		}

		public static void Draw(GUIContent label, SerializedProperty property, AngleDisplayType type)
		{
			var height = GetHeight(label, type);
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, type);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, AngleDisplayType type)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
			{
				property.quaternionValue = Draw(position, label, property.quaternionValue, type);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion

		#region Static Object Interface

		private static GUIContent[] _rawLabels = new GUIContent[]
		{
			new GUIContent("X", "The x component of the quaternion"),
			new GUIContent("Y", "The y component of the quaternion"),
			new GUIContent("Z", "The z component of the quaternion"),
			new GUIContent("W", "The w component of the quaternion")
		};

		private static GUIContent[] _eulerLabels = new GUIContent[]
		{
			new GUIContent("Z", "The rotation about the z axis (applied first)"),
			new GUIContent("X", "The rotation about the x axis (applied second)"),
			new GUIContent("Y", "The rotation about the y axis (applied third)")
		};

		private static GUIContent[] _axisAngleLabels = new GUIContent[]
		{
			new GUIContent("X", "The x component of the vector to rotate about"),
			new GUIContent("Y", "The y component of the vector to rotate about"),
			new GUIContent("Z", "The z component of the vector to rotate about"),
			new GUIContent("A", "The angle of rotation about the axis (in degrees)")
		};

		private static GUIContent[] _lookForwardLabels = new GUIContent[]
		{
			new GUIContent("X", "The x component of the forward vector"),
			new GUIContent("Y", "The y component of the forward vector"),
			new GUIContent("Z", "The z component of the forward vector")
		};

		private static GUIContent[] _lookUpLabels = new GUIContent[]
		{
			new GUIContent("X", "The x component of the up vector"),
			new GUIContent("Y", "The y component of the up vector"),
			new GUIContent("Z", "The z component of the up vector")
		};

		public static Quaternion Draw(GUIContent label, Quaternion quaternion, AngleDisplayType type)
		{
			var height = GetHeight(label, type);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, quaternion, type);
		}

		public static Quaternion Draw(Rect position, GUIContent label, Quaternion quaternion, AngleDisplayType type)
		{
			switch (type)
			{
				case AngleDisplayType.Raw:
				{
					_rawValues[0] = quaternion.x;
					_rawValues[1] = quaternion.y;
					_rawValues[2] = quaternion.z;
					_rawValues[3] = quaternion.w;

					using (var changes = new EditorGUI.ChangeCheckScope())
					{
						EditorGUI.MultiFloatField(position, label, _rawLabels, _rawValues);

						if (changes.changed)
							quaternion.Set(_rawValues[0], _rawValues[1], _rawValues[2], _rawValues[3]);
					}

					break;
				}
				case AngleDisplayType.Euler:
				{
					var euler = quaternion.eulerAngles;

					_eulerValues[0] = euler.z;
					_eulerValues[1] = euler.x;
					_eulerValues[2] = euler.y;

					using (var changes = new EditorGUI.ChangeCheckScope())
					{
						EditorGUI.MultiFloatField(position, label, _eulerLabels, _eulerValues);

						if (changes.changed)
							quaternion.eulerAngles = new Vector3(_eulerValues[1], _eulerValues[2], _eulerValues[0]);
					}

					break;
				}
				case AngleDisplayType.AxisAngle:
				{
					quaternion.ToAngleAxis(out var angle, out var axis);

					_axisAngleValues[0] = axis.x;
					_axisAngleValues[1] = axis.y;
					_axisAngleValues[2] = axis.z;
					_axisAngleValues[3] = angle;

					using (var changes = new EditorGUI.ChangeCheckScope())
					{
						EditorGUI.MultiFloatField(position, label, _axisAngleLabels, _axisAngleValues);

						if (changes.changed)
							quaternion = Quaternion.AngleAxis(_axisAngleValues[3], new Vector3(_axisAngleValues[0], _axisAngleValues[1], _axisAngleValues[2]));
					}

					break;
				}
				case AngleDisplayType.Look:
				{
					position = EditorGUI.PrefixLabel(position, label);

					var forward = quaternion * Vector3.forward;
					var up = quaternion * Vector3.up;

					_lookForwardValues[0] = forward.x;
					_lookForwardValues[1] = forward.y;
					_lookForwardValues[2] = forward.z;

					_lookUpValues[0] = up.x;
					_lookUpValues[1] = up.y;
					_lookUpValues[2] = up.z;

					var height = EditorGUIUtility.singleLineHeight;
					var forwardRect = RectHelper.TakeHeight(ref position, height);
					RectHelper.TakeHeight(ref position, EditorGUIUtility.standardVerticalSpacing);
					var upRect = position;

					using (var changes = new EditorGUI.ChangeCheckScope())
					{
						EditorGUI.MultiFloatField(forwardRect, _lookForwardLabels, _lookForwardValues);
						EditorGUI.MultiFloatField(upRect, _lookUpLabels, _lookUpValues);

						if (changes.changed)
							quaternion = Quaternion.LookRotation(new Vector3(_lookForwardValues[0], _lookForwardValues[1], _lookForwardValues[2]), new Vector3(_lookUpValues[0], _lookUpValues[1], _lookUpValues[2]));
					}

					break;
				}
			}

			return quaternion;
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
				return GetHeight(label, (attribute as AngleDisplayAttribute).Type);
			else
				return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);
			Draw(position, label, property, (attribute as AngleDisplayAttribute).Type);
		}

		#endregion
	}
}
