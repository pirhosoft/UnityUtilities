using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class HandleColorScope : GUI.Scope
	{
		private Color _color;

		public HandleColorScope()
		{
			_color = Handles.color;
		}

		public HandleColorScope(Color color)
		{
			_color = Handles.color;

			Handles.color = color;
		}

		protected override void CloseScope()
		{
			Handles.color = _color;
		}
	}
}
