using System;

namespace IsuzuEditorExtension.AnimationClipCreator.BodyShapeEditTab
{
    [Serializable]
    public class MuscleElement
    {
        public MuscleElement(int muscleId, bool isExport, string muscleName, float muscleValue)
        {
            this.MuscleId = muscleId;
            this.IsExport = isExport;
            this.MuscleName = muscleName;
            this.MuscleValue = muscleValue;
        }

        public int MuscleId { get; set; }

        public bool IsExport { get; set; }

        public string MuscleName { get; set; }

        public float MuscleValue { get; set; }
    }
}