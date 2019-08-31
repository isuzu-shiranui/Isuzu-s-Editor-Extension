using System.Collections.Generic;
using System.Linq;
using IsuzuEditorExtension.Utils;
using UnityEditor;

namespace IsuzuEditorExtension
{
    internal sealed class EditorSettings
    {
        private const string SettingPref = "IsuzuEditorExtension.";

        private static readonly EditorSettings DefaultInstance = new EditorSettings();

        public static EditorSettings Default
        {
            get { return DefaultInstance; }
        }

        public bool EnableCustomHierarchyView
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyView), true);
            }
            set { EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyView), value); }
        }

        public bool EnableCustomHierarchyAlternateRowColor
        {
            get
            {
                return EditorPrefs.GetBool(
                    SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyAlternateRowColor), true);
            }
            set
            {
                EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyAlternateRowColor),
                    value);
            }
        }

        public bool EnableCustomHierarchyIcon
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyIcon), true);
            }
            set { EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyIcon), value); }
        }

        public bool EnableCustomHierarchyStaticButton
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyStaticButton),
                    true);
            }
            set
            {
                EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyStaticButton), value);
            }
        }

        public bool EnableCustomHierarchyVisivilityButton
        {
            get
            {
                return EditorPrefs.GetBool(
                    SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyVisivilityButton), true);
            }
            set
            {
                EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyVisivilityButton),
                    value);
            }
        }

        public bool EnableCustomHierarchyLockButton
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyLockButton),
                    true);
            }
            set
            {
                EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyLockButton), value);
            }
        }

        public bool EnableCustomHierarchyTagLabel
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyTagLabel),
                    true);
            }
            set { EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyTagLabel), value); }
        }

        public bool EnableCustomHierarchyLayerLabel
        {
            get
            {
                return EditorPrefs.GetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyLayerLabel),
                    true);
            }
            set
            {
                EditorPrefs.SetBool(SettingPref + Utility.NameOf(() => this.EnableCustomHierarchyLayerLabel), value);
            }
        }

        //---//

        public IEnumerable<string> SkinnedMeshRendererExcludes
        {
            get
            {
                return EditorPrefs
                    .GetString(SettingPref + Utility.NameOf(() => this.SkinnedMeshRendererExcludes), "vrc.")
                    .Split(',').Select(x => x.ToString());
            }
            set
            {
                EditorPrefs.SetString(
                    SettingPref + Utility.NameOf(() => this.SkinnedMeshRendererExcludes),
                    value.Aggregate((now, next) => string.Format("{0},{1}", now, next)));
            }
        }
    }
}