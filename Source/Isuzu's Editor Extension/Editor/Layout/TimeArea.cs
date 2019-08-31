using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class TimeArea : ZoomableArea
    {
        public enum TimeRulerDragMode
        {
            None,
            Start,
            End,
            Dragging,
            Cancel
        }

        private static TimeAreaStyle styles;

        private static float originalTime;
        private static float pickOffset;


        public TimeArea()
        {
            float[] tickModulos =
            {
                0.0005f,
                0.001f,
                0.005f,
                0.01f,
                0.05f,
                0.1f,
                0.5f,
                1f,
                5f,
                10f,
                50f,
                100f,
                500f,
                1000f,
                5000f,
                10000f
            };
            this.Ticks = new TickHandler();
            this.Ticks.SetTickModulos(tickModulos);
        }

        internal TickHandler Ticks { get; set; }

        public float GetMajorTickDistance(float frameRate)
        {
            const float result = 0f;
            for (var i = 0; i < this.Ticks.TickLevels; i++)
                if (this.Ticks.GetStrengthOfLevel(i) > 0.5f)
                    return this.Ticks.GetPeriodOfLevel(i);

            return result;
        }


        public void DrawMajorTicks(Rect position, float frameRate)
        {
            var color = Handles.color;
            GUI.BeginGroup(position);
            if (Event.current.type != EventType.Repaint)
            {
                GUI.EndGroup();
                return;
            }

            InitStyles();
            this.SetTickMarkerRanges();
            this.Ticks.SetTickStrengths(3f, 80f, true);
            var textColor = styles.TimelineTick.normal.textColor;
            textColor.a = 0.3f;
            Handles.color = textColor;
            for (var i = 0; i < this.Ticks.TickLevels; i++)
            {
                var strengthOfLevel = this.Ticks.GetStrengthOfLevel(i);
                if (!(strengthOfLevel > 0.5f)) continue;
                var ticksAtLevel = this.Ticks.GetTicksAtLevel(i, true);
                foreach (var t in ticksAtLevel)
                {
                    if (!(t >= 0f)) continue;
                    var num = Mathf.RoundToInt(t * frameRate);
                    var num2 = this.FrameToPixel(num, frameRate, position);
                    Handles.DrawLine(new Vector3(num2, 0f, 0f), new Vector3(num2, position.height, 0f));
                    if (strengthOfLevel > 0.8f)
                        Handles.DrawLine(new Vector3(num2 + 1f, 0f, 0f), new Vector3(num2 + 1f, position.height, 0f));
                }
            }

            GUI.EndGroup();
            Handles.color = color;
        }


        private static string FormatFrame(int frame, float frameRate)
        {
            var num = (int) frameRate;
            var length = num.ToString().Length;
            var num2 = frame / num;
            var num3 = frame % frameRate;
            return string.Format("{0}:{1}", num2.ToString(), num3.ToString().PadLeft(length, '0'));
        }


        public float FrameToPixel(float i, float frameRate, Rect rect)
        {
            return (i - this.ShownArea.xMin * frameRate) * rect.width / (this.ShownArea.width * frameRate);
        }


        private static void InitStyles()
        {
            if (styles == null) styles = new TimeAreaStyle();
        }


        private void SetTickMarkerRanges()
        {
            this.Ticks.SetRanges(this.ShownArea.xMin, this.ShownArea.xMax, this.DrawRect.xMin, this.DrawRect.xMax);
        }


        public void TimeRuler(Rect position, float frameRate)
        {
            var color = Handles.color;
            GUI.BeginGroup(position);
            if (Event.current.type != EventType.Repaint)
            {
                GUI.EndGroup();
                return;
            }

            InitStyles();
            this.SetTickMarkerRanges();
            this.Ticks.SetTickStrengths(3f, 80f, true);
            var textColor = styles.TimelineTick.normal.textColor;
            textColor.a = 0.3f;
            Handles.color = textColor;
            for (var i = 0; i < this.Ticks.TickLevels; i++)
            {
                var strengthOfLevel = this.Ticks.GetStrengthOfLevel(i);
                if (strengthOfLevel > 0.2f)
                {
                    var ticksAtLevel = this.Ticks.GetTicksAtLevel(i, true);
                    foreach (var t in ticksAtLevel)
                        if (t >= this.HRangeMin && t <= this.HRangeMax)
                        {
                            var num = Mathf.RoundToInt(t * frameRate);
                            var num2 = position.height * Mathf.Min(1f, strengthOfLevel) * 0.7f;
                            var num3 = this.FrameToPixel(num, frameRate, position);
                            Handles.DrawLine(new Vector3(num3, position.height - num2 + 0.5f, 0f),
                                new Vector3(num3, position.height - 0.5f, 0f));
                            if (strengthOfLevel > 0.5f)
                                Handles.DrawLine(new Vector3(num3 + 1f, position.height - num2 + 0.5f, 0f),
                                    new Vector3(num3 + 1f, position.height - 0.5f, 0f));
                        }
                }
            }

            GL.End();
            var levelWithMinSeparation = this.Ticks.GetLevelWithMinSeparation(40f);
            var ticksAtLevel2 = this.Ticks.GetTicksAtLevel(levelWithMinSeparation, false);
            foreach (var t in ticksAtLevel2)
                if (t >= this.HRangeMin && t <= this.HRangeMax)
                {
                    var num4 = Mathf.RoundToInt(t * frameRate);
                    var num5 = Mathf.Floor(this.FrameToPixel(num4, frameRate, this.Rect));
                    var text = FormatFrame(num4, frameRate);
                    GUI.Label(new Rect(num5 + 3f, -3f, 40f, 20f), text, styles.TimelineTick);
                }

            GUI.EndGroup();
            Handles.color = color;
        }

        public static void DrawPlayhead(float x, float yMin, float yMax, float thickness, float alpha)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            InitStyles();
            var halfThickness = thickness * 0.5f;
            var lineColor = styles.playhead.normal.textColor * new Vector4(1, 1, 1, alpha);
            if (thickness > 1f)
            {
                var labelRect = Rect.MinMaxRect(x - halfThickness, yMin, x + halfThickness, yMax);
                EditorGUI.DrawRect(labelRect, lineColor);
            }
            else
            {
                DrawVerticalLine(x, yMin, yMax, lineColor);
            }
        }

        private static void DrawVerticalLine(float x, float minY, float maxY, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var backupCol = Handles.color;

//            HandleUtility.ApplyWireMaterial();
            GL.Begin(Application.platform == RuntimePlatform.WindowsEditor ? GL.QUADS : GL.LINES);
            DrawVerticalLineFast(x, minY, maxY, color);
            GL.End();

            Handles.color = backupCol;
        }

        private static void DrawVerticalLineFast(float x, float minY, float maxY, Color color)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                GL.Color(color);
                GL.Vertex(new Vector3(x - 0.5f, minY, 0));
                GL.Vertex(new Vector3(x + 0.5f, minY, 0));
                GL.Vertex(new Vector3(x + 0.5f, maxY, 0));
                GL.Vertex(new Vector3(x - 0.5f, maxY, 0));
            }
            else
            {
                GL.Color(color);
                GL.Vertex(new Vector3(x, minY, 0));
                GL.Vertex(new Vector3(x, maxY, 0));
            }
        }

        public TimeRulerDragMode BrowseRuler(Rect position, int id, ref float time, float frameRate, bool pickAnywhere,
            GUIStyle thumbStyle)
        {
            var evt = Event.current;
            var pickRect = position;
            if (time != -1)
            {
                pickRect.x = Mathf.Round(this.TimeToPixel(time, position)) - thumbStyle.overflow.left;
                pickRect.width = thumbStyle.fixedWidth + thumbStyle.overflow.horizontal;
            }

            switch (evt.GetTypeForControl(id))
            {
                case EventType.Repaint:
                    if (time != -1)
                    {
                        var hover = position.Contains(evt.mousePosition);
                        pickRect.x += thumbStyle.overflow.left;
                        thumbStyle.Draw(pickRect, id == GUIUtility.hotControl, hover || id == GUIUtility.hotControl,
                            false, false);
                    }

                    break;
                case EventType.MouseDown:
                    if (pickRect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        pickOffset = evt.mousePosition.x - this.TimeToPixel(time, position);
                        evt.Use();
                        return TimeRulerDragMode.Start;
                    }
                    else if (pickAnywhere && position.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;

                        var newT = SnapTimeToWholeFPS(this.PixelToTime(evt.mousePosition.x, position), frameRate);
                        originalTime = time;
                        if (newT != time)
                            GUI.changed = true;
                        time = newT;
                        pickOffset = 0;
                        evt.Use();
                        return TimeRulerDragMode.Start;
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        var newT = SnapTimeToWholeFPS(this.PixelToTime(evt.mousePosition.x - pickOffset, position),
                            frameRate);
                        if (newT != time)
                            GUI.changed = true;
                        time = newT;

                        evt.Use();
                        return TimeRulerDragMode.Dragging;
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        return TimeRulerDragMode.End;
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.hotControl == id && evt.keyCode == KeyCode.Escape)
                    {
                        if (time != originalTime)
                            GUI.changed = true;
                        time = originalTime;

                        GUIUtility.hotControl = 0;
                        evt.Use();
                        return TimeRulerDragMode.Cancel;
                    }

                    break;
            }

            return TimeRulerDragMode.None;
        }

        private static float SnapTimeToWholeFPS(float time, float frameRate)
        {
            if (frameRate == 0)
                return time;
            return Mathf.Round(time * frameRate) / frameRate;
        }

        private float PixelToTime(float pixelX, Rect rect)
        {
            return (pixelX - rect.x) * this.ShownArea.width / rect.width + this.ShownArea.x;
        }

        private float TimeToPixel(float time, Rect rect)
        {
            return (time - this.ShownArea.x) / this.ShownArea.width * rect.width + rect.x;
        }


        private class TimeAreaStyle
        {
            public readonly GUIStyle playhead = "AnimationPlayHead";
            public readonly GUIStyle TimelineTick = "AnimationTimelineTick";
            public GUIStyle labelTickMarks = "CurveEditorLabelTickMarks";
        }
    }
}