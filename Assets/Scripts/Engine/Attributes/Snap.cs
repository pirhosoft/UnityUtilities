using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class SnapAttribute : PropertyAttribute
	{
		public float SnapValue { get; private set; }

		public SnapAttribute(int snapValue) => SnapValue = snapValue;
		public SnapAttribute(float snapValue) => SnapValue = snapValue;
	}
}
