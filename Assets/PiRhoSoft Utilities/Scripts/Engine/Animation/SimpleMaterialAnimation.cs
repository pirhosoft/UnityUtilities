using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Renderer))]
	[AddComponentMenu("PiRhoSoft Utility/Animation/Simple Material Animation")]
	public class SimpleMaterialAnimation : MonoBehaviour, IAnimationNotifier
	{
		private static readonly int _progressId = Shader.PropertyToID("_Progress");

		[Tooltip("The duration of the animation (in seconds)")]
		public float Duration = 2.0f;

		public bool IsDone => _progress >= Duration;

		private Renderer _renderer;
		private MaterialPropertyBlock _propertyBlock;

		private float _progress = 0.0f;

		void OnEnable()
		{
			_progress = 0.0f;
			_renderer = GetComponent<Renderer>();
			_propertyBlock = new MaterialPropertyBlock();
		}

		void OnDisable()
		{
			_propertyBlock = null;
			_renderer = null;
		}
		
		void LateUpdate()
		{
			_progress += Time.deltaTime;

			var progress = Mathf.Clamp01(_progress / Duration);

			_renderer.GetPropertyBlock(_propertyBlock);
			_renderer.material.SetFloat(_progressId, progress);
			_renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}
