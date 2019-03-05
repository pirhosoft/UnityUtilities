using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[DisallowMultipleComponent]
	[AddComponentMenu("PiRho Soft/Animation/Simple Animation Player")]
	public class SimpleAnimationPlayer : AnimationPlayer
	{
		[Tooltip("Whether to play the animation when the object awakes")]
		public bool PlayOnAwake = true;

		[Tooltip("The animation to play")]
		public AnimationClip Animation;

		protected override void Awake()
		{
			base.Awake();

			if (Animation && PlayOnAwake)
				PlayAnimation(Animation);
		}
	}
}
