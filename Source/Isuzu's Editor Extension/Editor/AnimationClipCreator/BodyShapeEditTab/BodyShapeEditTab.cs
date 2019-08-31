using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BodyShapeEditTab
{
    internal class BodyShapeEditTab : AnimationClipCreatorTabBase
    {
        private const string searchStringStateKey = "BodyShapeEditTab_SearchString";

        private readonly string[] muscleSpecialNames =
        {
            "LeftHand.Thumb.1 Stretched",
            "LeftHand.Thumb.Spread",
            "LeftHand.Thumb.2 Stretched",
            "LeftHand.Thumb.3 Stretched",
            "LeftHand.Index.1 Stretched",
            "LeftHand.Index.Spread",
            "LeftHand.Index.2 Stretched",
            "LeftHand.Index.3 Stretched",
            "LeftHand.Middle.1 Stretched",
            "LeftHand.Middle.Spread",
            "LeftHand.Middle.2 Stretched",
            "LeftHand.Middle.3 Stretched",
            "LeftHand.Ring.1 Stretched",
            "LeftHand.Ring.Spread",
            "LeftHand.Ring.2 Stretched",
            "LeftHand.Ring.3 Stretched",
            "LeftHand.Little.1 Stretched",
            "LeftHand.Little.Spread",
            "LeftHand.Little.2 Stretched",
            "LeftHand.Little.3 Stretched",
            "RightHand.Thumb.1 Stretched",
            "RightHand.Thumb.Spread",
            "RightHand.Thumb.2 Stretched",
            "RightHand.Thumb.3 Stretched",
            "RightHand.Index.1 Stretched",
            "RightHand.Index.Spread",
            "RightHand.Index.2 Stretched",
            "RightHand.Index.3 Stretched",
            "RightHand.Middle.1 Stretched",
            "RightHand.Middle.Spread",
            "RightHand.Middle.2 Stretched",
            "RightHand.Middle.3 Stretched",
            "RightHand.Ring.1 Stretched",
            "RightHand.Ring.Spread",
            "RightHand.Ring.2 Stretched",
            "RightHand.Ring.3 Stretched",
            "RightHand.Little.1 Stretched",
            "RightHand.Little.Spread",
            "RightHand.Little.2 Stretched",
            "RightHand.Little.3 Stretched"
        };

        private Animator animator;
        private bool bodyFold;

        private bool headFold;
        private HumanPose humanPose;

        private HumanPoseHandler humanPoseHandler;
        private bool isAllOn;
        private bool isInitialized;

        private bool leftHandFold;

        private MuscleTreeView muscleTreeView;
        private bool rightHandFold;

        private Vector2 scrollPosition;

        private SearchField searchField;

        public BodyShapeEditTab(string title, EditorWindow parentWindow) : base(title, parentWindow)
        {
        }


        public override void Initialize(GameObject root)
        {
            this.animator = root.GetComponent<Animator>();
            if (this.animator == null)
            {
                this.isInitialized = false;
                return;
            }

            var avatar = this.animator.avatar;
            this.isInitialized = avatar.isHuman && avatar.isValid;

            if (!this.isInitialized) return;

            this.humanPoseHandler = new HumanPoseHandler(avatar, this.animator.transform);
            this.humanPoseHandler.GetHumanPose(ref this.humanPose);

            var state = new TreeViewState();
            var header = new MuscleTableHeader(null);
            this.muscleTreeView = new MuscleTreeView(state, header, this.humanPose, this.humanPoseHandler)
            {
                searchString = SessionState.GetString(searchStringStateKey, "")
            };
            this.searchField = new SearchField();
            this.searchField.downOrUpArrowKeyPressed += this.muscleTreeView.SetFocusAndEnsureSelectedItem;
        }

        public override void OnInspectorGUI(Rect rect = new Rect())
        {
            if (!this.isInitialized)
            {
                EditorGUILayout.LabelField("This is not Humanoid Character.");
                return;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
                this.muscleTreeView.searchString = this.searchField.OnToolbarGUI(this.muscleTreeView.searchString);
            }

            this.muscleTreeView.OnGUI(new Rect(rect.width, 45, this.ParentWindow.position.width - rect.x,
                this.ParentWindow.position.height - 80 - EditorGUIUtility.singleLineHeight));
        }

        public override AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            if (!this.isInitialized) return animationClip;

            var renameIndex = 0;
            this.muscleTreeView.ExpandAll();
            var rows = this.muscleTreeView.GetRows().OfType<MuscleTreeItem>();
            this.muscleTreeView.CollapseAll();
            var muscleTreeItems = rows as MuscleTreeItem[] ?? rows.ToArray();
            foreach (var muscle in HumanTrait.MuscleName.Select((s, i) => new {MuscleName = s, Index = i}))
            {
                var row = muscleTreeItems.First(x => x.MuscleElement.MuscleName == muscle.MuscleName);
                var propertyName = muscle.Index < 55 ? muscle.MuscleName : this.muscleSpecialNames[renameIndex++];

                if (!row.MuscleElement.IsExport) continue;

                var curveBinding = new EditorCurveBinding
                {
                    type = typeof(Animator),
                    path = "",
                    propertyName = propertyName
                };

                var curve = new AnimationCurve();
                curve.AddKey(0f, row.MuscleElement.MuscleValue);
                curve.AddKey(0.01f, row.MuscleElement.MuscleValue);

                AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
            }

            return animationClip;
        }

        public override void OnDestroy()
        {
            if (this.humanPoseHandler == null) return;
            this.humanPoseHandler.Dispose();
            this.humanPoseHandler = null;
        }
    }
}