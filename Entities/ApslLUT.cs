using System.Collections.Generic;

namespace Entities
{
    public class ApslLUT
    {
        private Dictionary<Enums.ApslAxis, int> edges;

        public ApslLUT(ApslLimitsBundle apslBundle)
        {
            Edges = new Dictionary<Enums.ApslAxis, int>
            {
                { Enums.ApslAxis.Axis_X_MinEdgePos, apslBundle.Axis_X_MinEdgeStepsPosition },
                { Enums.ApslAxis.Axis_X_MaxEdgePos, apslBundle.Axis_X_MaxEdgeStepsPosition },
                { Enums.ApslAxis.Axis_X_HomeEdgePos, apslBundle.Axis_X_HomeStepsPosition },

                { Enums.ApslAxis.Axis_Y_MinEdgePos, apslBundle.Axis_Y_MinEdgeStepsPosition },
                { Enums.ApslAxis.Axis_Y_MaxEdgePos, apslBundle.Axis_Y_MaxEdgeStepsPosition },
                { Enums.ApslAxis.Axis_Y_HomeEdgePos, apslBundle.Axis_Y_HomeStepsPosition },

                { Enums.ApslAxis.Axis_Z_MinEdgePos, apslBundle.Axis_Z_MinEdgeStepsPosition },
                { Enums.ApslAxis.Axis_Z_MaxEdgePos, apslBundle.Axis_Z_MaxEdgeStepsPosition },
                { Enums.ApslAxis.Axis_Z_HomeEdgePos, apslBundle.Axis_Z_HomeStepsPosition }
            };
        }

        public Dictionary<Enums.ApslAxis, int> Edges { get => edges; set => edges = value; }
    }
}
