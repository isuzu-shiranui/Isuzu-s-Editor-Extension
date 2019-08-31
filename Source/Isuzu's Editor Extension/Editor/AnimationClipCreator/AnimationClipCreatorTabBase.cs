using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator
{
    public class AnimationClipCreatorTabBase
    {
        protected readonly EditorWindow ParentWindow;

        protected AnimationClipCreatorTabBase(string title, EditorWindow parentWindow)
        {
            this.Title = title;
            this.ParentWindow = parentWindow;
        }

        public string Title { get; private set; }

        public virtual void Initialize(GameObject root)
        {
        }

        public virtual void OnInspectorGUI(Rect rect = new Rect())
        {
        }

        public virtual AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            return animationClip;
        }

        public virtual void OnDestroy()
        {
        }
    }
}