using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class AnimEditor
    {
        private AnimationClip animationClip;
        private AnimationClipInfoProperties animationClipInfoProperties;
        private AnimationClipSettings animationClipSettings;

        private bool dirtyQualityCurves;
        private float draggingAdditivePoseFrame;

        private bool draggingRange;
        private bool draggingRangeBegin;
        private bool draggingRangeEnd;
        private float draggingStartFrame;
        private float draggingStopFrame;
        private TimeArea eventTimeArea;
        private float m_AdditivePoseFrame;
        private float m_InitialClipLength;
        private bool m_LoopBlend;
        private bool m_LoopBlendOrientation;
        private bool m_LoopBlendPositionXZ;
        private bool m_LoopBlendPositionY;
        private bool m_LoopTime;
        private bool m_ShowCurves;
        private bool m_ShowEvents;
        private float m_StartFrame;
        private float m_StopFrame;
        private float s_EventTimelineMax;
        private TimeArea timeArea;

        public void Initialize(AnimationClip targetClip)
        {
            this.animationClip = targetClip;
            this.animationClipSettings = AnimationUtility.GetAnimationClipSettings(this.animationClip);
            this.animationClipInfoProperties =
                new AnimationClipInfoProperties(this.animationClip, this.animationClipSettings);

            this.timeArea = new TimeArea
            {
                HRangeLocked = false,
                HSlider = true,
                HRangeMin = 0,
                HRangeMax = this.animationClip.length,
                Margin = 10,
                ScaleWithWindow = true,
                IgnoreScrollWheelUntilClicked = true
            };

            this.eventTimeArea = new TimeArea
            {
                HRangeLocked = true,
                HSlider = false,
                HRangeMin = 0,
                HRangeMax = this.s_EventTimelineMax,
                Margin = 10,
                ScaleWithWindow = true,
                IgnoreScrollWheelUntilClicked = true
            };
            this.eventTimeArea.SetShownHRangeInsideMargins(0, this.s_EventTimelineMax);
            this.eventTimeArea.Ticks.SetTickModulosForFrameRate(60);


            this.timeArea.SetShownHRangeInsideMargins(0, this.animationClip.length);
            this.timeArea.Ticks.SetTickModulosForFrameRate(this.animationClip.frameRate);
        }

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 30;

            EditorGUILayout.BeginHorizontal();
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Label(Styles.Length, EditorStyles.miniLabel, GUILayout.Width(50 - 4));
                    GUILayout.Label(this.GetClipLength().ToString("0.000", CultureInfo.InvariantCulture.NumberFormat),
                        EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(this.animationClip.frameRate + " FPS", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!this.animationClip.legacy)
                this.MuscleClipGUI();
            else
                this.AnimationClipGUI();
        }

        private float GetClipLength()
        {
            if (this.animationClipInfoProperties == null)
                return this.animationClip.length;
            return (this.animationClipInfoProperties.LastFrame - this.animationClipInfoProperties.FirstFrame) /
                   this.animationClip.frameRate;
        }

        private void ClipRangeGUI(ref float startFrame, ref float stopFrame, out bool changedStart,
            out bool changedStop,
            bool showAdditivePoseFrame, ref float additivePoseFrame, out bool changedAdditivePoseFrame)
        {
            changedStart = false;
            changedStop = false;
            changedAdditivePoseFrame = false;

            this.draggingRangeBegin = false;
            this.draggingRangeEnd = false;

            var invalidRange =
                startFrame + 0.01f < this.animationClipSettings.startTime * this.animationClip.frameRate ||
                startFrame - 0.01f > this.animationClipSettings.stopTime * this.animationClip.frameRate ||
                stopFrame + 0.01f < this.animationClipSettings.startTime * this.animationClip.frameRate ||
                stopFrame - 0.01f > this.animationClipSettings.stopTime * this.animationClip.frameRate;
            var fixRange = false;
            if (invalidRange)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("The clip range is outside of the range of the source take.",
                    EditorStyles.wordWrappedMiniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                if (GUILayout.Button("Clamp Range"))
                    fixRange = true;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            var timeRect = GUILayoutUtility.GetRect(10, 18 + 15);
            GUI.Label(timeRect, "", "TE Toolbar");

            if (Event.current.type == EventType.Repaint) this.timeArea.Rect = timeRect;
            this.timeArea.BeginViewGUI(false);
            this.timeArea.EndViewGUI();
            timeRect.height -= 15;

            var startHandleId = GUIUtility.GetControlID(3126789, FocusType.Passive);
            var stopHandleId = GUIUtility.GetControlID(3126789, FocusType.Passive);
            var additiveHandleId = GUIUtility.GetControlID(3126789, FocusType.Passive);

            GUI.BeginGroup(new Rect(timeRect.x + 1, timeRect.y + 1, timeRect.width - 2, timeRect.height - 2));
            {
                timeRect.x = timeRect.y = -1;

                var startPixel = this.timeArea.FrameToPixel(0, this.animationClip.frameRate, timeRect);
                var stopPixel = this.timeArea.FrameToPixel(1, this.animationClip.frameRate, timeRect);
                GUI.Label(new Rect(startPixel, timeRect.y, stopPixel - startPixel, timeRect.height), "",
                    EditorStyles.label);

                this.timeArea.TimeRuler(timeRect, this.animationClip.frameRate);

                using (new EditorGUI.DisabledScope(invalidRange))
                {
                    var startTime = startFrame / this.animationClip.frameRate;

                    var inPoint =
                        this.timeArea.BrowseRuler(timeRect, startHandleId, ref startTime, 0, false, "TL InPoint");

                    if (inPoint == TimeArea.TimeRulerDragMode.Cancel)
                    {
                        startFrame = this.draggingStartFrame;
                    }
                    else if (inPoint != TimeArea.TimeRulerDragMode.None)
                    {
                        startFrame = startTime * this.animationClip.frameRate;
                        // Snapping bias. Snap to whole frames when zoomed out.
                        startFrame = RoundBasedOnMinimumDifference(startFrame,
                            this.timeArea.PixelDeltaToTime(timeRect) * this.animationClip.frameRate * 10);
                        changedStart = true;
                    }

                    var stopTime = stopFrame / this.animationClip.frameRate;

                    var outPoint =
                        this.timeArea.BrowseRuler(timeRect, stopHandleId, ref stopTime, 0, false, "TL OutPoint");
                    if (outPoint == TimeArea.TimeRulerDragMode.Cancel)
                    {
                        stopFrame = this.draggingStopFrame;
                    }
                    else if (outPoint != TimeArea.TimeRulerDragMode.None)
                    {
                        stopFrame = stopTime * this.animationClip.frameRate;
                        // Snapping bias. Snap to whole frames when zoomed out.
                        stopFrame = RoundBasedOnMinimumDifference(stopFrame,
                            this.timeArea.PixelDeltaToTime(timeRect) * this.animationClip.frameRate * 10);
                        changedStop = true;
                    }

                    // Additive pose frame Handle
                    if (showAdditivePoseFrame)
                    {
                        var additivePoseTime = additivePoseFrame / this.animationClip.frameRate;
                        var additivePoint = this.timeArea.BrowseRuler(timeRect, additiveHandleId, ref additivePoseTime,
                            0, false, "TL playhead");
                        if (additivePoint == TimeArea.TimeRulerDragMode.Cancel)
                        {
                            additivePoseFrame = this.draggingAdditivePoseFrame;
                        }
                        else if (additivePoint != TimeArea.TimeRulerDragMode.None)
                        {
                            additivePoseFrame = additivePoseTime * this.animationClip.frameRate;
                            // Snapping bias. Snap to whole frames when zoomed out.
                            additivePoseFrame = RoundBasedOnMinimumDifference(additivePoseFrame,
                                this.timeArea.PixelDeltaToTime(timeRect) * this.animationClip.frameRate * 10);
                            changedAdditivePoseFrame = true;
                        }
                    }
                }

                if (GUIUtility.hotControl == startHandleId)
                    changedStart = true;
                if (GUIUtility.hotControl == stopHandleId)
                    changedStop = true;
                if (GUIUtility.hotControl == additiveHandleId)
                    changedAdditivePoseFrame = true;
            }

            GUI.EndGroup();


            // Start and stop time float fields
            using (new EditorGUI.DisabledScope(invalidRange))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    startFrame = EditorGUILayout.FloatField(Styles.StartFrame, Mathf.Round(startFrame * 1000) / 1000);
                    if (EditorGUI.EndChangeCheck())
                        changedStart = true;

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    stopFrame = EditorGUILayout.FloatField(Styles.EndFrame, Mathf.Round(stopFrame * 1000) / 1000);
                    if (EditorGUI.EndChangeCheck())
                        changedStop = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            changedStart |= fixRange;
            changedStop |= fixRange;

            if (changedStart)
                startFrame = Mathf.Clamp(startFrame,
                    this.animationClipSettings.startTime * this.animationClip.frameRate,
                    Mathf.Clamp(stopFrame, this.animationClipSettings.startTime * this.animationClip.frameRate,
                        stopFrame));

            if (changedStop)
                stopFrame = Mathf.Clamp(stopFrame, startFrame,
                    this.animationClipSettings.stopTime * this.animationClip.frameRate);

            if (changedAdditivePoseFrame)
                additivePoseFrame = Mathf.Clamp(additivePoseFrame,
                    this.animationClipSettings.startTime * this.animationClip.frameRate,
                    this.animationClipSettings.stopTime * this.animationClip.frameRate);

            // Keep track of whether we're currently dragging the range or not
            if (changedStart || changedStop || changedAdditivePoseFrame)
            {
                if (!this.draggingRange) this.draggingRangeBegin = true;
                this.draggingRange = true;
            }
            else if (this.draggingRange && GUIUtility.hotControl == 0 && Event.current.type == EventType.Repaint)
            {
                this.draggingRangeEnd = true;
                this.draggingRange = false;
                this.dirtyQualityCurves = true;
//                Repaint(); //Todo:EditorWindow.Repait();
            }

            GUILayout.Space(10);
        }

        private void MuscleClipGUI()
        {
            EditorGUI.BeginChangeCheck();

            this.m_StartFrame = this.draggingRange
                ? this.m_StartFrame
                : this.animationClipSettings.startTime * this.animationClip.frameRate;
            this.m_StopFrame = this.draggingRange
                ? this.m_StopFrame
                : this.animationClipSettings.stopTime * this.animationClip.frameRate;
            this.m_AdditivePoseFrame = this.draggingRange
                ? this.m_AdditivePoseFrame
                : this.animationClipSettings.additiveReferencePoseTime * this.animationClip.frameRate;

            var startTime = this.m_StartFrame / this.animationClip.frameRate;
            var stopTime = this.m_StopFrame / this.animationClip.frameRate;
            var additivePoseTime = this.m_AdditivePoseFrame / this.animationClip.frameRate;

//            MuscleClipQualityInfo clipQualityInfo = MuscleClipUtility.GetMuscleClipQualityInfo(m_Clip, startTime,
//                stopTime);

            var isHumanClip = this.animationClip.isHumanMotion;
            var changedStart = false;
            var changedStop = false;
            var changedAdditivePoseFrame = false;

            if (this.animationClipInfoProperties != null)
                //                if (hasAnyRootCurves)
//                {
//                    if (m_DirtyQualityCurves)
//                        CalculateQualityCurves();
//
//                    // Calculate curves AFTER first repaint to be more responsive.
//                    if (m_QualityCurves[0] == null && Event.current.type == EventType.Repaint)
//                    {
//                        m_DirtyQualityCurves = true;
//                        Repaint();
//                    }
//                }

                this.ClipRangeGUI(ref this.m_StartFrame, ref this.m_StopFrame, out changedStart, out changedStop,
                    this.animationClipSettings.hasAdditiveReferencePose, ref this.m_AdditivePoseFrame,
                    out changedAdditivePoseFrame);

            if (!this.draggingRange)
            {
                this.animationClipSettings.startTime = startTime;
                this.animationClipSettings.stopTime = stopTime;
                this.animationClipSettings.additiveReferencePoseTime = additivePoseTime;
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            var toggleLoopTimeRect = EditorGUILayout.GetControlRect();
            this.LoopToggle(toggleLoopTimeRect, Styles.LoopTime, ref this.animationClipSettings.loopTime);

            Rect toggleLoopPoseRect;
            using (new EditorGUI.DisabledScope(!this.animationClipSettings.loopTime))
            {
                EditorGUI.indentLevel++;

                // Loop pose
                // Toggle
                toggleLoopPoseRect = EditorGUILayout.GetControlRect();
                this.LoopToggle(toggleLoopPoseRect, Styles.LoopPose, ref this.animationClipSettings.loopBlend);

                // Offset
                this.animationClipSettings.cycleOffset = EditorGUILayout.FloatField(Styles.LoopCycleOffset,
                    this.animationClipSettings.cycleOffset);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            var showCurves = isHumanClip && (changedStart || changedStop);

            if (this.animationClipInfoProperties != null)
            {
                this.animationClipSettings.hasAdditiveReferencePose = EditorGUILayout.Toggle(
                    Styles.HasAdditiveReferencePose, this.animationClipSettings.hasAdditiveReferencePose);
                using (new EditorGUI.DisabledScope(!this.animationClipSettings.hasAdditiveReferencePose))
                {
                    EditorGUI.indentLevel++;

                    this.m_AdditivePoseFrame =
                        EditorGUILayout.FloatField(Styles.AdditiveReferencePoseFrame, this.m_AdditivePoseFrame);
                    this.m_AdditivePoseFrame = Mathf.Clamp(this.m_AdditivePoseFrame,
                        this.animationClipSettings.startTime * this.animationClip.frameRate,
                        this.animationClipSettings.stopTime * this.animationClip.frameRate);

                    this.animationClipSettings.additiveReferencePoseTime =
                        this.m_AdditivePoseFrame / this.animationClip.frameRate;
                    EditorGUI.indentLevel--;
                }
            }

            bool wasChanged;

            // Additional curves
            if (this.animationClipInfoProperties != null)
            {
                // Don't make toggling foldout cause GUI.changed to be true (shouldn't cause undoable action etc.)
                wasChanged = GUI.changed;
                this.m_ShowCurves = EditorGUILayout.Foldout(this.m_ShowCurves, Styles.Curves, true);
                GUI.changed = wasChanged;
                if (this.m_ShowCurves) this.CurveGUI();
            }

            if (this.animationClipInfoProperties != null)
            {
                wasChanged = GUI.changed;
                this.m_ShowEvents = EditorGUILayout.Foldout(this.m_ShowEvents, Styles.Events, true);
                GUI.changed = wasChanged;
                if (this.m_ShowEvents) this.EventsGUI();
            }

            if (this.draggingRangeBegin)
            {
                this.m_LoopTime = this.animationClipSettings.loopTime;
                this.m_LoopBlend = this.animationClipSettings.loopBlend;
                this.m_LoopBlendOrientation = this.animationClipSettings.loopBlendOrientation;
                this.m_LoopBlendPositionY = this.animationClipSettings.loopBlendPositionY;
                this.m_LoopBlendPositionXZ = this.animationClipSettings.loopBlendPositionXZ;

                this.animationClipSettings.loopTime = false;
                this.animationClipSettings.loopBlend = false;
                this.animationClipSettings.loopBlendOrientation = false;
                this.animationClipSettings.loopBlendPositionY = false;
                this.animationClipSettings.loopBlendPositionXZ = false;

                this.draggingStartFrame = this.animationClipSettings.startTime * this.animationClip.frameRate;
                this.draggingStopFrame = this.animationClipSettings.stopTime * this.animationClip.frameRate;
                this.draggingAdditivePoseFrame =
                    this.animationClipSettings.additiveReferencePoseTime * this.animationClip.frameRate;

                //case 790259: The length of the clip will be changed by this inspector, so we can't use it for sampling
                this.animationClipSettings.startTime = 0;
                this.animationClipSettings.stopTime = this.m_InitialClipLength;

//                AnimationUtility.SetAnimationClipSettingsNoDirty(this.animationClip, animationClipSettings);

//                DestroyController();
            }

            if (this.draggingRangeEnd)
            {
                this.animationClipSettings.loopTime = this.m_LoopTime;
                this.animationClipSettings.loopBlend = this.m_LoopBlend;
                this.animationClipSettings.loopBlendOrientation = this.m_LoopBlendOrientation;
                this.animationClipSettings.loopBlendPositionY = this.m_LoopBlendPositionY;
                this.animationClipSettings.loopBlendPositionXZ = this.m_LoopBlendPositionXZ;
            }

            if (EditorGUI.EndChangeCheck() || this.draggingRangeEnd)
                if (!this.draggingRange)
                {
                    Undo.RegisterCompleteObjectUndo(this.animationClip, "Muscle Clip Edit");
//                    AnimationUtility.SetAnimationClipSettingsNoDirty(this.animationClip, animationClipSettings);
                    EditorUtility.SetDirty(this.animationClip);
//                    DestroyController();
                }
        }

        private void CurveGUI()
        {
            if (this.animationClipSettings == null)
                return;

            float time = 0;

            foreach (var editorCurveBinding in AnimationUtility.GetCurveBindings(this.animationClip))
            {
                var curve = AnimationUtility.GetEditorCurve(this.animationClip, editorCurveBinding);
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(GUIContent.none, "OL Minus", GUILayout.Width(17)))
                    {
                        this.animationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type,
                            editorCurveBinding.propertyName, null);
                    }
                    else
                    {
                        GUILayout.BeginVertical(GUILayout.Width(125));

                        var prevName = editorCurveBinding.propertyName;
                        var newName = EditorGUILayout.DelayedTextField(prevName, EditorStyles.textField);
                        if (prevName != newName)
                        {
                            this.animationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, prevName,
                                null);
                            this.animationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, newName,
                                curve);
                        }

                        var keyCount = curve.length;
                        var isKey = false;
                        var keyIndex = keyCount - 1;


                        for (var keyIter = 0; keyIter < keyCount; keyIter++)
                            if (Mathf.Abs(curve.keys[keyIter].time - time) < 0.0001f)
                            {
                                isKey = true;
                                keyIndex = keyIter;
                                break;
                            }
                            else if (curve.keys[keyIter].time > time)
                            {
                                keyIndex = keyIter;
                                break;
                            }

                        GUILayout.BeginHorizontal();

//                        if (GUILayout.Button(Styles.PrevKeyContent))
//                        {
//                            if (keyIndex > 0)
//                            {
//                                keyIndex--;
//                                m_AvatarPreview.timeControl.normalizedTime = curve.keys[keyIndex].time;
//                            }
//                        }

//                        if (GUILayout.Button(Styles.NextKeyContent))
//                        {
//                            if (isKey && keyIndex < keyCount - 1) keyIndex++;
//                            m_AvatarPreview.timeControl.normalizedTime = curve.keys[keyIndex].time;
//                        }

                        float val, newVal;
                        using (new EditorGUI.DisabledScope(!isKey))
                        {
                            val = curve.Evaluate(time);
                            newVal = EditorGUILayout.FloatField(val, GUILayout.Width(60));
                        }

                        var addKey = false;

                        if (val != newVal)
                        {
                            if (isKey) curve.RemoveKey(keyIndex);

                            addKey = true;
                        }

                        using (new EditorGUI.DisabledScope(isKey))
                        {
                            if (GUILayout.Button(Styles.AddKeyframeContent)) addKey = true;
                        }

                        if (addKey)
                        {
                            var key = new Keyframe
                            {
//                                time = time, 
                                value = newVal,
                                inTangent = 0,
                                outTangent = 0
                            };
                            curve.AddKey(key);
                            this.animationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type,
                                editorCurveBinding.propertyName, curve);
//                            UnityEditorInternal.AnimationCurvePreviewCache.ClearCache();
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();

                        EditorGUILayout.CurveField(curve, GUILayout.Height(40));

                        var curveRect = GUILayoutUtility.GetLastRect();

                        keyCount = curve.length;

//                        TimeArea.DrawPlayhead(curveRect.x + time * curveRect.width, curveRect.yMin, curveRect.yMax, 1f, 1f);

                        for (var keyIter = 0; keyIter < keyCount; keyIter++)
                        {
                            var keyTime = curve.keys[keyIter].time;

                            Handles.color = Color.white;
                            Handles.DrawLine(
                                new Vector3(curveRect.x + keyTime * curveRect.width,
                                    curveRect.y + curveRect.height - 10, 0),
                                new Vector3(curveRect.x + keyTime * curveRect.width, curveRect.y + curveRect.height,
                                    0));
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(GUIContent.none, "OL Plus", GUILayout.Width(17)))
            {
                // AddCurve
            }

            GUILayout.EndHorizontal();
        }

        private void EventsGUI()
        {
            if (this.animationClipInfoProperties == null)
                return;

            GUILayout.BeginHorizontal();
//            if (GUILayout.Button(Styles.AddEventContent, GUILayout.Width(25)))
//            {
//                m_EventManipulationHandler.SelectEvent(m_ClipInfo.GetEvents(), m_ClipInfo.GetEventCount() - 1, m_ClipInfo);
//                needsToGenerateClipInfo = true;
//            }

            var timeRect = GUILayoutUtility.GetRect(10, 18 + 15);
            timeRect.xMin += 5;
            timeRect.xMax -= 4;
            GUI.Label(timeRect, "", "TE Toolbar");

            if (Event.current.type == EventType.Repaint)
                this.eventTimeArea.Rect = timeRect;
            timeRect.height -= 15;
            this.eventTimeArea.TimeRuler(timeRect, 100.0f);


            GUI.BeginGroup(new Rect(timeRect.x + 1, timeRect.y + 1, timeRect.width - 2, timeRect.height - 2));
            {
                var localTimeRect = new Rect(-1, -1, timeRect.width, timeRect.height);

                var events = this.animationClip.events;

                //if (m_EventManipulationHandler.HandleEventManipulation(localTimeRect, ref events, m_ClipInfo)) // had changed
                {
                    // m_ClipInfo.SetEvents(events);
                }

                // Current time indicator
                //TimeArea.DrawPlayhead(m_EventTimeArea.TimeToPixel(m_AvatarPreview.timeControl.normalizedTime, localTimeRect), localTimeRect.yMin, localTimeRect.yMax, 2f, 1f);
            }


            GUI.EndGroup();

            GUILayout.EndHorizontal();

            // m_EventManipulationHandler.Draw(timeRect);
        }

        private void LoopToggle(Rect r, GUIContent content, ref bool val)
        {
            if (!this.draggingRange)
            {
                val = EditorGUI.Toggle(r, content, val);
            }
            else
            {
                EditorGUI.LabelField(r, content, GUIContent.none);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Toggle(r, " ", false);
                }
            }
        }

        private void AnimationClipGUI()
        {
            if (this.animationClipInfoProperties != null)
            {
                var startFrame = this.animationClipInfoProperties.FirstFrame;
                var stopFrame = this.animationClipInfoProperties.LastFrame;
                float additivePoseFrame = 0;
                var changedStart = false;
                var changedStop = false;
                var changedAdditivePoseFrame = false;
                this.ClipRangeGUI(ref startFrame, ref stopFrame, out changedStart, out changedStop, false,
                    ref additivePoseFrame, out changedAdditivePoseFrame);
                if (changedStart) this.animationClipInfoProperties.FirstFrame = startFrame;
                if (changedStop) this.animationClipInfoProperties.LastFrame = stopFrame;
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            if (this.animationClipInfoProperties != null)
                this.animationClipInfoProperties.Loop =
                    EditorGUILayout.Toggle(Styles.AddLoopFrame, this.animationClipInfoProperties.Loop);

            EditorGUI.BeginChangeCheck();
            var wrap = this.animationClipInfoProperties != null
                ? this.animationClipInfoProperties.WrapMode
                : (int) this.animationClip.wrapMode;
            wrap = (int) (WrapModeFixed) EditorGUILayout.EnumPopup(Styles.WrapMode, (WrapModeFixed) wrap);
            if (!EditorGUI.EndChangeCheck()) return;

            if (this.animationClipInfoProperties != null)
                this.animationClipInfoProperties.WrapMode = wrap;
            else
                this.animationClip.wrapMode = (WrapMode) wrap;
        }

        private static float RoundBasedOnMinimumDifference(float valueToRound, float minDifference)
        {
            if (minDifference == 0)
                return DiscardLeastSignificantDecimal(valueToRound);
            return (float) Math.Round(valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference),
                MidpointRounding.AwayFromZero);
        }

        private static float DiscardLeastSignificantDecimal(float v)
        {
            var decimals = Mathf.Clamp((int) (5 - Mathf.Log10(Mathf.Abs(v))), 0, 15);
            return (float) Math.Round(v, decimals, MidpointRounding.AwayFromZero);
        }

        private static int GetNumberOfDecimalsForMinimumDifference(float minDifference)
        {
            return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(minDifference))), 0, 15);
        }

        private static class Styles
        {
            public static readonly GUIContent StartFrame = new GUIContent("Start", "Start frame of the clip.");
            public static readonly GUIContent EndFrame = new GUIContent("End", "End frame of the clip.");
            public static readonly GUIContent AddLoopFrame = new GUIContent("Add Loop Frame");
            public static readonly GUIContent WrapMode = new GUIContent("Wrap Mode");
            public static readonly GUIContent Length = new GUIContent("Length");

            public static readonly GUIContent HasAdditiveReferencePose = new GUIContent("Additive Reference Pose",
                "Enable to define the additive reference pose frame.");

            public static readonly GUIContent AdditiveReferencePoseFrame = new GUIContent("Pose Frame", "Pose Frame.");

            public static readonly GUIContent LoopTime = new GUIContent("Loop Time",
                "Enable to make the animation play through and then restart when the end is reached.");

            public static readonly GUIContent LoopPose =
                new GUIContent("Loop Pose", "Enable to make the animation loop seamlessly.");

            public static readonly GUIContent LoopCycleOffset = new GUIContent("Cycle Offset",
                "Offset to the cycle of a looping animation, if we want to start it at a different time.");


            public static readonly GUIContent Curves = new GUIContent("Curves", "Parameter-related curves.");
            public static readonly GUIContent Events = new GUIContent("Events");
            public static GUIContent LoopMatch = new GUIContent("loop match");

            public static GUIContent PrevKeyContent = new GUIContent("Animation.PrevKey", "Go to previous key frame.");

            public static GUIContent NextKeyContent = new GUIContent("Animation.NextKey", "Go to next key frame.");

            public static readonly GUIContent AddKeyframeContent =
                new GUIContent("Animation.AddKeyframe", "Add Keyframe.");
        }

        private enum WrapModeFixed
        {
            Default = 0,
            Once = 1,
            Loop = 2,
            PingPong = 4,
            ClampForever = 8
        }
    }
}