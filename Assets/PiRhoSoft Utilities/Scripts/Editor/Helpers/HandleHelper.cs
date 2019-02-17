using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public static class HandleHelper
	{
		private const float _handleSize = 0.1f;

		private static int _handleHash = "HandleHash".GetHashCode();
		private static Vector2[] _pointsCache = new Vector2[5];
		private static Vector2 _currentMousePosition;
		private static Vector2 _startMousePosition;
		private static Vector2 _startPosition;

		private static Color _outline;
		private static Color _fill;

		private static GUIStyle _textStyle = new GUIStyle();

		public static void DrawText(Vector2 position, string text, TextAnchor alignment, Color color)
		{
			using (new HandleColorScope(color))
			{
				_textStyle.fontStyle = FontStyle.Bold;
				_textStyle.alignment = alignment;
				_textStyle.normal.textColor = color;

				Handles.Label(position, text, _textStyle);
			}
		}

		public static void DrawArrow(Vector2 position, Vector2 direction, float length, Color color)
		{
			using (new HandleColorScope(color))
			{
				var controlID = GUIUtility.GetControlID(_handleHash, FocusType.Passive);
				Handles.ArrowHandleCap(controlID, position, Quaternion.LookRotation(direction), length, EventType.Repaint);
			}
		}

		public static void DrawLine(Vector2 start, Vector2 end, Color color)
		{
			using (new HandleColorScope(color))
				Handles.DrawLine(start, end);
		}

		public static void DrawCircle(Vector2 position, float radius, Color color)
		{
			using (new HandleColorScope(color))
				Handles.DrawSolidDisc(position, Vector3.back, radius);
		}

		public static void DrawBezier(Vector2 start, Vector2 end, Color color)
		{
			var direction = end - start;
			var dirFactor = Mathf.Clamp(direction.magnitude, 20.0f, 80.0f);

			direction.Normalize();
			var project = Vector3.Project(direction, Vector3.right);

			var startTan = start + (Vector2)project * dirFactor;
			var endTan = end - (Vector2)project * dirFactor;

			Handles.DrawBezier(start, end, startTan, endTan, color, null, 3.0f);
		}

		public static Rect BoundsHandle(Rect bounds, Vector2 snap, Color rectangleOutline, Color rectangleFill, Color circleOutline, Color circleFill, float handleSize = _handleSize)
		{
			var selectedPosition = MoveHandle(bounds.center, bounds.size, snap, rectangleOutline, rectangleFill);
			var position = selectedPosition - bounds.center;

			bounds = ScaleHandles(bounds, snap, circleOutline, circleFill, handleSize);
			bounds.position += position;

			return bounds;
		}

		public static Vector2 MoveHandle(Vector2 position, Vector2 size, Vector2 snap, Color outline, Color fill)
		{
			var controlID = GUIUtility.GetControlID(_handleHash, FocusType.Passive);
			return RectangleHandle(controlID, position, size, snap, fill, outline);
		}

		public static Vector2 MoveHandle(Vector2 position, Vector2 snap, Color outline, Color fill, float handleSize = _handleSize)
		{
			using (new HandleColorScope())
			{
				_outline = outline;
				_fill = fill;

				return Handles.FreeMoveHandle(position, Quaternion.identity, handleSize, snap, CircleHandle);
			}
		}

		public static Rect ScaleHandles(Rect bounds, Vector2 snap, Color outline, Color fill, float handleSize = _handleSize)
		{
			var size = HandleUtility.GetHandleSize(bounds.center) * handleSize;

			var leftPosition = new Vector2(bounds.xMin, bounds.center.y);
			var rightPosition = new Vector2(bounds.xMax, bounds.center.y);
			var topPosition = new Vector2(bounds.center.x, bounds.yMax);
			var bottomPosition = new Vector2(bounds.center.x, bounds.yMin);
			var topLeftPosition = new Vector2(bounds.xMin, bounds.yMax);
			var topRightPosition = new Vector2(bounds.xMax, bounds.yMax);
			var bottomLeftPosition = new Vector2(bounds.xMin, bounds.yMin);
			var bottomRightPosition = new Vector2(bounds.xMax, bounds.yMin);

			var selectedLeftPosition = MoveHandle(leftPosition, snap, outline, fill);
			var selectedRightPosition = MoveHandle(rightPosition, snap, outline, fill);
			var selectedTopPosition = MoveHandle(topPosition, snap, outline, fill);
			var selectedBottomPosition = MoveHandle(bottomPosition, snap, outline, fill);
			var selectedTopLeftPosition = MoveHandle(topLeftPosition, snap, outline, fill);
			var selectedTopRightPosition = MoveHandle(topRightPosition, snap, outline, fill);
			var selectedBottomLeftPosition = MoveHandle(bottomLeftPosition, snap, outline, fill);
			var selectedBottomRightPosition = MoveHandle(bottomRightPosition, snap, outline, fill);

			var left = selectedLeftPosition.x - leftPosition.x;
			var right = selectedRightPosition.x - rightPosition.x;
			var top = selectedTopPosition.y - topPosition.y;
			var bottom = selectedBottomPosition.y - bottomPosition.y;
			var topLeft = selectedTopLeftPosition - topLeftPosition;
			var topRight = selectedTopRightPosition - topRightPosition;
			var bottomLeft = selectedBottomLeftPosition - bottomLeftPosition;
			var bottomRight = selectedBottomRightPosition - bottomRightPosition;

			bounds.xMin += left + topLeft.x + bottomLeft.x;
			bounds.xMax += right + topRight.x + bottomRight.x;
			bounds.yMin += bottom + bottomLeft.y + bottomRight.y;
			bounds.yMax += top + topLeft.y + topRight.y;

			bounds.xMin = Mathf.Min(bounds.xMin, rightPosition.x);
			bounds.xMax = Mathf.Max(bounds.xMax, leftPosition.x);
			bounds.yMin = Mathf.Min(bounds.yMin, topPosition.y);
			bounds.yMax = Mathf.Max(bounds.yMax, bottomPosition.y);

			return bounds;
		}

		private static void CircleHandle(int id, Vector3 position, Quaternion rotation, float size, EventType eventType)
		{
			Handles.color = _fill;
			Handles.DrawSolidDisc(position, Vector3.back, size);
			Handles.color = _outline;
			Handles.DrawWireDisc(position, Vector3.back, size);
			Handles.CircleHandleCap(id, position, rotation, size, eventType);
		}

		private static Vector2 RectangleHandle(int id, Vector2 position, Vector2 size, Vector3 snap, Color fill, Color outline)
		{
			var current = Event.current;
			var area = new Rect(position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y);

			var min = HandleUtility.WorldToGUIPoint(area.min);
			var max = HandleUtility.WorldToGUIPoint(area.max);

			switch (current.GetTypeForControl(id))
			{
				case EventType.Layout:
				{
					HandleUtility.AddControl(id, DistanceToRectangle(position, Camera.current.transform.rotation, size * 0.5f));
					break;
				}
				case EventType.MouseDown:
				{
					if (HandleUtility.nearestControl == id && current.button == 0)
					{
						GUIUtility.hotControl = id;
						_currentMousePosition = (_startMousePosition = current.mousePosition);
						_startPosition = position;
						current.Use();
						EditorGUIUtility.SetWantsMouseJumping(1);
					}

					break;
				}
				case EventType.MouseDrag:
				{
					if (GUIUtility.hotControl == id)
					{
						var a = _currentMousePosition;
						var delta = current.delta;
						var x = delta.x;
						var a2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(_startPosition));

						_currentMousePosition = a + new Vector2(x, 0f - current.delta.y) * EditorGUIUtility.pixelsPerPoint;
						a2 += (Vector3)(_currentMousePosition - _startMousePosition);
						position = Handles.inverseMatrix.MultiplyPoint(Camera.current.ScreenToWorldPoint(a2));

						var b = position - _startPosition;
						b.x = MathHelper.Snap(b.x, snap.x);
						b.y = MathHelper.Snap(b.y, snap.y);
						position = _startPosition + b;

						GUI.changed = true;
						current.Use();
					}

					break;
				}
				case EventType.MouseUp:
				{
					if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
					{
						GUIUtility.hotControl = 0;
						current.Use();
						EditorGUIUtility.SetWantsMouseJumping(0);
					}

					break;
				}
				case EventType.MouseMove:
				{
					if (id == HandleUtility.nearestControl)
					{
						HandleUtility.Repaint();
					}

					break;
				}
				case EventType.Repaint:
				{
					if (id == GUIUtility.hotControl)
						outline = Handles.selectedColor;
					else if (id == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
						outline = Handles.preselectionColor;

					Handles.DrawSolidRectangleWithOutline(area, fill, outline);

					break;
				}
			}

			return position;
		}

		private static float DistanceToRectangle(Vector2 position, Quaternion rotation, Vector2 size)
		{
			var b = rotation * new Vector2(size.x, 0f);
			var b2 = rotation * new Vector2(0f, size.y);
			var mousePosition = Event.current.mousePosition;

			_pointsCache[0] = HandleUtility.WorldToGUIPoint((Vector3)position + b + b2);
			_pointsCache[1] = HandleUtility.WorldToGUIPoint((Vector3)position + b - b2);
			_pointsCache[2] = HandleUtility.WorldToGUIPoint((Vector3)position - b - b2);
			_pointsCache[3] = HandleUtility.WorldToGUIPoint((Vector3)position - b + b2);
			_pointsCache[4] = _pointsCache[0];

			var flag = false;
			var num = 4;

			for (var i = 0; i < 5; i++)
			{
				if (_pointsCache[i].y > mousePosition.y != _pointsCache[num].y > mousePosition.y && mousePosition.x < (_pointsCache[num].x - _pointsCache[i].x) * (mousePosition.y - _pointsCache[i].y) / (_pointsCache[num].y - _pointsCache[i].y) + _pointsCache[i].x)
					flag = !flag;

				num = i;
			}

			if (!flag)
			{
				var num2 = -1f;
				num = 1;

				for (var j = 0; j < 4; j++)
				{
					var num4 = HandleUtility.DistancePointToLineSegment(mousePosition, _pointsCache[j], _pointsCache[num++]);
					if (num4 < num2 || num2 < 0f)
						num2 = num4;
				}

				return num2;
			}

			return 0f;
		}
	}
}
