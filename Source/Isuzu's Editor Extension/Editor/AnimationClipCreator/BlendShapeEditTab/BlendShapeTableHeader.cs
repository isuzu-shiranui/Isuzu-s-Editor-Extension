using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BlendShapeEditTab
{
    public class BlendShapeTableHeader : MultiColumnHeader
    {
        public BlendShapeTableHeader(MultiColumnHeaderState state) : base(state)
        {
            var columns = new List<MultiColumnHeaderState.Column>();

            foreach (BlendShapeTreeColumn value in Enum.GetValues(typeof(BlendShapeTreeColumn)))
                switch (value)
                {
                    case BlendShapeTreeColumn.InternalId:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            width = 0,
                            maxWidth = 0,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.Id:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("ID"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 40,
                            minWidth = 40,
                            autoResize = false,
                            allowToggleVisibility = true
                        });
                        break;
                    case BlendShapeTreeColumn.CheckBox:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("Export"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 50,
                            minWidth = 50,
                            autoResize = false,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.Name:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("Root/Blend Shape Name"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 100,
                            minWidth = 100,
                            autoResize = true,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.FloatValue:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("Blend Shape Value"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 150,
                            minWidth = 150,
                            autoResize = true,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.MinButton:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent(""),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 50,
                            minWidth = 50,
                            autoResize = false,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.MaxButton:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent(""),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 50,
                            minWidth = 50,
                            autoResize = false,
                            allowToggleVisibility = false
                        });
                        break;
                    case BlendShapeTreeColumn.ResetButton:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent(""),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 60,
                            minWidth = 60,
                            autoResize = false,
                            allowToggleVisibility = false
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            this.state = new MultiColumnHeaderState(columns.ToArray());
        }
    }
}