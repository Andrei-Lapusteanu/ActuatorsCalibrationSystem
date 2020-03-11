using System;

namespace Entities
{
    [Serializable]
    public class ApslLimitsBundle
    {
        private int axis_X_MinEdgeStepsPosition;
        private int axis_X_MaxEdgeStepsPosition;
        private int axis_X_HomeStepsPosition;
        private int axis_Y_MinEdgeStepsPosition;
        private int axis_Y_MaxEdgeStepsPosition;
        private int axis_Y_HomeStepsPosition;
        private int axis_Z_MinEdgeStepsPosition;
        private int axis_Z_MaxEdgeStepsPosition;
        private int axis_Z_HomeStepsPosition;
        private int globalMinEdgeStepsPosition;
        private int globalMaxEdgeStepsPosition;

        public int Axis_X_MinEdgeStepsPosition { get => axis_X_MinEdgeStepsPosition; set => axis_X_MinEdgeStepsPosition = value; }
        public int Axis_X_MaxEdgeStepsPosition { get => axis_X_MaxEdgeStepsPosition; set => axis_X_MaxEdgeStepsPosition = value; }
        public int Axis_X_HomeStepsPosition { get => axis_X_HomeStepsPosition; set => axis_X_HomeStepsPosition = value; }
        public int Axis_Y_MinEdgeStepsPosition { get => axis_Y_MinEdgeStepsPosition; set => axis_Y_MinEdgeStepsPosition = value; }
        public int Axis_Y_MaxEdgeStepsPosition { get => axis_Y_MaxEdgeStepsPosition; set => axis_Y_MaxEdgeStepsPosition = value; }
        public int Axis_Y_HomeStepsPosition { get => axis_Y_HomeStepsPosition; set => axis_Y_HomeStepsPosition = value; }
        public int Axis_Z_MinEdgeStepsPosition { get => axis_Z_MinEdgeStepsPosition; set => axis_Z_MinEdgeStepsPosition = value; }
        public int Axis_Z_MaxEdgeStepsPosition { get => axis_Z_MaxEdgeStepsPosition; set => axis_Z_MaxEdgeStepsPosition = value; }
        public int Axis_Z_HomeStepsPosition { get => axis_Z_HomeStepsPosition; set => axis_Z_HomeStepsPosition = value; }
        public int GlobalMinEdgeStepsPosition { get => globalMinEdgeStepsPosition; set => globalMinEdgeStepsPosition = value; }
        public int GlobalMaxEdgeStepsPosition { get => globalMaxEdgeStepsPosition; set => globalMaxEdgeStepsPosition = value; }
    }
}
