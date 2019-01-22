﻿using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class ArrayDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowReorder = true;
		public bool AllowCollapse = true;
		public bool ShowEditButton = false;
		public bool InlineChildren = false;
		public Type UseAssetList = null;
		public string EmptyText = null;
	}
}