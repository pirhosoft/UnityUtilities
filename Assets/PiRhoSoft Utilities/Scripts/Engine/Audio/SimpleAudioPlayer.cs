using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[DisallowMultipleComponent]
	[AddComponentMenu("PiRhoSoft Utility/Audio/Simple Audio Player")]
	public class SimpleAudioPlayer : AudioPlayer
	{
		[Tooltip("Whether to play the sound when the object awakes")]
		public bool PlayOnAwake = true;

		[Tooltip("The sound to play")]
		public AudioClip Sound;

		protected override void Awake()
		{
			base.Awake();

			if (Sound && PlayOnAwake)
				PlaySound(Sound);
		}
	}
}
