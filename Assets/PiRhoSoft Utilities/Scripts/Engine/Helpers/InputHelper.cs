using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public struct ButtonState
	{
		public bool Pressed;
		public bool Held;
		public bool Released;

		public ButtonState(bool pressed, bool held, bool released)
		{
			Pressed = pressed;
			Held = held;
			Released = released;
		}

		public ButtonState(string button)
		{
			Pressed = Input.GetButtonDown(button);
			Held = Input.GetButton(button);
			Released = Input.GetButtonUp(button);
		}

		public ButtonState(KeyCode key)
		{
			Pressed = Input.GetKeyDown(key);
			Held = Input.GetKey(key);
			Released = Input.GetKeyUp(key);
		}
	}

	public static class InputHelper
	{
		private static Dictionary<string, ButtonData> _manualButtons = new Dictionary<string, ButtonData>();
		private static Dictionary<string, AxisData> _manualAxes = new Dictionary<string, AxisData>();
		private static Dictionary<string, AxisValue> _previousAxes = new Dictionary<string, AxisValue>();

		public static void LateUpdate()
		{
			UpdateManualButtons();
			UpdatePreviousAxisValues();
		}

		#region Manual Input

		public static void SetButton(string button, bool down)
		{
			var data = GetManualButtonData(button);
			data.Pressed = !data.Held && down;
			data.Released = data.Held && !down;
			data.Held = down;
		}

		public static void RemoveButton(string button)
		{
			_manualButtons.Remove(button);
		}

		public static void SetAxis(string axis, float value)
		{
			var data = GetManualAxisData(axis);
			data.Value = value;
		}

		public static void RemoveAxis(string axis)
		{
			_manualAxes.Remove(axis);
		}

		private static ButtonData GetManualButtonData(string button)
		{
			if (!_manualButtons.TryGetValue(button, out ButtonData data))
			{
				data = new ButtonData();
				_manualButtons.Add(button, data);
			}

			return data;
		}

		private static AxisData GetManualAxisData(string axis)
		{
			if (!_manualAxes.TryGetValue(axis, out AxisData data))
			{
				data = new AxisData();
				_manualAxes.Add(axis, data);
			}

			return data;
		}

		private static void UpdateManualButtons()
		{
			foreach (var button in _manualButtons)
			{
				button.Value.Pressed = false;
				button.Value.Released = false;
			}
		}

		#endregion

		#region Button State

		public static bool IsButtonAvailable(string button)
		{
			if (_manualButtons.ContainsKey(button))
				return true;

			try
			{
				Input.GetButton(button);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static ButtonState GetButtonState(KeyCode key, string button)
		{
			if (!string.IsNullOrEmpty(button))
			{
				if (_manualButtons.TryGetValue(button, out ButtonData state))
					return new ButtonState(state.Pressed, state.Held, state.Released);

				try
				{
					return new ButtonState(button);
				}
				catch { }
			}

			return new ButtonState(key);
		}

		public static bool GetButtonDown(KeyCode key, string button)
		{
			if (!string.IsNullOrEmpty(button))
			{
				if (_manualButtons.TryGetValue(button, out ButtonData state))
					return state.Held;

				try
				{
					return Input.GetButton(button);
				}
				catch { }
			}

			return Input.GetKey(key);
		}
		
		public static bool GetWasButtonPressed(KeyCode key, string button)
		{
			if (!string.IsNullOrEmpty(button))
			{
				if (_manualButtons.TryGetValue(button, out ButtonData state))
					return state.Pressed;

				try
				{
					return Input.GetButtonDown(button);
				}
				catch { }
			}

			return Input.GetKeyDown(key);
		}
		
		public static bool GetWasButtonReleased(KeyCode key, string button)
		{
			if (!string.IsNullOrEmpty(button))
			{
				if (_manualButtons.TryGetValue(button, out ButtonData state))
					return state.Released;

				try
				{
					return Input.GetButtonUp(button);
				}
				catch { }
			}

			return Input.GetKeyUp(key);
		}

		#endregion

		#region Axis State

		public static float GetAxis(string axis)
		{
			return GetCurrentAxisValue(axis);
		}

		public static ButtonState GetAxisState(string axis, float magnitude)
		{
			var previousValue = GetPreviousAxisValue(axis);
			var currentValue = GetCurrentAxisValue(axis);

			var wasPressed = IsAxisPressed(previousValue, magnitude);
			var isPressed = IsAxisPressed(currentValue, magnitude);

			return new ButtonState
			{
				Pressed = !wasPressed && isPressed,
				Held = isPressed,
				Released = wasPressed && !isPressed
			};
		}

		public static bool GetAxisDown(string axis, float magnitude)
		{
			var currentValue = GetCurrentAxisValue(axis);

			return IsAxisPressed(currentValue, magnitude);
		}

		public static bool GetWasAxisPressed(string axis, float magnitude)
		{
			var previousValue = GetPreviousAxisValue(axis);
			var currentValue = GetCurrentAxisValue(axis);

			return !IsAxisPressed(previousValue, magnitude) && IsAxisPressed(currentValue, magnitude);
		}

		public static bool GetWasAxisReleased(string axis, float magnitude)
		{
			var previousValue = GetPreviousAxisValue(axis);
			var currentValue = GetCurrentAxisValue(axis);

			return IsAxisPressed(previousValue, magnitude) && !IsAxisPressed(currentValue, magnitude);
		}

		private static float GetCurrentAxisValue(string axis)
		{
			if (_manualAxes.TryGetValue(axis, out AxisData data))
				return data.Value;
			else
				return Input.GetAxisRaw(axis);
		}

		private static float GetPreviousAxisValue(string axis)
		{
			if (!_previousAxes.TryGetValue(axis, out AxisValue value))
			{
				value = new AxisValue { Value = 0.0f };
				_previousAxes.Add(axis, value);
			}

			return value.Value;
		}

		private static void UpdatePreviousAxisValues()
		{
			foreach (var axis in _previousAxes)
				axis.Value.Value = GetCurrentAxisValue(axis.Key);
		}

		private static bool IsAxisPressed(float value, float magnitude)
		{
			return (magnitude < 0.0f && value < magnitude) || (magnitude > 0.0f && value > magnitude);
		}

		#endregion

		#region Support Classes

		private class ButtonData
		{
			public bool Pressed;
			public bool Held;
			public bool Released;
		}

		private class AxisData
		{
			public float Value;
		}

		private class AxisValue
		{
			public float Value;
		}

		#endregion
	}
}
