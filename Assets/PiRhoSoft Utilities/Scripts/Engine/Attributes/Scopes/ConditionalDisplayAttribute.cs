namespace PiRhoSoft.UtilityEngine
{
	public abstract class ConditionalDisplayAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue;

		protected ConditionalDisplayAttribute() : base(DefaultOrder) { }

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
