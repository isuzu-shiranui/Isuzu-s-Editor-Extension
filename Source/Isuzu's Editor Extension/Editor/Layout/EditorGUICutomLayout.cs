using System;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class EditorGUICustomLayout
    {
        /// <summary>
        ///     チェックボックスのスタイル。
        /// </summary>
        private static readonly GUIStyle SmallTickbox;

        /// <summary>
        ///     オプションアイコンの黒い版
        /// </summary>
        private static readonly Texture2D PaneOptionsIconDark;

        /// <summary>
        ///     オプションアイコンの白い版
        /// </summary>
        private static readonly Texture2D PaneOptionsIconLight;

        /// <summary>
        ///     黒ヘッダー
        /// </summary>
        private static readonly Color HeaderBackgroundDarkColor;

        /// <summary>
        ///     白ヘッダー
        /// </summary>
        private static readonly Color HeaderBackgroundLightColor;

        /// <inheritdoc />
        static EditorGUICustomLayout()
        {
            SmallTickbox = new GUIStyle("ShurikenToggle");

            PaneOptionsIconDark = (Texture2D) EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            PaneOptionsIconLight = (Texture2D) EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");

            HeaderBackgroundDarkColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);
            HeaderBackgroundLightColor = new Color(1f, 1f, 1f, 0.2f);
        }

        /// <summary>
        ///     ヘッダーのオプションアイコン。
        ///     エディタがPro版かで色を変える。
        /// </summary>
        private static Texture2D PaneOptionsIcon
        {
            get { return EditorGUIUtility.isProSkin ? PaneOptionsIconDark : PaneOptionsIconLight; }
        }

        /// <summary>
        ///     ヘッダーの背景色。
        ///     エディタがPro版かで色を変える。
        /// </summary>
        private static Color HeaderBackgroundColor
        {
            get { return EditorGUIUtility.isProSkin ? HeaderBackgroundDarkColor : HeaderBackgroundLightColor; }
        }

        /// <summary>
        ///     タイトル付きのカスタム折り畳みグループUI
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="foldField">たたまれているか</param>
        /// <param name="materialPropertyAction">グループ内のコンテンツ</param>
        public static bool PropertyFoldGroup(string title, bool foldField, Action materialPropertyAction)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, HeaderBackgroundColor);

            // Title
            EditorGUI.LabelField(labelRect, new GUIContent(title), EditorStyles.boldLabel);

            // foldout
            foldField = GUI.Toggle(foldoutRect, foldField, GUIContent.none, EditorStyles.foldout);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
                if (labelRect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                        foldField = !foldField;
                    e.Use();
                }

            if (foldField)
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    materialPropertyAction();
                }

            GUILayout.Space(1);

            return foldField;
        }

        /// <summary>
        ///     タイトル、トグル付きのカスタム折り畳みグループUI
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="foldField">たたまれているか</param>
        /// <param name="isActive"></param>
        /// <param name="materialPropertyAction">グループ内のコンテンツ</param>
        internal static void PropertyToggleFoldGroup(string title, ref bool foldField, ref bool isActive,
            Action materialPropertyAction)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var toggleRect = backgroundRect;
            toggleRect.x += 16f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;

            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, HeaderBackgroundColor);

            // Title
            using (new EditorGUI.DisabledScope(!isActive))
            {
                EditorGUI.LabelField(labelRect, new GUIContent(title), EditorStyles.boldLabel);
            }

            // foldout
            foldField = GUI.Toggle(foldoutRect, foldField, GUIContent.none, EditorStyles.foldout);

            // Active checkbox
            isActive = GUI.Toggle(toggleRect, isActive, GUIContent.none, SmallTickbox);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
                if (labelRect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                        foldField = !foldField;
                    e.Use();
                }

            if (foldField)
                using (new EditorGUI.DisabledScope(!isActive))
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    materialPropertyAction();
                }

            GUILayout.Space(1);
        }

        internal static void TabPanel(string title, Action materialPropertyAction)
        {
        }
    }
}