namespace PiRhoSoft.UtilityEngine
{
	public abstract class ConditionalDisplayAttribute : PropertyScopeAttribute
	{
		protected ConditionalDisplayAttribute() : base(int.MaxValue) { }

		public string Property { get; protected set; }

		public string StringValue;
		public int EnumValue = 0;
		public int IntValue = 0;
		public float FloatValue = 0.0f;
		public bool Invert = false;
	}

	public class ConditionalDisplaySelfAttribute : ConditionalDisplayAttribute
	{
		public ConditionalDisplaySelfAttribute(string property) : base() => Property = property;
	}

	public class ConditionalDisplayOtherAttribute : ConditionalDisplayAttribute
	{
		public ConditionalDisplayOtherAttribute(string property) : base() => Property = property;
	}
}
