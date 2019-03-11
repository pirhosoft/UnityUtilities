using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class TypeDisplayAttribute : PropertyAttribute
	{
		public Type RootType { get; private set; }
		public bool AllowNone = true;

		public TypeDisplayAttribute(Type rootType) => RootType = rootType;
	}
}
