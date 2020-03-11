using Controller;
using Entities;
using System.Windows;
using System.Windows.Controls;

namespace View.UserControls
{
    public partial class ActuatorMoveContinuouslyAxisX : UserControl
    {
        private MoveContinuouslyController mcc;
        private int listIndex;
        private ActuatorPositionSoftwareLimits apsl;

        public ActuatorMoveContinuouslyAxisX() { }

        public ActuatorMoveContinuouslyAxisX(MainController controller, ActuatorPositionSoftwareLimits apsl, MainControlsPanel mcp)
        {
            InitializeComponent();
            this.mcc = new MoveContinuouslyController(mcp, controller, apsl);
            this.listIndex = mcc.GetListIndexByAxis(Enums.Axis.X);
            this.apsl = apsl;
        }

        private void ButtonRetractAxisX_Click(object sender, RoutedEventArgs e)
        {
            mcc.Retract(listIndex, apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Min));
            //mcc.Retract(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_X_MinEdgePos));
        }

        private void ButtonStopAxisX_Click(object sender, RoutedEventArgs e)
        {
            mcc.SoftStop(listIndex);
        }

        private void ButtonExpandAxisX_Click(object sender, RoutedEventArgs e)
        {
            mcc.Expand(listIndex, apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Max));
            //mcc.Expand(listIndex, apsl.GetActEdgeValByAxis(Enums.ApslAxis.Axis_X_MaxEdgePos));
        }
    }
}
