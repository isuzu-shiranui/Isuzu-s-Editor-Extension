using System;

namespace IsuzuEditorExtension.AnimationClipCreator.BlendShapeEditTab
{
    [Serializable]
    public class BlendShapeTreeElement
    {
        public BlendShapeTreeElement(bool isExport, string blendShapeName, float blendShapeValue, string meshName,
            int blendShapeId)
        {
            this.IsExport = isExport;
            this.BlendShapeName = blendShapeName;
            this.BlendShapeValue = blendShapeValue;
            this.MeshName = meshName;
            this.BlendShapeId = blendShapeId;
        }

        public string MeshName { get; set; }

        public bool IsExport { get; set; }

        public string BlendShapeName { get; set; }

        public float BlendShapeValue { get; set; }

        public int BlendShapeId { get; set; }
    }
}