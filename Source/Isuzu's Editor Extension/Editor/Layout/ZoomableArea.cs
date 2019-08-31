using System;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal class ZoomableArea
    {
        private static Vector2 mouseDownPosition = new Vector2(-1000000f, -1000000f);

        private static readonly int ZoomableAreaHash = "ZoomableArea".GetHashCode();

        private readonly float hScaleMax;
        private readonly float hScaleMin;
        private readonly bool minimalGUI;
        private readonly Styles styles;

        private Rect drawArea;
        private int horizontalScrollbarID;
        private bool hSlider;
        private Rect lastShownAreaInsideMargins;
        private Vector2 scale;
        private Vector2 translation;

        protected ZoomableArea()
        {
            this.HRangeMin = float.NegativeInfinity;
            this.HRangeMax = float.PositiveInfinity;
            this.hScaleMin = 0.001f;
            this.hScaleMax = 100000f;
            this.hSlider = true;
            this.drawArea = new Rect(0f, 0f, 100f, 100f);
            this.scale = new Vector2(1f, -1f);
            this.translation = new Vector2(0f, 0f);
            this.lastShownAreaInsideMargins = new Rect(0f, 0f, 100f, 100f);
            this.minimalGUI = false;
            this.styles = new Styles();
        }

        public Vector2 Scale
        {
            get { return this.scale; }
            internal set { this.scale = value; }
        }

        internal Vector2 Translation
        {
            get { return this.translation; }
            set { this.translation = value; }
        }

        private float BottomMargin { get; set; }

        private Bounds DrawingBounds
        {
            get
            {
                var flag = this.HRangeMin > float.NegativeInfinity && this.HRangeMax < float.PositiveInfinity;
                return new Bounds(
                    new Vector3(!flag ? this.HScrollMax * 0.5f : (this.HRangeMin + this.HRangeMax) * 0.5f, 0f,
                        0f), new Vector3(!flag ? this.HScrollMax : this.HRangeMax - this.HRangeMin, 2f, 1f));
            }
        }

        internal Rect DrawRect
        {
            get { return this.drawArea; }
        }

        internal bool HRangeLocked { get; set; }

        internal float HRangeMax { get; set; }

        internal float HRangeMin { get; set; }

        internal bool HSlider
        {
            get { return this.hSlider; }
            set
            {
                var rect = this.Rect;
                this.hSlider = value;
                this.Rect = rect;
            }
        }

        internal bool IgnoreScrollWheelUntilClicked { get; set; }

        private float LeftMargin { get; set; }

        internal float Margin
        {
            set
            {
                this.BottomMargin = value;
                this.TopMargin = value;
                this.RightMargin = value;
                this.LeftMargin = value;
            }
        }

        private Vector2 MousePositionInDrawing
        {
            get { return this.ViewToDrawingTransformPoint(Event.current.mousePosition); }
        }

        internal Rect Rect
        {
            get
            {
                return new Rect(this.DrawRect.x, this.DrawRect.y, this.DrawRect.width,
                    this.DrawRect.height + (!this.hSlider ? 0f : this.styles.visualSliderWidth));
            }
            set
            {
                var rect = new Rect(value.x, value.y, value.width,
                    value.height - (!this.hSlider ? 0f : this.styles.visualSliderWidth));
                if (rect != this.drawArea)
                {
                    if (this.ScaleWithWindow)
                    {
                        this.drawArea = rect;
                        this.ShownAreaInsideMargins = this.lastShownAreaInsideMargins;
                    }
                    else
                    {
                        this.translation += new Vector2((rect.width - this.drawArea.width) / 2f, 0f);
                        this.drawArea = rect;
                    }
                }

                this.EnforceScaleAndRange();
            }
        }

        private float RightMargin { get; set; }


        internal bool ScaleWithWindow { get; set; }


        private float HScrollMax { get; set; }


        internal Rect ShownArea
        {
            get
            {
                return new Rect(-this.translation.x / this.scale.x,
                    -(this.translation.y - this.DrawRect.height) / this.scale.y,
                    this.DrawRect.width / this.scale.x, this.DrawRect.height / -this.scale.y);
            }
            set
            {
                this.scale.x = this.DrawRect.width / value.width;
                this.translation.x = -value.x * this.scale.x;
                this.translation.y = this.DrawRect.height - value.y * this.scale.y;
                this.EnforceScaleAndRange();
            }
        }


        private Rect ShownAreaInsideMargins
        {
            get { return this.ShownAreaInsideMarginsInternal; }
            set
            {
                this.ShownAreaInsideMarginsInternal = value;
                this.EnforceScaleAndRange();
            }
        }


        private Rect ShownAreaInsideMarginsInternal
        {
            get
            {
                var num = this.LeftMargin / this.scale.x;
                var num2 = this.RightMargin / this.scale.x;
                var num3 = this.TopMargin / this.scale.y;
                var num4 = this.BottomMargin / this.scale.y;
                var shownArea = this.ShownArea;
                shownArea.x += num;
                shownArea.y -= num3;
                shownArea.width -= num + num2;
                shownArea.height += num3 + num4;
                return shownArea;
            }
            set
            {
                this.scale.x = (this.DrawRect.width - this.LeftMargin - this.RightMargin) / value.width;
                this.translation.x = -value.x * this.scale.x + this.LeftMargin;
                this.translation.y = this.DrawRect.height - value.y * this.scale.y - this.TopMargin;
            }
        }


        private float TopMargin { get; set; }


        public void BeginViewGUI(bool handleUserInteraction)
        {
            if (this.styles.horizontalScrollbar == null) this.styles.InitGUIStyles();

            var drawArea = this.drawArea;
            drawArea.x = 0f;
            drawArea.y = 0f;
            GUILayout.BeginArea(this.DrawRect);
            if (handleUserInteraction)
            {
                var controlID = GUIUtility.GetControlID(ZoomableAreaHash, FocusType.Passive, drawArea);
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        if (drawArea.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.keyboardControl = controlID;
                            if (IsZoomEvent() || IsPanEvent())
                            {
                                GUIUtility.hotControl = controlID;
                                mouseDownPosition = this.MousePositionInDrawing;
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlID)
                        {
                            GUIUtility.hotControl = 0;
                            mouseDownPosition = new Vector2(-1000000f, -1000000f);
                        }

                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID)
                        {
                            if (IsZoomEvent())
                            {
                                this.Zoom(mouseDownPosition, false);
                                Event.current.Use();
                            }
                            else if (IsPanEvent())
                            {
                                this.Pan();
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.ScrollWheel:
                        if (drawArea.Contains(Event.current.mousePosition) && GUIUtility.keyboardControl == controlID &&
                            Event.current.control)
                        {
                            this.Zoom(this.MousePositionInDrawing, true);
                            Event.current.Use();
                        }

                        break;
                }
            }

            GUILayout.EndArea();
            this.horizontalScrollbarID =
                GUIUtility.GetControlID(MinMaxSliderControl.s_MinMaxSliderHash, FocusType.Passive);
            if (!this.minimalGUI || Event.current.type != EventType.Repaint) this.SliderGUI();
        }


        public void EndViewGUI()
        {
            if (this.minimalGUI && Event.current.type == EventType.Repaint) this.SliderGUI();
        }


        private void EnforceScaleAndRange()
        {
            var value = this.hScaleMax;
            if (this.HRangeMax != float.PositiveInfinity && this.HRangeMin != float.NegativeInfinity)
                value = Mathf.Min(this.hScaleMax, this.HRangeMax - this.HRangeMin);

            var lastShownAreaInsideMargins = this.lastShownAreaInsideMargins;
            var shownAreaInsideMargins = this.ShownAreaInsideMargins;
            if (shownAreaInsideMargins != lastShownAreaInsideMargins)
            {
                const float num = 1E-05f;
                if (shownAreaInsideMargins.width < lastShownAreaInsideMargins.width - num)
                {
                    var t = Mathf.InverseLerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width,
                        this.hScaleMin);
                    var x = Mathf.Lerp(lastShownAreaInsideMargins.x, shownAreaInsideMargins.x, t);
                    var width = Mathf.Lerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, t);
                    shownAreaInsideMargins =
                        new Rect(x, shownAreaInsideMargins.y, width, shownAreaInsideMargins.height);
                }

                if (shownAreaInsideMargins.height < lastShownAreaInsideMargins.height - num)
                {
                    var t2 = Mathf.InverseLerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, 1f);
                    var y = Mathf.Lerp(lastShownAreaInsideMargins.y, shownAreaInsideMargins.y, t2);
                    shownAreaInsideMargins = new Rect(shownAreaInsideMargins.x, y, shownAreaInsideMargins.width,
                        Mathf.Lerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, t2));
                }

                if (shownAreaInsideMargins.width > lastShownAreaInsideMargins.width + num)
                {
                    var t3 = Mathf.InverseLerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, value);
                    var x2 = Mathf.Lerp(lastShownAreaInsideMargins.x, shownAreaInsideMargins.x, t3);
                    var width2 = Mathf.Lerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, t3);
                    shownAreaInsideMargins =
                        new Rect(x2, shownAreaInsideMargins.y, width2, shownAreaInsideMargins.height);
                }

                if (shownAreaInsideMargins.height > lastShownAreaInsideMargins.height + num)
                {
                    var t4 = Mathf.InverseLerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, 1f);
                    var y2 = Mathf.Lerp(lastShownAreaInsideMargins.y, shownAreaInsideMargins.y, t4);
                    shownAreaInsideMargins = new Rect(shownAreaInsideMargins.x, y2, shownAreaInsideMargins.width,
                        Mathf.Lerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, t4));
                }

                if (shownAreaInsideMargins.xMin < this.HRangeMin) shownAreaInsideMargins.x = this.HRangeMin;

                if (shownAreaInsideMargins.xMax > this.HRangeMax)
                    shownAreaInsideMargins.x = this.HRangeMax - shownAreaInsideMargins.width;

                this.ShownAreaInsideMarginsInternal = shownAreaInsideMargins;
                this.lastShownAreaInsideMargins = shownAreaInsideMargins;
            }
        }

        private static bool IsPanEvent()
        {
            return Event.current.button == 0 && Event.current.alt ||
                   Event.current.button == 2 && !Event.current.command;
        }

        private static bool IsZoomEvent()
        {
            return Event.current.button == 1 && Event.current.alt;
        }

        private void Pan()
        {
            if (!this.HRangeLocked) this.translation.x += Event.current.delta.x;

            this.EnforceScaleAndRange();
        }


        internal void SetShownHRange(float min, float max)
        {
            this.scale.x = this.DrawRect.width / (max - min);
            this.translation.x = -min * this.scale.x;
            this.EnforceScaleAndRange();
        }

        internal void SetShownHRangeInsideMargins(float min, float max)
        {
            this.scale.x = (this.DrawRect.width - this.LeftMargin - this.RightMargin) / (max - min);
            this.translation.x = -min * this.scale.x + this.LeftMargin;
            this.EnforceScaleAndRange();
        }


        private void SliderGUI()
        {
            if (!this.hSlider) return;

            var drawingBounds = this.DrawingBounds;
            var shownAreaInsideMargins = this.ShownAreaInsideMargins;
            var num = this.styles.sliderWidth - this.styles.visualSliderWidth;
            var num2 = !this.HSlider ? 0f : num;
            if (!this.hSlider) return;

            var position = new Rect(this.DrawRect.x, this.DrawRect.yMax - num, this.DrawRect.width - num2,
                this.styles.sliderWidth);
            var width = shownAreaInsideMargins.width;
            var xMin = shownAreaInsideMargins.xMin;
            MinMaxSliderControl.MinMaxScroller(position, this.horizontalScrollbarID, ref xMin, ref width,
                drawingBounds.min.x, drawingBounds.max.x, float.NegativeInfinity, float.PositiveInfinity,
                this.styles.horizontalScrollbar, this.styles.horizontalMinMaxScrollbarThumb,
                this.styles.horizontalScrollbarLeftButton, this.styles.horizontalScrollbarRightButton, true);
            var num3 = xMin;
            var num4 = xMin + width;
            if (num3 > shownAreaInsideMargins.xMin) num3 = Mathf.Min(num3, num4 - this.hScaleMin);

            if (num4 < shownAreaInsideMargins.xMax) num4 = Mathf.Max(num4, num3 + this.hScaleMin);

            this.SetShownHRangeInsideMargins(num3, num4);
        }


        internal float TimeToPixel(float time, Rect rect)
        {
            var shownArea = this.ShownArea;
            return (time - shownArea.x) / shownArea.width * rect.width + rect.x;
        }


        internal float TimeToPixel(float time)
        {
            var shownArea = this.ShownArea;
            return (time - shownArea.x) / shownArea.width * this.drawArea.width + this.drawArea.x;
        }

        public float PixelDeltaToTime(Rect rect)
        {
            return this.ShownArea.width / rect.width;
        }

        private Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
        {
            return new Vector2((lhs.x - this.translation.x) / this.scale.x,
                (lhs.y - this.translation.y) / this.scale.y);
        }


        internal Vector2 ViewToDrawingTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x / this.scale.x, lhs.y / this.scale.y);
        }


        internal Vector2 DrawingToViewTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x * this.scale.x, lhs.y * this.scale.y);
        }


        internal Vector2 DrawingToViewTransformPoint(Vector2 lhs)
        {
            return new Vector2(lhs.x * this.scale.x + this.translation.x,
                lhs.y * this.scale.y + this.translation.y);
        }


        private void Zoom(Vector2 zoomAround, bool scrollWhell)
        {
            var num = Event.current.delta.x + Event.current.delta.y;
            if (scrollWhell) num = -num;

            var num2 = Mathf.Max(0.01f, 1f + num * 0.01f);
            if (!this.HRangeLocked)
            {
                this.translation.x -= zoomAround.x * (num2 - 1f) * this.scale.x;
                this.scale.x *= num2;
            }

            this.EnforceScaleAndRange();
        }


        [Serializable]
        internal class Styles
        {
            public GUIStyle horizontalMinMaxScrollbarThumb;


            public GUIStyle horizontalScrollbar;


            public GUIStyle horizontalScrollbarLeftButton;


            public GUIStyle horizontalScrollbarRightButton;


            public float sliderWidth;


            public float visualSliderWidth;

            public Styles()
            {
                this.visualSliderWidth = 15f;
                this.sliderWidth = 15f;
            }


            public void InitGUIStyles()
            {
                this.horizontalMinMaxScrollbarThumb = "horizontalMinMaxScrollbarThumb";
                this.horizontalScrollbarLeftButton = "horizontalScrollbarLeftbutton";
                this.horizontalScrollbarRightButton = "horizontalScrollbarRightbutton";
                this.horizontalScrollbar = GUI.skin.horizontalScrollbar;
            }
        }
    }
}