using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IsuzuEditorExtension
{
    public static class EditorResource
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture(string textureName)
        {
            if (Textures.ContainsKey(textureName)) return Textures[textureName];

            var texture = GetAsset<Texture2D>(textureName);
            if (texture == null) throw new Exception(string.Format("{0} not found.", textureName));

            Textures.Add(textureName, texture);
            return texture;
        }

        private static T GetAsset<T>(string assetName) where T : Object
        {
            var guid = AssetDatabase.FindAssets(assetName).FirstOrDefault();
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        }
    }
}