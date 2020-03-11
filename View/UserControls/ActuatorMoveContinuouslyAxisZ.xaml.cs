using Controller;
using Entities;
using System.Windows;
using System.Windows.Controls;

namespace View.UserControls
{
    public partial class ActuatorMoveContinuouslyAxisZ : UserControl
    {
        private MoveContinuouslyController mcc;
        private int listIndex;
        private ActuatorPositionSoftwareLimits apsl;

        public ActuatorMoveContinuouslyAxisZ() { }

        public ActuatorMoveContinuouslyAxisZ(MainController controller, ActuatorPositionSoftwareLimits apsl, MainControlsPanel mcp)
        {
            InitializeComponent();
            this.mcc = new MoveContinuouslyController(mcp, controller, apsl);
            this.listIndex = mcc.GetListIndexByAxis(Enums.Axis.Z);
            this.apsl = apsl;
        }

        private void ButtonRetractAxisZ_Click(object sender, RoutedEventArgs e)
        {
            mcc.Retract(listIndex, apsl.GetActEdgeVal(Enums.Axis.Z, Enums.ApslEdgeType.Min));
            //mcc.Retract(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_Z_MinEdgePos));
        }

        private void ButtonStopAxisZ_Click(object sender, RoutedEventArgs e)
        {
            mcc.SoftStop(listIndex);
        }

        private void ButtonExpandAxisZ_Click(object sender, RoutedEventArgs e)
        {
            mcc.Expand(listIndex, apsl.GetActEdgeVal(Enums.Axis.Z, Enums.ApslEdgeType.Max));
            //mcc.Expand(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_Z_MaxEdgePos));
        }
    }
}
