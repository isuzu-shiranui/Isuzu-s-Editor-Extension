using System.Linq;
using IsuzuEditorExtension.Layout;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IsuzuEditorExtension
{
    public class IsuzuEditorExtension
    {
        private static Vector2 ScrollPos;

        private static readonly ReorderableList ReorderableList;
        private static readonly EditorSettings Settings = EditorSettings.Default;

        static IsuzuEditorExtension()
        {
            var excludeList = Settings.SkinnedMeshRendererExcludes.ToList();
            ReorderableList = new ReorderableList(excludeList, typeof(string), true, true, true, true);
            ReorderableList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Exclude default check");
            ReorderableList.drawElementCallback += (rect, index, active, focused) =>
                excludeList[index] = EditorGUI.TextField(rect, excludeList[index]);
            ReorderableList.onAddCallback += reorderableList => excludeList.Add("");
        }

        [PreferenceItem("IsuzuEditorEx")]
        public static void PreferencesGUI()
        {
            var normalState = new GUIStyleState
            {
                textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.4f, 0.4f, 0.4f)
            };

            var headerStyle = new GUIStyle
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = normalState
            };

            using (var scroll = new EditorGUILayout.ScrollViewScope(ScrollPos))
            {
                EditorGUILayout.Separator();

                using (new NestedScope("Hierarchy", headerStyle))
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    Settings.EnableCustomHierarchyView =
                        EditorGUILayout.Toggle("Enable custom view", Settings.EnableCustomHierarchyView);
                    Settings.EnableCustomHierarchyAlternateRowColor = EditorGUILayout.Toggle("Enable alternate color",
                        Settings.EnableCustomHierarchyAlternateRowColor);
                    Settings.EnableCustomHierarchyIcon =
                        EditorGUILayout.Toggle("Enable icon", Settings.EnableCustomHierarchyIcon);
                    Settings.EnableCustomHierarchyStaticButton = EditorGUILayout.Toggle("Enable static button",
                        Settings.EnableCustomHierarchyStaticButton);
                    Settings.EnableCustomHierarchyVisivilityButton = EditorGUILayout.Toggle("Enable visivility button",
                        Settings.EnableCustomHierarchyVisivilityButton);
                    Settings.EnableCustomHierarchyLockButton = EditorGUILayout.Toggle("Enable lock button",
                        Settings.EnableCustomHierarchyLockButton);
                    Settings.EnableCustomHierarchyTagLabel =
                        EditorGUILayout.Toggle("Enable tag label", Settings.EnableCustomHierarchyTagLabel);
                    Settings.EnableCustomHierarchyLayerLabel = EditorGUILayout.Toggle("Enable layer label",
                        Settings.EnableCustomHierarchyLayerLabel);
                }

                using (new NestedScope("Skinned Mesh Renderer", headerStyle))
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    ReorderableList.DoLayoutList();
                }
            }
        }
    }
}