using IsuzuEditorExtension.AnimationClipCreator;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.ExportSettingTab
{
    public class ExportSettingTab : AnimationClipCreatorTabBase
    {
        private AnimationClip animationClip;
        private bool exportSettingsFold;
        private bool isLoopAnimation;

        public ExportSettingTab(string title, EditorWindow parentWindow) : base(title, parentWindow)
        {
        }

        public override void OnInspectorGUI(Rect rect = new Rect())
        {
            EditorGUILayout.HelpBox("Animation Clipを設定すると、そのアニメーションにプロパティとキーフレームを追加します。", MessageType.Info);
            EditorGUILayout.HelpBox("設定しない場合、新規にアニメーションクリップが生成されます。", MessageType.Info);
            this.animationClip = EditorGUILayout.ObjectField(new GUIContent("Animation Clip"),
                this.animationClip,
                typeof(AnimationClip), false) as AnimationClip;

            this.isLoopAnimation = EditorGUILayout.Toggle(new GUIContent("Is Loop Animation"), this.isLoopAnimation);
        }

        public override AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            if (this.animationClip != null) animationClip = this.animationClip;
            if (this.isLoopAnimation) animationClip.wrapMode = WrapMode.Loop;
            return animationClip;
        }
    }
}