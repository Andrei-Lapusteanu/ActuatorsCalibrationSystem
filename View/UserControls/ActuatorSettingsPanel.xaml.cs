using Controller;
using System.Windows.Controls;

namespace View.UserControls
{
    /// <summary>
    /// Interaction logic for ActuatorSettingsPanel.xaml
    /// </summary>
    public partial class ActuatorSettingsPanel : UserControl
    {
        ActuatorController actuator;

        public ActuatorSettingsPanel(ActuatorController actuator)
        {
            InitializeComponent();

            this.actuator = actuator;
        }
    }
}
