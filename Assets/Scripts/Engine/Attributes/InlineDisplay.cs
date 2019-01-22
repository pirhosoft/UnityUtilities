using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class InlineDisplayAttribute : PropertyAttribute
	{
		public bool PropagateLabel { get; private set; }
		public InlineDisplayAttribute(bool propagateLabel = false) => PropagateLabel = propagateLabel;
	}
}
