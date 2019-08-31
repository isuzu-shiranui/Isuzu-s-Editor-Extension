using System.Collections.Generic;
using System.Linq;
using IsuzuEditorExtension.Layout;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator
{
    /// <summary>
    /// </summary>
    internal class AnimationClipCreatorWindow : EditorWindow
    {
        private const float SPLITTER_WIDTH = 3;
        private readonly AvatarPreview avatarPreview;

        private readonly Vector2 posLeft = Vector2.zero;
        private readonly List<AnimationClipCreatorTabBase> tabs;
        private MouseCursor cursor;
        private bool dragging;
        private Vector2 posRight;
        private int previewLayer;

        private PreviewRenderUtility previewRenderUtility;

        private int selectedTabIndex;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private float splitterPos = 300f;
        private Rect splitterRect;

        private GUIStyle tabStyle;
        private GameObject targetObject;

        public AnimationClipCreatorWindow()
        {
            this.tabs = new List<AnimationClipCreatorTabBase>
            {
                new ExportSettingTab.ExportSettingTab("Export Settings", this),
                new BlendShapeEditTab.BlendShapeEditTab("Blend Shapes", this),
                new BodyShapeEditTab.BodyShapeEditTab("Body Shapes", this),
                new ItemRegisterTab("Register Item", this),
                new TestTab("Layout Test(Anim)", this)
            };

            this.avatarPreview = new AvatarPreview();
        }

        private GameObject TargetObject
        {
            get { return this.targetObject; }
            set
            {
                if (value == null || this.targetObject == value) return;

                if (this.targetObject != null)
                {
                    DestroyImmediate(this.targetObject);
                    this.targetObject = null;
                }

                this.targetObject = Instantiate(value);
                this.targetObject.transform.position = Vector3.zero;
                this.targetObject.transform.rotation = Quaternion.Euler(Vector3.zero);
                this.avatarPreview.OnDisable();
                this.avatarPreview.Initialize(this.targetObject);
                this.Initialize(this.targetObject);
            }
        }

        private void Initialize(GameObject value)
        {
            foreach (var animationClipCreatorTab in this.tabs) animationClipCreatorTab.Initialize(value);
        }

        [MenuItem("IsuzuEditorExtension/Animation Clip Creator")]
        private static void Create()
        {
            GetWindow<AnimationClipCreatorWindow>("Create Animation Clip");
        }

        private void OnEnable()
        {
            var skin = AssetDatabase.LoadAssetAtPath<GUISkin>(
                "Assets/Isuzu's Editor Extension/Styles/TabStyle.guiskin");
            this.tabStyle = skin.GetStyle("Tab");
        }

        private void OnGUI()
        {
            this.TargetObject =
                EditorGUILayout.ObjectField("Target Object", this.targetObject, typeof(GameObject), true) as GameObject;

            if (this.TargetObject == null || !this.TargetObject.GetComponent<Animator>()) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new GUILayout.ScrollViewScope(this.posLeft, GUILayout.Width(this.splitterPos),
                    GUILayout.MaxWidth(this.splitterPos),
                    GUILayout.MinWidth(this.splitterPos)))
                using (new EditorGUILayout.VerticalScope())
                {
                    var rect = new Rect(0, 0, this.splitterPos, this.position.height);
                    this.avatarPreview.DrawAvatarPreview(rect, GUIStyle.none);
                }

                GUILayout.Box("",
                    GUILayout.Width(SPLITTER_WIDTH),
                    GUILayout.MaxWidth(SPLITTER_WIDTH),
                    GUILayout.MinWidth(SPLITTER_WIDTH),
                    GUILayout.ExpandHeight(true));
                this.splitterRect = GUILayoutUtility.GetLastRect();

                using (new GUILayout.ScrollViewScope(this.posRight, GUILayout.ExpandWidth(true)))
                using (new EditorGUILayout.VerticalScope())
                {
                    var tabTitles = this.tabs.Select(x => x.Title).ToArray();
                    this.selectedTabIndex = GUILayout.Toolbar(this.selectedTabIndex, tabTitles, this.tabStyle);

                    var tabRect = new Rect(this.splitterRect);
                    tabRect.x += 10;
                    this.tabs[this.selectedTabIndex].OnInspectorGUI(tabRect);

                    GUILayout.FlexibleSpace();

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("OK"))
                        {
                            var savePath =
                                EditorUtility.SaveFilePanel("Save Animation Clip", Application.dataPath, "anim",
                                    "anim");
                            if (string.IsNullOrEmpty(savePath))
                            {
                                foreach (var animationClipCreatorTab in this.tabs) animationClipCreatorTab.OnDestroy();

                                this.Close();
                                return;
                            }

                            var createdClip = this.CreateAnimationClip(new AnimationClip());

                            AssetDatabase.CreateAsset(
                                createdClip,
                                AssetDatabase.GenerateUniqueAssetPath(savePath)
                            );
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();

                            this.Close();
                        }

                        if (GUILayout.Button("Cancel")) this.Close();
                    }
                }
            }

            if (Event.current == null) return;

            switch (Event.current.rawType)
            {
                case EventType.MouseDown:
                    if (this.splitterRect.Contains(Event.current.mousePosition)) this.dragging = true;
                    break;
                case EventType.MouseDrag:
                    if (this.dragging)
                    {
                        this.cursor = MouseCursor.SplitResizeLeftRight;
                        this.splitterPos += Event.current.delta.x;
                        this.splitterPos = Mathf.Clamp(this.splitterPos, 100, this.position.width - 100);
                        this.Repaint();
                    }

                    break;
                case EventType.MouseUp:
                    if (this.dragging) this.dragging = false;
                    break;
                case EventType.Repaint:
                    this.cursor = MouseCursor.SplitResizeLeftRight;
                    EditorGUIUtility.AddCursorRect(this.splitterRect, this.cursor);
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="targetClip"></param>
        private AnimationClip CreateAnimationClip(AnimationClip targetClip = null)
        {
            var clip = targetClip != null ? Instantiate(targetClip) : new AnimationClip();

            clip = this.tabs.Aggregate(clip,
                (current, animationClipCreatorTab) => animationClipCreatorTab.CreateAnimationClip(current));

            return clip;
        }

        private void OnDestroy()
        {
            DestroyImmediate(this.TargetObject);
            this.avatarPreview.OnDisable();
            foreach (var animationClipCreatorTab in this.tabs) animationClipCreatorTab.OnDestroy();
        }
    }
}