using Controller;
using Entities;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using View.UserControls;

namespace View
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PointInter
        {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out PointInter lpPoint);

        static MainController controller;
        string lastPressedNavBarButton = "buttonMainControlsPage";

        MainControlsPanel mainControlsPanel;
        ActuatorSettingsPanel actuatorOneSettings;
        ActuatorSettingsPanel actuatorTwoSettings;
        ActuatorSettingsPanel actuatorThreeSettings;
        ActuatorParametersConfigurationPanel actuatorParametersConfigurationPanel;
        ApplicationSettingsPanel appSettingsPanel;
        UIBrushes uiBrushes;

        public MainWindow()
        {
            InitializeComponent();

            controller = new MainController();
            uiBrushes = new UIBrushes();

            InitNavigationBar();
            InitUserControlPanels();
        }

        private void InitNavigationBar()
        {
            // Color Indicator
            rectangleSelectedPanel.Fill = uiBrushes.SelectedPanelIndicatorTint;

            switch (controller.Actuators.Count)
            {
                case 0:
                    buttonFirstCOMSettings.Visibility = Visibility.Hidden;
                    buttonSecondCOMSettings.Visibility = Visibility.Hidden;
                    buttonThirdCOMSettings.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    buttonSecondCOMSettings.Visibility = Visibility.Hidden;
                    buttonThirdCOMSettings.Visibility = Visibility.Hidden;
                    break;

                case 2:
                    buttonThirdCOMSettings.Visibility = Visibility.Hidden;
                    break;
            }

            InitNavigationBarButtonsText(controller.Actuators.Count);
        }

        private void InitNavigationBarButtonsText(int deviceCount)
        {
            switch (deviceCount)
            {
                case 1:
                    textBlockFirstCOMSettingsDeviceName.Text = Actuators.List[0].UIName + " Settings";
                    break;

                case 2:
                    textBlockFirstCOMSettingsDeviceName.Text = Actuators.List[0].UIName + " Settings";
                    textBlockSecondCOMSettingsDeviceName.Text = Actuators.List[1].UIName + " Settings";
                    break;

                case 3:
                    textBlockFirstCOMSettingsDeviceName.Text = Actuators.List[0].UIName + " Settings";
                    textBlockSecondCOMSettingsDeviceName.Text = Actuators.List[1].UIName + " Settings";
                    textBlockThirdCOMSettingsDeviceName.Text = Actuators.List[2].UIName + " Settings";
                    break;
            }
        }

        private void InitUserControlPanels()
        {
            // Init main control panel
            mainControlsPanel = new MainControlsPanel(controller, controller.Actuators.Count);

            // Implicitly switch to main control panel view
            SwitchUserControl(mainControlsPanel);

            // For each actuator found, create a new settings user control
            switch (controller.Actuators.Count)
            {
                case 1:
                    actuatorOneSettings = new ActuatorSettingsPanel(Actuators.List[0]);
                    break;

                case 2:
                    actuatorOneSettings = new ActuatorSettingsPanel(Actuators.List[0]);
                    actuatorTwoSettings = new ActuatorSettingsPanel(Actuators.List[1]);
                    break;

                case 3:
                    actuatorOneSettings = new ActuatorSettingsPanel(Actuators.List[0]);
                    actuatorTwoSettings = new ActuatorSettingsPanel(Actuators.List[1]);
                    actuatorThreeSettings = new ActuatorSettingsPanel(Actuators.List[2]);
                    break;
            }

            // Init data visualizer
            actuatorParametersConfigurationPanel = new ActuatorParametersConfigurationPanel(Actuators.List);

            // Init app settings
            appSettingsPanel = new ApplicationSettingsPanel();
        }

        private void AnimateNavigationBar(bool isFromNavigationButton)
        {
            double? tempTo = -1;

            // The navigation menu button cand expand or retract the navigation menu after every click
            // All other buttons in the navigation menu work a bit differently:
            //      If a button is clicked and the navigation menu is retracted, it stays retracted
            //      If a button is clicked and the navigation menu is expanded, it will retract
            //      bool isFromNavigationButton helps in determining from which type of button the click came from
            if (isFromNavigationButton == false)
            {
                tempTo = 40;
                acrylicPanelNavbar.TintOpacity = .2;
            }
            else if (gridNavBar.ActualWidth > 40)
            {
                acrylicPanelNavbar.TintOpacity = .2;
                tempTo = 40;
            }
            else
            {
                acrylicPanelNavbar.TintOpacity = .8;
                tempTo = 245;
            }

            ElasticEase ea = new ElasticEase
            {
                EasingMode = EasingMode.EaseOut,
                Oscillations = 2,
                Springiness = 12
            };

            DoubleAnimation doubleAnim = new DoubleAnimation
            {
                From = gridNavBar.ActualWidth,
                To = tempTo,
                Duration = TimeSpan.FromMilliseconds(700),
                EasingFunction = ea
            };

            gridNavBar.BeginAnimation(Grid.WidthProperty, doubleAnim);
            acrylicPanelNavbar.BeginAnimation(Grid.WidthProperty, doubleAnim);
            stackPanelMainNavigationBar.BeginAnimation(StackPanel.WidthProperty, doubleAnim);
            canvasNavBar.BeginAnimation(Canvas.WidthProperty, doubleAnim);
            buttonApplicationSettings.BeginAnimation(Button.WidthProperty, doubleAnim);
        }

        private void AnimateIndicator(double topMargin)
        {
            ElasticEase ea = new ElasticEase
            {
                EasingMode = EasingMode.EaseOut,
                Oscillations = 2,
                Springiness = 6
            };

            DoubleAnimation doubleAnim = new DoubleAnimation
            {
                From = Canvas.GetTop(rectangleSelectedPanel),
                To = topMargin,
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = ea
            };

            rectangleSelectedPanel.BeginAnimation(Canvas.TopProperty, doubleAnim);
            Canvas.SetTop(rectangleSelectedPanel, topMargin);
        }

        public void SwitchUserControl(UserControl nextUserControl)
        {
            gridMainControlPanel.Children.Clear();
            gridMainControlPanel.Children.Add(nextUserControl);
        }

        private double CalculateNewIndicatorPosition(Button selectedButton)
        {
            // Setting this arbitrarily because if not, Canvas.Gettop() which is called later on will return NaN
            Canvas.SetTop(rectangleSelectedPanel, 0);

            Thickness buttonPosition = new Thickness(0, selectedButton.TransformToAncestor(canvasNavBar).Transform(new Point(0, 0)).Y, 0, 0);

            return buttonPosition.Top = (buttonPosition.Top + (selectedButton.ActualHeight - rectangleSelectedPanel.ActualHeight) / 2) - 65;
        }

        private void ProcessViewChange(UserControl selectedUserControl, Button selectedButton)
        {
            SwitchUserControl(selectedUserControl);

            AnimateIndicator(CalculateNewIndicatorPosition(selectedButton));
            AnimateNavigationBar(false);
        }

        private void ButtonNavbarMenu_Click(object sender, RoutedEventArgs e)
        {
            AnimateNavigationBar(true);
        }

        // Stupid function because of stupid FluentWPF NuGet implementation
        private void MoveCursor()
        {
            GetCursorPos(out PointInter pos);

            SetCursorPos(pos.X + 1, pos.Y);
        }

        private void ButtonMainControlsPage_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonMainControlsPage")
            {
                ProcessViewChange(mainControlsPanel, buttonMainControlsPage);
                MoveCursor();
            }

            lastPressedNavBarButton = "buttonMainControlsPage";
        }

        private void ButtonParametersConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonParametersConfiguration")
                ProcessViewChange(actuatorParametersConfigurationPanel, buttonParametersConfiguration);

            lastPressedNavBarButton = "buttonParametersConfiguration";
        }

        private void ButtonFirstCOMSettings_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonFirstCOMSettings")
                ProcessViewChange(actuatorOneSettings, buttonFirstCOMSettings);

            lastPressedNavBarButton = "buttonFirstCOMSettings";
        }

        private void ButtonSecondCOMSettings_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonSecondCOMSettings")
                ProcessViewChange(actuatorTwoSettings, buttonSecondCOMSettings);

            lastPressedNavBarButton = "buttonSecondCOMSettings";
        }

        private void ButtonThirdCOMSettings_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonThirdCOMSettings")
                ProcessViewChange(actuatorThreeSettings, buttonThirdCOMSettings);

            lastPressedNavBarButton = "buttonThirdCOMSettings";
        }

        private void ButtonApplicationSettings_Click(object sender, RoutedEventArgs e)
        {
            if (lastPressedNavBarButton != "buttonApplicationSettings")
                ProcessViewChange(appSettingsPanel, buttonApplicationSettings);

            lastPressedNavBarButton = "buttonApplicationSettings";
        }

        private void ButtonExitApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonMaximizeApp_Click(object sender, RoutedEventArgs e)
        {
            //if (this.WindowState == WindowState.Normal)
            //{
            //    this.WindowState = WindowState.Maximized;
            //    textBlockMaximizeApplicationIcon.Text = "&#xE921;";
            //}
            //else
            //{
            //    this.WindowState = WindowState.Normal;
            //    textBlockMaximizeApplicationIcon.Text = "&#xE921;";
            //}

        }

        private void ButtonMinimizeApp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainControlsPanel.HandleActuatorSettingsUncheck();
            mainControlsPanel.AppConfigManager.SaveAllToConfiguration();
        }

        // Used to programatically scroll down notifications at start-up
        // Necessary because actual height of elements is unknown until rendered on screen
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            mainControlsPanel.HandleVideoFeedProcessingAlgorithmSelectionChange();
            mainControlsPanel.ManageCanvasScrolling(Enums.ScrollDirection.Down);
        }
    }
}

/*
 *  // Add the main control panel to the list
            userControlsList.Add(new UserControls.MainControlsPanel(controller));

            // Implicitly switch to its view
            SwitchUserControl(userControlsList[0]);

            // For each actuator found, create a new settings user control, and add it to the list
            for(int i = 0; i < controller.Actuators.Count; i++)
                userControlsList.Add(new UserControls.ActuatorSettingsPanel(controller.Actuators.List[i]));

            // Add the data visualizer user control to the list
            userControlsList.Add(new UserControls.ActuatorParametersConfigurationPanel(controller.Actuators.List));

            // Add the app settings user control to the list
            userControlsList.Add(new UserControls.ApplicationSettingsPanel());
*/

/*            BounceEase b = new BounceEase
        {
            Bounces = 3,
            Bounciness = 10,
            EasingMode = EasingMode.EaseOut
        };
*/
