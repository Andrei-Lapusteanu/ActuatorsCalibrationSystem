using Entities;
using System;

namespace Controller
{
    /// <summary>
    /// This class contains methods used to log events that occur in the application
    /// </summary>
    public class LogController
    {
        private const ActuatorController dummyActuator = null;

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public LogController() { }

        /// <summary>
        /// Log events that refer to the execution of methods contained in the class Controller.ActuatorController
        /// </summary>
        /// <param name="infoLog">represents all possible log types, of type Entities.Enums.ActuatorLog</param>
        /// <param name="actuator">object of type Controller.ActuatorController </param>
        /// <param name="ActuatorLogTiming">defines either start, end, or don't care of the time when event occurs, of type >Entities.Enums.ActuatorLogTiming</param>
        // Parameters ActuatorController and Enums.ActuatorLogTiming are optional
        // LogActuatorInfo can be called with either 1, 2, or 3 parameters
        public void LogActuatorInfo(Enums.ActuatorLog infoLog,
                                    ActuatorController actuator = dummyActuator,
                                    Enums.ActuatorLogTiming ActuatorLogTiming = Enums.ActuatorLogTiming.DontCare)
        {
            //switch (infoLog)
            //{
            //    case Enums.ActuatorLog.CloseDevice:
            //        Logger.Log.Info(actuator.DeviceName + " was successfully closed");
            //        break;

            //    case Enums.ActuatorLog.MoveHome:
            //        if (ActuatorLogTiming == Enums.ActuatorLogTiming.Start)
            //            Logger.Log.Info(actuator.DeviceName + " intends to move to home position (0 steps, 0 microsteps)" +
            //                            " with the speed" + " TO BE IMPELMENTED BLYAT");
            //        else
            //        {
            //            actuator.GetStatus(actuator.DeviceID);

            //            Logger.Log.Info(actuator.DeviceName + " arrived at home position (" + actuator.Status.uCurPosition + " steps and " +
            //                            actuator.Status.uCurPosition + " microsteps) with the speed");
            //        }
            //        break;

            //    case Enums.ActuatorLog.MoveLeft:
            //        Logger.Log.Info(actuator.DeviceName + " started moving continuously left");
            //        break;

            //    case Enums.ActuatorLog.MoveToPosition:
            //        if (ActuatorLogTiming == Enums.ActuatorLogTiming.Start)
            //            Logger.Log.Info(actuator.DeviceName + " intends to move to position " + actuator.PositionSteps + " steps and " +
            //                            actuator.PositionMicrosteps + " microsteps with the speed");
            //        else
            //        {
            //            actuator.GetStatus(actuator.DeviceID);

            //            Logger.Log.Info(actuator.DeviceName + " arrived at position " + actuator.Status.CurPosition + " steps and " +
            //                            actuator.Status.uCurPosition + " microsteps with the speed");
            //        }
            //        break;

            //    case Enums.ActuatorLog.MoveRelatively:
            //        if (ActuatorLogTiming == Enums.ActuatorLogTiming.Start)
            //            Logger.Log.Info(actuator.DeviceName + " intends to shift by " + actuator.DeltaSteps + " steps and " +
            //                            actuator.DeltaMicrosteps + " microsteps with the speed");
            //        else
            //        {
            //            actuator.GetStatus(actuator.DeviceID);

            //            Logger.Log.Info(actuator.DeviceName + " arrived at position " + actuator.Status.CurPosition + " steps and " +
            //                            actuator.Status.uCurPosition + " microsteps with the speed");
            //        }
            //        break;

            //    case Enums.ActuatorLog.PowerOff:
            //        Logger.Log.Info(actuator.DeviceName + " successfully powered off");
            //        break;

            //    case Enums.ActuatorLog.MoveRight:
            //        Logger.Log.Info(actuator.DeviceName + " started moving continuously right");
            //        break;

            //    case Enums.ActuatorLog.SoftStop:
            //        if (ActuatorLogTiming == Enums.ActuatorLogTiming.Start)
            //            Logger.Log.Info(actuator.DeviceName + " intends to soft stop");
            //        else
            //        {
            //            actuator.GetStatus(actuator.DeviceID);
            //            Logger.Log.Info(actuator.DeviceName + " soft stopped at postion " + actuator.Status.CurPosition + " steps and " +
            //                                actuator.Status.uCurPosition + " microsteps ");
            //        }
            //        break;

            //    case Enums.ActuatorLog.HardStop:
            //        if (ActuatorLogTiming == Enums.ActuatorLogTiming.Start)
            //            Logger.Log.Info(actuator.DeviceName + " intends to hard stop");
            //        else
            //        {
            //            actuator.GetStatus(actuator.DeviceID);
            //            Logger.Log.Info(actuator.DeviceName + " hard stopped at postion" + actuator.Status.CurPosition + " steps and " +
            //                                actuator.Status.uCurPosition + " microsteps ");
            //        }
            //        break;

            //    case Enums.ActuatorLog.SetZero:
            //        Logger.Log.Info(actuator.DeviceName + " successfully set its new home (zero) position");
            //        break;

            //    case Enums.ActuatorLog.WaitForStop:
            //        Logger.Log.Info(actuator.DeviceName + " successfully waited and stopped");
            //        break;

            //    case Enums.ActuatorLog.OpenDevice:
            //        Logger.Log.Info(actuator.DeviceName + " was successfully opened");
            //        break;

            //    case Enums.ActuatorLog.NewActuatorInContext:
            //        Logger.Log.Info("Actuator with ID = " + actuator.DeviceID + " and name = " + actuator.DeviceName + " was brought into context");
            //        break;

            //    default: break;
            //}
        }

