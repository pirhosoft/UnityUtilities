using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Angle Display")]
	public class AngleDisplayExample : MonoBehaviour
	{
		[AngleDisplay] public Quaternion Euler = Quaternion.identity;
		[AngleDisplay(Type = AngleDisplayType.AxisAngle)] public Quaternion AxisAngle = Quaternion.identity;
		[AngleDisplay(Type = AngleDisplayType.Look)] public Quaternion Look = Quaternion.identity;
		[AngleDisplay(Type = AngleDisplayType.Raw)] public Quaternion Raw = Quaternion.identity;

		public Quaternion Default = Quaternion.identity;
	}
}
