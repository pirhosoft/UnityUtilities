using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public static class MathHelper
	{
		public static int IntExponent(int value, int exponent)
		{
			var result = 1;

			for (var i = 0; i < exponent; i++)
				result *= value;

			return result;
		}

		public static int LogBase2(int value)
		{
			var result = 0;

			value >>= 1;

			while (value > 0)
			{
				value >>= 1;
				++result;
			}

			return result;
		}

		public static float Snap(float value, float snap)
		{
			return snap > 0.0f ? Mathf.Round(value / snap) * snap : value;
		}

		public static int LeastCommonMultiple(int a, int b)
		{
			var num1 = a > b ? a : b;
			var num2 = a > b ? b : a;

			for (var i = 1; i < num2; i++)
			{
				var multiple = num1 * i;
				if (multiple % num2 == 0)
					return multiple;
			}

			return num1 * num2;
		}
	}
}
