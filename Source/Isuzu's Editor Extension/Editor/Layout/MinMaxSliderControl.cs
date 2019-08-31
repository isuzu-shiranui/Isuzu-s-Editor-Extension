using System;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class MinMaxSliderControl
    {
        private const int FirstScrollWait = 250;


        private const int kFirstScrollWait = 250;


        private const int kScrollWait = 30;


        private static float nextScrollStepTime;


        private static readonly int repeatButtonHash = "repeatButton".GetHashCode();


        internal static readonly int s_MinMaxSliderHash = "MinMaxSlider".GetHashCode();


        private static MinMaxSliderState s_MinMaxSliderState;


        private static DateTime s_NextScrollStepTime = DateTime.Now;


        private static int scrollControlID;


        private static readonly int scrollWait = 30;

        private static void DoMinMaxSlider(Rect position, int id, ref float value, ref float size, float visualStart,
            float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
        {
            var current = Event.current;
            var flag = size == 0f;
            var num = Mathf.Min(visualStart, visualEnd);
            var num2 = Mathf.Max(visualStart, visualEnd);
            var num3 = Mathf.Min(startLimit, endLimit);
            var num4 = Mathf.Max(startLimit, endLimit);
            var minMaxSliderState = s_MinMaxSliderState;
            if (GUIUtility.hotControl == id && minMaxSliderState != null)
            {
                num = minMaxSliderState.dragStartLimit;
                num3 = minMaxSliderState.dragStartLimit;
                num2 = minMaxSliderState.dragEndLimit;
                num4 = minMaxSliderState.dragEndLimit;
            }

            const float num5 = 0f;
            var num6 = Mathf.Clamp(value, num, num2);
            var num7 = Mathf.Clamp(value + size, num, num2) - num6;
            var num8 = visualStart <= visualEnd ? 1f : -1f;
            if (slider == null || thumb == null) return;

            float num10;
            Rect position2;
            Rect rect;
            Rect rect2;
            float num11;
            if (horiz)
            {
                var num9 = thumb.fixedWidth == 0f ? thumb.padding.horizontal : thumb.fixedWidth;
                num10 = (position.width - slider.padding.horizontal - num9) / (num2 - num);
                position2 = new Rect((num6 - num) * num10 + position.x + slider.padding.left,
                    position.y + slider.padding.top, num7 * num10 + num9,
                    position.height - slider.padding.vertical);
                rect = new Rect(position2.x, position2.y, thumb.padding.left, position2.height);
                rect2 = new Rect(position2.xMax - thumb.padding.right, position2.y, thumb.padding.right,
                    position2.height);
                num11 = current.mousePosition.x - position.x;
            }
            else
            {
                var num12 = thumb.fixedHeight == 0f ? thumb.padding.vertical : thumb.fixedHeight;
                num10 = (position.height - slider.padding.vertical - num12) / (num2 - num);
                position2 = new Rect(position.x + slider.padding.left,
                    (num6 - num) * num10 + position.y + slider.padding.top,
                    position.width - slider.padding.horizontal, num7 * num10 + num12);
                rect = new Rect(position2.x, position2.y, position2.width, thumb.padding.top);
                rect2 = new Rect(position2.x, position2.yMax - thumb.padding.bottom, position2.width,
                    thumb.padding.bottom);
                num11 = current.mousePosition.y - position.y;
            }

            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (!position.Contains(current.mousePosition) || num - num2 == 0f) return;

                    if (minMaxSliderState == null)
                        minMaxSliderState = s_MinMaxSliderState =
                            new MinMaxSliderState();

                    if (position2.Contains(current.mousePosition))
                    {
                        minMaxSliderState.dragStartPos = num11;
                        minMaxSliderState.dragStartValue = value;
                        minMaxSliderState.dragStartSize = size;
                        minMaxSliderState.dragStartValuesPerPixel = num10;
                        minMaxSliderState.dragStartLimit = startLimit;
                        minMaxSliderState.dragEndLimit = endLimit;
                        if (rect.Contains(current.mousePosition))
                            minMaxSliderState.whereWeDrag = 1;
                        else if (rect2.Contains(current.mousePosition))
                            minMaxSliderState.whereWeDrag = 2;
                        else
                            minMaxSliderState.whereWeDrag = 0;

                        GUIUtility.hotControl = id;
                        current.Use();
                        return;
                    }

                    if (slider == GUIStyle.none) return;
                    if (size != 0f && flag)
                    {
                        if (horiz)
                        {
                            if (num11 > position2.xMax - position.x)
                                value += size * num8 * 0.9f;
                            else
                                value -= size * num8 * 0.9f;
                        }
                        else if (num11 > position2.yMax - position.y)
                        {
                            value += size * num8 * 0.9f;
                        }
                        else
                        {
                            value -= size * num8 * 0.9f;
                        }

                        minMaxSliderState.whereWeDrag = 0;
                        GUI.changed = true;
                        s_NextScrollStepTime =
                            DateTime.Now.AddMilliseconds(kFirstScrollWait);
                        var num13 = !horiz ? current.mousePosition.y : current.mousePosition.x;
                        var num14 = !horiz ? position2.y : position2.x;
                        minMaxSliderState.whereWeDrag = num13 <= num14 ? 3 : 4;
                    }
                    else
                    {
                        if (horiz)
                            value = (num11 - position2.width * 0.5f) / num10 + num - size * 0.5f;
                        else
                            value = (num11 - position2.height * 0.5f) / num10 + num - size * 0.5f;

                        minMaxSliderState.dragStartPos = num11;
                        minMaxSliderState.dragStartValue = value;
                        minMaxSliderState.dragStartSize = size;
                        minMaxSliderState.whereWeDrag = 0;
                        GUI.changed = true;
                    }

                    GUIUtility.hotControl = id;
                    value = Mathf.Clamp(value, num3, num4 - size);
                    current.Use();

                    return;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != id) return;
                    current.Use();
                    GUIUtility.hotControl = 0;

                    return;
                case EventType.MouseMove:
                case EventType.KeyDown:
                case EventType.KeyUp:
                case EventType.ScrollWheel:
                    return;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != id) return;
                    var num15 = (num11 - minMaxSliderState.dragStartPos) /
                                minMaxSliderState.dragStartValuesPerPixel;
                    switch (minMaxSliderState.whereWeDrag)
                    {
                        case 0:
                            value = Mathf.Clamp(minMaxSliderState.dragStartValue + num15, num3, num4 - size);
                            break;
                        case 1:
                            value = minMaxSliderState.dragStartValue + num15;
                            size = minMaxSliderState.dragStartSize - num15;
                            if (value < num3)
                            {
                                size -= num3 - value;
                                value = num3;
                            }

                            if (size < num5)
                            {
                                value -= num5 - size;
                                size = num5;
                            }

                            break;
                        case 2:
                            size = minMaxSliderState.dragStartSize + num15;
                            if (value + size > num4) size = num4 - value;

                            if (size < num5) size = num5;

                            break;
                    }

                    GUI.changed = true;
                    current.Use();
                    return;

                case EventType.Repaint:
                    slider.Draw(position, GUIContent.none, id);
                    thumb.Draw(position2, GUIContent.none, id);
                    if (GUIUtility.hotControl != id || !position.Contains(current.mousePosition) ||
                        num - num2 == 0f) return;

                    if (position2.Contains(current.mousePosition))
                    {
                        if (minMaxSliderState != null &&
                            (minMaxSliderState.whereWeDrag == 3 || minMaxSliderState.whereWeDrag == 4))
                            GUIUtility.hotControl = 0;

                        return;
                    }

                    if (DateTime.Now >= s_NextScrollStepTime)
                    {
                        var num13 = !horiz ? current.mousePosition.y : current.mousePosition.x;
                        var num14 = !horiz ? position2.y : position2.x;
                        if (minMaxSliderState != null &&
                            (num13 <= num14 ? 3 : 4) != minMaxSliderState.whereWeDrag) return;

                        if (size != 0f && flag)
                        {
                            if (horiz)
                            {
                                if (num11 > position2.xMax - position.x)
                                    value += size * num8 * 0.9f;
                                else
                                    value -= size * num8 * 0.9f;
                            }
                            else if (num11 > position2.yMax - position.y)
                            {
                                value += size * num8 * 0.9f;
                            }
                            else
                            {
                                value -= size * num8 * 0.9f;
                            }

                            if (minMaxSliderState != null) minMaxSliderState.whereWeDrag = -1;
                            GUI.changed = true;
                        }

                        value = Mathf.Clamp(value, num3, num4 - size);
                        s_NextScrollStepTime =
                            DateTime.Now.AddMilliseconds(kScrollWait);
                    }

                    return;
                default:
                    return;
            }
        }


        private static bool DoRepeatButton(Rect position, GUIContent content, GUIStyle style, FocusType focusType)
        {
            var controlID = GUIUtility.GetControlID(repeatButtonHash, focusType, position);
            var typeForControl = Event.current.GetTypeForControl(controlID);
            if (typeForControl == EventType.MouseDown)
            {
                if (!position.Contains(Event.current.mousePosition)) return false;
                GUIUtility.hotControl = controlID;
                Event.current.Use();

                return false;
            }

            if (typeForControl != EventType.MouseUp)
            {
                if (typeForControl != EventType.Repaint) return false;

                style.Draw(position, content, controlID);
                return controlID == GUIUtility.hotControl && position.Contains(Event.current.mousePosition);
            }

            if (GUIUtility.hotControl != controlID) return false;
            GUIUtility.hotControl = 0;
            Event.current.Use();
            return position.Contains(Event.current.mousePosition);
        }


        public static void MinMaxScroller(Rect position, int id, ref float value, ref float size, float visualStart,
            float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton,
            GUIStyle rightButton, bool horiz)
        {
            float num;
            if (horiz)
                num = size * 10f / position.width;
            else
                num = size * 10f / position.height;

            Rect position2;
            Rect rect;
            Rect rect2;
            if (horiz)
            {
                position2 = new Rect(position.x + leftButton.fixedWidth, position.y,
                    position.width - leftButton.fixedWidth - rightButton.fixedWidth, position.height);
                rect = new Rect(position.x, position.y, leftButton.fixedWidth, position.height);
                rect2 = new Rect(position.xMax - rightButton.fixedWidth, position.y, rightButton.fixedWidth,
                    position.height);
            }
            else
            {
                position2 = new Rect(position.x, position.y + leftButton.fixedHeight, position.width,
                    position.height - leftButton.fixedHeight - rightButton.fixedHeight);
                rect = new Rect(position.x, position.y, position.width, leftButton.fixedHeight);
                rect2 = new Rect(position.x, position.yMax - rightButton.fixedHeight, position.width,
                    rightButton.fixedHeight);
            }

            var num2 = Mathf.Min(visualStart, value);
            var num3 = Mathf.Max(visualEnd, value + size);
            MinMaxSlider(position2, ref value, ref size, num2, num3, num2, num3, slider, thumb,
                horiz);
            var flag = Event.current.type == EventType.MouseUp;

            if (ScrollerRepeatButton(id, rect, leftButton)) value -= num * (visualStart >= visualEnd ? -1f : 1f);

            if (ScrollerRepeatButton(id, rect2, rightButton)) value += num * (visualStart >= visualEnd ? -1f : 1f);

            if (flag && Event.current.type == EventType.Used) scrollControlID = 0;

            if (startLimit < endLimit)
            {
                value = Mathf.Clamp(value, startLimit, endLimit - size);
                return;
            }

            value = Mathf.Clamp(value, endLimit, startLimit - size);
        }


        public static void MinMaxSlider(Rect position, ref float value, ref float size, float visualStart,
            float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
        {
            DoMinMaxSlider(position,
                GUIUtility.GetControlID(s_MinMaxSliderHash, FocusType.Passive), ref value, ref size,
                visualStart, visualEnd, startLimit, endLimit, slider, thumb, horiz);
        }


        private static bool ScrollerRepeatButton(int scrollerID, Rect rect, GUIStyle style)
        {
            var result = false;
            if (!DoRepeatButton(rect, GUIContent.none, style, FocusType.Passive)) return false;
            var flag = scrollControlID != scrollerID;
            scrollControlID = scrollerID;
            if (flag)
            {
                result = true;
                nextScrollStepTime =
                    Time.realtimeSinceStartup + 0.001f * FirstScrollWait;
            }
            else if (Time.realtimeSinceStartup >= nextScrollStepTime)
            {
                result = true;
                nextScrollStepTime =
                    Time.realtimeSinceStartup + 0.001f * scrollWait;
            }

            return result;
        }


        private class MinMaxSliderState
        {
            public float dragEndLimit;


            public float dragStartLimit;


            public float dragStartPos;


            public float dragStartSize;


            public float dragStartValue;


            public float dragStartValuesPerPixel;


            public int whereWeDrag = -1;
        }
    }
}