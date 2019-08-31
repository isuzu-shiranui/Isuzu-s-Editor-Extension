using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace IsuzuEditorExtension.AnimationClipCreator.BodyShapeEditTab
{
    public class MuscleTableHeader : MultiColumnHeader
    {
        public MuscleTableHeader(MultiColumnHeaderState state) : base(state)
        {
            var columns = new List<MultiColumnHeaderState.Column>();

            foreach (MuscleTreeColumn value in Enum.GetValues(typeof(MuscleTreeColumn)))
                switch (value)
                {
                    case MuscleTreeColumn.InternalId:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            width = 0,
                            maxWidth = 0,
                            allowToggleVisibility = false
                        });
                        break;
                    case MuscleTreeColumn.Id:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("ID"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 20,
                            minWidth = 20,
                            autoResize = false,
                            allowToggleVisibility = true
                        });
                        break;
                    case MuscleTreeColumn.CheckBox:
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
                    case MuscleTreeColumn.Name:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("Muscle Name"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 100,
                            minWidth = 100,
                            autoResize = true,
                            allowToggleVisibility = false
                        });
                        break;
                    case MuscleTreeColumn.FloatValue:
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent("Muscle Value"),
                            headerTextAlignment = TextAlignment.Center,
                            canSort = false,
                            width = 150,
                            minWidth = 150,
                            autoResize = true,
                            allowToggleVisibility = false
                        });
                        break;
                    case MuscleTreeColumn.ResetButton:
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