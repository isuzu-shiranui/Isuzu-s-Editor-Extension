using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension
{
    public static class CustomHierarchyView
    {
        private static readonly MethodInfo GetIconForObject = typeof(EditorGUIUtility)
            .GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly EditorSettings Settings = EditorSettings.Default;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
        }

        private static void OnGUI(int instanceID, Rect selectionRect)
        {
            if (!Settings.EnableCustomHierarchyView) return;
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;

            AlternateRow(selectionRect);

            DrawIcon(selectionRect, gameObject);

            var rect = new Rect(selectionRect)
            {
                width = 16
            };
            rect.x += selectionRect.width;

            DrawLayerTagLabel(rect, gameObject);
            DrawLockButton(rect, gameObject);
            DrawVisibilityButton(rect, gameObject);
            DrawStaticButton(rect, gameObject);
        }

        private static void AlternateRow(Rect selectionRect)
        {
            if (!Settings.EnableCustomHierarchyAlternateRowColor) return;

            var index = (int) (selectionRect.y - 4) / 16;

            if (index % 2 == 0) return;

            var pos = new Rect(selectionRect)
            {
                x = 0,
                xMax = selectionRect.xMax
            };

            var color = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 2.0f) : new Color(0, 0, 0, 0.1f);
            GUI.Box(pos, string.Empty);
            GUI.color = color;
        }

        private static void DrawIcon(Rect selectionRect, GameObject gameObject)
        {
            if (!Settings.EnableCustomHierarchyIcon) return;
            var icon = GetIconForObject.Invoke(null, new object[] {gameObject}) as Texture;
            if (icon == null) return;

            var pos = new Rect(selectionRect)
            {
                x = selectionRect.x - 29,
                width = 14
            };

            GUI.DrawTexture(pos, icon, ScaleMode.ScaleToFit, true);
        }

        private static void DrawLayerTagLabel(Rect selectionRect, GameObject gameObject)
        {
            var tag = gameObject.tag;
            var layer = gameObject.layer;

            if (tag == "Untagged" && layer == 0) return;

            var layerName = LayerMask.LayerToName(layer);

            var style = new GUIStyle
            {
                normal =
                {
                    textColor = EditorGUIUtility.isProSkin
                        ? new Color(0.8f, 0.8f, 0.8f)
                        : new Color(0.1f, 0.1f, 0.1f)
                },
                fontSize = 8,
                alignment = TextAnchor.UpperRight,
                clipping = TextClipping.Clip
            };

            var tagWidth = tag == "Untagged" || !Settings.EnableCustomHierarchyTagLabel
                ? 0
                : style.CalcSize(new GUIContent(tag)).x;
            var layerWidth = layer == 0 || !Settings.EnableCustomHierarchyLayerLabel
                ? 0
                : style.CalcSize(new GUIContent(layerName)).x;


            var pos = new Rect(selectionRect)
            {
                height = 17,
                width = tagWidth > layerWidth ? tagWidth : layerWidth
            };
            pos.x -= 48;

            pos.width += 4;
            pos.x -= pos.width;


            EditorGUI.DrawRect(pos, AlternateRowColor(pos));

            pos.y -= 1;
            pos.width -= 4;
            pos.x += 2;

            if ((layer == 0 || !Settings.EnableCustomHierarchyLayerLabel) && tag != "Untagged" &&
                Settings.EnableCustomHierarchyTagLabel)
            {
                pos.y += 4;
                EditorGUI.LabelField(pos, tag, style);
            }
            else if (layer != 0 && (tag == "Untagged" || !Settings.EnableCustomHierarchyTagLabel) &&
                     Settings.EnableCustomHierarchyLayerLabel)
            {
                pos.y += 4;
                EditorGUI.LabelField(pos, layerName, style);
            }
            else if (Settings.EnableCustomHierarchyTagLabel && Settings.EnableCustomHierarchyLayerLabel)
            {
                EditorGUI.LabelField(pos, tag, style);
                pos.y += 8;
                EditorGUI.LabelField(pos, layerName, style);
                pos.y -= 7;
            }
        }

        private static void DrawLockButton(Rect selectionRect, GameObject gameObject)
        {
            if (!Settings.EnableCustomHierarchyLockButton) return;

            var pos = new Rect(selectionRect)
            {
                width = 16
            };
            pos.x -= 16;

            EditorGUI.DrawRect(pos, AlternateRowColor(pos));
            var isLock = (gameObject.hideFlags & HideFlags.NotEditable) != 0;
            if (GUI.Toggle(pos, isLock, string.Empty, "IN LockButton"))
                gameObject.hideFlags |= HideFlags.NotEditable;
            else
                gameObject.hideFlags &= ~HideFlags.NotEditable;
        }

        private static void DrawVisibilityButton(Rect selectionRect, GameObject gameObject)
        {
            if (!Settings.EnableCustomHierarchyVisivilityButton) return;
            var icon = gameObject.activeSelf
                ? EditorGUIUtility.IconContent("VisibilityOn")
                : EditorGUIUtility.IconContent("VisibilityOff");
            if (icon == null) return;

            var pos = new Rect(selectionRect);
            pos.x -= 32;
            pos.width = 16;

            EditorGUI.DrawRect(pos, AlternateRowColor(pos));
            if (GUI.Button(pos, icon, new GUIStyle())) gameObject.SetActive(!gameObject.activeSelf);
        }

        private static void DrawStaticButton(Rect selectionRect, GameObject gameObject)
        {
            if (!Settings.EnableCustomHierarchyStaticButton) return;
            var icon = gameObject.isStatic
                ? EditorResource.GetTexture("static_on")
                : EditorResource.GetTexture("static_off");
            if (icon == null) return;

            var pos = new Rect(selectionRect);
            pos.x -= 48;
            pos.width = 16;

            EditorGUI.DrawRect(pos, AlternateRowColor(pos));
            GUI.DrawTexture(pos, icon, ScaleMode.ScaleToFit, true);

            if (GUI.Button(pos, icon, new GUIStyle())) gameObject.isStatic = !gameObject.isStatic;
        }

        private static Color AlternateRowColor(Rect selectionRect)
        {
            var color = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.761f, 0.761f, 0.761f);
            if ((int) (selectionRect.y - 4) / 16 % 2 > 0 && Settings.EnableCustomHierarchyAlternateRowColor)
                color = EditorGUIUtility.isProSkin ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.691f, 0.691f, 0.691f);

            return color;
        }
    }
}