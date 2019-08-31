using System;
using System.Collections.Generic;
using System.Linq;
using IsuzuEditorExtension.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BodyShapeEditTab
{
    public class MuscleTreeView : TreeView
    {
        private readonly HumanPoseHandler humanPoseHandler;

        private readonly float[] initialHumanPoseValues;

        private readonly Dictionary<string, bool> muscleRootFoldToggleTable = new Dictionary<string, bool>();
        private HumanPose humanPose;

        public MuscleTreeView(TreeViewState state) : base(state)
        {
        }

        public MuscleTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, HumanPose humanPose,
            HumanPoseHandler humanPoseHandler) : base(state, multiColumnHeader)
        {
            this.rowHeight = 20;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = true;

            this.humanPose = humanPose;
            this.humanPoseHandler = humanPoseHandler;

            this.initialHumanPoseValues = humanPose.muscles.ToArray();

            multiColumnHeader.ResizeToFit();
            this.Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem
            {
                depth = -1
            };

            var index = 1;

            var muscleTreeItems = HumanTrait.MuscleName
                .Select((s, i) => new MuscleTreeItem(0, i, false, s, this.humanPose.muscles[i]));
            var treeItems = muscleTreeItems as MuscleTreeItem[] ?? muscleTreeItems.ToArray();

            var bodTreeItems = treeItems.Take(9);
            root.AddChild(this.CreateChild(ref index, "Body", bodTreeItems));

            var leftArmTreeItems = treeItems.Skip(37).Take(9);
            root.AddChild(this.CreateChild(ref index, "Left Arm", leftArmTreeItems));

            var rightArmTreeItems = treeItems.Skip(46).Take(9);
            root.AddChild(this.CreateChild(ref index, "Right Arm", rightArmTreeItems));

            var leftLegTreeItems = treeItems.Skip(21).Take(8);
            root.AddChild(this.CreateChild(ref index, "Left Leg", leftLegTreeItems));

            var rightLegTreeItems = treeItems.Skip(29).Take(8);
            root.AddChild(this.CreateChild(ref index, "Right Leg", rightLegTreeItems));

            var headTreeItems = treeItems.Skip(9).Take(12);
            root.AddChild(this.CreateChild(ref index, "Head", headTreeItems));

            var leftFingersTreeItems = treeItems.Skip(55).Take(20);
            root.AddChild(this.CreateChild(ref index, "Left Fingers", leftFingersTreeItems));

            var rightFingersTreeItems = treeItems.Skip(75).Take(20);
            root.AddChild(this.CreateChild(ref index, "Right Fingers", rightFingersTreeItems));


            foreach (var x in root.children.SelectMany(x => x.children)) x.id = index++;

            return root;
        }

        private TreeViewItem CreateChild(ref int index, string title, IEnumerable<MuscleTreeItem> muscleTreeItems)
        {
            var treeViewItem = new TreeViewItem
            {
                depth = 0,
                id = index,
                displayName = title
            };

            this.muscleRootFoldToggleTable.Add(title + ":" + index++, false);

            foreach (var muscleTreeItem in muscleTreeItems) treeViewItem.AddChild(muscleTreeItem);

            return treeViewItem;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            var muscleTreeItem = item as MuscleTreeItem;

            return muscleTreeItem != null && (muscleTreeItem.MuscleElement.MuscleName.Contains(search) ||
                                              muscleTreeItem.MuscleElement.MuscleName.Contains(search));
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var muscleTreeViewItem = args.item as MuscleTreeItem;

            if (muscleTreeViewItem != null)
                for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    var rect = args.GetCellRect(i);
                    var column = (MuscleTreeColumn) args.GetColumn(i);
                    var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                    var element = muscleTreeViewItem.MuscleElement;
                    var index = muscleTreeViewItem.MuscleElement.MuscleId;
                    switch (column)
                    {
                        case MuscleTreeColumn.InternalId:
                            break;
                        case MuscleTreeColumn.Id:
                            rect.x += 15;
                            EditorGUI.LabelField(rect, (index + 1).ToString(), labelStyle);
                            break;
                        case MuscleTreeColumn.CheckBox:
                            rect.x += 25;
                            element.IsExport =
                                EditorGUI.Toggle(rect, element.IsExport);
                            break;
                        case MuscleTreeColumn.Name:
                            rect.x += 35;
                            EditorGUI.LabelField(rect, element.MuscleName, labelStyle);
                            break;
                        case MuscleTreeColumn.FloatValue:
                            var value = EditorGUI.Slider(rect, element.MuscleValue,
                                HumanTrait.GetMuscleDefaultMin(index),
                                HumanTrait.GetMuscleDefaultMax(index));
                            element.MuscleValue = value;
                            this.humanPose.muscles[index] = value;
                            this.humanPoseHandler.SetHumanPose(ref this.humanPose);
                            break;
                        case MuscleTreeColumn.ResetButton:
                            if (GUI.Button(rect, "Reset"))
                            {
                                element.MuscleValue =
                                    this.initialHumanPoseValues[index];
                                this.humanPose.muscles[index] =
                                    element.MuscleValue;
                                this.humanPoseHandler.SetHumanPose(ref this.humanPose);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(Utility.NameOf(() => column), column, null);
                    }
                }
            else
                for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    var rect = args.GetCellRect(i);
                    var column = (MuscleTreeColumn) args.GetColumn(i);
                    var boldLabelStyle = args.selected ? EditorStyles.whiteBoldLabel : EditorStyles.boldLabel;
                    var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                    var itemID = args.item.id;
                    var value = this.muscleRootFoldToggleTable[args.item.displayName + ":" + itemID];
                    switch (column)
                    {
                        case MuscleTreeColumn.InternalId:
                            break;
                        case MuscleTreeColumn.Id:
                            rect.x += 5;
                            EditorGUI.LabelField(rect, args.item.id.ToString(), labelStyle);
                            break;
                        case MuscleTreeColumn.CheckBox:
                            rect.x += 15;
                            rect.xMax = rect.x + 15;
                            var toggle = EditorGUI.Toggle(rect, value);
                            if (value != toggle)
                            {
                                foreach (var treeViewItem in this.rootItem.children
                                    .First(x => x.displayName == args.item.displayName).children
                                    .Cast<MuscleTreeItem>()) treeViewItem.MuscleElement.IsExport = toggle;
                                this.muscleRootFoldToggleTable[args.item.displayName + ":" + itemID] = toggle;
                            }

                            break;
                        case MuscleTreeColumn.Name:
                            this.columnIndexForTreeFoldouts = 3;

                            rect.x += this.foldoutWidth + 2;
                            EditorGUI.LabelField(rect, args.item.displayName,
                                boldLabelStyle);
                            break;
                        case MuscleTreeColumn.FloatValue:
                            break;
                        case MuscleTreeColumn.ResetButton:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(Utility.NameOf(() => column), column, null);
                    }
                }
        }
    }
}