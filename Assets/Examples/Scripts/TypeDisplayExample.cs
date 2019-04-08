using PiRhoSoft.UtilityEngine;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityExample
{
	[AddComponentMenu("PiRho Soft/Examples/Type Display")]
	public class TypeDisplayExample : MonoBehaviour
	{
		[TypeDisplay(typeof(Enum))] public string Enum;
		[TypeDisplay(typeof(Object))] public string Object;
		[TypeDisplay(typeof(Object), ShowAbstractOptions = false)] public string ConcreteObject;
	}
}
