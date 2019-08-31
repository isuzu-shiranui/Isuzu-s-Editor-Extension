using IsuzuEditorExtension.Layout;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator
{
    public class TestTab : AnimationClipCreatorTabBase
    {
        private AnimEditor animEditor;

        public TestTab(string title, EditorWindow parentWindow) : base(title, parentWindow)
        {
        }

        public override void Initialize(GameObject root)
        {
            this.animEditor = new AnimEditor();
            var animation = new AnimationClip();

            var curveBinding = new EditorCurveBinding
            {
                type = typeof(SkinnedMeshRenderer),
                path = "Root",
                propertyName = "propName"
            };


            var curve = new AnimationCurve();
            curve.AddKey(0f, 0);
            curve.AddKey(1f, 1);

            AnimationUtility.SetEditorCurve(animation, curveBinding, curve);

            this.animEditor.Initialize(animation);
        }

        public override void OnInspectorGUI(Rect rect = new Rect())
        {
            this.animEditor.OnGUI();
        }


        public override AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            return animationClip;
        }

        public override void OnDestroy()
        {
        }
    }
}