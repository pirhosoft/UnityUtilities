using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Renderer))]
	public class MaterialAnimation : MonoBehaviour
	{
		private static readonly int _progressId = Shader.PropertyToID("_Progress");

		public float Progress = 0.0f;

		private Renderer _renderer;
		private MaterialPropertyBlock _propertyBlock;

		void OnEnable()
		{
			_renderer = GetComponent<Renderer>();
			_propertyBlock = new MaterialPropertyBlock();
		}

		void OnDisable()
		{
			_renderer = null;
			_propertyBlock = null;
		}

		protected virtual void LateUpdate()
		{
			_renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetFloat(_progressId, Progress);
			_renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}
