using Controller;
using Entities;
using ImageProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ximc;

namespace View.UserControls
{
    /// <summary>
    /// Interaction logic for MainControlsPanel.xaml
    /// </summary>
    public partial class MainControlsPanel : UserControl
    {

        #region Members

        MainController controller;

        ApplicationConfigurationManager appConfigManager;
        ActuatorPositionSoftwareLimits apsl;
        TextboxAxisLinkLUT textBoxAxisLink;

        BackgroundWorker bgWorkerGetStatsForUI;
        BackgroundWorker bgWorkerFeedProcessedImage;
        BackgroundWorker bgWorkerNotificationPolling;

        public ImageProcessingCore IPcore;

        UIBrushes uiBrushes;

        int deviceCount;
        bool virtualMode = false;
        bool isSoftwareInit = true;

        private int comboBoxOneIndex = -1;
        private int comboBoxTwoIndex = -1;
        private int comboBoxThreeIndex = -1;

        private static readonly Regex _signedRegex = new Regex("[^0-9-]");
        private static readonly Regex _unsignedRegex = new Regex("[^0-9]");

        int notificationsRowIndex = 1;
        //TO DO - ILog notifLogger = LogManager.GetLogger("NotificationLog");

        // TO DO
        static readonly object _lockerEnum = new object();
        static readonly object _lockerGetCount = new object();

        public ActuatorPositionSoftwareLimits Apsl { get => apsl; set => apsl = value; }
        public ApplicationConfigurationManager AppConfigManager { get => appConfigManager; set => appConfigManager = value; }
        public TextboxAxisLinkLUT TextBoxAxisLink { get => textBoxAxisLink; set => textBoxAxisLink = value; }

        #endregion Members

        #region Constructor

        public MainControlsPanel(MainController controller, int deviceCount)
        {
            InitializeComponent();
            this.controller = controller;
            this.IPcore = new ImageProcessingCore();
            this.uiBrushes = new UIBrushes();
            this.AppConfigManager = new ApplicationConfigurationManager(this);
            this.textBoxAxisLink = new TextboxAxisLinkLUT();
            this.Apsl = new ActuatorPositionSoftwareLimits();
            this.deviceCount = deviceCount;
            InitializeMainControlsPanel();
        }

        #endregion Constructor

        #region Background Workers

        void bgWorkerFeedProcessedImage_DoWork(object obj, DoWorkEventArgs e)
        {
            BitmapSource bmpSource = null;
            System.Drawing.Point centerPoint = new System.Drawing.Point();
            int distance = 0;

            //BitmapSource bmpSrc;

            //while (true)
            //{
            //    bmpSrc = engWrapper.GetNewFrame();
            //    bmpSrc.Freeze();
            //    bgWorkerFeedProcessedImage.ReportProgress(0, bmpSrc);
            //}

            while (bgWorkerFeedProcessedImage.CancellationPending == false)
            {
                if (IPcore.VideoFeedSettings.IsEnabled == true)
                {
                    if (IPcore.IDCSettings.IsEnabled == true)
                        ImageDistanceCalibrationAlgorithm(ref bmpSource, ref distance);

                    else if (IPcore.TASettings.IsEnabled == true)
                        ThresholdingAlgorithms(ref bmpSource, ref centerPoint);

                    else
                        ThresholdingAlgorithms(ref bmpSource, ref centerPoint);
                }
            }
        }

        void bgWorkerFeedProcessedImage_ProgressChanged(object obj, ProgressChangedEventArgs e)
        {
            masterImg.Source = (BitmapSource)e.UserState;
        }

        void bgWorkerFeedProcessedImage_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs e)
        {
            masterImg.Source = null;
            textBlockVideoFeedFPS.Text = "FPS: 0";
        }

        void bgWorkerGetStatsForUI_DoWork(object obj, DoWorkEventArgs e)
        {
            while (bgWorkerGetStatsForUI.CancellationPending == false)
            {
                bgWorkerGetStatsForUI.ReportProgress(0);
                Thread.Sleep(25);
            }
        }

        // TO DO
        int device = -1;
        void bgWorkerGetStatsForUI_ProgressChanged(object obj, ProgressChangedEventArgs e)
        {
            device++;
            controller.ChangeContext(device);

            GetCurrentPositionForAxis(controller.ActuatorInContext);

            if (device == 0)
                GetStatsForUI(controller.ActuatorInContext, controller.ActuatorInContext.DeviceID, Enums.UIDevice.One);

            else if (device == 1)
                GetStatsForUI(controller.ActuatorInContext, controller.ActuatorInContext.DeviceID, Enums.UIDevice.Two);

            else if (device == 2)
                GetStatsForUI(controller.ActuatorInContext, controller.ActuatorInContext.DeviceID, Enums.UIDevice.Three);

            if (device == Actuators.List.Count - 1)
                device = -1;
        }

