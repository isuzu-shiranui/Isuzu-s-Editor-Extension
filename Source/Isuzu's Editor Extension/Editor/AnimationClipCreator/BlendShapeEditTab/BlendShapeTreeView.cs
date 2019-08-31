using System;
using System.Collections.Generic;
using System.Linq;
using IsuzuEditorExtension.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BlendShapeEditTab
{
    public class BlendShapeTreeView : TreeView
    {
        private readonly Dictionary<string, bool> foldBulkToggleTable = new Dictionary<string, bool>();
        private readonly float[] initialBlendShapeValues;
        private readonly SkinnedMeshRenderer[] skinnedMeshRenderers;

        public BlendShapeTreeView(TreeViewState state) : base(state)
        {
        }

        public BlendShapeTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader,
            IEnumerable<SkinnedMeshRenderer> skinnedMeshRenderers, GameObject root) : base(state, multiColumnHeader)
        {
            this.rowHeight = 20;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = true;

            var meshRenderers = skinnedMeshRenderers as SkinnedMeshRenderer[] ?? skinnedMeshRenderers.ToArray();
            this.skinnedMeshRenderers = meshRenderers.Where(x => x.sharedMesh.blendShapeCount > 0).ToArray();

            var tmp = new List<float>();
            foreach (var skinnedMeshRenderer in meshRenderers)
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    tmp.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));

            this.initialBlendShapeValues = tmp.ToArray();

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
            foreach (var skinnedMeshRenderer in this.skinnedMeshRenderers)
            {
                var root2 = new TreeViewItem
                {
                    depth = 0,
                    id = index,
                    displayName = skinnedMeshRenderer.name
                };
                root.AddChild(root2);
                this.foldBulkToggleTable.Add(skinnedMeshRenderer.name + ":" + index++, false);
            }

            for (var i = 0; i < root.children.Count; i++)
            {
                var skinnedMeshRenderer = this.skinnedMeshRenderers[i];
                var item = root.children[i];

                for (var i1 = 0; i1 < skinnedMeshRenderer.sharedMesh.blendShapeCount; i1++)
                {
                    var blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i1);
                    item.AddChild(new BlendShapeTreeItem(index++,
                        !EditorSettings.Default.SkinnedMeshRendererExcludes.Any(x => blendShapeName.Contains(x)),
                        blendShapeName,
                        skinnedMeshRenderer.GetBlendShapeWeight(i1),
                        skinnedMeshRenderer.name,
                        i1)
                    {
                        depth = 1
                    });
                }
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var blendShapeTreeItem = args.item as BlendShapeTreeItem;
            if (blendShapeTreeItem != null)
                for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    var rect = args.GetCellRect(i);
                    var column = (BlendShapeTreeColumn) args.GetColumn(i);
                    var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                    var element = blendShapeTreeItem.BlendShapeTreeElement;

                    switch (column)
                    {
                        case BlendShapeTreeColumn.InternalId:
                            break;
                        case BlendShapeTreeColumn.Id:
                            rect.x += 15;
//                            EditorGUI.LabelField(rect, (element.BlendShapeId + 1).ToString(), labelStyle);
                            EditorGUI.LabelField(rect, args.item.id.ToString(), labelStyle);
                            break;
                        case BlendShapeTreeColumn.CheckBox:
                            rect.x += 25;
                            element.IsExport =
                                EditorGUI.Toggle(rect, element.IsExport);
                            break;
                        case BlendShapeTreeColumn.Name:
                            rect.x += 35;
                            EditorGUI.LabelField(rect, blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName,
                                labelStyle);
                            break;
                        case BlendShapeTreeColumn.FloatValue:
                            this.SetBlendShapeValue(element, EditorGUI.Slider(rect, element.BlendShapeValue, 0, 100f));
                            break;
                        case BlendShapeTreeColumn.MinButton:
                            if (GUI.Button(rect, "Min")) this.SetBlendShapeValue(element, 0f);
                            break;
                        case BlendShapeTreeColumn.MaxButton:
                            if (GUI.Button(rect, "Max")) this.SetBlendShapeValue(element, 100f);
                            break;
                        case BlendShapeTreeColumn.ResetButton:
                            if (GUI.Button(rect, "Reset"))
                                this.SetBlendShapeValue(element,
                                    this.initialBlendShapeValues[args.item.id - this.skinnedMeshRenderers.Length - 1]);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(Utility.NameOf(() => column), column, null);
                    }
                }
            else
                for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    var rect = args.GetCellRect(i);
                    var column = (BlendShapeTreeColumn) args.GetColumn(i);
                    var boldLabelStyle = args.selected ? EditorStyles.whiteBoldLabel : EditorStyles.boldLabel;
                    var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                    var value = this.foldBulkToggleTable[args.item.displayName + ":" + args.item.id];
                    switch (column)
                    {
                        case BlendShapeTreeColumn.InternalId:
                            break;
                        case BlendShapeTreeColumn.Id:
                            rect.x += 5;
                            EditorGUI.LabelField(rect, args.item.id.ToString(), labelStyle);
                            break;
                        case BlendShapeTreeColumn.CheckBox:
                            rect.x += 15;
                            rect.xMax = rect.x + 15;
                            var toggle = EditorGUI.Toggle(rect, value);
                            if (value != toggle)
                            {
                                foreach (var element in this.GetRows().Where(x => x is BlendShapeTreeItem)
                                    .Cast<BlendShapeTreeItem>().Select(x => x.BlendShapeTreeElement))
                                    element.IsExport = toggle;

                                this.foldBulkToggleTable[args.item.displayName + ":" + args.item.id] = toggle;
                            }

                            break;
                        case BlendShapeTreeColumn.Name:
                            this.columnIndexForTreeFoldouts = 3;

                            rect.x += this.foldoutWidth + 2;
                            EditorGUI.LabelField(rect, args.item.displayName,
                                boldLabelStyle);
                            break;
                        case BlendShapeTreeColumn.FloatValue:
                            break;
                        case BlendShapeTreeColumn.MinButton:
                            break;
                        case BlendShapeTreeColumn.MaxButton:
                            break;
                        case BlendShapeTreeColumn.ResetButton:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(Utility.NameOf(() => column), column, null);
                    }
                }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            var blendShapeTreeItem = item as BlendShapeTreeItem;

            return blendShapeTreeItem != null &&
                   (blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName.Contains(search) ||
                    blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName.Contains(search));
        }

        private void SetBlendShapeValue(BlendShapeTreeElement element, float value)
        {
            element.BlendShapeValue = value;
            foreach (var skinnedMeshRenderer in this.skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.name != element.MeshName) continue;

                skinnedMeshRenderer.SetBlendShapeWeight(element.BlendShapeId, value);
                break;
            }
        }
    }
}