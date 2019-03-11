using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum AngleDisplayType
	{
		Raw,
		Euler,
		AxisAngle,
		Look
	}

	public class AngleDisplayAttribute : PropertyAttribute
	{
		public AngleDisplayType Type = AngleDisplayType.Euler;
	}
}
