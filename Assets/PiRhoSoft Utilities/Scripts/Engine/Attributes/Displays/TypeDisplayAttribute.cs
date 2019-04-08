using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class TypeDisplayAttribute : PropertyAttribute
	{
		public Type RootType { get; private set; }
		public bool ShowNoneOption = true;
		public bool ShowAbstractOptions = true;

		public TypeDisplayAttribute(Type rootType) => RootType = rootType;
	}
}
