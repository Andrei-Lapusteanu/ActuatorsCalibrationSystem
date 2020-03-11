using Controller;
using System.Collections.Generic;
using System.Windows.Controls;

namespace View.UserControls
{
    /// <summary>
    /// Interaction logic for ActuatorParametersConfigurationPanel.xaml
    /// </summary>
    public partial class ActuatorParametersConfigurationPanel : UserControl
    {
        List<ActuatorController> actuators;

        public ActuatorParametersConfigurationPanel(List<ActuatorController> actuators)
        {
            InitializeComponent();

            this.actuators = actuators;
        }
    }
}