        /// <summary>
        /// Log events that refer to errors raised in methods contained in the class Controller.ActuatorController
        /// </summary>
        /// <param name="errorLog">represents all possible log types, of type Entities.Enums.ActuatorLog</param>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="ex">exception object from parent caller, of type System.Exception</param>
        public void LogActuatorError(Enums.ActuatorLog errorLog, int deviceID = -1, Exception ex = null)
        {
            switch (errorLog)
            {
                case Enums.ActuatorLog.CloseDevice:
                    Logger.Log.Error("API.close_device(ref deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.MoveHome:
                    Logger.Log.Error("API.command_home(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.MoveLeft:
                    Logger.Log.Error("API.command_left(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.MoveToPosition:
                    Logger.Log.Error("API.command_move(deviceID, base.PositionSteps = positionSteps, base.PositionMicrosteps = positionMicrosteps) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.MoveRelatively:
                    Logger.Log.Error("API.command_movr(deviceID, base.DeltaSteps = deltaSteps, base.DeltaMicrosteps = deltaSteps) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.PowerOff:
                    Logger.Log.Error("API.command_power_off(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.MoveRight:
                    Logger.Log.Error(" API.command_right(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.SoftStop:
                    Logger.Log.Error("API.command_sstp(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.HardStop:
                    Logger.Log.Error("API.command_stop(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.WaitForStop:
                    Logger.Log.Error("API.command_wait_for_stop(deviceID, base.WaitForStopMs = ms) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.OpenDevice:
                    Logger.Log.Error("API.open_device(deviceName) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                case Enums.ActuatorLog.SetZero:
                    Logger.Log.Error(" API.command_zero(deviceID) failed on device with ID = " + deviceID + ", exception thrown is: " + ex);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Log events that refer to the execution of methods contained in the class Controller.Actuators
        /// </summary>
        /// <param name="infoLog">represents all possible log types, of type Entities.Enums.ActuatorLog</param>
        /// <param name="deviceID">Deprecated. Identifies actuator, of type int</param>
        public void LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog infoLog, int deviceID = -1)
        {
            //switch (infoLog)
            //{
            //    case Enums.MultipleActuatorsLog.FreeEnumeratedActuators:
            //        Logger.Log.Info("Successfully freed enumerated actuators from memory");
            //        break;

            //    case Enums.MultipleActuatorsLog.EnumerateActuators:
            //        Logger.Log.Info("Successfully enumerated actuators into memory");
            //        break;

            //    case Enums.MultipleActuatorsLog.GetDeviceCount:
            //        Logger.Log.Info("Successfully got device count");
            //        break;

            //    case Enums.MultipleActuatorsLog.AddActuatorsToList:
            //        Logger.Log.Info("Successfully added found actuators to list");
            //        break;

            //    case Enums.MultipleActuatorsLog.GetActuatorNames:
            //        Logger.Log.Info("Successfully got actuator names");
            //        break;

            //    case Enums.MultipleActuatorsLog.TryOpenActuators:
            //        Logger.Log.Info("Successfully tried to open actuators and got their respective device ID");
            //        break;

            //    case Enums.MultipleActuatorsLog.GetActuatorInformation:
            //        Logger.Log.Info("Successfully retrieved all available actuator information");
            //        break;

            //    case Enums.MultipleActuatorsLog.CloseActuators:
            //        Logger.Log.Info("Successfully closed actuators");
            //        break;

            //    case Enums.MultipleActuatorsLog.NoDevicesFound:
            //        Logger.Log.Info("No devices were found to be connected");
            //        break;

            //    case Enums.MultipleActuatorsLog.RemovedActuator:
            //        Logger.Log.Info("Successfully removed actuator with ID = " + deviceID);
            //        break;

            //    case Enums.MultipleActuatorsLog.ProcessNewActuator:
            //        Logger.Log.Info("Successfully indexed and tested functionality of newly connected actuator with ID = " + deviceID);
            //        break;

            //    default: break;
            //}
        }

        /// <summary>
        /// Log events that refer to errors raised in methods contained in the class Controller.Actuators
        /// </summary>
        /// <param name="errorLog">represents all possible log types, of type Entities.Enums.ActuatorLog</param>
        /// <param name="deviceID">Deprecated. Identifies actuator, of type int</param>
        /// <param name="ex">exception object from parent caller, of type System.Exception</param>
        public void LogMultipleActuatorsError(Enums.MultipleActuatorsLog errorLog, int deviceID = -1, Exception ex = null)
        {
            switch (errorLog)
            {
                case Enums.MultipleActuatorsLog.FreeEnumeratedActuators:
                    Logger.Log.Error("FreeEnumeratedActuators() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.EnumerateActuators:
                    Logger.Log.Error("EnumerateActuators() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.GetDeviceCount:
                    Logger.Log.Error("GetDeviceCount() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.AddActuatorsToList:
                    Logger.Log.Error("AddActuatorsToList() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.GetActuatorNames:
                    Logger.Log.Error("GetActuatorNames() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.TryOpenActuators:
                    Logger.Log.Error("TryOpenActuatorsAndGetID() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.GetActuatorInformation:
                    Logger.Log.Info("GetActuatorInformation() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.CloseActuators:
                    Logger.Log.Info("CloseActuators() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                case Enums.MultipleActuatorsLog.NoDevicesFound:
                    Logger.Log.Error("No devices were found to be connected");
                    break;

                case Enums.MultipleActuatorsLog.ProcessNewActuator:
                    Logger.Log.Error("ProcessNewActuator() from Actuators.cs failed, exception thrown is: " + ex);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Log errors raised in method Controller.ActuatorErrorHandler.CheckForActuatorErrors()
        /// </summary>
        /// <param name="ctrlErr">all possible controller error types, of type Entities.Enums.ControllerError</param>
        /// <param name="errVal">error value, of type uint</param>
        /// <param name="errorMessage">error message, of type string</param>
        public void LogControllerError(Enums.ControllerError ctrlErr, uint errVal, string errorMessage)
        {
            switch (ctrlErr)
            {
                case Enums.ControllerError.EncoderStateMalfunction:
                    Logger.Log.Error("Controller error: EncoderStareMalfunction, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.CommandError:
                    Logger.Log.Error("Controller error: CommandError, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.DataIntegrityError:
                    Logger.Log.Error("Controller error: DataIntegrityError, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.ValueError:
                    Logger.Log.Error("Controller error: ValueError, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.StateAlarm:
                    Logger.Log.Error("Controller error: StateAlarm, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.PowerControllerOverheat:
                    Logger.Log.Error("Controller error: PowerControllerOverheat, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.PowerVolatageOverload:
                    Logger.Log.Error("Controller error: PowerVolatageOverload, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.PowerCurrentOverload:
                    Logger.Log.Error("Controller error: PowerCurrentOverload, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.UsbVoltageOverload:
                    Logger.Log.Error("Controller error: UsbVoltageOverload, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.UsbCurrentOverload:
                    Logger.Log.Error("Controller error: UsbCurrentOverload, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.CurrentLimitExceeded:
                    Logger.Log.Error("Controller error: CurrentLimitExceeded, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.LeftLimitReached:
                    Logger.Log.Error("Controller error: LeftLimitReached, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.RightLimitReached:
                    Logger.Log.Error("Controller error: RightLimitReached, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.PowerOff:
                    Logger.Log.Error("Controller error: PowerOff, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.TemperatureOutsideLimits:
                    Logger.Log.Error("Controller error: TemperatureOutsideLimits, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.TimeoutError:
                    Logger.Log.Error("Controller error: EncoderStareMalfunction, " +
                                     String.Format("{0:X}", errVal).ToString() + " -  " + errorMessage);
                    break;

                case Enums.ControllerError.NoVoltage:
                    Logger.Log.Error("Controller error: No voltage, UPWR = 0");
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Comprehensive method which logs all members of Entities.ActuatorEntity objects
        /// </summary>
        /// <param name="actuator">object of type Controller.ActuatorController </param>
        public void LogAllSettings(ActuatorController actuator)
        {
            Logger.Log.Info(
                "\nActuator entire log: \n\n" +
                "accessories_settings_t:" +
                "\n\t LimitSwitchesSettings:    " + actuator.AccessoriesSettings.LimitSwitchesSettings +
                "\n\t MagneticBrakeInfo:        " + actuator.AccessoriesSettings.MagneticBrakeInfo +
                "\n\t MBRatedCurrent:           " + actuator.AccessoriesSettings.MBRatedCurrent +
                "\n\t MBRatedVoltage:           " + actuator.AccessoriesSettings.MBRatedVoltage +
                "\n\t MBSettings:               " + actuator.AccessoriesSettings.MBSettings +
                "\n\t MBTorque:                 " + actuator.AccessoriesSettings.MBTorque +
                "\n\t TemperatureSensorInfo:    " + actuator.AccessoriesSettings.TemperatureSensorInfo +
                "\n\t TSGrad:                   " + actuator.AccessoriesSettings.TSGrad +
                "\n\t TSMax:                    " + actuator.AccessoriesSettings.TSMax +
                "\n\t TSMin:                    " + actuator.AccessoriesSettings.TSMin +
                "\n\t TSSettings:               " + actuator.AccessoriesSettings.TSSettings +

                "\n\n analog_data_t:" +
                "\n\t A1Voltage:                " + actuator.AnalogData.A1Voltage +
                "\n\t A1Voltage_ADC:            " + actuator.AnalogData.A1Voltage_ADC +
                "\n\t A2Voltage:                " + actuator.AnalogData.A2Voltage +
                "\n\t A2Voltage_ADC:            " + actuator.AnalogData.A2Voltage_ADC +
                "\n\t ACurrent:                 " + actuator.AnalogData.ACurrent +
                "\n\t ACurrent_ADC:             " + actuator.AnalogData.ACurrent_ADC +
                "\n\t B1Voltage:                " + actuator.AnalogData.B1Voltage +
                "\n\t B1Voltage_ADC:            " + actuator.AnalogData.B1Voltage_ADC +
                "\n\t B2Voltage:                " + actuator.AnalogData.B2Voltage +
                "\n\t B2Voltage_ADC:            " + actuator.AnalogData.B2Voltage_ADC +
                "\n\t deprecated:               " + actuator.AnalogData.deprecated +
                "\n\t FullCurrent:              " + actuator.AnalogData.FullCurrent +
                "\n\t FullCurrent_ADC:          " + actuator.AnalogData.FullCurrent_ADC +
                "\n\t H5:                       " + actuator.AnalogData.H5 +
                "\n\t H5_ADC:                   " + actuator.AnalogData.H5_ADC +
                "\n\t Joy:                      " + actuator.AnalogData.Joy +
                "\n\t Joy_ADC:                  " + actuator.AnalogData.Joy_ADC +
                "\n\t L:                        " + actuator.AnalogData.L +
                "\n\t L5:                       " + actuator.AnalogData.L5 +
                "\n\t L5_ADC:                   " + actuator.AnalogData.L5_ADC +
                "\n\t Pot:                      " + actuator.AnalogData.Pot +
                "\n\t Pot_ADC:                  " + actuator.AnalogData.Pot_ADC +
                "\n\t R:                        " + actuator.AnalogData.R +
                "\n\t SupVoltage:               " + actuator.AnalogData.SupVoltage +
                "\n\t SupVoltage_ADC:           " + actuator.AnalogData.SupVoltage_ADC +
                "\n\t Temp:                     " + actuator.AnalogData.Temp +
                "\n\t Temp_ADC:                 " + actuator.AnalogData.Temp_ADC +

                "\n\n brake_settings_t:" +
                "\n\t t1:                       " + actuator.BrakeSettings.t1 +
                "\n\t t2:                       " + actuator.BrakeSettings.t2 +
                "\n\t t3:                       " + actuator.BrakeSettings.t3 +
                "\n\t t4:                       " + actuator.BrakeSettings.t4 +

                "\n\n calibration_settings_t:" +
                "\n\t CSS1_A:                   " + actuator.CalibrationSettings.CSS1_A +
                "\n\t CSS1_B:                   " + actuator.CalibrationSettings.CSS1_B +
                "\n\t CSS2_A:                   " + actuator.CalibrationSettings.CSS2_A +
                "\n\t CSS2_B:                   " + actuator.CalibrationSettings.CSS2_B +
                "\n\t FullCurrent_A:            " + actuator.CalibrationSettings.FullCurrent_A +
                "\n\t FullCurrent_B:            " + actuator.CalibrationSettings.FullCurrent_B +

                "\n\n chart_data_t:" +
                "\n\t DutyCycle:                " + actuator.ChartData.DutyCycle +
                "\n\t Joy:                      " + actuator.ChartData.Joy +
                "\n\t Pot:                      " + actuator.ChartData.Pot +
                "\n\t WindingCurrentA:          " + actuator.ChartData.WindingCurrentA +
                "\n\t WindingCurrentB:          " + actuator.ChartData.WindingCurrentB +
                "\n\t WindingCurrentC:          " + actuator.ChartData.WindingCurrentC +
                "\n\t WindingVoltageA:          " + actuator.ChartData.WindingVoltageA +
                "\n\t WindingVoltageB:          " + actuator.ChartData.WindingVoltageB +
                "\n\t WindingVoltageC:          " + actuator.ChartData.WindingVoltageC +

                "\n\n controller_name_t:" +
                "\n\t ControllerName:           " + actuator.ControllerName.ControllerName +
                "\n\t CtrlFlags:                " + actuator.ControllerName.CtrlFlags.ToString("X") +

                "\n\n control_settings_t:" +
                "\n\t DeltaPosition:            " + actuator.ControlSettings.DeltaPosition +
                "\n\t Flags:                    " + actuator.ControlSettings.Flags +
                "\n\t MaxClickTime:             " + actuator.ControlSettings.MaxClickTime +
                "\n\t MaxSpeed:                 " + actuator.ControlSettings.MaxSpeed +
                "\n\t Timeout:                  " + actuator.ControlSettings.Timeout +
                "\n\t uDeltaPosition:           " + actuator.ControlSettings.uDeltaPosition +
                "\n\t uMaxSpeed:                " + actuator.ControlSettings.uMaxSpeed +

                "\n\n ctp_settings_t:" +
                "\n\t CTPFlags:                 " + actuator.CtpSettings.CTPFlags.ToString("X") +
                "\n\t CTPMinError:              " + actuator.CtpSettings.CTPMinError +

                "\n\n device_information_t:" +
                "\n\t Major:                    " + actuator.DeviceInformation.Major +
                "\n\t Manufacturer:             " + actuator.DeviceInformation.Manufacturer +
                "\n\t ManufacturerId:           " + actuator.DeviceInformation.ManufacturerId +
                "\n\t Minor:                    " + actuator.DeviceInformation.Minor +
                "\n\t ProductDescription:       " + actuator.DeviceInformation.ProductDescription +
                "\n\t Release:                  " + actuator.DeviceInformation.Release +

                "\n\n edges_settings_t:" +
                "\n\t BorderFlags               " + actuator.EdgesSettings.BorderFlags +
                "\n\t EnderFlags                " + actuator.EdgesSettings.EnderFlags +
                "\n\t LeftBorder                " + actuator.EdgesSettings.LeftBorder +
                "\n\t RightBorder               " + actuator.EdgesSettings.RightBorder +
                "\n\t uLeftBorder               " + actuator.EdgesSettings.uLeftBorder +
                "\n\t uRightBorder              " + actuator.EdgesSettings.uRightBorder +

                "\n\n encoder_information_t:" +
                "\n\t Manufacturer:             " + actuator.EncoderInformation.Manufacturer +
                "\n\t PartNumber:               " + actuator.EncoderInformation.PartNumber +

                "\n\n encoder_settings_t:" +
                "\n\t EncoderSettings           " + actuator.EncoderSettings.EncoderSettings +
                "\n\t MaxCurrentConsumption     " + actuator.EncoderSettings.MaxCurrentConsumption +
                "\n\t MaxOperatingFrequency     " + actuator.EncoderSettings.MaxOperatingFrequency +
                "\n\t PPR                       " + actuator.EncoderSettings.PPR +
                "\n\t SupplyVoltageMax          " + actuator.EncoderSettings.SupplyVoltageMax +
                "\n\t SupplyVoltageMin          " + actuator.EncoderSettings.SupplyVoltageMin +

                "\n\n encoder_settings_t:" +
                "\n\t Antiplay                  " + actuator.EngineSettings.Antiplay +
                "\n\t EngineFlags               " + actuator.EngineSettings.EngineFlags +
                "\n\t MicrostepMode             " + actuator.EngineSettings.MicrostepMode +
                "\n\t NomCurrent                " + actuator.EngineSettings.NomCurrent +
                "\n\t NomSpeed                  " + actuator.EngineSettings.NomSpeed +
                "\n\t NomVoltage                " + actuator.EngineSettings.NomVoltage +
                "\n\t StepsPerRev               " + actuator.EngineSettings.StepsPerRev +
                "\n\t uNomSpeed                 " + actuator.EngineSettings.uNomSpeed +

                "\n\n encoder_settings_t:" +
                "\n\t DriverType                " + actuator.EntypeSettings.DriverType +
                "\n\t EngineType                " + actuator.EntypeSettings.EngineType +

                "\n\n extio_settings_t:" +
                "\n\t EXTIOModeFlags            " + actuator.ExtioSettings.EXTIOModeFlags.ToString("X") +
                "\n\t EXTIOSetupFlags           " + actuator.ExtioSettings.EXTIOSetupFlags.ToString("X") +

                "\n\n home_settings_t:" +
                "\n\t FastHome                  " + actuator.HomeSettings.FastHome +
                "\n\t HomeDelta                 " + actuator.HomeSettings.HomeDelta +
                "\n\t HomeFlags                 " + actuator.HomeSettings.HomeFlags +
                "\n\t SlowHome                  " + actuator.HomeSettings.SlowHome +
                "\n\t uFastHome                 " + actuator.HomeSettings.uFastHome +
                "\n\t uHomeDelta                " + actuator.HomeSettings.uHomeDelta +
                "\n\t uSlowHome                 " + actuator.HomeSettings.uSlowHome +

                "\n\n measurements_t:" +
                "\n\t Error                     " + actuator.MeasurementsData.Error +
                "\n\t Length                    " + actuator.MeasurementsData.Length +
                "\n\t Speed                     " + actuator.MeasurementsData.Speed +

                "\n\n motor_information_t:" +
                "\n\t Manufacturer              " + actuator.MotorInformation.Manufacturer +
                "\n\t PartNumber                " + actuator.MotorInformation.PartNumber +

                "\n\n motor_settings_t:" +
                "\n\t DetentTorque              " + actuator.MotorSettings.DetentTorque +
                "\n\t MaxCurrent                " + actuator.MotorSettings.MaxCurrent +
                "\n\t MaxCurrentTime            " + actuator.MotorSettings.MaxCurrentTime +
                "\n\t MaxSpeed                  " + actuator.MotorSettings.MaxSpeed +
                "\n\t MechanicalTimeConstant    " + actuator.MotorSettings.MechanicalTimeConstant +
                "\n\t MotorType                 " + actuator.MotorSettings.MotorType +
                "\n\t NoLoadCurrent             " + actuator.MotorSettings.NoLoadCurrent +
                "\n\t NoLoadSpeed               " + actuator.MotorSettings.NoLoadSpeed +
                "\n\t NominalCurrent            " + actuator.MotorSettings.NominalCurrent +
                "\n\t NominalPower              " + actuator.MotorSettings.NominalPower +
                "\n\t NominalSpeed              " + actuator.MotorSettings.NominalSpeed +
                "\n\t NominalTorque             " + actuator.MotorSettings.NominalTorque +
                "\n\t NominalVoltage            " + actuator.MotorSettings.NominalVoltage +
                "\n\t Phases                    " + actuator.MotorSettings.Phases +
                "\n\t Poles                     " + actuator.MotorSettings.Poles +
                "\n\t ReservedField             " + actuator.MotorSettings.ReservedField +
                "\n\t RotorInertia              " + actuator.MotorSettings.RotorInertia +
                "\n\t SpeedConstant             " + actuator.MotorSettings.SpeedConstant +
                "\n\t SpeedTorqueGradient       " + actuator.MotorSettings.SpeedTorqueGradient +
                "\n\t StallTorque               " + actuator.MotorSettings.StallTorque +
                "\n\t TorqueConstant            " + actuator.MotorSettings.TorqueConstant +
                "\n\t WindingInductance         " + actuator.MotorSettings.WindingInductance +
                "\n\t WindingResistance         " + actuator.MotorSettings.WindingResistance +

                "\n\n move_settings_t:" +
                "\n\t Accel                     " + actuator.MoveSettings.Accel +
                "\n\t AntiplaySpeed             " + actuator.MoveSettings.AntiplaySpeed +
                "\n\t Decel                     " + actuator.MoveSettings.Decel +
                "\n\t Speed                     " + actuator.MoveSettings.Speed +
                "\n\t uAntiplaySpeed            " + actuator.MoveSettings.uAntiplaySpeed +
                "\n\t uSpeed                    " + actuator.MoveSettings.uSpeed +

                "\n\n pid_settings_t:" +
                "\n\t Kdf                       " + actuator.PidSettings.Kdf +
                "\n\t KdU                       " + actuator.PidSettings.KdU +
                "\n\t Kif                       " + actuator.PidSettings.Kif +
                "\n\t KiU                       " + actuator.PidSettings.KiU +
                "\n\t Kpf                       " + actuator.PidSettings.Kpf +
                "\n\t KpU                       " + actuator.PidSettings.KpU +

                "\n\n get_position_t:" +
                "\n\t EncPosition               " + actuator.GetPositionData.EncPosition +
                "\n\t Position                  " + actuator.GetPositionData.Position +
                "\n\t uPosition                 " + actuator.GetPositionData.uPosition +

                "\n\n get_position_t:" +
                "\n\t CurrentSetTime            " + actuator.PowerSettings.CurrentSetTime +
                "\n\t CurrReductDelay           " + actuator.PowerSettings.CurrReductDelay +
                "\n\t HoldCurrent               " + actuator.PowerSettings.HoldCurrent +
                "\n\t PowerFlags                " + actuator.PowerSettings.PowerFlags +
                "\n\t PowerOffDelay             " + actuator.PowerSettings.PowerOffDelay +

                "\n\n get_position_t:" +
                "\n\tCriticalIpwr               " + actuator.SecureSettings.CriticalIpwr +
                "\n\tCriticalIusb               " + actuator.SecureSettings.CriticalIusb +
                "\n\tCriticalT                  " + actuator.SecureSettings.CriticalT +
                "\n\tCriticalUpwr               " + actuator.SecureSettings.CriticalUpwr +
                "\n\tCriticalUusb               " + actuator.SecureSettings.CriticalUusb +
                "\n\tFlags                      " + actuator.SecureSettings.Flags +
                "\n\tLowUpwrOff                 " + actuator.SecureSettings.LowUpwrOff +
                "\n\tMinimumUusb                " + actuator.SecureSettings.MinimumUusb +

                "\n\n status_t:" +
                "\n\t CmdBufFreeSpace           " + actuator.Status.CmdBufFreeSpace +
                "\n\t CurPosition               " + actuator.Status.CurPosition +
                "\n\t CurSpeed                  " + actuator.Status.CurSpeed +
                "\n\t CurT                      " + actuator.Status.CurT +
                "\n\t EncPosition               " + actuator.Status.EncPosition +
                "\n\t EncSts                    " + actuator.Status.EncSts +
                "\n\t Flags                     " + actuator.Status.Flags +
                "\n\t GPIOFlags                 " + actuator.Status.GPIOFlags +
                "\n\t Ipwr                      " + actuator.Status.Ipwr +
                "\n\t Iusb                      " + actuator.Status.Iusb +
                "\n\t MoveSts                   " + actuator.Status.MoveSts +
                "\n\t MvCmdSts                  " + actuator.Status.MvCmdSts +
                "\n\t PWRSts                    " + actuator.Status.PWRSts +
                "\n\t uCurPosition              " + actuator.Status.uCurPosition +
                "\n\t uCurSpeed                 " + actuator.Status.uCurSpeed +
                "\n\t Upwr                      " + actuator.Status.Upwr +
                "\n\t Uusb                      " + actuator.Status.Uusb +
                "\n\t WindSts                   " + actuator.Status.WindSts +

                "\n\n status_calb_t:" +
                "\n\t CmdBufFreeSpace           " + actuator.StatusCalibration.CmdBufFreeSpace +
                "\n\t CurPosition               " + actuator.StatusCalibration.CurPosition +
                "\n\t CurSpeed                  " + actuator.StatusCalibration.CurSpeed +
                "\n\t CurT                      " + actuator.StatusCalibration.CurT +
                "\n\t EncPosition               " + actuator.StatusCalibration.EncPosition +
                "\n\t EncSts                    " + actuator.StatusCalibration.EncSts +
                "\n\t Flags                     " + actuator.StatusCalibration.Flags +
                "\n\t GPIOFlags                 " + actuator.StatusCalibration.GPIOFlags +
                "\n\t Ipwr                      " + actuator.StatusCalibration.Ipwr +
                "\n\t Iusb                      " + actuator.StatusCalibration.Iusb +
                "\n\t MoveSts                   " + actuator.StatusCalibration.MoveSts +
                "\n\t MvCmdSts                  " + actuator.StatusCalibration.MvCmdSts +
                "\n\t PWRSts                    " + actuator.StatusCalibration.PWRSts +
                "\n\t Upwr                      " + actuator.StatusCalibration.Upwr +
                "\n\t Uusb                      " + actuator.StatusCalibration.Uusb +
                "\n\t WindSts                   " + actuator.StatusCalibration.WindSts +

                "\n\n sync_in_settings_t:" +
                "\n\t ClutterTime               " + actuator.SyncInSettings.ClutterTime +
                "\n\t Position                  " + actuator.SyncInSettings.Position +
                "\n\t Speed                     " + actuator.SyncInSettings.Speed +
                "\n\t SyncInFlags               " + actuator.SyncInSettings.SyncInFlags +
                "\n\t uPosition                 " + actuator.SyncInSettings.uPosition +
                "\n\t uSpeed                    " + actuator.SyncInSettings.uSpeed +

                "\n\n sync_out_settings_t:" +
                "\n\t ClutterTime               " + actuator.SyncOutSettings.Accuracy +
                "\n\t Position                  " + actuator.SyncOutSettings.SyncOutFlags +
                "\n\t Speed                     " + actuator.SyncOutSettings.SyncOutPeriod +
                "\n\t SyncInFlags               " + actuator.SyncOutSettings.SyncOutPulseSteps +
                "\n\t uPosition                 " + actuator.SyncOutSettings.uAccuracy
                );
        }
    }
}
