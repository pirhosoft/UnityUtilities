using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Float Display")]
	public class FloatDisplayExample : MonoBehaviour
	{
		[Tooltip("A normal float property without FloatDisplay")]
		public float Normal;

		[Tooltip("A popup with a predefined set of valid options")]
		[FloatDisplay(new string[] { "Zero", "One Point Five", "Ninety", "Negative 10", "Two Thousand" }, new float[] { 0.0f, 1.5f, 90.0f, -10.0f, 2000.0f })]
		public float Popup;

		[Tooltip("A slider with range from 0 to 100 in increments of 0.5")]
		[FloatDisplay(0.0f, 100.0f, 0.5f)]
		public float Slider;

		[Tooltip("A two component slider with range from -20 to 40 in increments of 0.1")]
		[FloatDisplay(nameof(Maximum), -20.0f, 40.0f, 0.1f)]
		public float Range;
		[HideInInspector]
		public float Maximum;
	}
}
