using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator
{
    internal class ItemRegisterTab : AnimationClipCreatorTabBase
    {
        private readonly List<GameObject> itemList;
        private ReorderableList reorderableList;
        private Vector2 scrollPosition;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        public ItemRegisterTab(string title, EditorWindow parentWindow) : base(title, parentWindow)
        {
            this.itemList = new List<GameObject>();
        }

        // ReSharper disable once ParameterHidesMember
        public override void Initialize(GameObject root)
        {
            this.reorderableList = new ReorderableList(this.itemList, typeof(GameObject));

            this.reorderableList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "Register Items"); };
            this.reorderableList.drawElementCallback += (rect, index, active, focused) =>
            {
                var element = this.itemList[index];
                var previousClip = this.itemList[index];
                this.itemList[index] = EditorGUI.ObjectField(rect, element, typeof(GameObject), true) as GameObject;
                if (element != null && !this.itemList[index].Equals(previousClip))
                {
//                    EditorUtility.SetDirty(this.target);
                }
            };

            this.reorderableList.onAddCallback += list => { this.itemList.Add(null); };

            this.reorderableList.onChangedCallback += list =>
            {
//                EditorUtility.SetDirty(this.target);
            };
        }

        public override void OnInspectorGUI(Rect rect = new Rect())
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(this.scrollPosition))
            {
                this.scrollPosition = scrollView.scrollPosition;
                this.reorderableList.DoLayoutList();
            }
        }
    }
}