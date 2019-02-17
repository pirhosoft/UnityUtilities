using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ContextMarginScope : GUI.Scope
	{
		private float _margin;

		public ContextMarginScope(float margin)
		{
			_margin = RectHelper.ContextMargin;
			RectHelper.ContextMargin = margin;
		}

		protected override void CloseScope()
		{
			RectHelper.ContextMargin = _margin;
		}
	}
}
