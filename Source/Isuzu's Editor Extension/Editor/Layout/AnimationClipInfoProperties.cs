using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal class AnimationClipInfoProperties
    {
        private readonly AnimationClip animationClip;
        private readonly AnimationClipSettings animationClipSettings;

        public AnimationClipInfoProperties(AnimationClip animationClip, AnimationClipSettings animationClipSettings)
        {
            this.animationClip = animationClip;
            this.animationClipSettings = animationClipSettings;
        }

        public float FirstFrame
        {
            get { return this.TimeToFrame(this.animationClipSettings.startTime); }
            set
            {
                this.animationClipSettings.startTime = this.FrameToTime(value);
                SetAnimationClipSettings(this.animationClip, this.animationClipSettings);
            }
        }

        public float LastFrame
        {
            get { return this.TimeToFrame(this.animationClipSettings.stopTime); }
            set
            {
                this.animationClipSettings.stopTime = this.FrameToTime(value);
                SetAnimationClipSettings(this.animationClip, this.animationClipSettings);
            }
        }

        public int WrapMode
        {
            get { return (int) this.animationClip.wrapMode; }
            set { this.animationClip.wrapMode = (WrapMode) value; }
        }

        public bool Loop
        {
            get { return this.animationClipSettings.loopTime; }
            set
            {
                this.animationClipSettings.loopTime = value;
                SetAnimationClipSettings(this.animationClip, this.animationClipSettings);
            }
        }

        private float TimeToFrame(float time)
        {
            return (this.animationClipSettings.startTime - time) / this.animationClip.frameRate;
        }

        private float FrameToTime(float frame)
        {
            return frame * this.animationClip.frameRate;
        }

        private static void SetAnimationClipSettings(AnimationClip animationClip,
            AnimationClipSettings animationClipSettings)
        {
            var methodInfo = typeof(AnimationUtility).GetMethod("SetAnimationClipSettings",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo != null) methodInfo.Invoke(null, new object[] {animationClip, animationClipSettings});
        }
    }
}