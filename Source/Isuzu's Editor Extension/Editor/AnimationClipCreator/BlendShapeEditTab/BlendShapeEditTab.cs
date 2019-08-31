using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BlendShapeEditTab
{
    internal class BlendShapeEditTab : AnimationClipCreatorTabBase
    {
        private const string SEARCH_STRING_STATE_KEY = "BlendShapeEditTab_SearchString";

        private BlendShapeTreeView blendShapeTreeView;

        private bool isInitialized;
        private Vector2 scrollPosition;
        private SearchField searchField;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        public BlendShapeEditTab(string title, EditorWindow parentWindow) : base(title, parentWindow)
        {
        }

        public override void Initialize(GameObject root)
        {
            this.skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

            this.isInitialized = this.skinnedMeshRenderers != null && this.skinnedMeshRenderers.Any();

            if (!this.isInitialized) return;

            var state = new TreeViewState();
            var header = new BlendShapeTableHeader(null);
            this.blendShapeTreeView = new BlendShapeTreeView(state, header, this.skinnedMeshRenderers, root)
            {
                searchString = SessionState.GetString(SEARCH_STRING_STATE_KEY, "")
            };
            this.searchField = new SearchField();
            this.searchField.downOrUpArrowKeyPressed += this.blendShapeTreeView.SetFocusAndEnsureSelectedItem;
        }

        public override void OnInspectorGUI(Rect rect = new Rect())
        {
            if (!this.isInitialized)
            {
                EditorGUILayout.LabelField("Does not contain Skinned Mesh Renderer or Blend Shapes");
                return;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
                this.blendShapeTreeView.searchString =
                    this.searchField.OnToolbarGUI(this.blendShapeTreeView.searchString);
            }

            this.blendShapeTreeView.OnGUI(new Rect(rect.width, 45, this.ParentWindow.position.width - rect.x,
                this.ParentWindow.position.height - 80 - EditorGUIUtility.singleLineHeight));
        }

        public override AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            if (!this.isInitialized) return animationClip;

            this.blendShapeTreeView.ExpandAll();
            var blendShapeTreeItems = this.blendShapeTreeView.GetRows().OfType<BlendShapeTreeItem>();
            this.blendShapeTreeView.CollapseAll();
            foreach (var blendShapeTreeItem in blendShapeTreeItems.Select((item, i) =>
                new {item.BlendShapeTreeElement, i}))
            {
                var skinnedMeshRenderer =
                    this.skinnedMeshRenderers.First(x => x.name == blendShapeTreeItem.BlendShapeTreeElement.MeshName);

                var path = GetHierarchyPath(skinnedMeshRenderer.transform);
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    if (blendShapeTreeItem.BlendShapeTreeElement.IsExport &&
                        skinnedMeshRenderer.GetBlendShapeWeight(i) <= 0) continue;

                    var curveBinding = new EditorCurveBinding
                    {
                        type = typeof(SkinnedMeshRenderer),
                        path = path,
                        propertyName = "blendShape." + skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i)
                    };

                    var curve = new AnimationCurve();
                    curve.AddKey(0f, skinnedMeshRenderer.GetBlendShapeWeight(i));
                    curve.AddKey(0.01f, skinnedMeshRenderer.GetBlendShapeWeight(i));

                    AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
                }
            }

            return animationClip;
        }


        public override void OnDestroy()
        {
            //this.ResetBlendShape();
        }

        private void ResetBlendShape()
        {
            if (this.skinnedMeshRenderers == null) return;
            foreach (var skinnedMeshRenderer in this.skinnedMeshRenderers)
            {
                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                for (var i = 0; i < sharedMesh.blendShapeCount; i++)
                {
                    var sliderMin = 0f;

                    var frameCount = sharedMesh.GetBlendShapeFrameCount(i);
                    for (var j = 0; j < frameCount; j++)
                    {
                        var frameWeight = sharedMesh.GetBlendShapeFrameWeight(i, j);
                        sliderMin = Mathf.Min(frameWeight, sliderMin);
                    }

                    skinnedMeshRenderer.SetBlendShapeWeight(i, sliderMin);
                }
            }
        }

        private static string GetHierarchyPath(Transform self)
        {
            var path = self.gameObject.name;
            var parent = self.parent;
            while (parent.parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}