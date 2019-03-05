using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PiRhoSoft.UtilityEngine
{
	interface IAnimationNotifier
	{
		bool IsDone { get; }
	}

	[DisallowMultipleComponent]
	[RequireComponent(typeof(Animator))]
	[AddComponentMenu("PiRhoSoft Utility/Animation/Animation Player")]
	public class AnimationPlayer : MonoBehaviour, IAnimationNotifier
	{
		private const string _infiniteLoopingWarning = "(UAAPIL) Unable to wait on animation for {0}: the animation '{1}' was set to loop and would have never finished";

		private Animator _animator;
		private PlayableGraph _graph;
		private AnimationPlayableOutput _output;
		private AnimationClipPlayable _currentAnimation;
		private double _animationSpeed = 1.0;
		private bool _started = false;

		public bool IsDone
		{
			get
			{
				var done = _currentAnimation.IsDone();

				if (_started && done) // Reset started here since PlayAnimation cannot
					_started = false;

				return !_started || done;
			}
		}

		protected virtual void Awake()
		{
			_animator = GetComponent<Animator>();
			_animator.runtimeAnimatorController = null;

			_graph = PlayableGraph.Create();
			_output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);

			_graph.Play();
		}

		protected virtual void OnDestroy()
		{
			if (_graph.IsValid())
			{
				_graph.Stop();
				_graph.Destroy();
			}
		}

		public void PlayAnimation(AnimationClip animation)
		{
			_started = true;

			if (_currentAnimation.IsValid())
				_currentAnimation.Destroy();

			_currentAnimation = AnimationClipPlayable.Create(_graph, animation);
			_output.SetSourcePlayable(_currentAnimation);
			_currentAnimation.Play();

			if (!animation.isLooping)
				_currentAnimation.SetDuration(_currentAnimation.GetAnimationClip().length);
		}

		public IEnumerator PlayAnimationAndWait(AnimationClip animation)
		{
			PlayAnimation(animation);

			if (!animation.isLooping)
			{
				while (!IsDone)
					yield return null;
			}
			else
			{
				Debug.LogFormat(this, _infiniteLoopingWarning, name, animation.name);
			}
		}

		public void Pause()
		{
			_animationSpeed = _currentAnimation.GetSpeed();

			// Using _currentAnimation.Pause on a frame including an event will continue to trigger the event every
			// frame. Setting the speed to 0 doesn't have this issue however GetPlayState will not return Paused.
			_currentAnimation.SetSpeed(0.0f);
		}

		public void Unpause()
		{
			_currentAnimation.SetSpeed(_animationSpeed);
		}
	}
}
