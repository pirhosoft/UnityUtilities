using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class ConditionalDisplayAttribute : PropertyAttribute
	{
		public string Property { get; private set; }

		public string StringValue;
		public int EnumValue = 0;
		public int IntValue = 0;
		public float FloatValue = 0.0f;
		public bool BoolValue = true;
		public bool HasReference = true;

		public ConditionalDisplayAttribute(string property) => Property = property;
	}
}
