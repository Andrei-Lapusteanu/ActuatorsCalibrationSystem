using System.Windows.Controls;

namespace View.UserControls
{
    /// <summary>
    /// Interaction logic for ControlTitleBar.xaml
    /// </summary>
    public partial class ControlTitleBar : UserControl
    {
        private UIBrushes uiBrushes;

        public ControlTitleBar()
        {
            this.InitializeComponent();
            uiBrushes = new UIBrushes();

            titleGradientOne.Color = uiBrushes.MediumBordeaux;
            titleGradientTwo.Color = uiBrushes.DarkerBordeaux;
        }

        public string TextBlockTitleBar
        {
            get { return textBlockControlName.Text; }
            set { textBlockControlName.Text = value; }
        }
    }
}
