using System;
using UnityEditor;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class NestedScope : IDisposable
    {
        public NestedScope()
        {
            EditorGUI.indentLevel++;
        }

        public NestedScope(string title, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(title, options);
            EditorGUI.indentLevel++;
        }

        public NestedScope(string title, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(title, style, options);
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}