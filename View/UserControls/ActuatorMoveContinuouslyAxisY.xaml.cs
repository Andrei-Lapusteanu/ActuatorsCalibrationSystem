using Controller;
using Entities;
using System.Windows;
using System.Windows.Controls;

namespace View.UserControls
{
    public partial class ActuatorMoveContinuouslyAxisY : UserControl
    {
        private MoveContinuouslyController mcc;
        private int listIndex;
        private ActuatorPositionSoftwareLimits apsl;
        public ActuatorMoveContinuouslyAxisY() { }

        public ActuatorMoveContinuouslyAxisY(MainController controller, ActuatorPositionSoftwareLimits apsl, MainControlsPanel mcp)
        {
            InitializeComponent();
            this.mcc = new MoveContinuouslyController(mcp, controller, apsl);
            this.listIndex = mcc.GetListIndexByAxis(Enums.Axis.Y);
            this.apsl = apsl;
        }

        private void ButtonRetractAxisY_Click(object sender, RoutedEventArgs e)
        {
            mcc.Retract(listIndex, apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Min));
            //mcc.Retract(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_Y_MinEdgePos));
        }

        private void ButtonStopAxisY_Click(object sender, RoutedEventArgs e)
        {
            mcc.SoftStop(listIndex);
        }

        private void ButtonExpandAxisY_Click(object sender, RoutedEventArgs e)
        {
            mcc.Expand(listIndex, apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Max));
            //mcc.Expand(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_Y_MaxEdgePos));
        }
    }
}
