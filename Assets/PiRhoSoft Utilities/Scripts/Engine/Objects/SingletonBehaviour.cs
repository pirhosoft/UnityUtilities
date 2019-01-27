using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
	{
		private const string _secondInstanceWarning = "(USBSI) A second instance of SingletonBehaviour type {0} was created";

		public static T Instance { get; private set; }

		protected virtual void Awake()
		{
			if (Instance == null)
			{
				Instance = this as T;
			}
			else
			{
				Debug.LogWarningFormat(_secondInstanceWarning, GetType().Name);
				Destroy(this);
			}
		}

		protected virtual void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}
	}
}
