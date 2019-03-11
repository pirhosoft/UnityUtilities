using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum ClassDisplayType
	{
		Indented,
		Inline,
		Contained
	}

	public class ClassDisplayAttribute : PropertyAttribute
	{
		public ClassDisplayType Type = ClassDisplayType.Contained;
	}
}
