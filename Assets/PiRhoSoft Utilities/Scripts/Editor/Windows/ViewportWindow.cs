using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class ViewportWindow : EditorWindow
	{
		public float ScrollWheelZoomAmount = 0.1f;
		public float MinimumZoom = 0.001f;
		public float MaximumZoom = 4.0f;

		public float ZoomAmount { get; private set; } = 1.0f;
		public Rect ViewArea { get; private set; }
		public Matrix4x4 ViewMatrix { get; private set; }

		private InputManager _input;
		private Rect _previousPosition;

		void OnEnable()
		{
			_input = new InputManager();
			GoTo(ViewArea.center, ZoomAmount);
			Setup(_input);
		}

		void OnDisable()
		{
			Teardown();
			_input = null;
		}

		void OnGUI()
		{
			if (_previousPosition != position)
			{
				// when the window resizes recompute the view matrix
				GoTo(ViewArea.center, ZoomAmount);
				_previousPosition = position;
			}

			PreDraw(position);

			var transform = ReplaceTransform();
			var clipping = ReplaceClipping();

			Draw(ViewArea);
			Process();

			RestoreClipping(clipping);
			RestoreTransform(transform);

			PostDraw(position);
		}

		protected virtual void Setup(InputManager input)
		{
			UseScrollWheelToZoom();
			UseMouseDragToPan(true, true, true);
		}

		protected virtual void Teardown()
		{
		}

		protected virtual void Process()
		{
			_input.Update();
		}

		protected virtual void PreDraw(Rect rect) { }
		protected abstract void Draw(Rect rect);
		protected virtual void PostDraw(Rect rect) { }

		#region Input

		protected void UseScrollWheelToZoom()
		{
			_input.Create<InputManager.EventTrigger>()
				.SetEvent(EventType.ScrollWheel)
				.AddAction(() =>
				{
					var change = Mathf.Sign(Event.current.delta.y) * -ScrollWheelZoomAmount;
					Zoom(change, Event.current.mousePosition);
				});
		}

		protected void UseMouseDragToPan(bool altLeftClick, bool middleClick, bool rightClick)
		{
			if (altLeftClick)
			{
				_input.Create<InputManager.MouseTrigger>()
					.SetEvent(EventType.MouseDrag, InputManager.MouseButton.Left, false, false, true)
					.AddAction(() => Pan(Event.current.delta));
			}

			if (middleClick)
			{
				_input.Create<InputManager.MouseTrigger>()
					.SetEvent(EventType.MouseDrag, InputManager.MouseButton.Middle)
					.AddAction(() => Pan(Event.current.delta));
			}

			if (rightClick)
			{
				// when using a context menu the drag event triggers when right clicking in two different locations
				// so the first drag event is ignored

				var canDrag = false;

				_input.Create<InputManager.MouseTrigger>()
					.SetEvent(EventType.MouseDown, InputManager.MouseButton.Right)
					.AddAction(() => canDrag = false);

				_input.Create<InputManager.MouseTrigger>()
					.SetEvent(EventType.MouseDrag, InputManager.MouseButton.Right)
					.AddAction(() =>
					{
						if (canDrag)
							Pan(Event.current.delta);

						canDrag = true;
					});
			}
		}

		#endregion

		#region Drawing

		protected void DrawOffsetBackground(Rect rect, Texture2D texture)
		{
			// to keep the vertical grid lines aligned with the snap positions, the v coordinate needs to be adjusted
			// since window coordinates have positive down and texture coordinates have positive up
			var yAdjustment = Mathf.Repeat(rect.height, texture.height);

			var xOffset = rect.x / texture.width;
			var yOffset = (-rect.y - yAdjustment) / texture.height;

			var tileX = rect.size.x / texture.width;
			var tileY = rect.size.y / texture.height;

			// the grid is drawn in window space rather than viewport space so it needs to be offset by the viewport
			// position
			GUI.DrawTextureWithTexCoords(rect, texture, new Rect(new Vector2(xOffset, yOffset), new Vector2(tileX, tileY)));
		}

		#endregion

		#region Viewport

		public Vector2 WindowToViewport(Vector2 GraphPosition)
		{
			var viewportPosition = ViewMatrix.inverse.MultiplyPoint(GraphPosition);
			return new Vector2(viewportPosition.x, viewportPosition.y) + ViewArea.min;
		}

		public Vector2 ViewportToWindow(Vector2 viewportPosition)
		{
			var GraphPosition = ViewMatrix.MultiplyPoint(viewportPosition - ViewArea.min);
			return new Vector2(GraphPosition.x, GraphPosition.y);
		}

		public void GoTo(Vector2 location, float zoom)
		{
			ZoomAmount = Mathf.Clamp(zoom, MinimumZoom, MaximumZoom);

			var size = position.size / ZoomAmount;

			ViewMatrix = Matrix4x4.Scale(new Vector3(ZoomAmount, ZoomAmount, 1.0f));
			ViewArea = new Rect(location.x - size.x * 0.5f, location.y - size.y * 0.5f, size.x, size.y);

			Repaint();
		}

		public void Reset()
		{
			GoTo(Vector2.zero, 1.0f);
		}

		public void Show(Vector2 location)
		{
			GoTo(location, ZoomAmount);
		}

		public void ShowAll(Rect rect, RectOffset margin)
		{
			// margins are in window space since they are most likely used to account for toolbars and the like

			var scale = Mathf.Min((position.width - margin.left - margin.right) / rect.width, (position.height - margin.top - margin.bottom) / rect.height);

			rect.x += (margin.right - margin.left) * 0.5f / scale;
			rect.y += (margin.bottom - margin.top) * 0.5f / scale;

			GoTo(rect.center, Math.Min(scale, 1.0f));
		}

		public void Pan(Vector2 screenAmount)
		{
			GoTo(ViewArea.center - screenAmount, ZoomAmount);
		}

		public void Zoom(float amount, Vector2 centerPoint)
		{
			// since this is generally used from places that call it with a fixed amount (e.g a scroll wheel), instead
			// of clamping, ignore the change if it would cause it to escape the bounds thus keeping the zoom amount at
			// consistent increments

			var zoom = ZoomAmount + amount;

			if (zoom >= MinimumZoom && zoom <= MaximumZoom)
			{
				var offset = (centerPoint - ViewArea.center) * (1.0f - (ZoomAmount / zoom));

				// expanded version of the above:
				//var oldPoint = (centerPoint - ViewArea.center) * ZoomAmount;
				//var newPoint = (oldPoint / zoom) + ViewArea.center;
				//var offset = centerPoint - newPoint;

				GoTo(ViewArea.center + offset, zoom);
			}
		}

		#endregion

		#region GUI Transforming

		private Matrix4x4 ReplaceTransform()
		{
			var matrix = GUI.matrix;
			GUI.matrix = ViewMatrix;
			return matrix;
		}

		private void RestoreTransform(Matrix4x4 transform)
		{
			GUI.matrix = transform;
		}

		private Rect ReplaceClipping()
		{
			// the gui matrix applies to the clipping rectangle for the window so it needs to be removed
			// and reapplied with an adjusted rect that takes the zoom and panning into account

			// the concept for this implementation comes from the GUIScaleUtility in the MIT licensed Node Editor
			// Framework: http://www.levingaeher.com/NodeEditor/

			// https://github.com/Seneral/Node_Editor_Framework/blob/develop/Node_Editor/Utilities/GUI/GUIScaleUtility.cs

			var clipping = GUIClip.GetTopRect();
			var inverse = GUI.matrix.inverse;
			var min = inverse.MultiplyPoint(clipping.min);
			var max = inverse.MultiplyPoint(clipping.max);

			// these are not documented but are public - using BeginGroup/EndGroup, which is documented, does not
			// expose the scrollOffset parameter which is necessary to work around the not entirely clear wierdness
			// that Unity does with draw positioning

			GUI.EndClip();
			GUI.BeginClip(new Rect(min, max - min), -ViewArea.min, Vector2.zero, false);

			return clipping;
		}

		private void RestoreClipping(Rect clipping)
		{
			GUI.EndClip();
			GUI.BeginClip(clipping);
		}

		#endregion
	}
}
