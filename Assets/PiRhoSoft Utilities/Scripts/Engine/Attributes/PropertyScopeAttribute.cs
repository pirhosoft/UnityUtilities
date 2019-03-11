using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public abstract class PropertyScopeAttribute : PropertyAttribute
	{
		protected PropertyScopeAttribute(int drawOrder)
		{
			order = drawOrder;
		}
	}
}
