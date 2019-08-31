using UnityEditor.IMGUI.Controls;

namespace IsuzuEditorExtension.AnimationClipCreator.BodyShapeEditTab
{
    public class MuscleTreeItem : TreeViewItem
    {
        public MuscleTreeItem(int internalId, int id, bool isExport, string muscleName, float muscleValue) :
            base(internalId)
        {
            this.MuscleElement = new MuscleElement(id, isExport, muscleName, muscleValue);
        }

        public MuscleElement MuscleElement { get; set; }
    }
}