        void bgWorkerGetStatsForUI_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs e)
        {
            Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Cannot retrieve actuator data for UI, background worker shutting down"));
        }

        void bgWorkerNotificationPolling_DoWork(object obj, DoWorkEventArgs e)
        {
            while (bgWorkerNotificationPolling.CancellationPending == false)
            {
                bgWorkerNotificationPolling.ReportProgress(0);
                Thread.Sleep(50);
            }
        }

        void bgWorkerNotificationPolling_ProgressChanged(object obj, ProgressChangedEventArgs e)
        {
            // Notifications that are created outside the View are polled from here
            try
            {
                foreach (Notification notif in Notification.NotifList)
                {
                    if (!notif.WasDisplayed)
                    {
                        NotifyUI(notif.NotifType, notif.NotifString);
                        notif.WasDisplayed = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); /*TO DO*/
            }
        }

        void bgWorkerNotificationPolling_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs e)
        {

        }

        #endregion

        #region Events

        #region Background Worker (bgWorkerGetStatsForUI) Events

        private void GetStatsForUI(ActuatorController actuator, int deviceID, Enums.UIDevice uiDevice)
        {
            try
            {
                switch (uiDevice)
                {
                    case Enums.UIDevice.One:
                        textBlockActuatorStatusSpeedDeviceOne.Text = controller.ActuatorInContext.GetCurrentSpeedSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroSpeedDeviceOne.Text = controller.ActuatorInContext.GetCurrentSpeedMicrosteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusPositionDeviceOne.Text = controller.ActuatorInContext.GetCurrentPositionSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroPositionDeviceOne.Text = controller.ActuatorInContext.GetCurrentPositionMicroSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusAxisDeviceOne.Text = controller.ActuatorInContext.Axis.ToString();

                        textBlockPowerStatusVoltageDeviceOne.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).Upwr / 100).ToString("0.00");
                        textBlockPowerStatusCurrentDeviceOne.Text = actuator.GetStatusCalibration(deviceID, actuator.Calibration).Ipwr.ToString();
                        textBlockPowerStatusTemperatureDeviceOne.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).CurT / 10).ToString("0");

                        break;

                    case Enums.UIDevice.Two:
                        textBlockActuatorStatusSpeedDeviceTwo.Text = controller.ActuatorInContext.GetCurrentSpeedSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroSpeedDeviceTwo.Text = controller.ActuatorInContext.GetCurrentSpeedMicrosteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusPositionDeviceTwo.Text = controller.ActuatorInContext.GetCurrentPositionSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroPositionDeviceTwo.Text = controller.ActuatorInContext.GetCurrentPositionMicroSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusAxisDeviceTwo.Text = controller.ActuatorInContext.Axis.ToString();

                        textBlockPowerStatusVoltageDeviceTwo.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).Upwr / 100).ToString("0.00");
                        textBlockPowerStatusCurrentDeviceTwo.Text = actuator.GetStatusCalibration(deviceID, actuator.Calibration).Ipwr.ToString();
                        textBlockPowerStatusTemperatureDeviceTwo.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).CurT / 10).ToString("0");

                        break;

                    case Enums.UIDevice.Three:
                        textBlockActuatorStatusSpeedDeviceThree.Text = controller.ActuatorInContext.GetCurrentSpeedSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroSpeedDeviceThree.Text = controller.ActuatorInContext.GetCurrentSpeedMicrosteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusPositionDeviceThree.Text = controller.ActuatorInContext.GetCurrentPositionSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusMicroPositionDeviceThree.Text = controller.ActuatorInContext.GetCurrentPositionMicroSteps(controller.ActuatorInContext.DeviceID).ToString();
                        textBlockActuatorStatusAxisDeviceThree.Text = controller.ActuatorInContext.Axis.ToString();

                        textBlockPowerStatusVoltageDeviceThree.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).Upwr / 100).ToString("0.00");
                        textBlockPowerStatusCurrentDeviceThree.Text = actuator.GetStatusCalibration(deviceID, actuator.Calibration).Ipwr.ToString();
                        textBlockPowerStatusTemperatureDeviceThree.Text = (actuator.GetStatusCalibration(deviceID, actuator.Calibration).CurT / 10).ToString("0");

                        break;
                }

                // BBC19
                // Do work for bounding box panel
                if (comboBoxVideoFeedCalibrationAlgorithm.SelectedIndex == 2)
                {
                    HandleBoundingBoxButtonValidity();
                    BoundingBoxUpdateUI();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //TO DO - move
        int axisXPos = 0;
        int axisYPos = 0;
        private void GetCurrentPositionForAxis(ActuatorController actuator)
        {
            switch (actuator.Axis)
            {
                case Enums.Axis.X:
                    axisXPos = actuator.GetCurrentPositionSteps(actuator.DeviceID);
                    break;

                case Enums.Axis.Y:
                    axisYPos = actuator.GetCurrentPositionSteps(actuator.DeviceID);
                    break;
            }
        }

        #endregion

        #region ChecBox Apply for all Events

        private void CheckBoxActuatorSettingApplyForAll_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxActuatorSettingApplyForAll.IsChecked == true)
            {
                ChangeActuatorSettingsDeviceCountUI(1);
                SetGlobalEdgeLimitsUI(false);
            }
            else
            {
                HandleActuatorSettingsUncheck();
            }

            HandleTextBoxTypingSafetyAfterEdgeSet();
            //CheckEveryTextBoxTypingSafety();
            SetSliderValueBindings();
        }

        public void HandleActuatorSettingsUncheck()
        {
            ChangeActuatorSettingsDeviceCountUI(controller.Actuators.Count);

            // This is a small patch
            textBoxActuatorSettingsMinEdgeOne.Text = Apsl.GetDeviceOneMinEgde();
            textBoxActuatorSettingsMaxEdgeOne.Text = Apsl.GetDeviceOneMaxEgde();

            SetEdgeLimitsPerAxisUI();
        }

        // TO DO - aranajeaza
        private void CheckBoxMoveRelativelyApplyForAll_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyComboBoxApplyForAllAction();
        }

        #endregion

        #region Connection Settings Events

        private void ComboBoxConnectionSettingsHandleSwap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxConnectionSettingsHandleSwap();

            // If software is initializing, beacuse not all UI elements are filled, SetEdgeLimitsPerAxisUI() 
            // will reset Apsl.LimBundle to 0, which is unwanted
            if (isSoftwareInit == true)
                SetEdgeLimitsPerAxisUI(true);
            else
                SetEdgeLimitsPerAxisUI(false);
        }

        private void checkBoxConnectionSettingsLockSetZero_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxConnectionSettingsLockSetZero.IsChecked == true)
            {
                buttonConnectionSettingsSetZeroDeviceOne.IsEnabled = false;
                buttonConnectionSettingsSetZeroDeviceTwo.IsEnabled = false;
                buttonConnectionSettingsSetZeroDeviceThree.IsEnabled = false;

                Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Locked setting of zero position"));

            }
            else
            {
                buttonConnectionSettingsSetZeroDeviceOne.IsEnabled = true;
                buttonConnectionSettingsSetZeroDeviceTwo.IsEnabled = true;
                buttonConnectionSettingsSetZeroDeviceThree.IsEnabled = true;

                Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Be very careful when setting zero position! Improperly setting value could cause physical damage!"));
            }
        }

        private void buttonConnectionSettingsSetZeroDeviceOne_Click(object sender, RoutedEventArgs e)
        {
            controller.ChangeContext(0);
            controller.ActuatorSetZero(controller.ActuatorInContext.DeviceID);
        }

        private void buttonConnectionSettingsSetZeroDeviceTwo_Click(object sender, RoutedEventArgs e)
        {
            controller.ChangeContext(1);
            controller.ActuatorSetZero(controller.ActuatorInContext.DeviceID);
        }

        private void buttonConnectionSettingsSetZeroDeviceThree_Click(object sender, RoutedEventArgs e)
        {
            controller.ChangeContext(2);
            controller.ActuatorSetZero(controller.ActuatorInContext.DeviceID);
        }

        #endregion

        #region Move Relatively One Device Events

        private void ButtonMoveToPositionOneDevice_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Actuators.List.Count; i++)
                MoveToPositionOneDeviceEventHandling(textBoxMoveToPositionStepsOneDevice.Text, textBoxMoveToPositionMicroStepsOneDevice.Text, i, textBoxMoveToPositionStepsOneDevice);
        }

        private void ButtonShiftOnOneDevice_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < controller.Actuators.Count; i++)
                ShiftOnPositionOneDeviceEventhandling(textBoxShiftToStepsOneDevice.Text, textBoxShiftToMicroStepsOneDevice.Text, i, textBoxShiftToStepsOneDevice);
        }

        #endregion

        #region Move Relatively Multiple Devices Events

        private void ButtonMoveRelativelyDeviceOne_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyMultipleDevices(buttonMoveRelativelyDeviceOne, textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, 0);
        }

        private void ButtonMoveRelativelyDeviceTwo_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyMultipleDevices(buttonMoveRelativelyDeviceTwo, textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, 1);
        }

        private void ButtonMoveRelativelyDeviceThree_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyMultipleDevices(buttonMoveRelativelyDeviceThree, textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, 2);
        }

        private void buttonMoveRelativelyHomeDeviceOne_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyHomeMultipleDevices(comboBoxConnectionSettingsDeviceOne.SelectedIndex, 0);
        }

        private void buttonMoveRelativelyHomeDeviceTwo_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyHomeMultipleDevices(comboBoxConnectionSettingsDeviceTwo.SelectedIndex, 1);
        }

        private void buttonMoveRelativelyHomeDeviceThree_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyHomeMultipleDevices(comboBoxConnectionSettingsDeviceThree.SelectedIndex, 2);
        }

        private void buttonMoveRelativelyHomeAllDevices_Click(object sender, RoutedEventArgs e)
        {
            HandleMoveRelativelyHomeAllDevices();
        }

        private void ComboBoxMoveRelativelyDeviceOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleMoveRelativelyButtonTextChange(comboBoxMoveRelativelyDeviceOne, buttonMoveRelativelyDeviceOne);
        }

        private void ComboBoxMoveRelativelyDeviceTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleMoveRelativelyButtonTextChange(comboBoxMoveRelativelyDeviceTwo, buttonMoveRelativelyDeviceTwo);

        }

        private void ComboBoxMoveRelativelyDeviceThree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleMoveRelativelyButtonTextChange(comboBoxMoveRelativelyDeviceThree, buttonMoveRelativelyDeviceThree);
        }

        #endregion

        #region Set Device Edges Events

        private void buttonActuatorSettingsSetEdgeValues_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxActuatorSettingApplyForAll.IsChecked == true)
                SetGlobalEdgeLimitsUI(true);
            else if (checkBoxActuatorSettingApplyForAll.IsChecked == false)
            {
                SetEdgeLimitsPerAxisUI();
            }

            SetSliderValueBindings();
        }


        #endregion

        #region Text Box Events

        // Handle textBox input (for unsigned values)
        private void TextBoxHandleUnsignedValue(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _unsignedRegex.IsMatch(e.Text);
        }

        // Handle textBox input (for signed values)
        private void TextBoxHandleSignedValue(object sender, TextCompositionEventArgs e)
        {
            CheckSignedTextboxValidityAtPreviewInput(e);
        }

        // Disallow space key
        private void TextBoxSpaceKeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // Make sure unallowed characters are not pasted into unsigned value textBoxes
        private void TextBoxUnsignedPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (_unsignedRegex.IsMatch(text) == true)
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        // Make sure unallowed characters are not pasted into signed value textBoxes
        private void TextBoxSignedPastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (_signedRegex.IsMatch(text) == true || CheckSignedTextboxValidityAtPaste(text, e) == false)
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        private void TextBoxMoveRelativelyWarningHandler(object sender, TextChangedEventArgs e)
        {
            TextBox senderTextBox = (sender as TextBox);
            string textBoxUid = senderTextBox.Uid;
            string textBoxName = senderTextBox.Name;

            if (controller.Actuators.Count == 1 || checkBoxMoveRelativelyApplyForAll.IsChecked == true || textBoxUid == "OneDevice")
            {
                if (senderTextBox.Name == "textBoxMoveToPositionStepsOneDevice" || senderTextBox.Name == "textBoxMoveToPositionMicroStepsOneDevice")
                    HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveToPositionStepsOneDevice, textBoxMoveToPositionMicroStepsOneDevice, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveToPositionOneDevice);

                else if (senderTextBox.Name == "textBoxShiftToStepsOneDevice" || senderTextBox.Name == "textBoxShiftToMicroStepsOneDevice")
                    HandleMoveRelativelyTextBoxTypingSafety(textBoxShiftToStepsOneDevice, textBoxShiftToMicroStepsOneDevice, Enums.TextBoxValueSafetyType.ShiftOn, buttonShiftOnOneDevice);
            }
            else if (controller.Actuators.Count > 1)
            {
                switch (comboBoxMoveRelativelyDeviceOne.SelectedIndex)
                {
                    case 0:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceOne);
                        break;

                    case 1:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceOne);
                        break;
                }

                switch (comboBoxMoveRelativelyDeviceTwo.SelectedIndex)
                {
                    case 0:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceTwo);
                        break;

                    case 1:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceTwo);
                        break;
                }

                switch (comboBoxMoveRelativelyDeviceThree.SelectedIndex)
                {
                    case 0:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceThree);
                        break;

                    case 1:
                        HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceThree);
                        break;
                }
            }
        }

        private void TextBoxActuatorSettingsSpeedValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.Speed);
        }

        private void TextBoxActuatorSettingsMicroSpeedValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.MicroSpeed);
        }

        private void TextBoxActuatorSettingsAccelerationValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.Acceleration);
        }

        private void TextBoxActuatorSettingsDecelerationValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.Deceleration);
        }

        private void TextBoxActuatorSettingsMinEdgeValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxEdgeTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.MinEgde);
        }

        private void TextBoxActuatorSettingsMaxEdgeValidityHandling(object sender, TextChangedEventArgs e)
        {
            HandleActuatorSettingsTextBoxEdgeTypingSafety(e.Source as TextBox, Enums.TextBoxActuatorSettingsType.MaxEdge);
        }

        private void BoundingBoxTextBoxValidityHandlingAxisXTopLeft(object sender, TextChangedEventArgs e)
        {
            HandleBoundingBoxTextBoxTypingSafety(e.Source as TextBox, Enums.Axis.X, Enums.BoundingBoxCorner.TopLeft);
        }

        private void BoundingBoxTextBoxValidityHandlingAxisXBottomRight(object sender, TextChangedEventArgs e)
        {
            HandleBoundingBoxTextBoxTypingSafety(e.Source as TextBox, Enums.Axis.X, Enums.BoundingBoxCorner.BottomRight);
        }

        private void BoundingBoxTextBoxValidityHandlingAxisYTopLeft(object sender, TextChangedEventArgs e)
        {
            HandleBoundingBoxTextBoxTypingSafety(e.Source as TextBox, Enums.Axis.Y, Enums.BoundingBoxCorner.TopLeft);
        }

        private void BoundingBoxTextBoxValidityHandlingAxisYBottomRight(object sender, TextChangedEventArgs e)
        {
            HandleBoundingBoxTextBoxTypingSafety(e.Source as TextBox, Enums.Axis.Y, Enums.BoundingBoxCorner.BottomRight);
        }

        #endregion

        #region Image Processing Settings Events

        private void SliderImageProcessingSettingsPrecision_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TASettings != null)
            {
                IPcore.TASettings.Precision = IPcore.TASettings.CalculatePrecision(sliderImageProcessingSettingsPrecision.Value);
                IPcore.TASettings.CurrentPrecisionSliderValue = sliderImageProcessingSettingsPrecision.Value;
            }
        }

        private void SliderImageProcessingSettingsStaticThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                IPcore.TASettings.StaticThresholdValue = (int)sliderImageProcessingSettingsStaticThreshold.Value;
        }

        private void SliderImageProcessingSettingsQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                IPcore.TASettings.Quality = (int)sliderImageProcessingSettingsQuality.Value;
        }

        private void SliderImageProcessingSettingsBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                IPcore.TASettings.Brightness = (int)sliderImageProcessingSettingsBrightness.Value;
        }

        private void ButtonImageProcessingSettingsResetPrecision_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                sliderImageProcessingSettingsPrecision.Value = IPcore.TASettings.ResetPrecision();
        }

        private void ButtonImageProcessingSettingsResetStaticThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                sliderImageProcessingSettingsStaticThreshold.Value = (int)IPcore.TASettings.ResetStaticThresholdValue();
        }

        private void ButtonImageProcessingSettingsResetQuality_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TASettings != null)
                sliderImageProcessingSettingsQuality.Value = IPcore.TASettings.ResetQuality();
        }

        private void ButtonImageProcessingSettingsResetBrightness_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TASettings != null)
            {
                sliderImageProcessingSettingsBrightness.Value = IPcore.TASettings.ResetBrightness();
            }
        }

        #endregion

        #region Tracking Settings Events

        private void SlideTrackingSettingsPixelTolerance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TrackingSettings != null)
                IPcore.TrackingSettings.PixelTolerance = (int)(sliderTrackingSettingsPixelTolerance.Value + 1);
        }

        private void SlideTrackingSettingsTrackingSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IPcore != null && IPcore.TrackingSettings != null)
                IPcore.TrackingSettings.FrameSkipCount = (int)(sliderTrackingSettingsTrackingSpeed.Value + 1);
        }

        private void ButtonTrackingSettingsResetPixelTolerance_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TrackingSettings != null)
                sliderTrackingSettingsPixelTolerance.Value = IPcore.TrackingSettings.ResetPixelTolerance();
        }

        private void ButtonTrackingSettingsResetTrackingSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore != null && IPcore.TrackingSettings != null)
                sliderTrackingSettingsTrackingSpeed.Value = IPcore.TrackingSettings.ResetFrameSkipCount() - 1;
        }

        #endregion

        #region Video Feed Events

        private void ButtonStartStopVideoFeed_Click(object sender, RoutedEventArgs e)
        {
            HandleVideoFeedStartStop(IPcore.VideoFeedSettings.IsEnabled, e);
        }

        private void ComboBoxVideoFeedProcessingAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { HandleVideoFeedProcessingAlgorithmSelectionChange(); }
            catch { /*TO DO*/ }
        }

        private void comboBoxVideoFeedCalibrationAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { HandleVideoFeedCalibrationAlgorithmSelectionChange(); }
            catch { /*TO DO*/ }
        }

        private void comboBoxVideoFeedCalibrationForTracking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleVideoFeedTrackingAlgorithmSelectionChange(e);
        }

        private void ButtonStartStopTracking_Click(object sender, RoutedEventArgs e)
        {
            HandleTrackingStartStop(IPcore.TrackingSettings.IsEnabled, e);
        }

        private void ButtonCaptureImage_Click(object sender, RoutedEventArgs e)
        {
            HandleVideoFeedCapture();
        }

        private void buttonOpenCaptureFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenVideoFeedCaptureFolder();
        }

        private void ButtonCenterImage_Click(object sender, RoutedEventArgs e)
        {
            HandleVideoFeedRecentering();
        }


        private void buttonVideoFeedMirrorHorizontally_Click(object sender, RoutedEventArgs e)
        {
            if(IPcore.VideoFeedSettings.IsMirroredY == true)
            {
                IPcore.VideoFeedSettings.IsMirroredY = false;
                buttonVideoFeedMirrorHorizontally.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/mirror_x_off.png", UriKind.Relative)));
            }
            else
            {
                IPcore.VideoFeedSettings.IsMirroredY = true;
                buttonVideoFeedMirrorHorizontally.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/mirror_x_on.png", UriKind.Relative)));
            }
        }

        private void buttonVideoFeedMirrorVertically_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore.VideoFeedSettings.IsMirroredX == true)
            {
                IPcore.VideoFeedSettings.IsMirroredX = false;
                buttonVideoFeedMirrorVertically.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/mirror_y_off.png", UriKind.Relative)));
            }
            else
            {
                IPcore.VideoFeedSettings.IsMirroredX = true;
                buttonVideoFeedMirrorVertically.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/mirror_y_on.png", UriKind.Relative)));
            }
        }

        private void buttonVideoFeedInvertImage_Click(object sender, RoutedEventArgs e)
        {
            if (IPcore.VideoFeedSettings.IsInverted == true)
            {
                IPcore.VideoFeedSettings.IsInverted = false;
                buttonVideoFeedInvertImage.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/invert_off.png", UriKind.Relative)));
            }
            else
            {
                IPcore.VideoFeedSettings.IsInverted = true;
                buttonVideoFeedInvertImage.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/invert_on.png", UriKind.Relative)));
            }
        }

        private void buttonVideoFeedProcessedImage_Click(object sender, RoutedEventArgs e)
        {
            if(IPcore.VideoFeedSettings.IsProcessedImageFed == true)
            {
                IPcore.VideoFeedSettings.IsProcessedImageFed = false;
                buttonVideoFeedProcessedImage.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/wand_off.png", UriKind.Relative)));
            }
            else
            {
                IPcore.VideoFeedSettings.IsProcessedImageFed = true;
                buttonVideoFeedProcessedImage.Background = new ImageBrush(new BitmapImage(new Uri("../../View/img/wand_on.png", UriKind.Relative)));
            }
        }

        //private void CheckBoxVideoFeedShowProcessedImage_Click(object sender, RoutedEventArgs e)
        //{
        //    if (checkBoxVideoFeedShowProcessedImage.IsChecked == true)
        //        IPcore.VideoFeedSettings.IsProcessedImageFed = true;
        //    else
        //        IPcore.VideoFeedSettings.IsProcessedImageFed = false;
        //}

        private void ButtonDistanceCalibrationSet_Click(object sender, RoutedEventArgs e)
        {
            IPcore.IDCSettings.IsCalibrated = true;
            IPcore.IDCSettings.IsEnabled = false;
            IPcore.TASettings.IsEnabled = true;

            buttonDistanceCalibrationSet.IsEnabled = false;
            buttonDistanceCalibrationSet.Background = uiBrushes.Gray;

            CalculateCalibrationDistances(IPcore.IDCSettings.DistancePixels);
        }

        private void ButtonVideoFeedSettingsSet_Click(object sender, RoutedEventArgs e)
        {
            HandleVideoFeedImageSizeSettings();
        }

        //private void checkBoxVideoFeedMirrorImageHorizontally_Click(object sender, RoutedEventArgs e)
        //{
        //    if (checkBoxVideoFeedMirrorImageHorizontally.IsChecked == true)
        //        IPcore.VideoFeedSettings.IsMirroredY = true;
        //    else
        //        IPcore.VideoFeedSettings.IsMirroredY = false;
        //}

        //private void checkBoxVideoFeedMirrorImageVertically_Click(object sender, RoutedEventArgs e)
        //{
        //    if (checkBoxVideoFeedMirrorImageVertically.IsChecked == true)
        //        IPcore.VideoFeedSettings.IsMirroredX = true;
        //    else
        //        IPcore.VideoFeedSettings.IsMirroredX = false;
        //}

        //private void CheckBoxVideoFeedShowInvertImage_Click(object sender, RoutedEventArgs e)
        //{
        //    if (checkBoxVideoFeedShowInvertImage.IsChecked == true)
        //        IPcore.TASettings.IsInverted = true;
        //    else
        //        IPcore.TASettings.IsInverted = false;
        //}

        #endregion

        #region Bounding Box Calibration Events

        private void buttonBoundingBoxSetTopLeft_Click(object sender, RoutedEventArgs e)
        {
            SetBoundingBoxCorner(Enums.BoundingBoxCorner.TopLeft, sender);
        }

        private void buttonBoundingBoxSetBottomRight_Click(object sender, RoutedEventArgs e)
        {
            SetBoundingBoxCorner(Enums.BoundingBoxCorner.BottomRight, sender);
        }

        private void checkBoxBBCManual_Click(object sender, RoutedEventArgs e)
        {
            SetBoundingBoxValidity();
        }

        private void buttonBBCReset_Click(object sender, RoutedEventArgs e)
        {
            IPcore.BoundingBoxCalib.ResetBoundingBox();

            VideoFeedCheckCalibratedAlgorithms();

            ToggleManualEdgeSettings(Enums.ToggleType.Enabled);

            ToggleBoundingBoxButtonsValidity(Enums.BoundingBoxCorner.TopLeft, Enums.ToggleType.Enabled);
            ToggleBoundingBoxButtonsValidity(Enums.BoundingBoxCorner.BottomRight, Enums.ToggleType.Enabled);
        }

        #endregion

        #endregion Events

        #region Methods

        #region Initializers

        private void InitializeMainControlsPanel()
        {
            try
            {
                // TO DO  - comment
                AppConfigManager.LoadInitFromConfiguration();
                GetConnectionSettingsComboBoxSelectedItemIndex();
                ArrangeComponents(deviceCount);
                SetLabelsText();
                SetSliderValueBindings();
                SetBoundingBoxValidity();

                GenerateTextBoxPairs();
                ComboBoxConnectionSettingsHandleSwap();

                InitializebgWorkers();
                CheckEveryTextBoxTypingSafety();

                // USB CAMERA
                //new Thread(() =>
                //{
                //    IPcore.StartCapture();
                //}).Start();

                // DCAM
                IPcore.StartHamamatsu();

                IPcore.TASettings.SetSliderValues((int)sliderImageProcessingSettingsPrecision.Value,
                                     (int)sliderImageProcessingSettingsQuality.Value,
                                     (int)sliderImageProcessingSettingsBrightness.Value);
                IPcore.TASettings.SetConnectedAxis(controller.Actuators);
                IPcore.TrackingSettings.SetDefaultValues((int)sliderTrackingSettingsPixelTolerance.Value,
                                                  (int)sliderTrackingSettingsTrackingSpeed.Value);

                HandleVideoFeedStartStop(IPcore.VideoFeedSettings.IsEnabled, null);
                HandleTrackingStartStop(IPcore.TrackingSettings.IsEnabled, null);

                AppConfigManager.LoadTextBoxValuesFromConfig();
                LoadApslValuesToUI();
                VideoFeedCheckCalibratedAlgorithms();

                isSoftwareInit = false;
                SetEdgeLimitsPerAxisUI(false);
                HandleActuatorSettingsTextBoxEdgeValueValidityForAll();
                HandleMoveRelativelyComboBoxApplyForAllAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void InitializebgWorkers()
        {
            bgWorkerGetStatsForUI = new BackgroundWorker();
            bgWorkerFeedProcessedImage = new BackgroundWorker();
            bgWorkerNotificationPolling = new BackgroundWorker();

            bgWorkerGetStatsForUI.DoWork += new DoWorkEventHandler(bgWorkerGetStatsForUI_DoWork);
            bgWorkerGetStatsForUI.ProgressChanged += new ProgressChangedEventHandler(bgWorkerGetStatsForUI_ProgressChanged);
            bgWorkerGetStatsForUI.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerGetStatsForUI_RunWorkerCompleted);
            bgWorkerGetStatsForUI.WorkerReportsProgress = true;
            bgWorkerGetStatsForUI.WorkerSupportsCancellation = true;

            bgWorkerFeedProcessedImage.DoWork += new DoWorkEventHandler(bgWorkerFeedProcessedImage_DoWork);
            bgWorkerFeedProcessedImage.ProgressChanged += new ProgressChangedEventHandler(bgWorkerFeedProcessedImage_ProgressChanged);
            bgWorkerFeedProcessedImage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerFeedProcessedImage_RunWorkerCompleted);
            bgWorkerFeedProcessedImage.WorkerReportsProgress = true;
            bgWorkerFeedProcessedImage.WorkerSupportsCancellation = true;

            bgWorkerNotificationPolling.DoWork += new DoWorkEventHandler(bgWorkerNotificationPolling_DoWork);
            bgWorkerNotificationPolling.ProgressChanged += new ProgressChangedEventHandler(bgWorkerNotificationPolling_ProgressChanged);
            bgWorkerNotificationPolling.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerNotificationPolling_RunWorkerCompleted);
            bgWorkerNotificationPolling.WorkerReportsProgress = true;
            bgWorkerNotificationPolling.WorkerSupportsCancellation = true;

            bgWorkerGetStatsForUI.RunWorkerAsync();
            bgWorkerNotificationPolling.RunWorkerAsync();
            //bgWorkerFeedProcessedImage.RunWorkerAsync();
        }

        #endregion

        #region Image Processing

        private void ThresholdingAlgorithms(ref BitmapSource bmpSource, ref System.Drawing.Point centerPoint)
        {
            // Processing time counter
            Stopwatch s = Stopwatch.StartNew();

            // Grab processed image and coordinates
            (bmpSource, centerPoint) = IPcore.StartThresholdingAlgorithms(MapStepsToImage());

            // Stop time counter
            s.Stop();

            // Check if image returned is valid
            if (bmpSource != null)
            {
                // Make immutable by freezing bits, remove any thread accessing issues
                bmpSource.Freeze();

                // Check tracking point set buttom validity
                CheckImageDistanceSetTrackingPointButtonValidity(centerPoint);

                // Update video feed
                bgWorkerFeedProcessedImage.ReportProgress(0, bmpSource);

                // Release frozen bitmap - dirty, but prevents memory leak
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Set currenly detected point
                IPcore.TrackingSettings.CurrentlyDetectedPoint = centerPoint;

                // Tracking algorithm (both axis)
                TrackingPrototype(centerPoint, (int)bmpSource.PixelHeight, (int)bmpSource.PixelWidth);

                // Update image processing settings
                UpdateImageProcessingSettings(s, ref bmpSource);
            }

            // If frame is invalid (null)
            else
                HandleNullFrameReturn();
        }

        private void ImageDistanceCalibrationAlgorithm(ref BitmapSource bmpSource, ref int distance)
        {
            // Processing time counter
            Stopwatch s = Stopwatch.StartNew();

            // Grab processed image and distance
            (bmpSource, distance) = IPcore.StartImageDistanceCalibration();

            // Stop time counter
            s.Stop();

            // Check if image returned is valid
            if (bmpSource != null)
            {
                // Make immutable by freezing bits, remove any thread accessing issues
                bmpSource.Freeze();

                // Check calibration button validity
                if (IPcore.IDCSettings.IsCalibrated == false)
                    CheckImageCalibrationButtonValidity(distance);

                // Update image processing settings
                UpdateImageCalibrationSettingsPixelDistanceUI(distance);

                // Update video feed
                bgWorkerFeedProcessedImage.ReportProgress(0, bmpSource);

                // Release frozen bitmap - dirty, but prevents memory leak
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Update image processing settings
                UpdateImageProcessingSettings(s, ref bmpSource);
            }

            // If frame is invalid (null)
            else
                HandleNullFrameReturn();
        }

        private void UpdateImageProcessingSettings(Stopwatch s, ref BitmapSource bmpSource)
        {
            IPcore.VideoFeedSettings.FrameCount++;
            IPcore.VideoFeedSettings.FrameTimeMs = s.ElapsedMilliseconds;
            IPcore.VideoFeedSettings.FramesPerSecond = IPcore.VideoFeedSettings.CalculateFramesPerSecond(IPcore.VideoFeedSettings.FrameTimeMs);

            IPcore.VideoFeedSettings.ImageHeight = (int)bmpSource.PixelHeight;
            IPcore.VideoFeedSettings.ImageWidth = (int)bmpSource.PixelWidth;

            if (IPcore.VideoFeedSettings.FrameCount % IPcore.VideoFeedSettings.FramesPerSecondUpdateSkip == 0)
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        textBlockVideoFeedFPS.Text = "FPS: " + IPcore.VideoFeedSettings.FramesPerSecond.ToString("0.00");
                    });
                }
                catch { /*TO DO*/ }
        }

        public BitmapImage BitmapToBitmapImage(ref Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bmpImage = new BitmapImage();

            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

            bmpImage.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            bmpImage.StreamSource = memoryStream;
            bmpImage.EndInit();
            bmpImage.Freeze();

            return bmpImage;
        }

        private void HandleNullFrameReturn()
        {
            IPcore.StopCapture();
            HandleVideoFeedStartStop(IPcore.VideoFeedSettings.IsEnabled = false, null);
            HandleTrackingStartStop(IPcore.TrackingSettings.IsEnabled = false, null);
            Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Could not retrieve frame from capturing device. Please check connection"));
        }

        #endregion

        #region Tracking Prototype

        private void TrackingPrototype(System.Drawing.Point trackingPoint, int imgHeight, int imgWidth)
        {
            // If tracking is enabled
            if (IPcore.TrackingSettings.IsEnabled == true)
            {
                // TO DO - Document and integrate in processing granulation
                // Do not track every frame => do not stress actuator
                // FrameSkipCount > 0 indicates how many frames to skip until actuator should track point
                if (++IPcore.TrackingSettings.FrameCount % IPcore.TrackingSettings.FrameSkipCount == 0)
                {
                    // Get image center point
                    System.Drawing.Point imgCenter = new System.Drawing.Point(imgWidth / 2, imgWidth / 2);

                    // For each connected actuator
                    foreach (ActuatorController actuator in Actuators.List)
                    {
                        switch (actuator.Axis)
                        {
                            // If found actuator corresponds to axis X, track accordingly
                            case Enums.Axis.X:
                                TrackingAxisX(trackingPoint, imgCenter, imgWidth);
                                break;

                            // If found actuator corresponds to axis Y, track accordingly
                            case Enums.Axis.Y:
                                TrackingAxisY(trackingPoint, imgCenter, imgHeight);
                                break;
                        }
                    }
                    // Update latest center pixel point
                    IPcore.TrackingSettings.LastestCenterPixelPoint = trackingPoint;
                }
            }
        }

        private void TrackingAxisX(System.Drawing.Point trackingPoint, System.Drawing.Point imgCenter, int imgWidth)
        {
            // Do not call tracking method if center has not changed its position
            if (IPcore.TrackingSettings.LastestCenterPixelPoint.X != trackingPoint.X || IPcore.TrackingSettings.JustChangedAxis == true)
            {
                // Apply pixel tolerance - by what pixel distance actuator should move
                if (trackingPoint.X + IPcore.TrackingSettings.PixelTolerance <= (IPcore.TrackingSettings.LastestTrackedPixelPoint.X) ||
                    trackingPoint.X - IPcore.TrackingSettings.PixelTolerance >= (IPcore.TrackingSettings.LastestTrackedPixelPoint.X) || IPcore.TrackingSettings.JustChangedAxis == true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (comboBoxVideoFeedCalibrationForTracking.Text == "Distance calibration")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, IPcore.IDCSettings.BoxTopLeft.X, IPcore.IDCSettings.BoxBottomRight.X, Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Max)), 0);

                        if (comboBoxVideoFeedCalibrationForTracking.Text == "Bounding box calib.")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, IPcore.BoundingBoxCalib.TopLeftPixelPos.X, IPcore.BoundingBoxCalib.BottomRightPixelPos.X, Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Max)), 0);

                        else if (comboBoxVideoFeedCalibrationForTracking.Text == "None")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, 0, imgWidth, Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.X, Enums.ApslEdgeType.Max)), 0);

                        // trackingPoint.X == int.MinValue mean that there were no contorurs found => actuator remains in last known position
                        if (trackingPoint.X != int.MinValue)
                            try
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    controller.ChangeContextByAxis(Enums.Axis.X);
                                    MoveToPositionOneDeviceEventHandling(IPcore.TrackingSettings.LastestTrackedStepsPoint.X.ToString(), 0.ToString(), 0, null, Enums.Axis.X);
                                });
                            }
                            catch { /*TO DO*/ }
                    });
                }
                IPcore.TrackingSettings.JustChangedAxis = false;
            }
        }

        private void TrackingAxisY(System.Drawing.Point trackingPoint, System.Drawing.Point imgCenter, int imgHeight)
        {
            // Do not call tracking method if center has not changed its position
            if (IPcore.TrackingSettings.LastestCenterPixelPoint.Y != trackingPoint.Y || IPcore.TrackingSettings.JustChangedAxis == true)
            {
                // Apply pixel tolerance - by what pixel distance actuator should move
                if (trackingPoint.Y + IPcore.TrackingSettings.PixelTolerance <= (IPcore.TrackingSettings.LastestTrackedPixelPoint.Y) ||
                    trackingPoint.Y - IPcore.TrackingSettings.PixelTolerance >= (IPcore.TrackingSettings.LastestTrackedPixelPoint.Y) || IPcore.TrackingSettings.JustChangedAxis == true)
                {

                    this.Dispatcher.Invoke(() =>
                    {

                        if (comboBoxVideoFeedCalibrationForTracking.Text == "Distance calibration")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, MapRange(trackingPoint.Y, IPcore.IDCSettings.BoxTopLeft.Y, IPcore.IDCSettings.BoxBottomRight.Y, Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Max)));

                        else if (comboBoxVideoFeedCalibrationForTracking.Text == "Bounding box calib.")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, MapRange(trackingPoint.Y, IPcore.BoundingBoxCalib.TopLeftPixelPos.Y, IPcore.BoundingBoxCalib.BottomRightPixelPos.Y, Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Max)));

                        else if (comboBoxVideoFeedCalibrationForTracking.Text == "None")
                            IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, MapRange(trackingPoint.Y, 0, imgHeight, Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Min), Apsl.GetActEdgeVal(Enums.Axis.Y, Enums.ApslEdgeType.Max)));

                        // trackingPoint.X == int.MinValue mean that there were no contorurs found => actuator remains in last known position
                        if (trackingPoint.Y != int.MinValue)
                            try
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    controller.ChangeContextByAxis(Enums.Axis.Y);
                                    MoveToPositionOneDeviceEventHandling(IPcore.TrackingSettings.LastestTrackedStepsPoint.Y.ToString(), 0.ToString(), 0, null, Enums.Axis.Y);
                                });
                            }
                            catch { /*TO DO*/ }
                    });
                }
                IPcore.TrackingSettings.JustChangedAxis = false;
            }
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="trackingPoint"></param>
        /// <param name="imgCenter"></param>
        /// <param name="imgHeight"></param>
        private void TrackingPrototypeAxisX(System.Drawing.Point trackingPoint, System.Drawing.Point imgCenter, int imgHeight)
        {
            // Do not call tracking method if center has not changed its position
            if (IPcore.TrackingSettings.LastestCenterPixelPoint.X != trackingPoint.X || IPcore.TrackingSettings.JustChangedAxis == true)
            {
                // Apply pixel tolerance - by what pixel distance actuator should move
                if (trackingPoint.X + IPcore.TrackingSettings.PixelTolerance <= (IPcore.TrackingSettings.LastestTrackedPixelPoint.X) ||
                    trackingPoint.X - IPcore.TrackingSettings.PixelTolerance >= (IPcore.TrackingSettings.LastestTrackedPixelPoint.X) || IPcore.TrackingSettings.JustChangedAxis == true)
                {
                    if (trackingPoint.X < imgCenter.X)
                        //IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X));//, 0, imgCenter.X, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices / 2), 0);
                        IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, 0, imgCenter.X, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices / 2), 0);

                    else if (trackingPoint.X == imgCenter.X)
                        IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, 0);

                    else if (trackingPoint.X > imgCenter.X)
                        IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, imgCenter.X, imgHeight, Apsl.MaxEdgePositionStepsAllDevices / 2, Apsl.MaxEdgePositionStepsAllDevices), 0);

                    //IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, 1023, 736, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices), 0);


                    // trackingPoint.X == int.MinValue mean that there were no contorurs found => actuator remains in last known position
                    if (trackingPoint.X != int.MinValue)
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                controller.ChangeContextByAxis(Enums.Axis.X);
                                MoveToPositionOneDeviceEventHandling(IPcore.TrackingSettings.LastestTrackedStepsPoint.X.ToString(), 0.ToString());
                            });
                        }
                        catch { /*TO DO*/ }
                }
                IPcore.TrackingSettings.JustChangedAxis = false;
            }
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="trackingPoint"></param>
        /// <param name="imgCenter"></param>
        /// <param name="imgWidth"></param>
        private void TrackingPrototypeAxisY(System.Drawing.Point trackingPoint, System.Drawing.Point imgCenter, int imgWidth)
        {
            // Do not call tracking method if center has not changed its position
            if (IPcore.TrackingSettings.LastestCenterPixelPoint.Y != trackingPoint.Y || IPcore.TrackingSettings.JustChangedAxis == true)
            {
                // Apply pixel tolerance - by what pixel distance actuator should move
                if (trackingPoint.Y + IPcore.TrackingSettings.PixelTolerance <= (IPcore.TrackingSettings.LastestTrackedPixelPoint.Y) ||
                    trackingPoint.Y - IPcore.TrackingSettings.PixelTolerance >= (IPcore.TrackingSettings.LastestTrackedPixelPoint.Y) || IPcore.TrackingSettings.JustChangedAxis == true)
                {
                    //if (trackingPoint.Y < imgCenter.Y)
                    //    IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, MapRange(trackingPoint.Y, 0, imgCenter.Y, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices / 2));

                    //else if (trackingPoint.Y == imgCenter.Y)
                    //    IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, 0);

                    //else if (trackingPoint.Y > imgCenter.Y)
                    //    IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(0, MapRange(trackingPoint.Y, imgCenter.Y, imgWidth, Apsl.MaxEdgePositionStepsAllDevices / 2, Apsl.MaxEdgePositionStepsAllDevices));

                    IPcore.TrackingSettings.LastestTrackedStepsPoint = new System.Drawing.Point(MapRange(trackingPoint.X, 736, 1023, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices), 0);

                    // trackingPoint.Y == int.MinValue mean that there were no contorurs found => actuator remains in last known position
                    if (trackingPoint.Y != int.MinValue)
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                controller.ChangeContextByAxis(Enums.Axis.Y);
                                MoveToPositionOneDeviceEventHandling(IPcore.TrackingSettings.LastestTrackedStepsPoint.Y.ToString(), 0.ToString());
                            });
                        }
                        catch { /*TO DO*/ }
                }
                IPcore.TrackingSettings.JustChangedAxis = false;
            }
        }


        /// <summary>
        /// REDO
        /// </summary>
        /// <returns></returns>
        private System.Drawing.Point MapStepsToImage()
        {
            this.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < Actuators.List.Count; i++)
                {
                    ActuatorController actuator = Actuators.List[i];

                    switch (actuator.Axis)
                    {
                        case Enums.Axis.X:
                            if (comboBoxVideoFeedCalibrationForTracking.Text == "Distance calibration")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(MapRange(axisXPos, Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition, IPcore.IDCSettings.BoxTopLeft.X, IPcore.IDCSettings.BoxBottomRight.X),
                                                                                                 IPcore.TrackingSettings.LastestTrackedPixelPoint.Y);

                            else if (comboBoxVideoFeedCalibrationForTracking.Text == "Bounding box calib.")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(MapRange(axisXPos, Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition, IPcore.BoundingBoxCalib.TopLeftPixelPos.X, IPcore.BoundingBoxCalib.BottomRightPixelPos.X),
                                                                                         IPcore.TrackingSettings.LastestTrackedPixelPoint.Y);
                            else if (comboBoxVideoFeedCalibrationForTracking.Text == "None")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(MapStepsToImagePerAxis(axisXPos, IPcore.VideoFeedSettings.ImageWidth, Enums.Axis.X),
                                                                                         IPcore.TrackingSettings.LastestTrackedPixelPoint.Y);
                            break;

                        case Enums.Axis.Y:
                            if (comboBoxVideoFeedCalibrationForTracking.Text == "Distance calibration")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(IPcore.TrackingSettings.LastestTrackedPixelPoint.X,
                                                                                        MapRange(axisYPos, Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition, IPcore.IDCSettings.BoxTopLeft.Y, IPcore.IDCSettings.BoxBottomRight.Y));

                            else if (comboBoxVideoFeedCalibrationForTracking.Text == "Bounding box calib.")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(IPcore.TrackingSettings.LastestTrackedPixelPoint.X,
                                                                                        MapRange(axisYPos, Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition, IPcore.BoundingBoxCalib.TopLeftPixelPos.Y, IPcore.BoundingBoxCalib.BottomRightPixelPos.Y));

                            else if (comboBoxVideoFeedCalibrationForTracking.Text == "None")
                                IPcore.TrackingSettings.LastestTrackedPixelPoint = new System.Drawing.Point(IPcore.TrackingSettings.LastestTrackedPixelPoint.X,
                                                                                        MapStepsToImagePerAxis(axisYPos, IPcore.VideoFeedSettings.ImageHeight, Enums.Axis.Y));
                            break;
                    }
                }
            });

            return IPcore.TrackingSettings.LastestTrackedPixelPoint;
        }

        // TO DO
        // NullReferenceException at closing
        private int MapStepsToImagePerAxis(int stepsPosition, int imgDimension, Enums.Axis axis)
        {
            int retVal = 0;

            if (imgDimension > 0)
            {
                switch (axis)
                {
                    case Enums.Axis.X:
                        retVal = MapRange(stepsPosition, Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition, 0, imgDimension);
                        break;

                    case Enums.Axis.Y:
                        retVal = MapRange(stepsPosition, Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition, 0, imgDimension);
                        break;

                    default:
                        retVal = 0;
                        break;
                }
                return retVal;
            }
            else return 0;
        }

        // TO DO
        // NullReferenceException at closing
        private int MapStepsToImagePerAxis_OLD(int stepsPosition, int imgDimension, Enums.Axis axis)
        {
            int retVal = 0;

            if (imgDimension > 0)
            {
                switch (axis)
                {
                    case Enums.Axis.X:
                        retVal = MapRange(stepsPosition, Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition, 0, imgDimension);
                        break;

                    case Enums.Axis.Y:
                        retVal = MapRange(stepsPosition, Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition, Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition, 0, imgDimension);
                        break;

                    default:
                        retVal = 0;
                        break;
                }
                return retVal;
            }
            else return 0;
        }


        private int MapStepsToImagePerAxis_OLD(int stepsPosition, int imgDimension)
        {
            if (imgDimension > 0)
            {
                //if (stepsPosition < 0)
                //    return MapRange(stepsPosition, Apsl.MinEdgePositionStepsAllDevices, 0, 0, imgDimension / 2);

                //if (stepsPosition > 0)
                //{
                //int mappedValue = MapRange(stepsPosition, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices, 1023, 736);
                //return mappedValue;
                return MapRange(stepsPosition, Apsl.MinEdgePositionStepsAllDevices, Apsl.MaxEdgePositionStepsAllDevices, 0, imgDimension);
                //}

                //else return imgDimension / 2;
            }
            else return 0;
        }

        private int MapRange(float value, float from1, float to1, float from2, float to2)
        {
            float a = ((value - from1) / (float)(to1 - from1) * (to2 - from2) + from2);
            return (int)a;
        }

        //private int MapRange(float value)
        //{
        //    //float a = value * (float)(IPcore.IDCSettings.StepsPerPixel);// (float)IPcore.IDCSettings.MilimetersPerPixel));                        

        //    float a = (value - 813) * 50;// (float)(IPcore.IDCSettings.StepsPerPixel);// (float)IPcore.IDCSettings.MilimetersPerPixel));                        


        //    return (int)a;
        //}

        private void CalculateFeedFrameRate(Stopwatch s)
        {
            s.Stop();

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    textBlockVideoFeedFPS.Text = "FPS: ";
                });
            }
            catch { /*TO DO*/ }
        }

        #endregion

        #region Video Feed Start/Stop Handling

        public void HandleVideoFeedStartStop(bool isVideoFeedEnabled, RoutedEventArgs e)
        {
            // If the routed event e is null => the method is not called from firing an event
            // If the routed event e is not null => the method is called from firing an event
            if (e != null)
            {
                if (isVideoFeedEnabled == true)
                {
                    IPcore.VideoFeedSettings.IsEnabled = false;
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Video feed was stopped"));
                }
                else
                {
                    IPcore.VideoFeedSettings.IsEnabled = true;
                    (int actualFrameWidth, int actualFrameheight) = IPcore.GetResolution();
                    textBoxVideoFeedSettingsActualSize.Text = actualFrameWidth + "x" + actualFrameheight;
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Video feed was started"));
                }
            }
            else IPcore.VideoFeedSettings.IsEnabled = isVideoFeedEnabled;

            StartStopVideoFeed(IPcore.VideoFeedSettings.IsEnabled);
            ChangeStartStopVideoFeedButtonAppearance(IPcore.VideoFeedSettings.IsEnabled);
            DisableVideoFeedControlsTA(IPcore.VideoFeedSettings.IsEnabled);
        }

        private void StartStopVideoFeed(bool isVideoFeedEnabled)
        {
            if (isVideoFeedEnabled == true)
                bgWorkerFeedProcessedImage.RunWorkerAsync();
            else
            {
                bgWorkerFeedProcessedImage.CancelAsync();
                HandleTrackingStartStop(false, null);
            }
        }

        private void ChangeStartStopVideoFeedButtonAppearance(bool isVideoFeedEnabled)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (isVideoFeedEnabled == true)
                    {
                        buttonStartStopVideoFeed.Content = "Stop feed";
                        buttonStartStopVideoFeed.Background = uiBrushes.BordeauxAccentLight;
                    }
                    else
                    {
                        buttonStartStopVideoFeed.Content = "Start feed";
                        buttonStartStopVideoFeed.Background = uiBrushes.GreenStart;
                    }
                });
            }
            catch { /*TO DO*/ }
        }

        private void DisableVideoFeedControlsTA(bool isVideoFeedEnabled)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (isVideoFeedEnabled == false)
                    {
                        HandleTrackingStartStop(IPcore.TrackingSettings.IsEnabled = false, null);

                        buttonStartStopTracking.Background = uiBrushes.Gray;
                        buttonStartStopTracking.IsEnabled = false;

                        //checkBoxVideoFeedShowProcessedImage.IsEnabled = false;
                    }
                    else
                    {
                        buttonStartStopTracking.Background = uiBrushes.GreenStart;
                        buttonStartStopTracking.IsEnabled = true;

                        //checkBoxVideoFeedShowProcessedImage.IsEnabled = true;
                    }
                });
            }
            catch { /*TO DO*/ }
        }

        #endregion

        #region Tracking Start/Stop Handling

        private void HandleTrackingStartStop(bool isTrackingEnabled, RoutedEventArgs e)
        {
            // If the routed event e is null => the method is not called from firing an event
            // If the routed event e is not null => the method is called from firing an event
            if (e != null)
            {
                if (isTrackingEnabled == true)
                {
                    IPcore.TrackingSettings.IsEnabled = false;
                    comboBoxVideoFeedCalibrationAlgorithm.IsEnabled = true;

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Tracking was stopped"));
                }
                else
                {
                    IPcore.TrackingSettings.IsEnabled = true;
                    //checkBoxVideoFeedShowProcessedImage.IsChecked = true;
                    IPcore.VideoFeedSettings.IsProcessedImageFed = true;
                    comboBoxVideoFeedCalibrationAlgorithm.SelectedIndex = 0; // Force to none
                    comboBoxVideoFeedCalibrationAlgorithm.IsEnabled = false;

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Tracking was started"));
                }
            }
            else
                IPcore.TrackingSettings.IsEnabled = isTrackingEnabled;

            ChangeStartStopTrackingButtonAppearance(IPcore.TrackingSettings.IsEnabled);
        }

        private void ChangeStartStopTrackingButtonAppearance(bool isVideoFeedEnabled)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (isVideoFeedEnabled == true)
                    {
                        buttonStartStopTracking.Content = "Stop tracking";
                        buttonStartStopTracking.Background = uiBrushes.BordeauxAccentLight;
                    }
                    else if (buttonStartStopTracking.IsEnabled == true)
                    {
                        buttonStartStopTracking.Content = "Start tracking";
                        buttonStartStopTracking.Background = uiBrushes.GreenStart;
                    }
                    else
                    {
                        buttonStartStopTracking.Content = "Start tracking";
                        buttonStartStopTracking.Background = uiBrushes.Gray;
                    }
                });
            }
            catch { /*TO DO*/ }
        }

        #endregion

        #region ComboBox SelectionChanged Algorithm Handling

        public void HandleVideoFeedProcessingAlgorithmSelectionChange()
        {
            switch (comboBoxVideoFeedProcessingAlgorithm.SelectedIndex)
            {
                // No algorithm
                case 0:
                    IPcore.TASettings.ImgProcAlgorithm = Enums.ImgProcAlgorithm.None;
                    HandleNoneVideoFeedAlgorithmChange();
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "No image processing algorithm was selected"));
                    break;

                // Static threshold algorithm
                case 1:
                    IPcore.TASettings.ImgProcAlgorithm = Enums.ImgProcAlgorithm.StaticThresh;
                    HandleTAVideoFeedAlgorithmChange();
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Static thresholding algorithm was selected"));
                    break;

                // Dynamic threshold contour algorithm
                case 2:
                    IPcore.TASettings.ImgProcAlgorithm = Enums.ImgProcAlgorithm.DynamicThresh;
                    HandleTAVideoFeedAlgorithmChange();
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Dynamic thresholding algorithm was seleceted"));
                    break;

                // Otsu threshold
                case 3:
                    IPcore.TASettings.ImgProcAlgorithm = Enums.ImgProcAlgorithm.OstuThresh;
                    HandleTAVideoFeedAlgorithmChange();
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Otsu thresholding algorithm was seleceted"));
                    break;
            }

            SwapPrecisionAndStaticThresholdControls();
        }

        public void HandleNoneVideoFeedAlgorithmChange()
        {
            IPcore.IDCSettings.IsEnabled = false;
            HandleTrackingStartStop(false, null);

            // Enable/disable image processing algorithms
            if (IPcore.TASettings.CalibrationAlgorithm == Enums.CalibrationAlgorithm.None)
            {
                IPcore.TASettings.IsEnabled = true;
                ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Enabled);
            }
            else
            {
                IPcore.TASettings.IsEnabled = false;
                ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Disabled);
            }

            // Show/hide/rescale UI elements
            ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Disabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowInvertImage, Enums.ToggleType.Disabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowProcessedImage, Enums.ToggleType.Disabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageHorizontally, Enums.ToggleType.Enabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageVertically, Enums.ToggleType.Enabled);
        }

        public void HandleTAVideoFeedAlgorithmChange()
        {
            IPcore.IDCSettings.IsEnabled = false;
            HandleTrackingStartStop(false, null);

            // Enable/disable image processing algorithms
            if (IPcore.TASettings.CalibrationAlgorithm == Enums.CalibrationAlgorithm.None)
            {
                IPcore.TASettings.IsEnabled = true;
                ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Enabled);
            }
            else
            {
                IPcore.TASettings.IsEnabled = false;
                ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Disabled);
            }

            // Show/hide/rescale UI elements
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowInvertImage, Enums.ToggleType.Enabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowProcessedImage, Enums.ToggleType.Enabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageHorizontally, Enums.ToggleType.Enabled);
            //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageVertically, Enums.ToggleType.Enabled);
        }

        private void SwapPrecisionAndStaticThresholdControls()
        {
            if (IPcore.TASettings.ImgProcAlgorithm == Enums.ImgProcAlgorithm.StaticThresh)
            {
                sliderImageProcessingSettingsPrecision.Visibility = Visibility.Hidden;
                buttonImageProcessingSettingsResetPrecision.Visibility = Visibility.Hidden;

                sliderImageProcessingSettingsStaticThreshold.Visibility = Visibility.Visible;
                buttonImageProcessingSettingsResetStaticThreshold.Visibility = Visibility.Visible;
                textBlockImageProcessingSettingsPrecision.Text = "Threshold";
            }
            else
            {
                sliderImageProcessingSettingsPrecision.Visibility = Visibility.Visible;
                buttonImageProcessingSettingsResetPrecision.Visibility = Visibility.Visible;
                textBlockImageProcessingSettingsPrecision.Text = "Precision";

                sliderImageProcessingSettingsStaticThreshold.Visibility = Visibility.Hidden;
                buttonImageProcessingSettingsResetStaticThreshold.Visibility = Visibility.Hidden;
            }
        }

        private void HandleVideoFeedCalibrationAlgorithmSelectionChange()
        {
            switch (comboBoxVideoFeedCalibrationAlgorithm.SelectedIndex)
            {
                // No algorithm
                case 0:
                    IPcore.TASettings.CalibrationAlgorithm = Enums.CalibrationAlgorithm.None;
                    IPcore.TASettings.IsEnabled = false;
                    IPcore.IDCSettings.IsEnabled = false;
                    HandleTrackingStartStop(false, null);
                    ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowInvertImage, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowProcessedImage, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageHorizontally, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageVertically, Enums.ToggleType.Enabled);
                    comboBoxVideoFeedProcessingAlgorithm.IsEnabled = true;
                    comboBoxVideoFeedCalibrationForTracking.IsEnabled = true;

                    break;

                // Image distance calibration
                case 1:
                    IPcore.TASettings.CalibrationAlgorithm = Enums.CalibrationAlgorithm.ImageDistance;
                    comboBoxVideoFeedProcessingAlgorithm.SelectedIndex = 2; // Force dynamic threshold algorithm
                    IPcore.TASettings.IsEnabled = false;
                    IPcore.IDCSettings.IsEnabled = true;
                    HandleTrackingStartStop(false, null);
                    ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Disabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowInvertImage, Enums.ToggleType.Disabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowProcessedImage, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageHorizontally, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageVertically, Enums.ToggleType.Enabled);
                    comboBoxVideoFeedProcessingAlgorithm.IsEnabled = false;
                    comboBoxVideoFeedCalibrationForTracking.IsEnabled = false;

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Image distance calibration algorithm was selected"));
                    break;

                // Bounding Box Calibration
                case 2:
                    IPcore.TASettings.CalibrationAlgorithm = Enums.CalibrationAlgorithm.BoundingBox;
                    IPcore.TASettings.IsEnabled = false;
                    IPcore.IDCSettings.IsEnabled = false;
                    HandleTrackingStartStop(false, null);
                    ToggleVideoFeedComponentValidity(buttonStartStopTracking, Enums.ToggleType.Disabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowInvertImage, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedShowProcessedImage, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageHorizontally, Enums.ToggleType.Enabled);
                    //ToggleVideoFeedComponentValidity(checkBoxVideoFeedMirrorImageVertically, Enums.ToggleType.Enabled);
                    comboBoxVideoFeedProcessingAlgorithm.IsEnabled = true;
                    comboBoxVideoFeedCalibrationForTracking.IsEnabled = false;

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Bounding box calibration algorithm was seleceted"));
                    break;
            }

            comboBoxVideoFeedCalibrationForTracking.SelectedIndex = 0;  // force to none
            CheckBoundingBoxPanelVisibility();
        }

        private void ToggleVideoFeedComponentValidity(UIElement element, Enums.ToggleType toggleType)
        {
            Button buttonDummy = new Button();
            CheckBox checkBoxDummy = new CheckBox();

            switch (toggleType)
            {
                case Enums.ToggleType.Enabled:

                    element.Visibility = Visibility.Visible;

                    if (element.GetType() == buttonDummy.GetType())
                    {
                        (element as Button).Height = 32;
                        (element as Button).Margin = new Thickness(5, 0, 5, 5);
                    }
                    else if (element.GetType() == checkBoxDummy.GetType())
                    {
                        (element as CheckBox).Height = 19;
                        (element as CheckBox).Margin = new Thickness(5, 0, 5, 5);
                    }
                    break;

                case Enums.ToggleType.Disabled:

                    element.Visibility = Visibility.Hidden;

                    if (element.GetType() == buttonDummy.GetType())
                    {
                        (element as Button).Height = 0;
                        (element as Button).Margin = new Thickness(0);
                    }
                    else if (element.GetType() == checkBoxDummy.GetType())
                    {
                        (element as CheckBox).Height = 0;
                        (element as CheckBox).Margin = new Thickness(0);
                    }

                    break;

                default:
                    break;
            }
        }

        private void HandleVideoFeedTrackingAlgorithmSelectionChange(SelectionChangedEventArgs e)
        {
            string cbText = (e.AddedItems[0] as ComboBoxItem).Content as string;

            if (IPcore != null)
                switch (cbText)
                {
                    case "None":
                        IPcore.TrackingSettings.TrackingAlgorithm = Enums.TrackingAlgoirthm.None;

                        break;

                    case "Distance calibration":
                        IPcore.TrackingSettings.TrackingAlgorithm = Enums.TrackingAlgoirthm.ImageDistance;

                        break;

                    case "Bounding box calib.":
                        IPcore.TrackingSettings.TrackingAlgorithm = Enums.TrackingAlgoirthm.BoundingBox;

                        break;

                    default:
                        IPcore.TrackingSettings.TrackingAlgorithm = Enums.TrackingAlgoirthm.None;

                        break;
                }
        }

        #endregion

        #region Image Distance Calibration Handling

        private void CheckImageCalibrationButtonValidity(int distance)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (distance < 1 || textBoxDistanceCalibrationKnownDistance.Text == "" || IPcore.VideoFeedSettings.IsEnabled == false)
                    {
                        buttonDistanceCalibrationSet.IsEnabled = false;
                        buttonDistanceCalibrationSet.Background = uiBrushes.Gray;
                    }
                    else
                    {
                        buttonDistanceCalibrationSet.IsEnabled = true;
                        buttonDistanceCalibrationSet.Background = uiBrushes.GreenStart;
                    }
                });
            }
            catch { /*TO DO*/}
        }

        private void CheckImageDistanceSetTrackingPointButtonValidity(System.Drawing.Point centerPoint)
        {
            if (IPcore.IDCSettings.IsCalibrated == true && IPcore.IDCSettings.IsTrackingPointSet == false)
            {
                if (centerPoint.X != int.MinValue && centerPoint.Y != int.MinValue)
                    this.Dispatcher.Invoke(() =>
                    {
                        buttonDistanceCalibrationSetTrackingPoint.IsEnabled = true;
                        buttonDistanceCalibrationSetTrackingPoint.Background = uiBrushes.GreenStart;
                    });
                else
                    this.Dispatcher.Invoke(() =>
                    {
                        buttonDistanceCalibrationSetTrackingPoint.IsEnabled = false;
                        buttonDistanceCalibrationSetTrackingPoint.Background = uiBrushes.Gray;
                    });
            }
            else
                this.Dispatcher.Invoke(() =>
                {
                    buttonDistanceCalibrationSetTrackingPoint.IsEnabled = false;
                    buttonDistanceCalibrationSetTrackingPoint.Background = uiBrushes.Gray;
                });
        }

        private void CalculateCalibrationDistances(int distance)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    IPcore.IDCSettings.CalculateMilimetersPerPixel(int.Parse(textBoxDistanceCalibrationKnownDistance.Text), distance);
                    IPcore.IDCSettings.CalculateTranslatedDistanceMilimeters();
                    IPcore.IDCSettings.CalculateStepsPerPixel();

                    textBlockDistanceCalibrationMilimetersPerPixelValue.Text = IPcore.IDCSettings.MilimetersPerPixel.ToString("0.000") + " mm";
                    textBlockDistanceCalibrationStepsPerPixelValue.Text = IPcore.IDCSettings.StepsPerPixel.ToString("0") + " st";

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Image distance was successfully calibrated"));
                });
            }
            catch { /*TO DO*/ }
        }

        private void UpdateImageCalibrationSettingsPixelDistanceUI(int distance)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    IPcore.IDCSettings.DistancePixels = distance;
                    textBlockDistanceCalibrationPixelDistanceValue.Text = IPcore.IDCSettings.DistancePixels.ToString() + " px";
                });
            }
            catch { /*TO DO*/ }
        }

        #endregion

        #region Video Feed Image Buttons Handling

        private void HandleVideoFeedCapture()
        {
            if (IPcore.VideoFeedSettings.IsEnabled)
            {
                string time = DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + " " + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + " " + DateTime.Now.Millisecond.ToString();
                string filePath = Environment.CurrentDirectory + '\\' + IPcore.VideoFeedSettings.ImageCaptureFolderPath + '\\' + time + ".bmp";

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    BitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                    bmpEncoder.Frames.Add(BitmapFrame.Create(masterImg.Source as BitmapSource));
                    bmpEncoder.Save(fileStream);

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Image was captured to file " + filePath));
                }
            }
            else if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot capture image, please start video feed first")
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot capture image, please start video feed first"));
        }

        private void OpenVideoFeedCaptureFolder()
        {
            if (Directory.Exists(IPcore.VideoFeedSettings.ImageCaptureFolderPath))
                System.Diagnostics.Process.Start("explorer.exe", IPcore.VideoFeedSettings.ImageCaptureFolderPath);
            else
                Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Capture folder does not exist at path " + Path.GetFullPath(IPcore.VideoFeedSettings.ImageCaptureFolderPath)));
        }

        private void HandleVideoFeedRecentering()
        {
            if (IPcore.VideoFeedSettings.IsEnabled)
                zoomBorder.Reset();
            else if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot center image, please start video feed first")
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot center image, please start video feed first"));
        }

        #endregion

        #region Video Feed Settings Image Size Handling

        // TO DO - clean ASAP
        private void HandleVideoFeedImageSizeSettings()
        {
            if (textBoxVideoFeedSettingsFrameWidth.Text != "" && textBoxVideoFeedSettingsFrameHeight.Text != "")
            {
                if (int.Parse(textBoxVideoFeedSettingsFrameWidth.Text) != 0 && int.Parse(textBoxVideoFeedSettingsFrameHeight.Text) != 0)
                {
                    IPcore.StopCapture();

                    IPcore.VideoFeedSettings.ImageWidth = int.Parse(textBoxVideoFeedSettingsFrameWidth.Text);
                    IPcore.VideoFeedSettings.ImageHeight = int.Parse(textBoxVideoFeedSettingsFrameHeight.Text);

                    // IPcore.SetResolution(IPcore.VideoFeedSettings.ImageWidth, IPcore.VideoFeedSettings.ImageHeight);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Attempting to set resolution " + IPcore.VideoFeedSettings.ImageWidth + "x" + IPcore.VideoFeedSettings.ImageHeight));

                    IPcore.StartCaptureWithResolution(IPcore.VideoFeedSettings.ImageWidth, IPcore.VideoFeedSettings.ImageHeight);

                    (int actualFrameWidth, int actualFrameheight) = IPcore.GetResolution();
                    textBoxVideoFeedSettingsActualSize.Text = actualFrameWidth + "x" + actualFrameheight;

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Set resolution is " + actualFrameWidth + "x" + actualFrameheight));
                }
                else Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot apply settings because of invalid values"));
            }
            else Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot apply settings because of invalid values"));
        }

        #endregion

        #region Video Feed Track Using handling

        private void VideoFeedCheckCalibratedAlgorithms()
        {
            if (isSoftwareInit)
            {
                IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemOne = comboBoxVideoFeedCalibrationForTracking.Items[0];
                IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemTwo = comboBoxVideoFeedCalibrationForTracking.Items[1];
                IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemThree = comboBoxVideoFeedCalibrationForTracking.Items[2];
            }

            try
            {
                if (IPcore.IDCSettings.IsCalibrated == false)
                    comboBoxVideoFeedCalibrationForTracking.Items.Remove(IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemTwo);

                else if (IPcore.IDCSettings.IsCalibrated == true || IPcore.IDCSettings.IsTrackingPointSet == true)
                    comboBoxVideoFeedCalibrationForTracking.Items.Add(IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemTwo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                if (IPcore.BoundingBoxCalib.HasBoundingBoxBeenSet == false)
                    comboBoxVideoFeedCalibrationForTracking.Items.Remove(IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemThree);

                else
                    comboBoxVideoFeedCalibrationForTracking.Items.Add(IPcore.TrackingSettings.ComboBoxTrackingAlgorithmItemThree);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        #endregion

        #region Bounding Box Calibration Handling

        private void SetBoundingBoxCorner(Enums.BoundingBoxCorner bbCorner, object sender)
        {
            if (sender != null)
            {
                System.Drawing.Point pixelPoS = new System.Drawing.Point();

                if (checkBoxBBCManual.IsChecked == false)
                    pixelPoS = IPcore.TrackingSettings.CurrentlyDetectedPoint;

                int actuatorStepsPos_X = controller.GetActuatorPositionByAxis(Enums.Axis.X);
                int actuatorStepsPos_Y = controller.GetActuatorPositionByAxis(Enums.Axis.Y);

                if (bbCorner == Enums.BoundingBoxCorner.TopLeft)
                {
                    if (checkBoxBBCManual.IsChecked == true)
                        pixelPoS = new System.Drawing.Point(int.Parse(textBoxBBCTopLeftPixel_X.Text), int.Parse(textBoxBBCTopLeftPixel_Y.Text));

                    IPcore.BoundingBoxCalib.SetTopLeft(pixelPoS, actuatorStepsPos_X, actuatorStepsPos_Y);
                }
                else if (bbCorner == Enums.BoundingBoxCorner.BottomRight)
                {
                    if (checkBoxBBCManual.IsChecked == true)
                        pixelPoS = new System.Drawing.Point(int.Parse(textBoxBBCBottomRightPixel_X.Text), int.Parse(textBoxBBCBottomRightPixel_Y.Text));

                    IPcore.BoundingBoxCalib.SetBottomRight(pixelPoS, actuatorStepsPos_X, actuatorStepsPos_Y);
                }

                if (IPcore.BoundingBoxCalib.HasBoundingBoxBeenSet == true)
                    VideoFeedCheckCalibratedAlgorithms();

                SetBoundingBoxActuatorEdgeLimits(actuatorStepsPos_X, actuatorStepsPos_Y, bbCorner);
                ToggleBoundingBoxButtonsValidity(bbCorner, Enums.ToggleType.Disabled);
                LoadApslValuesToUI();
            }
        }

        public void SetBoundingBoxActuatorEdgeLimits(int actPos_X, int actPos_Y, Enums.BoundingBoxCorner bbCorner)
        {
            switch (bbCorner)
            {
                case Enums.BoundingBoxCorner.TopLeft:
                    Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition = actPos_X;
                    Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition = actPos_Y;
                    break;

                case Enums.BoundingBoxCorner.BottomRight:
                    Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition = actPos_X;
                    Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition = actPos_Y;
                    break;

                default:
                    break;
            }

            // BBC19
            //if (checkBoxVideoFeedBoundingBoxEnable.IsChecked == true)
            //    ToggleManualEdgeSettings(Enums.ToggleType.Disabled);
        }

        // TO DO - MOVE
        private void ToggleManualEdgeSettings(Enums.ToggleType toggleType)
        {
            bool isEnabledVal = true ? Enums.ToggleType.Enabled == toggleType : Enums.ToggleType.Disabled == toggleType;

            buttonActuatorSettingsSetEdgeValues.IsEnabled =
            textBoxActuatorSettingsMinEdgeOne.IsEnabled =
            textBoxActuatorSettingsMinEdgeTwo.IsEnabled =
            textBoxActuatorSettingsMinEdgeThree.IsEnabled =
            textBoxActuatorSettingsMaxEdgeOne.IsEnabled =
            textBoxActuatorSettingsMaxEdgeTwo.IsEnabled =
            textBoxActuatorSettingsMaxEdgeThree.IsEnabled =
            isEnabledVal;
        }

        private void ToggleBoundingBoxButtonsValidity(Enums.BoundingBoxCorner bbCorner, Enums.ToggleType toggleType)
        {
            switch (toggleType)
            {
                case Enums.ToggleType.Enabled:
                    if (bbCorner == Enums.BoundingBoxCorner.TopLeft)
                        buttonBoundingBoxSetTopLeft.IsEnabled = true;
                    else if (bbCorner == Enums.BoundingBoxCorner.BottomRight)
                        buttonBoundingBoxSetBottomRight.IsEnabled = true;
                    break;

                case Enums.ToggleType.Disabled:
                    if (bbCorner == Enums.BoundingBoxCorner.TopLeft)
                        buttonBoundingBoxSetTopLeft.IsEnabled = false;
                    else if (bbCorner == Enums.BoundingBoxCorner.BottomRight)
                        buttonBoundingBoxSetBottomRight.IsEnabled = false;
                    break;

                default:
                    break;
            }

        }

        private void SetBoundingBoxValidity()
        {
            if (checkBoxBBCManual.IsChecked == false)
            {
                textBoxBBCTopLeftPixel_X.IsEnabled = false;
                textBoxBBCTopLeftPixel_Y.IsEnabled = false;
                textBoxBBCBottomRightPixel_X.IsEnabled = false;
                textBoxBBCBottomRightPixel_Y.IsEnabled = false;
            }
            else if (checkBoxBBCManual.IsChecked == true)
            {
                textBoxBBCTopLeftPixel_X.IsEnabled = true;
                textBoxBBCTopLeftPixel_Y.IsEnabled = true;
                textBoxBBCBottomRightPixel_X.IsEnabled = true;
                textBoxBBCBottomRightPixel_Y.IsEnabled = true;
            }
        }

        private void CheckBoundingBoxPanelVisibility()
        {
            switch (comboBoxVideoFeedCalibrationAlgorithm.SelectedIndex)
            {
                case 0:
                    gridTrackingSettings.Visibility = Visibility.Visible;
                    gridDistanceCalibration.Visibility = Visibility.Hidden;
                    gridBoundingBoxCalibration.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    gridTrackingSettings.Visibility = Visibility.Hidden;
                    gridDistanceCalibration.Visibility = Visibility.Visible;
                    gridBoundingBoxCalibration.Visibility = Visibility.Hidden;
                    break;

                case 2:
                    gridTrackingSettings.Visibility = Visibility.Hidden;
                    gridDistanceCalibration.Visibility = Visibility.Hidden;
                    gridBoundingBoxCalibration.Visibility = Visibility.Visible;
                    break;

                default:
                    break;
            }
        }

        private void BoundingBoxUpdateUI()
        {
            if (checkBoxBBCManual.IsChecked == false)
            {
                if (IPcore.BoundingBoxCalib.HasTopLeftBeenSet == false)
                {
                    textBoxBBCTopLeftPixel_X.Text = IPcore.TrackingSettings.CurrentlyDetectedPoint.X.ToString();
                    textBoxBBCTopLeftPixel_Y.Text = IPcore.TrackingSettings.CurrentlyDetectedPoint.Y.ToString();
                }

                if (IPcore.BoundingBoxCalib.HasBottomRightBeenSet == false)
                {
                    textBoxBBCBottomRightPixel_X.Text = IPcore.TrackingSettings.CurrentlyDetectedPoint.X.ToString();
                    textBoxBBCBottomRightPixel_Y.Text = IPcore.TrackingSettings.CurrentlyDetectedPoint.Y.ToString();
                }
            }
        }

        #endregion



        #region Notifications Handling

        public void NotifyUI(Enums.NotificationType notifType, string notificationText)
        {
            // Create a new row in the notification panel & add it to the grid
            RowDefinition rd = CreateNotificationRow();
            gridNotificationsCanvas.RowDefinitions.Add(rd);

            // Create and format the text for the notification
            TextBlock tb = CreateNotificationTextBlock(notificationText, notifType);

            // Assign the appropiate icon for the notification
            System.Windows.Shapes.Rectangle img = AddNotificationIcon(notifType);

            // Change the notification panel's color according to the notification type
            ColorNotificationsPanel(notifType);

            // Add the TextBlock and Rectangle (image) to the new row, as a child to the grid
            for (int colIndex = 0; colIndex < gridNotificationsCanvas.ColumnDefinitions.Count; colIndex++)
                if (colIndex == 0)
                    gridNotificationsCanvas.Children.Add(AddUIElementToNotifications(img, colIndex));
                else
                    gridNotificationsCanvas.Children.Add(AddUIElementToNotifications(tb, colIndex));

            // Increase global row index
            notificationsRowIndex++;

            // Programatically scroll down so that the user sees the latest notification
            ManageCanvasScrolling(Enums.ScrollDirection.Down);

            // TO DO - maybe
            //notifLogger.Info(notificationText);
        }

        public RowDefinition CreateNotificationRow()
        {
            int minRowHeight = 30;

            RowDefinition rd = new RowDefinition
            {
                MinHeight = minRowHeight
            };
            return rd;
        }

        public TextBlock CreateNotificationTextBlock(string notificationText, Enums.NotificationType notifType)
        {
            TextBlock tb = new TextBlock
            {
                Text = notificationText,
                FontSize = 14,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 3)
            };

            switch (notifType)
            {
                case Enums.NotificationType.Info:
                    tb.Foreground = uiBrushes.NotificationBlue;
                    break;

                case Enums.NotificationType.Success:
                    tb.Foreground = uiBrushes.GreenStart;
                    break;

                case Enums.NotificationType.Warning:
                    tb.Foreground = uiBrushes.NotificationYellow;
                    break;

                case Enums.NotificationType.CriticalError:
                    tb.Foreground = uiBrushes.BordeauxAccentLight;
                    break;

                case Enums.NotificationType.Camera:
                    tb.Foreground = uiBrushes.NotificationPurple;
                    break;
            }

            return tb;
        }

        public System.Windows.Shapes.Rectangle AddNotificationIcon(Enums.NotificationType notifType)
        {
            System.Windows.Shapes.Rectangle img = new System.Windows.Shapes.Rectangle
            {
                Height = 20,
                Width = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            switch (notifType)
            {
                case Enums.NotificationType.Info:
                    img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"../../View/img/info.png", UriKind.Relative)) };
                    break;

                case Enums.NotificationType.Success:
                    img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"../../View/img/tick.png", UriKind.Relative)) };
                    break;

                case Enums.NotificationType.Warning:
                    img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"../../View/img/warning.png", UriKind.Relative)) };
                    break;

                case Enums.NotificationType.CriticalError:
                    img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"../../View/img/alert.png", UriKind.Relative)) };
                    break;

                case Enums.NotificationType.Camera:
                    img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"../../View/img/cam.png", UriKind.Relative)) };
                    break;
            }

            return img;
        }

        public UIElement AddUIElementToNotifications(UIElement uiElem, int colIndex)
        {
            Grid.SetColumn(uiElem, colIndex);
            Grid.SetRow(uiElem, notificationsRowIndex);

            return uiElem;
        }

        public void ColorNotificationsPanel(Enums.NotificationType notifType)
        {
            GradientStopCollection gsc = new GradientStopCollection(3);

            switch (notifType)
            {
                case Enums.NotificationType.Info:
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 55, 118, 162), 0));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 64, 64, 64), 0.3));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 50, 50, 50), 1));
                    //textblockNotificationHeader.Text = "Information";
                    break;

                case Enums.NotificationType.Success:
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 46, 112, 63), 0));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 64, 64, 64), 0.3));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 50, 50, 50), 1));
                    //textblockNotificationHeader.Text = "Success";
                    break;

                case Enums.NotificationType.Warning:
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 140, 136, 43), 0));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 64, 64, 64), 0.3));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 50, 50, 50), 1));
                    //textblockNotificationHeader.Text = "Warning";
                    break;

                case Enums.NotificationType.CriticalError:
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 162, 55, 55), 0));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 64, 64, 64), 0.3));
                    gsc.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 50, 50, 50), 1));
                    //textblockNotificationHeader.Text = "Critical Error";
                    break;

                    //TO DO PURPLE//
            }

            LinearGradientBrush lgb = new LinearGradientBrush(gsc, 90);
            //borderMainNotifications.Background = lgb;
        }

        public void ManageCanvasScrolling(Enums.ScrollDirection scrollDir)
        {
            this.UpdateLayout();

            double scrollAmount = gridNotificationsCanvas.ActualHeight - gridNotificationMainContainer.ActualHeight;

            if (scrollAmount > 0)
                notificationsScrollableArea.ProgrammedScrollDown(scrollAmount, scrollDir);
        }

        #endregion

        #region Typing Value Safety Handling

        // TO DO
        // This is an ugly way to resolve the issue of textbox warning highlighting at start-up, but for now it works
        private void CheckEveryTextBoxTypingSafety()
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsSpeedOne, Enums.TextBoxActuatorSettingsType.Speed);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsSpeedTwo, Enums.TextBoxActuatorSettingsType.Speed);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsSpeedThree, Enums.TextBoxActuatorSettingsType.Speed);

            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsMicroSpeedOne, Enums.TextBoxActuatorSettingsType.MicroSpeed);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsMicroSpeedTwo, Enums.TextBoxActuatorSettingsType.MicroSpeed);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsMicroSpeedThree, Enums.TextBoxActuatorSettingsType.MicroSpeed);

            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsAccelerationOne, Enums.TextBoxActuatorSettingsType.Acceleration);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsAccelerationTwo, Enums.TextBoxActuatorSettingsType.Acceleration);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsAccelerationThree, Enums.TextBoxActuatorSettingsType.Acceleration);

            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsDecelerationOne, Enums.TextBoxActuatorSettingsType.Deceleration);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsDecelerationTwo, Enums.TextBoxActuatorSettingsType.Deceleration);
            HandleActuatorSettingsTextBoxTypingSafety(textBoxActuatorSettingsDecelerationThree, Enums.TextBoxActuatorSettingsType.Deceleration);

            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveToPositionStepsOneDevice, textBoxMoveToPositionMicroStepsOneDevice, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveToPositionOneDevice);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxShiftToStepsOneDevice, textBoxShiftToMicroStepsOneDevice, Enums.TextBoxValueSafetyType.ShiftOn, buttonShiftOnOneDevice);

            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceOne);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceOne);

            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceTwo);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceTwo);

            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveRelativelyDeviceThree);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, Enums.TextBoxValueSafetyType.ShiftOn, buttonMoveRelativelyDeviceThree);

            s.Stop();
            Console.WriteLine(s.ElapsedMilliseconds);
        }

        private void HandleMoveRelativelyTextBoxTypingSafety(TextBox textBoxOne, TextBox textBoxTwo, Enums.TextBoxValueSafetyType textBoxValueSafetyType, Button buttonToToggle)
        {
            bool methodCheck = false;
            bool tryParseValueOne = int.TryParse(textBoxOne.Text, out int valSteps);
            bool tryParseValueTwo = int.TryParse(textBoxTwo.Text, out int valMicroSteps);

            switch (textBoxValueSafetyType)
            {
                case Enums.TextBoxValueSafetyType.MoveTo:
                    methodCheck = CheckMoveToPositionOneDeviceSafetyValues(valSteps, valMicroSteps, textBoxOne);
                    break;

                case Enums.TextBoxValueSafetyType.ShiftOn:
                    methodCheck = CheckShiftOnOneDeviceFinalPosition(valSteps, valMicroSteps, textBoxOne);
                    break;

            }

            if (tryParseValueOne == false || tryParseValueTwo == false || methodCheck == false)// || CheckMoveToPositionOneDeviceSafetyValues(valSteps, valMicroSteps) == false)
                ToggleMoveRelativelyControlsAtWarning(textBoxOne, textBoxTwo, Enums.TextBoxState.Warning, buttonToToggle);
            else
                ToggleMoveRelativelyControlsAtWarning(textBoxOne, textBoxTwo, Enums.TextBoxState.Default, buttonToToggle);
        }

        private void ToggleMoveRelativelyControlsAtWarning(TextBox textBoxOne, TextBox textBoxTwo, Enums.TextBoxState textBoxState, Button buttonToToggle)
        {
            SetTextBoxState(textBoxOne, textBoxState);
            SetTextBoxState(textBoxTwo, textBoxState);

            if (textBoxState == Enums.TextBoxState.Default)
                buttonToToggle.IsEnabled = true;
            else if (textBoxState == Enums.TextBoxState.Warning)
                buttonToToggle.IsEnabled = false;
        }

        private void SetTextBoxState(TextBox textBox, Enums.TextBoxState textBoxState)
        {
            switch (textBoxState)
            {
                case Enums.TextBoxState.Default:
                    textBox.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
                    break;

                case Enums.TextBoxState.Warning:
                    textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                    break;
            }
        }

        // to do
        private void HandleActuatorSettingsTextBoxTypingSafety(TextBox textBox, Enums.TextBoxActuatorSettingsType textBoxActuatorSettingsType)
        {
            uint.TryParse(textBoxActuatorSettingsSpeedOne.Text, out uint speed);
            uint.TryParse(textBoxActuatorSettingsMicroSpeedOne.Text, out uint uspeed);
            uint.TryParse(textBoxActuatorSettingsAccelerationOne.Text, out uint accel);
            uint.TryParse(textBoxActuatorSettingsDecelerationOne.Text, out uint decel);

            move_settings_t mst = new move_settings_t
            {
                Accel = accel,
                Decel = decel,
                Speed = speed,
                uSpeed = uspeed
            };

            bool tryParseValue = int.TryParse(textBox.Text, out int value);

            if (tryParseValue == false || HandleActuatorSettingsTextBoxValueValidity(textBox, textBoxActuatorSettingsType, ref mst) == false)
                textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];

            else
                textBox.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
        }

        private void HandleActuatorSettingsTextBoxEdgeTypingSafety(TextBox textBox, Enums.TextBoxActuatorSettingsType textBoxActuatorSettingsType)
        {
            /* Dirty trick because when loading value from app config for the first time, min edge textbox will fill
             * and will trigger this function.. as because max edge textbox has not yet been filled, try_Parse_1 will be 
             * true but tryParse_2 will be false 
             * 
             */
            if (!isSoftwareInit)
            {
                bool tryParse_1 = false;
                bool tryParse_2 = false;
                uint minEdgeOne = 0, maxEdgeOne = 0, minEdgeTwo = 0, maxEdgeTwo = 0, minEdgeThree = 0, maxEdgeThree = 0;
                int textBoxPairIndex = -1;
                TextBox textBoxPair = new TextBox();

                if (textBox == textBoxActuatorSettingsMinEdgeOne)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeOne.Text, out minEdgeOne);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeOne.Text, out maxEdgeOne);
                    textBoxPair = textBoxActuatorSettingsMaxEdgeOne;
                    textBoxPairIndex = 1;
                }
                else if (textBox == textBoxActuatorSettingsMaxEdgeOne)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeOne.Text, out minEdgeOne);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeOne.Text, out maxEdgeOne);
                    textBoxPair = textBoxActuatorSettingsMinEdgeOne;
                    textBoxPairIndex = 1;
                }
                else if (textBox == textBoxActuatorSettingsMinEdgeTwo)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeTwo.Text, out minEdgeTwo);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeTwo.Text, out maxEdgeTwo);
                    textBoxPair = textBoxActuatorSettingsMaxEdgeTwo;
                    textBoxPairIndex = 2;
                }
                else if (textBox == textBoxActuatorSettingsMaxEdgeTwo)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeTwo.Text, out minEdgeTwo);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeTwo.Text, out maxEdgeTwo);
                    textBoxPair = textBoxActuatorSettingsMinEdgeTwo;
                    textBoxPairIndex = 2;
                }
                else if (textBox == textBoxActuatorSettingsMinEdgeThree)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeThree.Text, out minEdgeThree);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeThree.Text, out maxEdgeThree);
                    textBoxPair = textBoxActuatorSettingsMaxEdgeThree;
                    textBoxPairIndex = 3;
                }
                else if (textBox == textBoxActuatorSettingsMaxEdgeThree)
                {
                    tryParse_1 = uint.TryParse(textBoxActuatorSettingsMinEdgeThree.Text, out minEdgeThree);
                    tryParse_2 = uint.TryParse(textBoxActuatorSettingsMaxEdgeThree.Text, out maxEdgeThree);
                    textBoxPair = textBoxActuatorSettingsMinEdgeThree;
                    textBoxPairIndex = 3;
                }

                if (tryParse_1 == true && tryParse_2 == true)
                {
                    switch (textBoxPairIndex)
                    {
                        case 1:
                            HandleActuatorSettingsTextBoxEdgeValueValidity(textBox, textBoxPair, minEdgeOne, maxEdgeOne);
                            break;

                        case 2:
                            HandleActuatorSettingsTextBoxEdgeValueValidity(textBox, textBoxPair, minEdgeTwo, maxEdgeTwo);
                            break;

                        case 3:
                            HandleActuatorSettingsTextBoxEdgeValueValidity(textBox, textBoxPair, minEdgeThree, maxEdgeThree);
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                    textBoxPair.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                }
            }
        }

        private void HandleActuatorSettingsTextBoxEdgeValueValidity(TextBox textBox, TextBox textBoxPair, uint minEdge, uint maxEdge)
        {
            if (Apsl != null)
                if (minEdge < Apsl.MinEdgePositionStepsAllDevices)
                {
                    // Mai mic de limita
                    textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                    textBoxPair.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];

                    if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot set minumum edge lower than global minimum " + Apsl.MinEdgePositionStepsAllDevices.ToString())
                        Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot set minumum edge lower than global minimum " + Apsl.MinEdgePositionStepsAllDevices.ToString()));
                }
                else if (maxEdge > 20000)
                {
                    // Mai mare de limita
                    textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                    textBoxPair.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];

                    if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot set maximum edge larger than global maximum 20000")
                        Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot set maximum edge larger than global maximum 20000"));
                }
                else if (minEdge >= maxEdge)
                {
                    // Mai mare sau egal
                    textBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                    textBoxPair.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];

                    if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Minimum edge cannot be lower than maximum edge")
                        Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Minimum edge cannot be lower than maximum edge"));
                }
                else
                {
                    // OK
                    textBox.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
                    textBoxPair.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
                }

            if (textBoxActuatorSettingsMinEdgeOne.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"] &&
               textBoxActuatorSettingsMaxEdgeOne.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"] &&
               textBoxActuatorSettingsMinEdgeTwo.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"] &&
               textBoxActuatorSettingsMaxEdgeTwo.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"] &&
               textBoxActuatorSettingsMinEdgeThree.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"] &&
               textBoxActuatorSettingsMaxEdgeThree.Style != (Style)Application.Current.Resources["TextBoxStyleCustomWarning"])
            {
                buttonActuatorSettingsSetEdgeValues.IsEnabled = true;
            }
            else buttonActuatorSettingsSetEdgeValues.IsEnabled = true;
        }

        private bool HandleActuatorSettingsTextBoxValueValidity(TextBox senderTextBox, Enums.TextBoxActuatorSettingsType textBoxActuatorSettingsType, ref move_settings_t mst)
        {
            bool methodCheck = false;

            switch (textBoxActuatorSettingsType)
            {
                case Enums.TextBoxActuatorSettingsType.Speed:
                    methodCheck = HandleActuatorSettingsTextBoxSpeed(senderTextBox, ref mst);
                    break;

                case Enums.TextBoxActuatorSettingsType.MicroSpeed:
                    methodCheck = HandleActuatorSettingsTextBoxMicroSpeed(senderTextBox, ref mst);
                    break;

                case Enums.TextBoxActuatorSettingsType.Acceleration:
                    methodCheck = HandleActuatorSettingsTextBoxAcceleration(senderTextBox, ref mst);
                    break;

                case Enums.TextBoxActuatorSettingsType.Deceleration:
                    methodCheck = HandleActuatorSettingsTextBoxDeceleration(senderTextBox, ref mst);
                    break;
            }

            return methodCheck;
        }

        // TO DO
        private bool HandleActuatorSettingsTextBoxEmpty(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (senderTextBox.Text == "")
            {
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement aborted, one or more actuator settings values are empty"));
                return false;
            }
            else return true;
        }

        // TO DO
        private bool HandleActuatorSettingsTextBoxZero(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (senderTextBox.Text == "0")
            {
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement (may be) aborted, minimum speed supported is 0 steps and 1 microsteps, acceleration and deceleration must be positive"));
                return false;
            }
            else return true;
        }

        private bool HandleActuatorSettingsTextBoxSpeed(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (HandleActuatorSettingsTextBoxEmpty(senderTextBox, ref mst) == true)
            {
                // These 3 checks are necessary because the minimum speed is 0 steps and 1 microsteps. Need to find out which textBox have teh values to be checked
                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsMicroSpeedOne, "textBoxActuatorSettingsSpeedOne") == false)
                    return false;

                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsMicroSpeedTwo, "textBoxActuatorSettingsSpeedTwo") == false)
                    return false;

                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsMicroSpeedThree, "textBoxActuatorSettingsSpeedThree") == false)
                    return false;

                if (int.Parse(senderTextBox.Text) > 1500)
                {
                    mst.Speed = 1500;
                    //textBoxActuatorSettingsSpeedOne.Text = "1500";
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Steps speed exceeds maximum value of 1500, value used will be 1500"));
                    return false;
                }
            }
            else return false;

            return true;
        }

        private bool HandleActuatorSettingsTextBoxMicroSpeed(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (HandleActuatorSettingsTextBoxEmpty(senderTextBox, ref mst) == true)
            {
                // These 3 checks are necessary because the minimul speed is 0 steps and 1 microsteps. Need to find out which textBox have teh values to be checked
                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsSpeedOne, "textBoxActuatorSettingsMicroSpeedOne") == false)
                    return false;

                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsSpeedTwo, "textBoxActuatorSettingsMicroSpeedTwo") == false)
                    return false;

                if (CheckActuatorSettingsRelatedTextBox(senderTextBox, textBoxActuatorSettingsSpeedThree, "textBoxActuatorSettingsMicroSpeedThree") == false)
                    return false;

                if (int.Parse(senderTextBox.Text) > 255)
                {
                    mst.Speed = 0;
                    //textBoxActuatorSettingsSpeedOne.Text = "1500";
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Microsteps speed exceeds maximum value of 255, value used will be 0"));
                    return false;
                }
            }
            else return false;

            return true;
        }

        private bool HandleActuatorSettingsTextBoxAcceleration(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (HandleActuatorSettingsTextBoxZero(senderTextBox, ref mst) == true && HandleActuatorSettingsTextBoxEmpty(senderTextBox, ref mst) == true)
            {
                if (int.Parse(senderTextBox.Text) > 4000)
                {
                    mst.Accel = 4000;
                    //textBoxActuatorSettingsAccelerationOne.Text = "4000";
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Acceleration value exceeds maximum of 4000, value used will be 4000"));
                    return false;
                }
                else return true;
            }
            else return false;
        }

        private bool HandleActuatorSettingsTextBoxDeceleration(TextBox senderTextBox, ref move_settings_t mst)
        {
            if (HandleActuatorSettingsTextBoxZero(senderTextBox, ref mst) == true && HandleActuatorSettingsTextBoxEmpty(senderTextBox, ref mst) == true)
            {
                if (int.Parse(senderTextBox.Text) > 4000)
                {
                    mst.Decel = 4000;
                    //textBoxActuatorSettingsDecelerationOne.Text = "4000";
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Deceleration value exceeds maximum of 4000, value used will be 4000"));
                    return false;
                }
                else return true;
            }
            else return false;
        }

        // TO DO
        private bool CheckActuatorSettingsRelatedTextBox(TextBox senderTextBox, TextBox relatedTextBox, string targetTextBoxName)
        {
            if (senderTextBox.Text != "" && relatedTextBox.Text != "")
            {
                if (int.Parse(senderTextBox.Text) == 0 && int.Parse(relatedTextBox.Text) == 0)
                    return false;
                else if (int.Parse(senderTextBox.Text) > 0)
                {
                    SetTextBoxState(senderTextBox, Enums.TextBoxState.Default);
                    SetTextBoxState(relatedTextBox, Enums.TextBoxState.Default);
                    return true;
                }
            }

            return true;
        }

        // A bit redundant because ROI outputs a square image, but this way it is generalized
        private void HandleBoundingBoxTextBoxTypingSafety(TextBox senderTextBox, Enums.Axis axis, Enums.BoundingBoxCorner corner)
        {
            if (senderTextBox != null && IPcore != null && senderTextBox.Text != null)
            {
                if (senderTextBox.Text != "")
                {
                    switch (axis)
                    {
                        // Image height
                        case Enums.Axis.X:
                            if (int.Parse(senderTextBox.Text) < 0 || int.Parse(senderTextBox.Text) > IPcore.VideoFeedSettings.ImageWidth)
                            {
                                ToggleBoundingBoxButtonsValidity(corner, Enums.ToggleType.Disabled);
                                senderTextBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                            }
                            else
                            {
                                ToggleBoundingBoxButtonsValidity(corner, Enums.ToggleType.Enabled);
                                senderTextBox.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
                            }
                            break;

                        // Image width
                        case Enums.Axis.Y:
                            if (int.Parse(senderTextBox.Text) < 0 || int.Parse(senderTextBox.Text) > IPcore.VideoFeedSettings.ImageHeight)
                            {
                                ToggleBoundingBoxButtonsValidity(corner, Enums.ToggleType.Disabled);
                                senderTextBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                            }
                            else
                            {
                                ToggleBoundingBoxButtonsValidity(corner, Enums.ToggleType.Enabled);
                                senderTextBox.Style = (Style)Application.Current.Resources["TextBoxRevealStyle"];
                            }
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    ToggleBoundingBoxButtonsValidity(corner, Enums.ToggleType.Disabled);
                    senderTextBox.Style = (Style)Application.Current.Resources["TextBoxStyleCustomWarning"];
                }

            }
        }



        #endregion

        #region Actuator Settings Value Safety Handling

        public bool TryConfiguringActuatorSettingsOneDevice(int listIndex = 0, TextBox sendertextBox = null)
        {
            uint speed = 0, uspeed = 0, accel = 0, decel = 0;

            // This check is necessary because the tracking algorithm uses controller.ChangeContextByAxis()
            if (IPcore.TrackingSettings.IsEnabled == false)
                controller.ChangeContext(listIndex);

            if (checkBoxActuatorSettingApplyForAll.IsChecked == true)
            {
                uint.TryParse(textBoxActuatorSettingsSpeedOne.Text, out speed);
                uint.TryParse(textBoxActuatorSettingsMicroSpeedOne.Text, out uspeed);
                uint.TryParse(textBoxActuatorSettingsAccelerationOne.Text, out accel);
                uint.TryParse(textBoxActuatorSettingsDecelerationOne.Text, out decel);
            }
            else
            {
                switch (listIndex)
                {
                    case 0:
                        uint.TryParse(textBoxActuatorSettingsSpeedOne.Text, out speed);
                        uint.TryParse(textBoxActuatorSettingsMicroSpeedOne.Text, out uspeed);
                        uint.TryParse(textBoxActuatorSettingsAccelerationOne.Text, out accel);
                        uint.TryParse(textBoxActuatorSettingsDecelerationOne.Text, out decel);
                        break;

                    case 1:
                        uint.TryParse(textBoxActuatorSettingsSpeedTwo.Text, out speed);
                        uint.TryParse(textBoxActuatorSettingsMicroSpeedTwo.Text, out uspeed);
                        uint.TryParse(textBoxActuatorSettingsAccelerationTwo.Text, out accel);
                        uint.TryParse(textBoxActuatorSettingsDecelerationTwo.Text, out decel);
                        break;

                    case 2:
                        uint.TryParse(textBoxActuatorSettingsSpeedThree.Text, out speed);
                        uint.TryParse(textBoxActuatorSettingsMicroSpeedThree.Text, out uspeed);
                        uint.TryParse(textBoxActuatorSettingsAccelerationThree.Text, out accel);
                        uint.TryParse(textBoxActuatorSettingsDecelerationThree.Text, out decel);
                        break;

                    default:
                        break;
                }
            }


            move_settings_t mst = new move_settings_t
            {
                Accel = accel,
                Decel = decel,
                Speed = speed,
                uSpeed = uspeed
            };

            bool result = false;

            if (checkBoxActuatorSettingApplyForAll.IsChecked == true)
            {
                controller.ActuatorInContext.SetMoveSettings(controller.ActuatorInContext.DeviceID, ref mst);
                return CheckActuatorSettingsOneDevice(ref mst, textBoxActuatorSettingsSpeedOne, textBoxActuatorSettingsMicroSpeedOne, textBoxActuatorSettingsAccelerationOne, textBoxActuatorSettingsDecelerationOne);
            }
            else
                switch (listIndex)
                {
                    case 0:
                        result = CheckActuatorSettingsOneDevice(ref mst, textBoxActuatorSettingsSpeedOne, textBoxActuatorSettingsMicroSpeedOne, textBoxActuatorSettingsAccelerationOne, textBoxActuatorSettingsDecelerationOne);
                        break;

                    case 1:
                        result = CheckActuatorSettingsOneDevice(ref mst, textBoxActuatorSettingsSpeedTwo, textBoxActuatorSettingsMicroSpeedTwo, textBoxActuatorSettingsAccelerationTwo, textBoxActuatorSettingsDecelerationTwo);
                        break;

                    case 2:
                        result = CheckActuatorSettingsOneDevice(ref mst, textBoxActuatorSettingsSpeedThree, textBoxActuatorSettingsMicroSpeedThree, textBoxActuatorSettingsAccelerationThree, textBoxActuatorSettingsDecelerationThree);
                        break;
                }

            if (result == true)
            {
                controller.ActuatorInContext.SetMoveSettings(controller.ActuatorInContext.DeviceID, ref mst);
                return true;
            }
            else return false;
        }

        private bool CheckActuatorSettingsOneDevice(ref move_settings_t mst, TextBox textBoxSpeed, TextBox textBoxMicroSpeed, TextBox textBoxAccel, TextBox textBoxDecel, TextBox senderTextBox = null)
        {
            if (textBoxSpeed.Text == "" ||
                textBoxMicroSpeed.Text == "" ||
                textBoxAccel.Text == "" ||
                textBoxDecel.Text == "")
            {
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement aborted, one or move actuator settings values are empty"));
                return false;
            }

            else if (textBoxSpeed.Text == "0")
            {
                if (textBoxMicroSpeed.Text == "0")
                {
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement aborted, minimum speed supported is 0 steps and 1 microsteps"));
                    return false;
                }
            }

            else if (CheckMicroStepsValue(int.Parse(textBoxMicroSpeed.Text), Apsl.MaxPositionMicroStepsAllDevices, Apsl.MinPositionMicroStepsAllDevices) == false)
                return false;

            else if (textBoxAccel.Text == "0")
            {
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement aborted, acceleration value must be a positive integer value. Recommended value: 1000"));
                return false;
            }

            else if (textBoxDecel.Text == "0")
            {
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Movement aborted, deceleration value must be a positive integer value. Recommended value: 1000"));
                return false;
            }

            else if (int.Parse(textBoxSpeed.Text) > 1500)
            {
                mst.Speed = 1500;
                textBoxSpeed.Text = "1500";
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Steps speed exceeds maximum value of 1500, value used will be 1500"));
            }

            else if (int.Parse(textBoxAccel.Text) > 4000)
            {
                mst.Accel = 4000;
                textBoxAccel.Text = "4000";
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Acceleretaion value exceeds maximum of 4000, value used will be 4000"));
            }

            else if (int.Parse(textBoxDecel.Text) > 4000)
            {
                mst.Decel = 4000;
                textBoxDecel.Text = "4000";
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Deceleretaion value exceeds maximum of 4000, value used will be 4000"));
            }

            return true;
        }

        #endregion

        #region Text Box Value Validity

        private void CheckSignedTextboxValidityAtPreviewInput(TextCompositionEventArgs e)
        {
            // Get characters from the text which is to be pasted
            char[] chars = (e.Source as TextBox).Text.ToCharArray();

            // Get caret (cursor) position inside the textBox
            int caretPosition = (e.Source as TextBox).SelectionStart;

            // Get selection lenght (highlighted text) inside textBox, if any
            int selectionLength = (e.Source as TextBox).SelectionLength;

            // Create a string containing the text from the textBox before the caret's position
            string preSelectionString = (e.Source as TextBox).Text.Substring(0, caretPosition);

            // Create a string containing the text from the textBox after the caret's position
            string postSelectionString = (e.Source as TextBox).Text.Substring(caretPosition + selectionLength, (e.Source as TextBox).Text.Length - (caretPosition + selectionLength));

            // Create a string that would represent the textBox's text after it would pe pasted
            string newTextBoxString = preSelectionString + e.Text + postSelectionString;

            // TO DO
            // Text cannot have 0 value, or start with a 0
            //if (newTextBoxString.Length >= 0 && newTextBoxString[0] == '0')
            // Text is not in correct format, cancel further processing
            // e.Handled = true;

            // Text cannot start with "-0"
            if (newTextBoxString.Length > 1 && newTextBoxString[0] == '-' && newTextBoxString[1] == '0')
                // Text is not in correct format, cancel further processing
                e.Handled = true;

            //  If there are illegal characters inside the new text
            else if (_signedRegex.IsMatch(newTextBoxString) == true)
                // Text is not in correct format, cancel further processing
                e.Handled = true;

            // If the lenght of the new text is larger than the maximum allowed 6 characters
            else if (newTextBoxString.Length > 6)
                // Text is not in correct format, cancel further processing
                e.Handled = true;

            else
                for (int i = 1; i < newTextBoxString.Length; i++)
                    // If any characters after the first character are "-" signs
                    if ((int)newTextBoxString[i] == 45)
                        // Text is not in correct format, cancel further processing
                        e.Handled = true;
        }

        private bool CheckSignedTextboxValidityAtPaste(string text, DataObjectPastingEventArgs e)
        {
            // Get characters from the text which is to be pasted
            char[] chars = text.ToCharArray();

            // If there are any characters to be pasted
            if (chars.Length > 0)
            {
                // If there are any caracters in the textBox in focus
                if ((e.Source as TextBox).Text.Length > 0)
                {
                    // Get caret (cursor) position inside the textBox
                    int caretPosition = (e.Source as TextBox).SelectionStart;

                    // Get selection lenght (highlighted text) inside textBox, if any
                    int selectionLength = (e.Source as TextBox).SelectionLength;

                    // Create a string containing the text from the textBox before the caret's position
                    string preSelectionString = (e.Source as TextBox).Text.Substring(0, caretPosition);

                    // Create a string containing the text from the textBox after the caret's position
                    string postSelectionString = (e.Source as TextBox).Text.Substring(caretPosition + selectionLength, (e.Source as TextBox).Text.Length - (caretPosition + selectionLength));

                    // Create a string that would represent the textBox's text after it would pe pasted
                    string newTextBoxString = preSelectionString + text + postSelectionString;

                    //  If there are illegal characters inside the new text
                    if (_signedRegex.IsMatch(newTextBoxString) == true)
                        // Text is not in correct format, return false
                        return false;

                    // If the lenght of the new text is larger than the maximum allowed 6 characters
                    else if (newTextBoxString.Length > 6)
                        // Text is not in correct format, return false
                        return false;

                    else
                        for (int i = 1; i < newTextBoxString.Length; i++)
                            // If any characters after the first character are "-" signs
                            if ((int)newTextBoxString[i] == 45)
                                // Text is not in correct format, return false
                                return false;
                }
            }
            // New text is in correct format, return true
            return true;
        }

        #endregion

        #region Connection Settings Swap Handling

        private void ComboBoxConnectionSettingsHandleSwap()
        {
            try
            {
                int[] newIndexValues = new int[3];

                newIndexValues[0] = comboBoxConnectionSettingsDeviceOne.SelectedIndex;
                newIndexValues[1] = comboBoxConnectionSettingsDeviceTwo.SelectedIndex;
                newIndexValues[2] = comboBoxConnectionSettingsDeviceThree.SelectedIndex;

                if (comboBoxOneIndex != -1 && comboBoxTwoIndex != -1 && comboBoxThreeIndex != -1)
                    if (newIndexValues[0] != comboBoxOneIndex)
                        GetDifferentConnectionSettingsComboBoxIndex(newIndexValues[0],
                                                comboBoxConnectionSettingsDeviceTwo,
                                                comboBoxConnectionSettingsDeviceThree).SelectedIndex = comboBoxOneIndex;

                    else if (newIndexValues[1] != comboBoxTwoIndex)
                        GetDifferentConnectionSettingsComboBoxIndex(newIndexValues[1],
                                                comboBoxConnectionSettingsDeviceOne,
                                                comboBoxConnectionSettingsDeviceThree).SelectedIndex = comboBoxTwoIndex;

                    else if (newIndexValues[2] != comboBoxThreeIndex)
                        GetDifferentConnectionSettingsComboBoxIndex(newIndexValues[2],
                                                                    comboBoxConnectionSettingsDeviceOne,
                                                                    comboBoxConnectionSettingsDeviceTwo).SelectedIndex = comboBoxThreeIndex;

                // TO DO - comment
                // Do not change order - Subsequent calls are dependent on the previous one to finish
                GetConnectionSettingsComboBoxSelectedItemIndex();

                // ELI
                SetActuatorAxis();

                ArrangeMoveContinuouslyPanel(deviceCount);
                SetLabelsText();
                SetSliderValueBindings();
                IPcore.TASettings.SetConnectedAxis(controller.Actuators);
                IPcore.TrackingSettings.JustChangedAxis = true;

                // TO DO - delete
                Console.WriteLine(comboBoxConnectionSettingsDeviceOne.SelectedIndex);
                Console.WriteLine(comboBoxConnectionSettingsDeviceTwo.SelectedIndex);
                Console.WriteLine(comboBoxConnectionSettingsDeviceThree.SelectedIndex);
            }
            catch
            { /* TO DO */
            }
        }

        private ComboBox GetDifferentConnectionSettingsComboBoxIndex(int newIndexValue, ComboBox cb1, ComboBox cb2)
        {
            if (cb1.SelectedIndex == newIndexValue)
                return cb1;
            else if (cb2.SelectedIndex == newIndexValue)
                return cb2;

            return null;
        }

        private void GetConnectionSettingsComboBoxSelectedItemIndex()
        {
            comboBoxOneIndex = comboBoxConnectionSettingsDeviceOne.SelectedIndex;
            comboBoxTwoIndex = comboBoxConnectionSettingsDeviceTwo.SelectedIndex;
            comboBoxThreeIndex = comboBoxConnectionSettingsDeviceThree.SelectedIndex;
        }

        private void SetActuatorAxis()
        {
            textBoxAxisLink.LookUpTable = new Dictionary<TextBoxPair, Enums.Axis>();

            // Loop through each connected actuator
            for (int i = 0; i < Actuators.List.Count; i++)
            {
                // Dictionary translating comboBox index to Axis (X, Y, or Z) enum
                Dictionary<int, Enums.Axis> dictionary = new Dictionary<int, Enums.Axis>()
                {
                    {0, Enums.Axis.X}, {1, Enums.Axis.Y}, {2, Enums.Axis.Z}
                };

                try
                {
                    // Get actuator
                    ActuatorController currentActuator = Actuators.List[i];

                    // Set actuator axis based on index of comboBox
                    if (i == 0)
                    {
                        currentActuator.SetAxis(currentActuator.DeviceID, dictionary[comboBoxOneIndex]);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairDeviceOne, dictionary[comboBoxOneIndex], Enums.AxisDeviceType.DeviceOne);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairDeviceTwo, dictionary[comboBoxTwoIndex], Enums.AxisDeviceType.DeviceTwo);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairDeviceThree, dictionary[comboBoxThreeIndex], Enums.AxisDeviceType.DeviceThree);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairOneDeviceMoveTo, Enums.Axis.DontCare, Enums.AxisDeviceType.MoveTo);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairOneDeviceShift, Enums.Axis.DontCare, Enums.AxisDeviceType.Shift);
                    }
                    else if (i == 1)
                    {
                        currentActuator.SetAxis(currentActuator.DeviceID, dictionary[comboBoxTwoIndex]);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairDeviceTwo, dictionary[comboBoxTwoIndex], Enums.AxisDeviceType.DeviceTwo);
                    }
                    else if (i == 2)
                    {
                        currentActuator.SetAxis(currentActuator.DeviceID, dictionary[comboBoxThreeIndex]);
                        textBoxAxisLink.CreateTextBoxAxisLink(textBoxAxisLink.PairDeviceThree, dictionary[comboBoxThreeIndex], Enums.AxisDeviceType.DeviceThree);
                    }

                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Actuator with ID = " + currentActuator.DeviceID + " and name " + currentActuator.UIName + " got connected as axis " + currentActuator.GetAxis(currentActuator.DeviceID).ToString()));
                }
                // TO DO - log
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        #endregion

        #region Move Relatively One Device handling

        private void MoveToPositionOneDeviceEventHandling(string steps, string uSteps, int MRlistIndex = 0, TextBox senderTextBox = null, Enums.Axis trackedAxis = Enums.Axis.DontCare)
        {
            if (CheckMoveToPositionOneDeviceSafetyValues(int.Parse(steps), int.Parse(uSteps), senderTextBox, trackedAxis) == true)
                if (TryConfiguringActuatorSettingsOneDevice(MRlistIndex) == true)
                    controller.ActuatorMoveToPosition(controller.ActuatorInContext.DeviceID, int.Parse(steps), int.Parse(uSteps));
        }

        private void ShiftOnPositionOneDeviceEventhandling(string steps, string uSteps, int MRlistIndex = 0, TextBox senderTextBox = null)
        {
            if (CheckShiftOnOneDeviceFinalPosition(int.Parse(steps), int.Parse(uSteps), senderTextBox, MRlistIndex) == true)
            {
                SetTextBoxState(senderTextBox, Enums.TextBoxState.Default);

                if (TryConfiguringActuatorSettingsOneDevice(MRlistIndex) == true)
                    controller.ActuatorMoveRelatively(controller.ActuatorInContext.DeviceID, int.Parse(steps), int.Parse(uSteps));
            }
            else
            {
                if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot shift all by desired amount, some actuators would go out of bounds")
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot shift all by desired amount, some actuators would go out of bounds"));

                SetTextBoxState(senderTextBox, Enums.TextBoxState.Warning);
            }
        }

        #endregion

        #region Move Relatively Multiple Devices Handling

        private void HandleMoveRelativelyMultipleDevices(Button targetButton, TextBox textBoxOne, TextBox textBoxTwo, int listIndex)
        {
            if (targetButton.Content.ToString() == "Move")
            {
                if (CheckMoveToPositionOneDeviceSafetyValues(int.Parse(textBoxOne.Text), int.Parse(textBoxMoveRelativelyToMicroStepsOne.Text), textBoxOne) == true)
                    if (TryConfiguringActuatorSettingsOneDevice(listIndex) == true)
                        controller.ActuatorMoveToPosition(Actuators.List[listIndex].DeviceID, int.Parse(textBoxOne.Text), int.Parse(textBoxTwo.Text));
            }
            else if (targetButton.Content.ToString() == "Shift")
            {
                if (controller.IsActuatorMoving(listIndex))
                {
                    if (CheckShiftOnOneDeviceFinalPosition(int.Parse(textBoxOne.Text), int.Parse(textBoxTwo.Text), textBoxOne, listIndex) == true)
                        if (TryConfiguringActuatorSettingsOneDevice(listIndex) == true)
                            controller.ActuatorMoveRelatively(Actuators.List[listIndex].DeviceID, int.Parse(textBoxOne.Text), int.Parse(textBoxTwo.Text));
                }
                else
                {
                    if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Please wait until actuator finishes movement")
                        Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Please wait until actuator finishes movement"));
                }
            }
        }


        private void HandleMoveRelativelyHomeMultipleDevices(int comboBoxSelectedIndex, int listIndex)
        {
            // Get min edge for specific axis
            int minEdge = Apsl.GetActEdgeVal(GetAxisForConnSetingsCheckbox(comboBoxSelectedIndex), Enums.ApslEdgeType.Min);

            if (minEdge <= 0)
            {
                // Move home
                if (TryConfiguringActuatorSettingsOneDevice(listIndex) == true)
                    controller.ActuatorMoveToPosition(Actuators.List[listIndex].DeviceID, 0, 0);
            }
            else if (minEdge > 0)
            {
                // Cannot move home
                if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot move home (zero position) because inferior edge limit is larger than zero")
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot move home (zero position) because inferior edge limit is larger than zero"));
            }
        }

        private void HandleMoveRelativelyHomeAllDevices()
        {
            for (int i = 0; i < controller.Actuators.Count; i++)
            {
                int minEdge = -1;
                Enums.Axis currentAxis = new Enums.Axis();

                if (i == 0)
                {
                    minEdge = Apsl.GetActEdgeVal(currentAxis = GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceOne.SelectedIndex), Enums.ApslEdgeType.Min);
                }
                else if (i == 1)
                {
                    minEdge = Apsl.GetActEdgeVal(currentAxis = GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceTwo.SelectedIndex), Enums.ApslEdgeType.Min);
                }
                else if (i == 2)
                {
                    minEdge = Apsl.GetActEdgeVal(currentAxis = GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceThree.SelectedIndex), Enums.ApslEdgeType.Min);
                }

                if (minEdge <= 0)
                {
                    // Move home
                    if (TryConfiguringActuatorSettingsOneDevice(i) == true)
                        controller.ActuatorMoveToPosition(Actuators.List[i].DeviceID, 0, 0);
                }
                else if (minEdge > 0)
                {
                    // Cannot move home
                    if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Cannot move home (zero position) axis " + currentAxis.ToString() + " because inferior edge limit is larger than zero")
                        Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Cannot move home (zero position) axis " + currentAxis.ToString() + " because inferior edge limit is larger than zero"));
                }
            }
        }

        #endregion

        #region Move Relatively Position Calculations

        private bool CheckMicroStepsValue(int microStepsValue, int maxMicroSteps, int minMicroSteps)
        {
            if (microStepsValue > maxMicroSteps)
            {
                Console.WriteLine("Warning: Microsteps position cannot be larger than " + maxMicroSteps +
                                  ". Please set values between " + minMicroSteps + " and " + maxMicroSteps + " microsteps");
                return false;
            }

            else if (microStepsValue < minMicroSteps)
            {
                Console.WriteLine("Warning: Microsteps position cannot be smaller than " + minMicroSteps +
                                  ". Please set values between " + minMicroSteps + " and " + maxMicroSteps + " microsteps");
                return false;
            }
            else return true;
        }

        private bool CheckMoveToPositionOneDeviceSafetyValues(int stepsValue, int microStepsValue, TextBox textBoxSender = null, Enums.Axis trackedAxis = Enums.Axis.DontCare)
        {
            int minEdgeSteps = 0, maxEdgeSteps = 0, minMicroSteps = 0, maxMicroSteps = 0;

            if (trackedAxis == Enums.Axis.DontCare)
            {
                minEdgeSteps = Apsl.GetActEdgeVal(textBoxAxisLink.GetAxisFromTextBox(textBoxSender), Enums.ApslEdgeType.Min);
                maxEdgeSteps = Apsl.GetActEdgeVal(textBoxAxisLink.GetAxisFromTextBox(textBoxSender), Enums.ApslEdgeType.Max);
            }
            else
            {
                minEdgeSteps = Apsl.GetActEdgeVal(trackedAxis, Enums.ApslEdgeType.Min);
                maxEdgeSteps = Apsl.GetActEdgeVal(trackedAxis, Enums.ApslEdgeType.Max);
            }

            // OO DO - OK ?
            minMicroSteps = Apsl.MinPositionMicroStepsAllDevices;
            maxMicroSteps = Apsl.MaxPositionMicroStepsAllDevices;

            if (CheckMicroStepsValue(microStepsValue, maxMicroSteps, minMicroSteps) == false)
                return false;

            else if (stepsValue > maxEdgeSteps)
            {
                Console.WriteLine("Warning: Position " + stepsValue + " steps and " + microStepsValue +
                                  " microsteps exceeds superior safety range! Please set values between " + minEdgeSteps +
                                  " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue == maxEdgeSteps && microStepsValue > 0)
            {
                Console.WriteLine("Warning: Position " + stepsValue + " steps and " + microStepsValue +
                                  " microsteps exceeds superior safety range! Please set values between " + minEdgeSteps +
                                  " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue == minEdgeSteps && microStepsValue < 0)
            {
                Console.WriteLine("Warning: Position " + stepsValue + " steps and " + microStepsValue +
                                  " microsteps exceeds inferior safety range! Please set values between " + minEdgeSteps +
                                  " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue < minEdgeSteps)
            {
                Console.WriteLine("Warning: Position " + stepsValue + " steps and " + microStepsValue +
                                  " microsteps exceeds inferior safety range! Please set values between " + minEdgeSteps +
                                  " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }
            else return true;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <param name="stepsValue"></param>
        /// <param name="microStepsValue"></param>
        /// <returns></returns>
        private bool CheckShiftOnOneDeviceFinalPosition(int stepsValue, int microStepsValue, TextBox textBoxSender = null, int MRListIndex = -1)
        {
            int deviceID = controller.ActuatorInContext.DeviceID;

            int minEdgeSteps = Apsl.GetActEdgeVal(textBoxAxisLink.GetAxisFromTextBox(textBoxSender), Enums.ApslEdgeType.Min);
            int maxEdgeSteps = Apsl.GetActEdgeVal(textBoxAxisLink.GetAxisFromTextBox(textBoxSender), Enums.ApslEdgeType.Max);
            int maxMicroSteps = Apsl.MaxPositionMicroStepsAllDevices;
            int minMicroSteps = Apsl.MinPositionMicroStepsAllDevices;
            int currentPositionSteps = 0, currentPositionMicroSteps = 0;

            if (checkBoxMoveRelativelyApplyForAll.IsChecked == false)
            {
                if (MRListIndex == -1)
                {
                    currentPositionSteps = controller.GetMaxStepsPosFromAllActuators();
                    currentPositionMicroSteps = controller.GetMaxMicroStepsPosFromAllActuators();
                }
                else
                {
                    controller.ChangeContext(MRListIndex);
                    currentPositionSteps = controller.ActuatorInContext.GetCurrentPositionSteps(deviceID);
                    currentPositionMicroSteps = controller.ActuatorInContext.GetCurrentPositionMicroSteps(deviceID);
                }
            }
            else
            {
                if (MRListIndex == -1)
                {
                    currentPositionSteps = controller.GetMaxStepsPosFromAllActuators();
                    currentPositionMicroSteps = controller.GetMaxMicroStepsPosFromAllActuators();
                }
                else
                {
                    controller.ChangeContext(MRListIndex);
                    currentPositionSteps = controller.ActuatorInContext.GetCurrentPositionSteps(deviceID);
                    currentPositionMicroSteps = controller.ActuatorInContext.GetCurrentPositionMicroSteps(deviceID);
                }
            }

            //if (MRListIndex == -1)
            //{
            //    currentPositionSteps = controller.ActuatorInContext.GetCurrentPositionSteps(deviceID);
            //    currentPositionMicroSteps = controller.ActuatorInContext.GetCurrentPositionMicroSteps(deviceID);
            //}
            //else if(checkBoxMoveRelativelyApplyForAll.IsChecked == true && MRListIndex == -1)
            //{
            //    currentPositionSteps = controller.GetMaxStepsPosFromAllActuators();
            //    currentPositionMicroSteps = controller.GetMaxMicroStepsPosFromAllActuators();
            //}
            //else if(checkBoxMoveRelativelyApplyForAll.IsChecked == true && MRListIndex != -1)
            //{
            //    controller.ChangeContext(MRListIndex);
            //    currentPositionSteps = controller.ActuatorInContext.GetCurrentPositionSteps(deviceID);
            //    currentPositionMicroSteps = controller.ActuatorInContext.GetCurrentPositionMicroSteps(deviceID);
            //}


            if (CheckMicroStepsValue(microStepsValue, maxMicroSteps, minMicroSteps) == false)
                return false;

            else if (stepsValue + currentPositionSteps > maxEdgeSteps)
            {
                Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps) + " steps and " + microStepsValue +
                              " microsteps will exceed superior safety range! Please set values between " + minEdgeSteps +
                              " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue + currentPositionSteps == maxEdgeSteps && microStepsValue > 0)
            {
                Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps) + " steps and " + microStepsValue +
                             " microsteps will exceed superior safety range! Please set values between " + minEdgeSteps +
                             " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue + currentPositionSteps == (maxEdgeSteps - 1) && microStepsValue > 0)
            {
                if (microStepsValue + currentPositionMicroSteps > 256)
                {
                    Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps + 1) + " steps and " + (microStepsValue + currentPositionMicroSteps - 256) +
                             " microsteps will exceed superior safety range! Please set values between " + minEdgeSteps +
                             " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                    return false;
                }
            }

            else if (stepsValue + currentPositionSteps < minEdgeSteps)
            {
                Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps) + " steps and " + microStepsValue +
                              " microsteps will exceed inferior safety range! Please set values between " + minEdgeSteps +
                              " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue + currentPositionSteps == minEdgeSteps && microStepsValue < 0)
            {
                Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps) + " steps and " + microStepsValue +
                              " microsteps will exceed inferior safety range! Please set values between " + minEdgeSteps +
                              " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                return false;
            }

            else if (stepsValue + currentPositionSteps == (minEdgeSteps + 1) && microStepsValue < 0)
            {
                if (microStepsValue + currentPositionMicroSteps < -256)
                {
                    Console.WriteLine("Warning: Position " + (stepsValue + currentPositionSteps - 1) + " steps and " + (microStepsValue + currentPositionMicroSteps + 256) +
                             " microsteps will exceed inferior safety range! Please set values between " + minEdgeSteps +
                             " steps, 0 μsteps and " + maxEdgeSteps + " steps, 0 μsteps");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Move Relatively button and label change

        private void HandleMoveRelativelyButtonTextChange(ComboBox senderComboBox, Button targetButton)
        {
            string[] parts = senderComboBox.SelectedItem.ToString().Split(' ');

            if (targetButton != null)
                targetButton.Content = parts[1];
        }

        #endregion

        #region Move Relatively ComboBox Apply For All

        private void HandleMoveRelativelyComboBoxApplyForAllAction()
        {
            if (checkBoxMoveRelativelyApplyForAll.IsChecked == true)
            {
                ShowGrid(gridMoveRelativelyFirstRowOneDevice);
                ShowGrid(gridMoveRelativelySecondRowOneDevice);
                ShowGrid(gridMoveRelativelyThirdRowOneDevice);

                HideGrid(gridMoveRelativelyFirstRowMultipleDevices);
                HideGrid(gridMoveRelativelySecondRowMultipleDevices);
                HideGrid(gridMoveRelativelyThirdRowMultipleDevices);

                rowDefinitionMoveRelativelyPanel.Height = new GridLength(160);

                rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                HideGrid(gridMoveRelativelyFirstRowOneDevice);
                HideGrid(gridMoveRelativelySecondRowOneDevice);
                HideGrid(gridMoveRelativelyThirdRowOneDevice);

                if (controller.Actuators.Count == 1)
                {
                    ShowGrid(gridMoveRelativelyFirstRowMultipleDevices);

                    rowDefinitionMoveRelativelyPanel.Height = new GridLength(130);
                    rowDefinitionMoveRelativelyRowDeviceTwo.Height = new GridLength(0, GridUnitType.Star);
                    rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(0, GridUnitType.Star);
                }
                else if (controller.Actuators.Count == 2)
                {
                    ShowGrid(gridMoveRelativelyFirstRowMultipleDevices);
                    ShowGrid(gridMoveRelativelySecondRowMultipleDevices);

                    rowDefinitionMoveRelativelyPanel.Height = new GridLength(160);
                    rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(0, GridUnitType.Star);
                }
                else if (controller.Actuators.Count == 3)
                {
                    ShowGrid(gridMoveRelativelyFirstRowMultipleDevices);
                    ShowGrid(gridMoveRelativelySecondRowMultipleDevices);
                    ShowGrid(gridMoveRelativelyThirdRowMultipleDevices);

                    rowDefinitionMoveRelativelyPanel.Height = new GridLength(190);
                    rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(1, GridUnitType.Star);

                }
            }
        }

        #endregion

        #region Application Configuration Manager methods

        public List<string> GetConnectionSettingsComboBoxSelectedIndex()
        {
            List<string> axisString = GetConnectionSettingsSelectedText();
            List<string> selectedIndexList = new List<string>();

            foreach (string axis in axisString)
                switch (axis)
                {
                    case "X":
                        selectedIndexList.Add("0");
                        break;

                    case "Y":
                        selectedIndexList.Add("1");
                        break;

                    case "Z":
                        selectedIndexList.Add("2");
                        break;

                    default:
                        break;
                }

            return selectedIndexList;
        }

        public List<string> GetMoveRelativelyMultipleDevicesComboBoxSelectedIndex()
        {
            List<string> selectedIndexList = new List<string>()
            {
                comboBoxMoveRelativelyDeviceOne.SelectedIndex.ToString(),
                comboBoxMoveRelativelyDeviceTwo.SelectedIndex.ToString(),
                comboBoxMoveRelativelyDeviceThree.SelectedIndex.ToString()
            };

            return selectedIndexList;
        }

        #endregion

        #region Set Labels Text

        private void SetLabelsText()
        {
            List<string> deviceNames = new List<string>();

            for (int i = 0; i < deviceCount; i++)
            {
                deviceNames.Add(Actuators.List[i].UIName);

                switch (i)
                {
                    case 0:
                        SetLabelsTextDeviceOne(deviceNames[i]);
                        break;

                    case 1:
                        SetLabelsTextDeviceTwo(deviceNames[i]);
                        break;

                    case 2:
                        SetLabelsTextDeviceThree(deviceNames[i]);
                        break;
                }
            }

            SetSliderAxisLabels(deviceNames);
        }

        private void SetLabelsTextDeviceOne(string deviceName)
        {
            textBlockctuatorSettingsDeviceOneLabel.Text = deviceName;
            textBlockConnectionSettingsDeviceOne.Text = deviceName + " to axis: ";
            textBlockMoveRelativelyDeviceOneLabel.Text = deviceName;
            textBlockActuatorStatusDeviceOneLabel.Text = deviceName;
            textBlockPowerStatusDeviceOneLabel.Text = deviceName;
        }

        private void SetLabelsTextDeviceTwo(string deviceName)
        {
            textBlockctuatorSettingsDeviceTwoLabel.Text = deviceName;
            textBlockConnectionSettingsDeviceTwo.Text = deviceName + " to axis: ";
            textBlockMoveRelativelyDeviceTwoLabel.Text = deviceName;
            textBlockActuatorStatusDeviceTwoLabel.Text = deviceName;
            textBlockPowerStatusDeviceTwoLabel.Text = deviceName;
        }

        private void SetLabelsTextDeviceThree(string deviceName)
        {
            textBlockctuatorSettingsDeviceThreeLabel.Text = deviceName;
            textBlockConnectionSettingsDeviceThree.Text = deviceName + " to axis: ";
            textBlockMoveRelativelyDeviceThreeLabel.Text = deviceName;
            textBlockActuatorStatusDeviceThreeLabel.Text = deviceName;
            textBlockPowerStatusDeviceThreeLabel.Text = deviceName;
        }

        private void SetSliderAxisLabels(List<string> deviceNames)
        {
            List<string> axisString = GetConnectionSettingsSelectedText();

            for (int i = 0; i < deviceNames.Count; i++)
                GetSliderLabelForSpecificAxis(axisString[i]).Text = "Axis " + axisString[i] + " (" + deviceNames[i] + ")";
        }

        public List<string> GetConnectionSettingsSelectedText()
        {
            string[] comboBoxOne = comboBoxConnectionSettingsDeviceOne.SelectedItem.ToString().Split(' ');
            string[] comboBoxTwo = comboBoxConnectionSettingsDeviceTwo.SelectedItem.ToString().Split(' ');
            string[] comboBoxThree = comboBoxConnectionSettingsDeviceThree.SelectedItem.ToString().Split(' ');

            List<string> axisString = new List<string>()
            {
                comboBoxOne[1],
                comboBoxTwo[1],
                comboBoxThree[1]
            };

            return axisString;
        }

        private TextBlock GetSliderLabelForSpecificAxis(string axis)
        {
            switch (axis)
            {
                case "X":
                    return textBlockMoveContinuouslyAxisX;

                case "Y":
                    return textBlockMoveContinuouslyAxisY;

                case "Z":
                    return textBlockMoveContinuouslyAxisZ;

                default:
                    return null;
            }
        }

        #endregion

        #region Slider Bindings

        private void SetSliderValueBindings()
        {
            List<string> axisString = GetConnectionSettingsSelectedText();

            for (int i = 0; i < axisString.Count; i++)
                switch (i)
                {
                    case 0:
                        CreateSliderValueBinding(GetSliderForSpecificAxis(axisString[i]), textBlockActuatorStatusPositionDeviceOne);
                        break;

                    case 1:
                        CreateSliderValueBinding(GetSliderForSpecificAxis(axisString[i]), textBlockActuatorStatusPositionDeviceTwo);
                        break;

                    case 2:
                        CreateSliderValueBinding(GetSliderForSpecificAxis(axisString[i]), textBlockActuatorStatusPositionDeviceThree);
                        break;
                }

            SetSliderEdgeValues();
        }

        private Slider GetSliderForSpecificAxis(string axis)
        {
            switch (axis)
            {
                case "X":
                    return sliderMoveContinuouslyAxisX;

                case "Y":
                    return sliderMoveContinuouslyAxisY;

                case "Z":
                    return sliderMoveContinuouslyAxisZ;

                default:
                    return null;
            }
        }

        private void CreateSliderValueBinding(Slider targetSlider, TextBlock targetTextBlock)
        {
            Binding newBinding = new Binding("Text")
            {
                Source = targetTextBlock
            };

            targetSlider.SetBinding(Slider.ValueProperty, newBinding);
        }

        private void SetSliderEdgeValues()
        {
            sliderMoveContinuouslyAxisX.Minimum = Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition;
            sliderMoveContinuouslyAxisY.Minimum = Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition;
            sliderMoveContinuouslyAxisZ.Minimum = Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition;
            //Apsl.ApslLimBundle.GlobalMinEdgeStepsPosition;

            sliderMoveContinuouslyAxisX.Maximum = Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition;
            sliderMoveContinuouslyAxisY.Maximum = Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition;
            sliderMoveContinuouslyAxisZ.Maximum = Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition;
            //Apsl.ApslLimBundle.GlobalMaxEdgeStepsPosition;
        }

        #endregion

        #region Set Edge Values

        private void SetGlobalEdgeLimitsUI(bool isSenderButton = false)
        {
            if (isSenderButton == true)
            {
                Apsl.ApslLimBundle.GlobalMinEdgeStepsPosition = int.Parse(textBoxActuatorSettingsMinEdgeOne.Text);
                Apsl.ApslLimBundle.GlobalMaxEdgeStepsPosition = int.Parse(textBoxActuatorSettingsMaxEdgeOne.Text);

                Apsl.SetGlobalEdgeLimits();
            }
            else
            {
                Apsl.SetGlobalEdgeLimits();

                textBoxActuatorSettingsMinEdgeOne.Text = Apsl.ApslLimBundle.GlobalMinEdgeStepsPosition.ToString();
                textBoxActuatorSettingsMaxEdgeOne.Text = Apsl.ApslLimBundle.GlobalMaxEdgeStepsPosition.ToString();
            }

            if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Please note that edge limits have changed and are identical for every axis")
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Please note that edge limits have changed and are identical for every axis"));
        }

        public void SetEdgeLimitsPerAxisUI(bool initSender = false)
        {
            if (Apsl != null)
                if (initSender == false)
                {
                    Apsl.SetEdgeLimitsPerAxis();
                    SetApslEdgeLimits();
                }
        }

        private void SetApslEdgeLimits()
        {
            int.TryParse(textBoxActuatorSettingsMinEdgeOne.Text, out int minEdgeOne);
            int.TryParse(textBoxActuatorSettingsMaxEdgeOne.Text, out int maxEdgeOne);
            int.TryParse(textBoxActuatorSettingsMinEdgeTwo.Text, out int minEdgeTwo);
            int.TryParse(textBoxActuatorSettingsMaxEdgeTwo.Text, out int maxEdgeTwo);
            int.TryParse(textBoxActuatorSettingsMinEdgeThree.Text, out int minEdgeThree);
            int.TryParse(textBoxActuatorSettingsMaxEdgeThree.Text, out int maxEdgeThree);

            // Small patch
            Apsl.SaveDeviceOneTextBoxValues(minEdgeOne, maxEdgeOne);

            //AssignEdgeLimitsToAxis(minEdgeOne, maxEdgeOne, minEdgeTwo, maxEdgeTwo, minEdgeThree, maxEdgeThree);

            AssignEdgeLimitsToAxis(minEdgeOne, maxEdgeOne, GetAxisForActuatorSettingsTextBox(textBoxActuatorSettingsMinEdgeOne));
            AssignEdgeLimitsToAxis(minEdgeTwo, maxEdgeTwo, GetAxisForActuatorSettingsTextBox(textBoxActuatorSettingsMinEdgeTwo));
            AssignEdgeLimitsToAxis(minEdgeThree, maxEdgeThree, GetAxisForActuatorSettingsTextBox(textBoxActuatorSettingsMinEdgeThree));

            HandleTextBoxTypingSafetyAfterEdgeSet();

            if (checkBoxActuatorSettingApplyForAll.IsChecked == true)
            {
                if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Global edge limits have been successfully set")
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Global edge limits have been successfully set"));
            }
            else
            {
                if (Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Edge limits for the " + controller.Actuators.Count.ToString() + " axis have been successfully set")
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Edge limits for the " + controller.Actuators.Count.ToString() + " axis have been successfully set"));
            }
        }

        private void AssignEdgeLimitsToAxis(int minEdge, int maxEdge, Enums.Axis axis)
        {
            switch (axis)
            {
                case Enums.Axis.X:
                    Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition = minEdge;
                    Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition = maxEdge;
                    break;

                case Enums.Axis.Y:
                    Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition = minEdge;
                    Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition = maxEdge;
                    break;

                case Enums.Axis.Z:
                    Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition = minEdge;
                    Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition = maxEdge;
                    break;

                default:
                    break;
            }
        }

        private void HandleTextBoxTypingSafetyAfterEdgeSet()
        {
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveToPositionStepsOneDevice, textBoxMoveToPositionMicroStepsOneDevice, Enums.TextBoxValueSafetyType.MoveTo, buttonMoveToPositionOneDevice);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxShiftToStepsOneDevice, textBoxShiftToMicroStepsOneDevice, Enums.TextBoxValueSafetyType.ShiftOn, buttonShiftOnOneDevice);

            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsOne, textBoxMoveRelativelyToMicroStepsOne, GetEnumFromComboBoxIndex(comboBoxMoveRelativelyDeviceOne.SelectedIndex), buttonMoveRelativelyDeviceOne);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsTwo, textBoxMoveRelativelyToMicroStepsTwo, GetEnumFromComboBoxIndex(comboBoxMoveRelativelyDeviceTwo.SelectedIndex), buttonMoveRelativelyDeviceTwo);
            HandleMoveRelativelyTextBoxTypingSafety(textBoxMoveRelativelyToStepsThree, textBoxMoveRelativelyToMicroStepsThree, GetEnumFromComboBoxIndex(comboBoxMoveRelativelyDeviceThree.SelectedIndex), buttonMoveRelativelyDeviceThree);
        }

        private Enums.Axis GetAxisForActuatorSettingsTextBox(TextBox textBox)
        {
            if (textBox == textBoxActuatorSettingsMinEdgeOne || textBox == textBoxActuatorSettingsMaxEdgeOne)
                return GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceOne.SelectedIndex);

            else if (textBox == textBoxActuatorSettingsMinEdgeTwo || textBox == textBoxActuatorSettingsMaxEdgeTwo)
                return GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceTwo.SelectedIndex);

            else if (textBox == textBoxActuatorSettingsMinEdgeThree || textBox == textBoxActuatorSettingsMaxEdgeThree)
                return GetAxisForConnSetingsCheckbox(comboBoxConnectionSettingsDeviceThree.SelectedIndex);

            else return new Enums.Axis();
        }

        private Enums.Axis GetAxisForConnSetingsCheckbox(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0:
                    return Enums.Axis.X;

                case 1:
                    return Enums.Axis.Y;

                case 2:
                    return Enums.Axis.Z;
            }

            return new Enums.Axis();
        }

        public void HandleBoundingBoxButtonValidity()
        {
            if (IPcore.BoundingBoxCalib.HasTopLeftBeenSet && !IPcore.BoundingBoxCalib.HasBottomRightBeenSet)
            {
                if (int.Parse(textBoxBBCTopLeftPixel_X.Text) < int.Parse(textBoxBBCBottomRightPixel_X.Text) &&
                    int.Parse(textBoxBBCTopLeftPixel_Y.Text) < int.Parse(textBoxBBCBottomRightPixel_Y.Text))
                    buttonBoundingBoxSetBottomRight.IsEnabled = true;
                else
                    buttonBoundingBoxSetBottomRight.IsEnabled = false;
            }
            else if (IPcore.BoundingBoxCalib.HasBottomRightBeenSet && !IPcore.BoundingBoxCalib.HasTopLeftBeenSet)
            {
                if (int.Parse(textBoxBBCTopLeftPixel_X.Text) < int.Parse(textBoxBBCBottomRightPixel_X.Text) &&
                    int.Parse(textBoxBBCTopLeftPixel_Y.Text) < int.Parse(textBoxBBCBottomRightPixel_Y.Text))
                    buttonBoundingBoxSetTopLeft.IsEnabled = true;
                else
                    buttonBoundingBoxSetTopLeft.IsEnabled = false;
            }
        }

        #endregion



        #region Arrange Components Main

        private void ArrangeComponents(int deviceCount)
        {
            ArrangeActuatorSettingsPanel(deviceCount);
            ArrangeConnectionSettingsPanel(deviceCount);
            ArrangeMoveRelativelyPanel(deviceCount);
            ArrangeMoveContinuouslyPanel(deviceCount);
            ArrangeActuatorStatusPanel(deviceCount);
            ArrangePowerStatusPanel(deviceCount);
            SetAllowForAllCheckBoxesVisibility();
        }

        private void SetAllowForAllCheckBoxesVisibility()
        {
            if (controller != null && controller.Actuators.Count == 1)
            {
                checkBoxActuatorSettingApplyForAll.Visibility = Visibility.Hidden;
                checkBoxMoveRelativelyApplyForAll.Visibility = Visibility.Hidden;
                rowDefinitionActuatorSettingsCheckBox.Height = new GridLength(10);
                rowDefinitionMoveRelativelyCheckBox.Height = new GridLength(0);
            }
            else if (controller != null && controller.Actuators.Count > 1)
            {
                checkBoxActuatorSettingApplyForAll.Visibility = Visibility.Visible;
                checkBoxMoveRelativelyApplyForAll.Visibility = Visibility.Visible;
                rowDefinitionActuatorSettingsCheckBox.Height = new GridLength(35);
                rowDefinitionMoveRelativelyCheckBox.Height = new GridLength(35);
            }
        }

        #endregion

        #region Arrange Actuator Settings Panel UI

        // TO DO - virtual mode
        private void ArrangeActuatorSettingsPanel(int deviceCount)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                if (virtualMode == true)
                    ChangeActuatorSettingsDeviceCountUI(int.Parse(parts[1]));
                else
                    ChangeActuatorSettingsDeviceCountUI(deviceCount);
            }
            catch { }
        }

        public void ChangeActuatorSettingsDeviceCountUI(int newAmount)
        {
            if (newAmount == 1)
            {
                columnDefinitionSettingsColumn.Width = new GridLength(250);
                //textBlockActuatorSettingsLabelDeviceOne.Text = "All";
                columnDefinitionActuatorSettingsLabelsColumn.Width = new GridLength(120);
                columnDefinitionActuatorSettingsDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorSettingsDeviceTwoColumn.Width = new GridLength(0);
                columnDefinitionActuatorSettingsDeviceThreeColumn.Width = new GridLength(0);
            }
            else if (newAmount == 2)
            {
                columnDefinitionSettingsColumn.Width = new GridLength(310);

                columnDefinitionActuatorSettingsLabelsColumn.Width = new GridLength(120);
                columnDefinitionActuatorSettingsDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorSettingsDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorSettingsDeviceThreeColumn.Width = new GridLength(0);
            }
            else if (newAmount == 3)
            {
                columnDefinitionSettingsColumn.Width = new GridLength(310);

                columnDefinitionActuatorSettingsLabelsColumn.Width = new GridLength(100);
                columnDefinitionActuatorSettingsDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorSettingsDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorSettingsDeviceThreeColumn.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        #endregion

        #region Arrange Connection Settings Panel UI

        // TO DO - virtual mode
        private void ArrangeConnectionSettingsPanel(int deviceCount)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                if (virtualMode == true)
                    ChangeConnectionSettingsDeviceCountUI(int.Parse(parts[1]));
                else
                    ChangeConnectionSettingsDeviceCountUI(deviceCount);
            }
            catch { }
        }

        private void ChangeConnectionSettingsDeviceCountUI(int newAmount)
        {
            if (newAmount == 1)
            {
                rowDefinitionConnectionSettingsDeviceOne.Height = new GridLength(1, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceTwo.Height = new GridLength(0, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceThree.Height = new GridLength(0, GridUnitType.Star);
            }
            else if (newAmount == 2)
            {
                rowDefinitionConnectionSettingsDeviceOne.Height = new GridLength(1, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceTwo.Height = new GridLength(1, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceThree.Height = new GridLength(0, GridUnitType.Star);
            }
            else if (newAmount == 3)
            {
                rowDefinitionConnectionSettingsDeviceOne.Height = new GridLength(1, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceTwo.Height = new GridLength(1, GridUnitType.Star);
                rowDefinitionConnectionSettingsDeviceThree.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        #endregion

        #region Arrange Move Relatively Panel UI

        // TO DO - virtual mode
        private void ArrangeMoveRelativelyPanel(int deviceCount)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                if (virtualMode == true)
                    ChangeMoveRelativelyDeviceCountUI(int.Parse(parts[1]));
                else { }
                // TO DO - CE FACE CHESTIA ASTA?
                ChangeMoveRelativelyDeviceCountUI(deviceCount);
            }
            catch { }
        }

        private void ChangeMoveRelativelyDeviceCountUI(int newAmount)
        {
            rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(1, GridUnitType.Star);

            if (newAmount == 1)
            {
                HideGrid(gridMoveRelativelyFirstRowMultipleDevices);
                HideGrid(gridMoveRelativelySecondRowMultipleDevices);
                HideGrid(gridMoveRelativelyThirdRowMultipleDevices);

                ShowGrid(gridMoveRelativelyFirstRowOneDevice);
                ShowGrid(gridMoveRelativelySecondRowOneDevice);
                ShowGrid(gridMoveRelativelyThirdRowOneDevice);
            }
            else if (newAmount == 2)
            {
                HideGrid(gridMoveRelativelyFirstRowOneDevice);
                HideGrid(gridMoveRelativelySecondRowOneDevice);
                HideGrid(gridMoveRelativelyThirdRowOneDevice);

                ShowGrid(gridMoveRelativelyFirstRowMultipleDevices);
                ShowGrid(gridMoveRelativelySecondRowMultipleDevices);

                rowDefinitionMoveRelativelyRowDeviceThree.Height = new GridLength(0, GridUnitType.Star);
            }
            else if (newAmount == 3)
            {
                HideGrid(gridMoveRelativelyFirstRowOneDevice);
                HideGrid(gridMoveRelativelySecondRowOneDevice);
                HideGrid(gridMoveRelativelyThirdRowOneDevice);

                ShowGrid(gridMoveRelativelyFirstRowMultipleDevices);
                ShowGrid(gridMoveRelativelySecondRowMultipleDevices);
                ShowGrid(gridMoveRelativelyThirdRowMultipleDevices);
            }
        }

        #endregion

        #region Arrange Move Continuously Panel UI

        // TO DO - virtual mode
        private void ArrangeMoveContinuouslyPanel(int deviceCount)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                if (virtualMode == true)
                    ChangeMoveContinuouslyDeviceCountUI(int.Parse(parts[1]));
                else
                    ChangeMoveContinuouslyDeviceCountUI(deviceCount);
            }
            catch { }
        }

        public void ChangeMoveContinuouslyDeviceCountUI(int deviceCount)
        {
            try
            {
                ResetMoveContinuouslyPanel();

                if (deviceCount.ToString() == "1")
                {
                    string[] partsOne = comboBoxConnectionSettingsDeviceOne.SelectedItem.ToString().Split(' ');

                    gridMoveRelativelyButtonsColumnOne.Visibility = Visibility.Hidden;
                    gridMoveRelativelyButtonsColumnThree.Visibility = Visibility.Hidden;

                    rowDefinitionMoveContinuouslyPanel.Height = new GridLength(200);

                    switch (partsOne[1])
                    {
                        case "X":
                            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisX(controller, Apsl, this));
                            break;

                        case "Y":
                            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisY(controller, Apsl, this));
                            break;

                        case "Z":
                            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisZ(controller, Apsl, this));
                            break;
                    }
                }
                else if (deviceCount.ToString() == "2")
                {
                    string[] partsOne = comboBoxConnectionSettingsDeviceOne.SelectedItem.ToString().Split(' ');
                    string[] partsTwo = comboBoxConnectionSettingsDeviceTwo.SelectedItem.ToString().Split(' ');

                    columnDefinitionMoveRelativelyButtonsColumnOne.Width = new GridLength(1, GridUnitType.Star);
                    columnDefinitionMoveRelativelyButtonsColumnTwo.Width = new GridLength(1, GridUnitType.Star);
                    columnDefinitionMoveRelativelyButtonsColumnThree.Width = new GridLength(0, GridUnitType.Star);

                    gridMoveRelativelyButtonsColumnThree.Visibility = Visibility.Hidden;

                    switch (partsOne[1])
                    {
                        case "X":
                            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnOne.Children.Clear();
                            gridMoveRelativelyButtonsColumnOne.Children.Add(new ActuatorMoveContinuouslyAxisX(controller, Apsl, this));
                            break;

                        case "Y":
                            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnOne.Children.Clear();
                            gridMoveRelativelyButtonsColumnOne.Children.Add(new ActuatorMoveContinuouslyAxisY(controller, Apsl, this));
                            break;

                        case "Z":
                            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnOne.Children.Clear();
                            gridMoveRelativelyButtonsColumnOne.Children.Add(new ActuatorMoveContinuouslyAxisZ(controller, Apsl, this));
                            break;
                    }

                    switch (partsTwo[1])
                    {
                        case "X":
                            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisX(controller, Apsl, this));
                            break;

                        case "Y":
                            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisY(controller, Apsl, this));
                            break;

                        case "Z":
                            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Visible;

                            gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                            gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisZ(controller, Apsl, this));
                            break;
                    }
                }
                else if (deviceCount.ToString() == "3")
                {
                    gridMoveRelativelyButtonsColumnOne.Children.Clear();
                    gridMoveRelativelyButtonsColumnTwo.Children.Clear();
                    gridMoveRelativelyButtonsColumnThree.Children.Clear();

                    gridMoveRelativelyButtonsColumnOne.Children.Add(new ActuatorMoveContinuouslyAxisZ(controller, Apsl, this));
                    gridMoveRelativelyButtonsColumnTwo.Children.Add(new ActuatorMoveContinuouslyAxisX(controller, Apsl, this));
                    gridMoveRelativelyButtonsColumnThree.Children.Add(new ActuatorMoveContinuouslyAxisY(controller, Apsl, this));

                    EnableAllSliders();
                }

                if (IsHiddenSliderAxisX() == true)
                    rowDefinitionMoveContinuouslyLowerSlider.Height = new GridLength(0);
            }
            catch
            {

            }
        }
        private void ResetMoveContinuouslyPanel()
        {
            columnDefinitionMoveRelativelyButtonsColumnOne.Width = new GridLength(1, GridUnitType.Star);
            columnDefinitionMoveRelativelyButtonsColumnTwo.Width = new GridLength(1.5, GridUnitType.Star);
            columnDefinitionMoveRelativelyButtonsColumnThree.Width = new GridLength(1, GridUnitType.Star);

            gridMoveRelativelyButtonsColumnOne.Visibility = Visibility.Visible;
            gridMoveRelativelyButtonsColumnTwo.Visibility = Visibility.Visible;
            gridMoveRelativelyButtonsColumnThree.Visibility = Visibility.Visible;

            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Visible;
            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Visible;
            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Visible;

            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Hidden;
            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Hidden;
            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Hidden;

            rowDefinitionMoveContinuouslyLowerSlider.Height = new GridLength(60);
            rowDefinitionMoveContinuouslyPanel.Height = new GridLength(250);
        }

        private void EnableAllSliders()
        {
            gridMoveContinuouslySliderAxisX.Visibility = Visibility.Visible;
            gridMoveContinuouslySliderAxisY.Visibility = Visibility.Visible;
            gridMoveContinuouslySliderAxisZ.Visibility = Visibility.Visible;
        }

        private bool IsHiddenSliderAxisX()
        {
            if (gridMoveContinuouslySliderAxisX.Visibility == Visibility.Hidden)
                return true;
            else return false;
        }

        #endregion

        #region Arrange Actuator Status Panel UI

        // TO DO - virtual mode
        private void ArrangeActuatorStatusPanel(int deviceID)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                if (virtualMode == true)
                    ChangeActuatorStatusDeviceCountUI(int.Parse(parts[1]));
                else
                    ChangeActuatorStatusDeviceCountUI(deviceCount);
            }
            catch { }
        }

        private void ChangeActuatorStatusDeviceCountUI(int newAmount)
        {
            if (newAmount == 1)
            {
                //textBlockActuatorSettingsLabelDeviceOne.Text = "All";
                columnDefinitionActuatorStatusLabelsColumn.Width = new GridLength(100);
                columnDefinitionActuatorStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorStatusDeviceTwoColumn.Width = new GridLength(0);
                columnDefinitionActuatorStatusDeviceThreeColumn.Width = new GridLength(0);

                columnDefinitionStatusColumn.Width = new GridLength(250);
            }
            else if (newAmount == 2)
            {
                columnDefinitionActuatorStatusLabelsColumn.Width = new GridLength(120);
                columnDefinitionActuatorStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorStatusDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorStatusDeviceThreeColumn.Width = new GridLength(0);

                columnDefinitionStatusColumn.Width = new GridLength(300);
            }
            else if (newAmount == 3)
            {
                columnDefinitionActuatorStatusLabelsColumn.Width = new GridLength(100);
                columnDefinitionActuatorStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorStatusDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionActuatorStatusDeviceThreeColumn.Width = new GridLength(1, GridUnitType.Star);

                columnDefinitionStatusColumn.Width = new GridLength(300);
            }
        }

        #endregion

        #region Arrange Power Status Panel UI

        // TO DO - virtual mode
        private void ArrangePowerStatusPanel(int deviceCount)
        {
            try
            {
                string[] parts = comboBoxTest.SelectedItem.ToString().Split(' ');

                ChangePowerStatusDeviceCountUI(int.Parse(parts[1]));

                if (virtualMode == true)
                    ChangePowerStatusDeviceCountUI(int.Parse(parts[1]));
                else
                    ChangePowerStatusDeviceCountUI(deviceCount);
            }
            catch { }
        }
        private void ChangePowerStatusDeviceCountUI(int newAmount)
        {
            if (newAmount == 1)
            {
                //textBlockActuatorSettingsLabelDeviceOne.Text = "All";
                columnDefinitionPowerStatusLabelsColumn.Width = new GridLength(100);
                columnDefinitionPowerStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionPowerStatusDeviceTwoColumn.Width = new GridLength(0);
                columnDefinitionPowerStatusDeviceThreeColumn.Width = new GridLength(0);
            }
            else if (newAmount == 2)
            {
                columnDefinitionPowerStatusLabelsColumn.Width = new GridLength(120);
                columnDefinitionPowerStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionPowerStatusDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionPowerStatusDeviceThreeColumn.Width = new GridLength(0);
            }
            else if (newAmount == 3)
            {
                columnDefinitionPowerStatusLabelsColumn.Width = new GridLength(100);
                columnDefinitionPowerStatusDeviceOneColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionPowerStatusDeviceTwoColumn.Width = new GridLength(1, GridUnitType.Star);
                columnDefinitionPowerStatusDeviceThreeColumn.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        #endregion

        #region Show/Hide Grid

        private void HideGrid(Grid targetGrid)
        {
            targetGrid.Visibility = Visibility.Hidden;
        }

        private void ShowGrid(Grid targetGrid)
        {
            targetGrid.Visibility = Visibility.Visible;
        }

        #endregion


        #region Testing only

        private Enums.TextBoxValueSafetyType GetEnumFromComboBoxIndex(int index)
        {
            Enums.TextBoxValueSafetyType retVal = new Enums.TextBoxValueSafetyType();

            switch (index)
            {
                case 0:
                    retVal = Enums.TextBoxValueSafetyType.MoveTo;
                    break;

                case 1:
                    retVal = Enums.TextBoxValueSafetyType.ShiftOn;
                    break;
            }

            return retVal;
        }

        public void LoadApslValuesToUI()
        {
            switch (comboBoxConnectionSettingsDeviceOne.SelectedIndex)
            {
                case 0:
                    textBoxActuatorSettingsMinEdgeOne.Text = Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeOne.Text = Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition.ToString();
                    break;

                case 1:
                    textBoxActuatorSettingsMinEdgeOne.Text = Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeOne.Text = Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition.ToString();
                    break;

                case 2:
                    textBoxActuatorSettingsMinEdgeOne.Text = Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeOne.Text = Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition.ToString();
                    break;

                default:
                    break;
            }

            switch (comboBoxConnectionSettingsDeviceTwo.SelectedIndex)
            {
                case 0:
                    textBoxActuatorSettingsMinEdgeTwo.Text = Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeTwo.Text = Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition.ToString();
                    break;

                case 1:
                    textBoxActuatorSettingsMinEdgeTwo.Text = Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeTwo.Text = Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition.ToString();
                    break;

                case 2:
                    textBoxActuatorSettingsMinEdgeTwo.Text = Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeTwo.Text = Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition.ToString();
                    break;

                default:
                    break;
            }

            switch (comboBoxConnectionSettingsDeviceThree.SelectedIndex)
            {
                case 0:
                    textBoxActuatorSettingsMinEdgeThree.Text = Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeThree.Text = Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition.ToString();
                    break;

                case 1:
                    textBoxActuatorSettingsMinEdgeThree.Text = Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeThree.Text = Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition.ToString();
                    break;

                case 2:
                    textBoxActuatorSettingsMinEdgeThree.Text = Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition.ToString();
                    textBoxActuatorSettingsMaxEdgeThree.Text = Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition.ToString();
                    break;

                default:
                    break;
            }
        }

        private void HandleActuatorSettingsTextBoxEdgeValueValidityForAll()
        {
            HandleActuatorSettingsTextBoxEdgeValueValidity(
                textBoxActuatorSettingsMinEdgeOne,
                textBoxActuatorSettingsMaxEdgeOne,
                uint.Parse(textBoxActuatorSettingsMinEdgeOne.Text),
                uint.Parse(textBoxActuatorSettingsMaxEdgeOne.Text));

            HandleActuatorSettingsTextBoxEdgeValueValidity(
                textBoxActuatorSettingsMinEdgeTwo,
                textBoxActuatorSettingsMaxEdgeTwo,
                uint.Parse(textBoxActuatorSettingsMinEdgeTwo.Text),
                uint.Parse(textBoxActuatorSettingsMaxEdgeTwo.Text));

            HandleActuatorSettingsTextBoxEdgeValueValidity(
                textBoxActuatorSettingsMinEdgeThree,
                textBoxActuatorSettingsMaxEdgeThree,
                uint.Parse(textBoxActuatorSettingsMinEdgeThree.Text),
                uint.Parse(textBoxActuatorSettingsMaxEdgeThree.Text));
        }

        private void GenerateTextBoxPairs()
        {
            textBoxAxisLink.PairDeviceOne = new TextBoxPair(textBoxMoveRelativelyToStepsOne,
            textBoxMoveRelativelyToMicroStepsOne);

            textBoxAxisLink.PairDeviceTwo = new TextBoxPair(textBoxMoveRelativelyToStepsTwo,
                textBoxMoveRelativelyToMicroStepsTwo);

            textBoxAxisLink.PairDeviceThree = new TextBoxPair(textBoxMoveRelativelyToStepsThree,
                textBoxMoveRelativelyToMicroStepsThree);

            textBoxAxisLink.PairOneDeviceMoveTo = new TextBoxPair(textBoxMoveToPositionStepsOneDevice,
                textBoxMoveToPositionMicroStepsOneDevice);

            textBoxAxisLink.PairOneDeviceShift = new TextBoxPair(textBoxShiftToStepsOneDevice,
                textBoxShiftToMicroStepsOneDevice);
        }

        private void ComboBoxTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArrangeComponents(comboBoxTest.SelectedIndex + 1);
        }

        // TO DELETE
        private void ButtonApplyImgProc_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ButtonSaveCFG_Click(object sender, RoutedEventArgs e)
        {
            AppConfigManager.SaveAllToConfiguration();
        }

        private void ButtonLoadCFG_Click(object sender, RoutedEventArgs e)
        {
            AppConfigManager.LoadInitFromConfiguration();
        }

        // To be deleted
        private void CheckBoxVirtualMode_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxVirtualMode.IsChecked == true)
                virtualMode = true;
            else
                virtualMode = false;
        }

        // This is very unsafe. Almost locked the engine on on edge. DO NOT USE
        private void ButtonConnectionSettingsCalibrateDeviceOne_Click(object sender, RoutedEventArgs e)
        {
            //List<int> sliderEdgeValues = controller.ActuatorCalibrateMovement(controller.ActuatorInContext.DeviceID);
            //foreach (int edgeValue in sliderEdgeValues)
            //   Console.WriteLine(edgeValue);
        }

        // This is very unsafe. Almost locked the engine on on edge. DO NOT USE
        private void ButtonConnectionSettingsCalibrateDeviceTwo_Click(object sender, RoutedEventArgs e)
        {
            // unsafe
        }

        // This is very unsafe. Almost locked the engine on on edge. DO NOT USE
        private void ButtonConnectionSettingsCalibrateDeviceThree_Click(object sender, RoutedEventArgs e)
        {
            // unsafe
        }

        #endregion

        #endregion Methods

        private void buttonDistanceCalibrationSetTrackingPoint_Click(object sender, RoutedEventArgs e)
        {
            DistanceCalibrationCalculateActuatorMovementArea();
        }

        private void buttonResetDistanceCalibration_Click(object sender, RoutedEventArgs e)
        {
            IPcore.IDCSettings.ResetDistanceCalibration();
            comboBoxVideoFeedCalibrationForTracking.SelectedIndex = 0; // force none
            VideoFeedCheckCalibratedAlgorithms();
            textBlockDistanceCalibrationMilimetersPerPixelValue.Text = "-";
            textBlockDistanceCalibrationStepsPerPixelValue.Text = "-";
        }

        private void DistanceCalibrationCalculateActuatorMovementArea()
        {
            float minEgdeX = Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition;
            float maxEdgeX = Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition;
            float minEdgeY = Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition;
            float maxEdgeY = Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition;
            System.Drawing.Point currentPoint = IPcore.TrackingSettings.CurrentlyDetectedPoint;

            controller.ChangeContextByAxis(Enums.Axis.X);
            int currentPosX = controller.ActuatorInContext.GetCurrentPositionSteps(controller.ActuatorInContext.DeviceID);
            controller.ChangeContextByAxis(Enums.Axis.Y);
            int currentPosY = controller.ActuatorInContext.GetCurrentPositionSteps(controller.ActuatorInContext.DeviceID);

            int borderPixelMinX = currentPoint.X;
            int borderPixelMaxX = currentPoint.X;
            int borderPixelMinY = currentPoint.Y;
            int borderPixelMaxY = currentPoint.Y;

            int counterMinX = 0;
            int counterMaxX = 0;
            int counterMinY = 0;
            int counterMaxY = 0;

            while ((float)currentPosX + ((float)++counterMinX * -IPcore.IDCSettings.StepsPerPixel) > minEgdeX)
                if (borderPixelMinX == 0)
                {
                    Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition = (int)((float)currentPosX + ((float)counterMinX * -IPcore.IDCSettings.StepsPerPixel));
                    break;
                }
                else
                    borderPixelMinX--;

            while ((float)currentPosX + ((float)++counterMaxX * IPcore.IDCSettings.StepsPerPixel) < maxEdgeX)
                if (borderPixelMaxX == IPcore.VideoFeedSettings.ImageWidth)
                {
                    Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition = (int)((float)currentPosX + ((float)counterMaxX * IPcore.IDCSettings.StepsPerPixel));
                    break;
                }
                else
                    borderPixelMaxX++;

            while ((float)currentPosY + ((float)++counterMinY * -IPcore.IDCSettings.StepsPerPixel) > minEdgeY)
                if (borderPixelMinY == 0)
                {
                    Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition = (int)((float)currentPosY + ((float)counterMinY * -IPcore.IDCSettings.StepsPerPixel));
                    break;
                }
                else
                    borderPixelMinY--;

            while ((float)currentPosY + ((float)++counterMaxY * IPcore.IDCSettings.StepsPerPixel) < maxEdgeY)
                if (borderPixelMaxY == IPcore.VideoFeedSettings.ImageHeight)
                {
                    Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition = (int)((float)currentPosY + ((float)counterMaxY * IPcore.IDCSettings.StepsPerPixel));
                    break;
                }
                else
                    borderPixelMaxY++;

            IPcore.IDCSettings.IsTrackingPointSet = true;

            buttonDistanceCalibrationSetTrackingPoint.IsEnabled = false;
            buttonDistanceCalibrationSetTrackingPoint.Background = uiBrushes.Gray;
            VideoFeedCheckCalibratedAlgorithms();

            LoadApslValuesToUI();
            IPcore.IDCSettings.SetTopLeftBoxCoords(borderPixelMinX, borderPixelMinY);
            IPcore.IDCSettings.SetBottomRightBoxCoords(borderPixelMaxX, borderPixelMaxY);
        }
    }
}