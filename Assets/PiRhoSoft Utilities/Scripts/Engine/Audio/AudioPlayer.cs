using System.Collections;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	interface IAudioNotifier
	{
		bool IsDone { get; }
	}

	[DisallowMultipleComponent]
	[RequireComponent(typeof(AudioSource))]
	public class AudioPlayer : MonoBehaviour, IAudioNotifier
	{
		private AudioSource _audio;
		private bool _started = false;

		public bool IsDone
		{
			get
			{
				var done = !_audio.isPlaying;

				if (_started && done) // Reset started here since PlaySound cannot
					_started = false;

				return !_started || done;
			}
		}

		protected virtual void Awake()
		{
			_audio = GetComponent<AudioSource>();
		}

		public void PlaySound(AudioClip sound)
		{
			_started = true;
			_audio.PlayOneShot(sound, 1.0f);
		}

		public IEnumerator PlaySoundAndWait(AudioClip sound)
		{
			PlaySound(sound);

			while (_audio.isPlaying)
				yield return null;
		}
	}
}
