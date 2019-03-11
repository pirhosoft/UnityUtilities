using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Int Display")]
	public class IntDisplayExample : MonoBehaviour
	{
		[Tooltip("A normal int property without IntDisplay")]
		public int Normal;

		[Tooltip("A popup with a predefined set of valid options")]
		[IntDisplay(new string[] { "Zero", "One ", "Ninety", "Negative 10", "Two Thousand" }, new int[] { 0, 1, 90, -10, 2000 })]
		public int Popup;

		[Tooltip("A slider with range from 0 to 100 in increments of 5")]
		[IntDisplay(1000, 100, 5)]
		public int Slider;

		[Tooltip("A two component slider with range from -20 to 40 in increments of 6")]
		[IntDisplay(nameof(Maximum), -24, 42, 6)]
		public int Minimum;
		[HideInInspector]
		public int Maximum;
	}
}
