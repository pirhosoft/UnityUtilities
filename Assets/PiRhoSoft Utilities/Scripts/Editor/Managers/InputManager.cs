using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PiRhoSoft.UtilityEditor
{
	public class InputManager
	{
		// based on concepts from the MIT licensed Unity Node Editor Base Extension
		// https://unitylist.com/p/tb/Unity-Node-Editor-Base

		private List<EventTrigger> _triggers = new List<EventTrigger>();

		public T Create<T>() where T : EventTrigger, new()
		{
			var trigger = new T();
			_triggers.Add(trigger);
			return trigger;
		}

		public void Clear()
		{
			_triggers.Clear();
		}

		public void Update()
		{
			foreach (var trigger in _triggers)
				trigger.Update();
		}

		public class EventTrigger
		{
			public List<Func<bool>> Conditions = new List<Func<bool>>();
			public Action Action;

			private EventType _type;
			private bool _shift;
			private bool _control;
			private bool _alt;

			public EventTrigger SetEvent(EventType type, bool shift = false, bool control = false, bool alt = false)
			{
				_type = type;
				_shift = shift;
				_control = control;
				_alt = alt;

				AddCondition(() => Event.current.type == _type && Event.current.shift == _shift && Event.current.control == _control && Event.current.alt == _alt);
				return this;
			}

			public EventTrigger AddCondition(Func<bool> condition)
			{
				Conditions.Add(condition);
				return this;
			}

			public EventTrigger AddAction(Action action)
			{
				Action += action;
				return this;
			}

			private bool AreConditionsMet()
			{
				foreach (var condition in Conditions)
				{
					if (!condition())
						return false;
				}

				return true;
			}

			internal void Update()
			{
				if (Action != null && AreConditionsMet())
					Action.Invoke();
			}
		}

		public class KeyboardTrigger : EventTrigger
		{
			private KeyCode _key;

			public KeyboardTrigger SetEvent(EventType type, KeyCode key, bool shift = false, bool control = false, bool alt = false)
			{
				_key = key;

				SetEvent(type, shift, control, alt);
				AddCondition(() => Event.current.keyCode == _key);
				return this;
			}
		}

		public enum MouseButton
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}

		public class MouseTrigger : EventTrigger
		{
			private int _button;

			public MouseTrigger SetEvent(EventType type, MouseButton button, bool shift = false, bool control = false, bool alt = false)
			{
				_button = (int)button;

				SetEvent(type, shift, control, alt);
				AddCondition(() => Event.current.button == _button);
				return this;
			}

			public MouseTrigger SetRawEvent(EventType type, MouseButton button, bool shift = false, bool control = false, bool alt = false)
			{
				// raw events will get sent to windows even when the mouse is outside its clipping area

				_button = (int)button;
				AddCondition(() => Event.current.rawType == type && Event.current.button == _button && Event.current.shift == shift && Event.current.control == control && Event.current.alt == alt);
				return this;
			}
		}
	}
}
