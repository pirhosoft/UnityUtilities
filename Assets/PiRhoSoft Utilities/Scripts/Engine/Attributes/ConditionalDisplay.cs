using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class ConditionalDisplayAttribute : PropertyAttribute
	{
		public string Property { get; protected set; }

		public string StringValue;
		public int EnumValue = 0;
		public int IntValue = 0;
		public float FloatValue = 0.0f;
		public bool Invert = false;
	}

	public class ConditionalDisplaySelfAttribute : ConditionalDisplayAttribute
	{
		public ConditionalDisplaySelfAttribute(string property) => Property = property;
	}

	public class ConditionalDisplayOtherAttribute : ConditionalDisplayAttribute
	{
		public ConditionalDisplayOtherAttribute(string property) => Property = property;
	}
}
