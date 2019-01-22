using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class LabelWidthScope : GUI.Scope
	{
		private float _width;

		public LabelWidthScope(float width)
		{
			_width = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = width;
		}

		protected override void CloseScope()
		{
			EditorGUIUtility.labelWidth = _width;
		}
	}
}
