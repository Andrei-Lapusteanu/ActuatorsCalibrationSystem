using Controller;
using Entities;
using System.Windows.Controls;

namespace View.UserControls
{
    public class MoveContinuouslyController : UserControl
    {
        MainControlsPanel mcp;
        MainController controller;
        ActuatorPositionSoftwareLimits apsl;

        public MoveContinuouslyController(MainControlsPanel mcp, MainController controller, ActuatorPositionSoftwareLimits apsl)
        {
            this.mcp = mcp;
            this.controller = controller;
            this.apsl = apsl;
        }

        public int GetListIndexByAxis(Enums.Axis axis)
        {
            foreach (ActuatorController actuator in Actuators.List)
                if (actuator.Axis == axis)
                    return actuator.ListIndex;

            return 0;
        }

        public void Retract(int listIndex, int minPos)
        {
            if (mcp.TryConfiguringActuatorSettingsOneDevice(listIndex) == true)
                controller.ActuatorMoveContinuouslyLeft(controller.ActuatorInContext.DeviceID, minPos);
        }

        public void SoftStop(int listIndex)
        {
            controller.ChangeContext(listIndex);
            controller.ActuatorSoftStop(controller.ActuatorInContext.DeviceID);
        }

        public void Expand(int listIndex, int maxPos)
        {
            if (mcp.TryConfiguringActuatorSettingsOneDevice(listIndex) == true)
                controller.ActuatorMoveContinuouslyRight(controller.ActuatorInContext.DeviceID, maxPos);

        }
    }
}
