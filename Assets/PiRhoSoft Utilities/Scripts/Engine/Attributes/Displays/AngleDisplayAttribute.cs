using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum AngleDisplayType
	{
		Euler,
		AxisAngle,
		Look,
		Raw
	}

	public class AngleDisplayAttribute : PropertyAttribute
	{
		public AngleDisplayType Type = AngleDisplayType.Euler;
	}
}
