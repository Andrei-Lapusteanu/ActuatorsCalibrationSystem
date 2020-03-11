using Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using View.UserControls;

namespace View
{
    public class ApplicationConfigurationManager
    {
        MainControlsPanel mcp;
        Configuration configuration;

        public ApplicationConfigurationManager(MainControlsPanel mcp)
        {
            this.mcp = mcp;
            ConfigureSettings();
        }

        private void ConfigureSettings()
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "App.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            //configuration = ConfigurationManager.Open(configFileMap, ConfigurationUserLevel.None);
            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public void SaveAllToConfiguration()
        {
            try
            {
                // Actuator Settings panel
                configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedOne"].Value = mcp.textBoxActuatorSettingsSpeedOne.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedTwo"].Value = mcp.textBoxActuatorSettingsSpeedTwo.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedThree"].Value = mcp.textBoxActuatorSettingsSpeedThree.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedOne"].Value = mcp.textBoxActuatorSettingsMicroSpeedOne.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedTwo"].Value = mcp.textBoxActuatorSettingsMicroSpeedTwo.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedThree"].Value = mcp.textBoxActuatorSettingsMicroSpeedThree.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationOne"].Value = mcp.textBoxActuatorSettingsAccelerationOne.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationTwo"].Value = mcp.textBoxActuatorSettingsAccelerationTwo.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationThree"].Value = mcp.textBoxActuatorSettingsAccelerationThree.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationOne"].Value = mcp.textBoxActuatorSettingsDecelerationOne.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationTwo"].Value = mcp.textBoxActuatorSettingsDecelerationTwo.Text;
                configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationThree"].Value = mcp.textBoxActuatorSettingsDecelerationThree.Text;

                //configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeOne"].Value = mcp.textBoxActuatorSettingsMinEdgeOne.Text;
                //configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeOne"].Value = mcp.textBoxActuatorSettingsMaxEdgeOne.Text;
                //configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeTwo"].Value = mcp.textBoxActuatorSettingsMinEdgeTwo.Text;
                //configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeTwo"].Value = mcp.textBoxActuatorSettingsMaxEdgeTwo.Text;
                //configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeThree"].Value = mcp.textBoxActuatorSettingsMinEdgeThree.Text;
                //configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeThree"].Value = mcp.textBoxActuatorSettingsMaxEdgeThree.Text;

                // Connection Settings Panel
                List<string> axisOrder = mcp.GetConnectionSettingsComboBoxSelectedIndex();
                configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceOneSelectedIndex"].Value = axisOrder[0];
                configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceTwoSelectedIndex"].Value = axisOrder[1];
                configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceThreeSelectedIndex"].Value = axisOrder[2];

                // Move relatively Panel
                // Multiple devices
                List<string> moveRelativelyIndexOrder = mcp.GetMoveRelativelyMultipleDevicesComboBoxSelectedIndex();
                configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceOneSelectedIndex"].Value = moveRelativelyIndexOrder[0];
                configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceTwoSelectedIndex"].Value = moveRelativelyIndexOrder[1];
                configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceThreeSelectedIndex"].Value = moveRelativelyIndexOrder[2];

                configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsOne"].Value = mcp.textBoxMoveRelativelyToStepsOne.Text;
                configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsOne"].Value = mcp.textBoxMoveRelativelyToMicroStepsOne.Text;

                configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsTwo"].Value = mcp.textBoxMoveRelativelyToStepsTwo.Text;
                configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsTwo"].Value = mcp.textBoxMoveRelativelyToMicroStepsTwo.Text;

                configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsThree"].Value = mcp.textBoxMoveRelativelyToStepsThree.Text;
                configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsThree"].Value = mcp.textBoxMoveRelativelyToMicroStepsThree.Text;

                // One device
                configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsOneDevice"].Value = mcp.textBoxMoveToPositionStepsOneDevice.Text;
                configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsOneDevice"].Value = mcp.textBoxMoveToPositionMicroStepsOneDevice.Text;

                configuration.AppSettings.Settings["textBoxShiftToStepsOneDevice"].Value = mcp.textBoxShiftToStepsOneDevice.Text;
                configuration.AppSettings.Settings["textBoxShiftToMicroStepsOneDevice"].Value = mcp.textBoxShiftToMicroStepsOneDevice.Text;

                // Software limits for actuator
                mcp.SetEdgeLimitsPerAxisUI();
                configuration.AppSettings.Settings["Axis_X_MinEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_X_MinEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_X_MaxEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_X_MaxEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_X_HomeStepsPosition"].Value = "0";

                configuration.AppSettings.Settings["Axis_Y_MinEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_Y_MinEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_Y_MaxEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_Y_MaxEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_Y_HomeStepsPosition"].Value = "0";

                configuration.AppSettings.Settings["Axis_Z_MinEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_Z_MinEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_Z_MaxEdgeStepsPosition"].Value = mcp.Apsl.ApslLimBundle.Axis_Z_MaxEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["Axis_Z_HomeStepsPosition"].Value = "0";

                configuration.AppSettings.Settings["minEdgePositionStepsAllDevices"].Value = mcp.Apsl.ApslLimBundle.GlobalMinEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["maxEdgePositionStepsAllDevices"].Value = mcp.Apsl.ApslLimBundle.GlobalMaxEdgeStepsPosition.ToString();
                configuration.AppSettings.Settings["homePositionStepsAllDevices"].Value = "0";

                configuration.AppSettings.Settings["MilimetersPerPixel"].Value = mcp.IPcore.IDCSettings.MilimetersPerPixel.ToString();
                configuration.AppSettings.Settings["StepsPerPixel"].Value = mcp.IPcore.IDCSettings.StepsPerPixel.ToString();

                // Image processing settings
                configuration.AppSettings.Settings["sliderImageProcessingSettingsPrecision"].Value = mcp.IPcore.TASettings.CurrentPrecisionSliderValue.ToString();
                configuration.AppSettings.Settings["sliderImageProcessingSettingsStaticThreshold"].Value = mcp.IPcore.TASettings.StaticThresholdValue.ToString();
                configuration.AppSettings.Settings["sliderImageProcessingSettingsQuality"].Value = mcp.IPcore.TASettings.Quality.ToString();
                configuration.AppSettings.Settings["sliderImageProcessingSettingsBrightness"].Value = mcp.IPcore.TASettings.Brightness.ToString();

                // Save the configuration to App.config
                configuration.Save(ConfigurationSaveMode.Minimal);
            }
            catch { }

        }

        public void LoadInitFromConfiguration()
        {
            try
            {
                // Software limits for actuators
                string axis_x_minStepsPos = configuration.AppSettings.Settings["Axis_X_MinEdgeStepsPosition"].Value;
                string axis_x_maxStepsPos = configuration.AppSettings.Settings["Axis_X_MaxEdgeStepsPosition"].Value;
                string axis_x_homeStepsPos = configuration.AppSettings.Settings["Axis_X_HomeStepsPosition"].Value;

                string axis_y_minStepsPos = configuration.AppSettings.Settings["Axis_Y_MinEdgeStepsPosition"].Value;
                string axis_y_maxStepsPos = configuration.AppSettings.Settings["Axis_Y_MaxEdgeStepsPosition"].Value;
                string axis_y_homeStepsPos = configuration.AppSettings.Settings["Axis_Y_HomeStepsPosition"].Value;

                string axis_z_minStepsPos = configuration.AppSettings.Settings["Axis_Z_MinEdgeStepsPosition"].Value;
                string axis_z_maxStepsPos = configuration.AppSettings.Settings["Axis_Z_MaxEdgeStepsPosition"].Value;
                string axis_z_homeStepsPos = configuration.AppSettings.Settings["Axis_Z_HomeStepsPosition"].Value;

                string minEdge = configuration.AppSettings.Settings["minEdgePositionStepsAllDevices"].Value;
                string maxEdge = configuration.AppSettings.Settings["maxEdgePositionStepsAllDevices"].Value;
                string homePos = configuration.AppSettings.Settings["homePositionStepsAllDevices"].Value;

                ApslLimitsBundle apslLB = new ApslLimitsBundle()
                {
                    Axis_X_MinEdgeStepsPosition = int.Parse(axis_x_minStepsPos),
                    Axis_X_MaxEdgeStepsPosition = int.Parse(axis_x_maxStepsPos),
                    Axis_X_HomeStepsPosition = int.Parse(axis_x_homeStepsPos),
                    Axis_Y_MinEdgeStepsPosition = int.Parse(axis_y_minStepsPos),
                    Axis_Y_MaxEdgeStepsPosition = int.Parse(axis_y_maxStepsPos),
                    Axis_Y_HomeStepsPosition = int.Parse(axis_y_homeStepsPos),
                    Axis_Z_MinEdgeStepsPosition = int.Parse(axis_z_minStepsPos),
                    Axis_Z_MaxEdgeStepsPosition = int.Parse(axis_z_maxStepsPos),
                    Axis_Z_HomeStepsPosition = int.Parse(axis_z_homeStepsPos),
                    GlobalMinEdgeStepsPosition = int.Parse(minEdge),
                    GlobalMaxEdgeStepsPosition = int.Parse(maxEdge)
                };

                mcp.Apsl = new ActuatorPositionSoftwareLimits(apslLB, int.Parse(minEdge), int.Parse(maxEdge), int.Parse(homePos));
                mcp.Apsl.MinEdgePositionStepsAllDevices = int.Parse(minEdge);
                mcp.Apsl.MaxEdgePositionStepsAllDevices = int.Parse(maxEdge);
                mcp.Apsl.MinPositionMicroStepsAllDevices = -255;
                mcp.Apsl.MaxPositionMicroStepsAllDevices = 255;

                // Actuator Settings panel
                mcp.textBoxActuatorSettingsSpeedOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedOne"].Value;
                mcp.textBoxActuatorSettingsSpeedTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedTwo"].Value;
                mcp.textBoxActuatorSettingsSpeedThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsSpeedThree"].Value;
                mcp.textBoxActuatorSettingsMicroSpeedOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedOne"].Value;
                mcp.textBoxActuatorSettingsMicroSpeedTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedTwo"].Value;
                mcp.textBoxActuatorSettingsMicroSpeedThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMicroSpeedThree"].Value;
                mcp.textBoxActuatorSettingsAccelerationOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationOne"].Value;
                mcp.textBoxActuatorSettingsAccelerationTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationTwo"].Value;
                mcp.textBoxActuatorSettingsAccelerationThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsAccelerationThree"].Value;
                mcp.textBoxActuatorSettingsDecelerationOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationOne"].Value;
                mcp.textBoxActuatorSettingsDecelerationTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationTwo"].Value;
                mcp.textBoxActuatorSettingsDecelerationThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsDecelerationThree"].Value;

                //mcp.textBoxActuatorSettingsMinEdgeOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeOne"].Value;
                //mcp.textBoxActuatorSettingsMaxEdgeOne.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeOne"].Value;
                //mcp.textBoxActuatorSettingsMinEdgeTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeTwo"].Value;
                //mcp.textBoxActuatorSettingsMaxEdgeTwo.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeTwo"].Value;
                //mcp.textBoxActuatorSettingsMinEdgeThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMinEdgeThree"].Value;
                //mcp.textBoxActuatorSettingsMaxEdgeThree.Text = configuration.AppSettings.Settings["textBoxActuatorSettingsMaxEdgeThree"].Value;

                // Move relatively Panel
                // Multiple devices
                mcp.comboBoxMoveRelativelyDeviceOne.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceOneSelectedIndex"].Value);
                mcp.comboBoxMoveRelativelyDeviceTwo.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceTwoSelectedIndex"].Value);
                mcp.comboBoxMoveRelativelyDeviceThree.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxMoveRelativelyDeviceThreeSelectedIndex"].Value);

                // Connection Settings Panel
                mcp.comboBoxConnectionSettingsDeviceOne.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceOneSelectedIndex"].Value);
                mcp.comboBoxConnectionSettingsDeviceTwo.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceTwoSelectedIndex"].Value);
                mcp.comboBoxConnectionSettingsDeviceThree.SelectedIndex = int.Parse(configuration.AppSettings.Settings["comboBoxConnectionSettingsDeviceThreeSelectedIndex"].Value);

                // Image processing settings
                mcp.sliderImageProcessingSettingsPrecision.Value = double.Parse(configuration.AppSettings.Settings["sliderImageProcessingSettingsPrecision"].Value);
                mcp.sliderImageProcessingSettingsStaticThreshold.Value = int.Parse(configuration.AppSettings.Settings["sliderImageProcessingSettingsStaticThreshold"].Value);
                mcp.sliderImageProcessingSettingsQuality.Value = int.Parse(configuration.AppSettings.Settings["sliderImageProcessingSettingsQuality"].Value);
                mcp.sliderImageProcessingSettingsBrightness.Value = int.Parse(configuration.AppSettings.Settings["sliderImageProcessingSettingsBrightness"].Value);

                //mcp.LoadApslValuesToUI(apslLB);

                // END INIT LOAD
            }
            catch (Exception ex)
            {
                // TO DO
                Console.WriteLine(ex.ToString());
            }
        }

        public void LoadTextBoxValuesFromConfig()
        {
            try
            {
                // Multiple devices
                mcp.textBoxMoveRelativelyToStepsOne.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsOne"].Value;
                mcp.textBoxMoveRelativelyToMicroStepsOne.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsOne"].Value;

                mcp.textBoxMoveRelativelyToStepsTwo.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsTwo"].Value;
                mcp.textBoxMoveRelativelyToMicroStepsTwo.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsTwo"].Value;

                mcp.textBoxMoveRelativelyToStepsThree.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsThree"].Value;
                mcp.textBoxMoveRelativelyToMicroStepsThree.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsThree"].Value;

                // One device
                mcp.textBoxMoveToPositionStepsOneDevice.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToStepsOneDevice"].Value;
                mcp.textBoxMoveToPositionMicroStepsOneDevice.Text = configuration.AppSettings.Settings["textBoxMoveRelativelyToMicroStepsOneDevice"].Value;

                mcp.textBoxShiftToStepsOneDevice.Text = configuration.AppSettings.Settings["textBoxShiftToStepsOneDevice"].Value;
                mcp.textBoxShiftToMicroStepsOneDevice.Text = configuration.AppSettings.Settings["textBoxShiftToMicroStepsOneDevice"].Value;

                mcp.IPcore.IDCSettings.MilimetersPerPixel = (float)Double.Parse(configuration.AppSettings.Settings["MilimetersPerPixel"].Value);
                mcp.IPcore.IDCSettings.StepsPerPixel = (float)Double.Parse(configuration.AppSettings.Settings["StepsPerPixel"].Value);

                mcp.textBlockDistanceCalibrationMilimetersPerPixelValue.Text = mcp.IPcore.IDCSettings.MilimetersPerPixel.ToString("0.000") + " mm";
                mcp.textBlockDistanceCalibrationStepsPerPixelValue.Text = mcp.IPcore.IDCSettings.StepsPerPixel.ToString("0") + " st";
            }
            catch (Exception ex)
            {
                // TO DO
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
