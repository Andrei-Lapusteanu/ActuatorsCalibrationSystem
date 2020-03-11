using Entities;
using System;
using System.Collections.Generic;
using ximc;

namespace Controller
{
    /// <summary>
    /// This class is responsible for creating a higher level wrapper, which contains techniques for error checking,
    /// result validarion, and logging, used to interface with the actuator controller's API.
    /// </summary>
    public class ActuatorController : ActuatorEntity
    {
        ActuatorErrorHandler actuatorErrorHandler;
        LogController logController;
        int listIndex = -1;

        /// <summary>
        /// Implicit constructor function
        /// </summary>
        public ActuatorController()
        {
            actuatorErrorHandler = new ActuatorErrorHandler();
            logController = new LogController();

            callback = new API.LoggingCallback(MyLog);
            API.set_logging_callback(callback, IntPtr.Zero);
        }

        /// <summary>
        /// Explicit constructor function
        /// </summary>
        /// <param name="index">identifies actuator, of type int</param>
        // TO DO - changed from public ActuatorController(int deviceID)
        public ActuatorController(int index)
        {
            actuatorErrorHandler = new ActuatorErrorHandler();
            logController = new LogController();
            this.ListIndex = index;
        }

        /// <summary>
        /// Closes the device specified by deviceID
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result CloseDevice(ref int deviceID)
        {
            try
            {
                if ((base.Result = API.close_device(ref deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.CloseDevice, this);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.CloseDevice, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Closes the device specified by deviceID without checking for errors
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result CloseDeviceNoErrorHandling(int deviceID)
        {
            return API.close_device(ref deviceID);
        }

        /// <summary>
        /// Defunct
        /// </summary>
        /// <param name="deviceName">identifies actuator, of type string</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result ProbeDeviceeNoErrorHandling(string deviceName)
        {
            return API.probe_device(deviceName);
        }

        /// <summary>
        /// Moves device specified by deviceID to the home position, which can be set using method Controller.ActuatorController.SetZeroPosition()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result MoveToHomePosition(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_home(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.MoveHome, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.MoveHome, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Unsafe. To be tested for validity   
        /// <br/>Moves the actuator specified by deviceID continuously to the left (retracting motion)
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result MoveContinuouslyLeft(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_left(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.MoveLeft, this);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.MoveLeft, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Moves the actuator specified by deviceID to an absolute position specified in actuator steps and microsteps
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="positionSteps">absolute value in actuator steps, of type int</param>
        /// <param name="positionMicrosteps">absolute value in actuator microsteps, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result MoveToPosition(int deviceID, int positionSteps, int positionMicrosteps)
        {
            try
            {
                if ((base.Result = API.command_move(deviceID, base.PositionSteps = positionSteps, base.PositionMicrosteps = positionMicrosteps)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.MoveToPosition, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.MoveToPosition, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Moves the actuator specified by deviceID by a delta position specified in actuator steps and microsteps
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="deltaSteps">relative actuator steps value, of type int</param>
        /// <param name="deltaMicrosteps">relative actuator microsteps value, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result MoveRelatively(int deviceID, int deltaSteps, int deltaMicrosteps)
        {
            try
            {
                if ((base.Result = API.command_movr(deviceID, base.DeltaSteps = deltaSteps, base.DeltaMicrosteps = deltaMicrosteps)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.MoveRelatively, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.MoveRelatively, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Powers off the actuator specified by deviceID
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result PowerOff(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_power_off(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.PowerOff, this);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.PowerOff, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }


        /// <summary>
        /// Unsafe. To be tested for validity <br/>
        ///Moves the actuator specified by deviceID continuously to the right (extending motion)
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result MoveContinuouslyRight(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_right(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.MoveRight, this);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.MoveRight, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Recommended method of stopping actuator <br/>
        ///Performs a decelerated stopping of the actuator specified by deviceID. The actuator's controller hardware
        /// is responsible for precomputing the deceleration distance and its parameters
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result SoftStop(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_sstp(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.SoftStop, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.SoftStop, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Unsafe method of stopping actuator. Refer to Controller.ActuatorController.SoftStop() method for a safer approach <br/>
        ///Stops the actuator specified by deviceID instantly. Unsafe because it can cause wear of the actuators' 
        /// hardware if used excessively
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result HardStop(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_stop(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.HardStop, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.HardStop, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        /// <summary>
        /// Method used for waiting for an action of the actuator specified by deviceID to finish before issuing another command. If not used, 
        /// then commands issued will be executed too rapidly. As such, the actuator will not have enough physical time in order to complete
        /// all of the requested commands
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="ms">delay in miliseconds, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result WaitForStopWhile(int deviceID, int ms)
        {
            try
            {
                while (API.command_wait_for_stop(deviceID, ms) != 0) ;
                logController.LogActuatorInfo(Enums.ActuatorLog.WaitForStop, this);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.WaitForStop, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
                return Result.error;
            }
            return Result.ok;
        }

        /// <summary>
        /// Hazardous. Could affect calibration if improperly used <br/>
        ///<The actuator specified by deviceID will set its current position to 0 steps and 0 microsteps
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public Result SetZeroPosition(int deviceID)
        {
            try
            {
                if ((base.Result = API.command_zero(deviceID)) == Result.ok)
                    logController.LogActuatorInfo(Enums.ActuatorLog.SetZero, this, Enums.ActuatorLogTiming.Start);
            }
            catch (Exception ex)
            {
                logController.LogActuatorError(Enums.ActuatorLog.SetZero, deviceID, ex);
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error occured. Handling is to be implemented");
            }
            return base.Result;
        }

        public accessories_settings_t GetAccessoriesSettings(int deviceID)
        {
            return API.get_accessories_settings(deviceID, out accessories_settings_t accSet) == Result.ok ?
                   base.AccessoriesSettings = accSet : new accessories_settings_t();
        }

        public analog_data_t GetAnalogData(int deviceID)
        {
            return API.get_analog_data(deviceID, out analog_data_t analogData) == Result.ok ?
                   base.AnalogData = analogData : new analog_data_t();
        }

        public brake_settings_t GetBrakeSettings(int deviceID)
        {
            return API.get_brake_settings(deviceID, out brake_settings_t brakeSet) == Result.ok ?
                   base.BrakeSettings = brakeSet : new brake_settings_t();
        }

        public calibration_settings_t GetCalibrationSettings(int deviceID)
        {
            return API.get_calibration_settings(deviceID, out calibration_settings_t calibSet) == Result.ok ?
                   base.CalibrationSettings = calibSet : new calibration_settings_t();
        }

        public chart_data_t GetChartData(int deviceID)
        {
            return API.get_chart_data(deviceID, out chart_data_t chartData) == Result.ok ?
                   base.ChartData = chartData : new chart_data_t();
        }

        public controller_name_t GetControllerName(int deviceID)
        {
            return API.get_controller_name(deviceID, out controller_name_t controllerName) == Result.ok ?
                   base.ControllerName = controllerName : new controller_name_t();
        }
        public control_settings_t GetControllerSettings(int deviceID)
        {
            return API.get_control_settings(deviceID, out control_settings_t controllerSet) == Result.ok ?
                   base.ControlSettings = controllerSet : new control_settings_t();
        }
        public ctp_settings_t GetCtpSettings(int deviceID)
        {
            return API.get_ctp_settings(deviceID, out ctp_settings_t ctpSettings) == Result.ok ?
                   base.CtpSettings = ctpSettings : new ctp_settings_t();
        }

        public device_information_t GetDeviceInformation(int deviceID)
        {
            return API.get_device_information(deviceID, out device_information_t devInfo) == Result.ok ?
                   base.DeviceInformation = devInfo : new device_information_t();
        }

        public string GetDeviceName(IntPtr deviceEnumeration, int deviceIndex)
        {
            return base.DeviceName = API.get_device_name(deviceEnumeration, deviceIndex);
        }

        public edges_settings_t GetEdgesSettings(int deviceID)
        {
            return API.get_edges_settings(deviceID, out edges_settings_t edgesSet) == Result.ok ?
                   base.EdgesSettings = edgesSet : new edges_settings_t();
        }

        public encoder_information_t GetEncoderInformation(int deviceID)
        {
            return API.get_encoder_information(deviceID, out encoder_information_t encInfo) == Result.ok ?
                   base.EncoderInformation = encInfo : new encoder_information_t();
        }

        public encoder_settings_t GetEncoderSettings(int deviceID)
        {
            return API.get_encoder_settings(deviceID, out encoder_settings_t encSet) == Result.ok ?
                   base.EncoderSettings = encSet : new encoder_settings_t();
        }

        public engine_settings_t GetEngineSettings(int deviceID)
        {
            return API.get_engine_settings(deviceID, out engine_settings_t engSet) == Result.ok ?
                   base.EngineSettings = engSet : new engine_settings_t();
        }

        public entype_settings_t GetEntypeSettings(int deviceID)
        {
            return API.get_entype_settings(deviceID, out entype_settings_t entypeSet) == Result.ok ?
                   base.EntypeSettings = entypeSet : new entype_settings_t();
        }

        public extio_settings_t GetExtioSettings(int deviceID)
        {
            return API.get_extio_settings(deviceID, out extio_settings_t extioSet) == Result.ok ?
                    base.ExtioSettings = extioSet : new extio_settings_t();
        }

        public home_settings_t GetHomeSettings(int deviceID)
        {
            return API.get_home_settings(deviceID, out home_settings_t homeSet) == Result.ok ?
                    base.HomeSettings = homeSet : new home_settings_t();
        }

        public measurements_t GetMeasurements(int deviceID)
        {
            return API.get_measurements(deviceID, out measurements_t meas) == Result.ok ?
                   base.MeasurementsData = meas : new measurements_t();
        }

        public motor_information_t GetMotorInformation(int deviceID)
        {
            return API.get_motor_information(deviceID, out motor_information_t motorInfo) == Result.ok ?
                   base.MotorInformation = motorInfo : new motor_information_t();
        }

        public motor_settings_t GetMotorSettings(int deviceID)
        {
            return API.get_motor_settings(deviceID, out motor_settings_t motorSet) == Result.ok ?
                   base.MotorSettings = motorSet : new motor_settings_t();
        }

        public move_settings_t GetMoveSettings(int deviceID)
        {
            return API.get_move_settings(deviceID, out move_settings_t moveSet) == Result.ok ?
                   base.MoveSettings = moveSet : new move_settings_t();
        }

        public pid_settings_t GetPidSettings(int deviceID)
        {
            return API.get_pid_settings(deviceID, out pid_settings_t pidSet) == Result.ok ?
                   base.PidSettings = pidSet : new pid_settings_t();
        }

        public get_position_t GetPosition(int deviceID)
        {
            return API.get_position(deviceID, out get_position_t position) == Result.ok ?
                   base.GetPositionData = position : new get_position_t();
        }

        public power_settings_t GetPowerSettings(int deviceID)
        {
            return API.get_power_settings(deviceID, out power_settings_t powerSet) == Result.ok ?
                   base.PowerSettings = powerSet : new power_settings_t();
        }

        public secure_settings_t GetSecureSettings(int deviceID)
        {
            return API.get_secure_settings(deviceID, out secure_settings_t secureSet) == Result.ok ?
                   base.SecureSettings = secureSet : new secure_settings_t();
        }

        public status_t GetStatus(int deviceID)
        {
            return API.get_status(deviceID, out status_t stat) == Result.ok ?
                   base.Status = stat : new status_t();
        }

        public status_calb_t GetStatusCalibration(int deviceID, calibration_t calibration)
        {
            return API.get_status_calb(deviceID, out status_calb_t statusCal, ref calibration) == Result.ok ?
                   base.StatusCalibration = statusCal : new status_calb_t();
        }

        public sync_in_settings_t GetSyncInSettings(int deviceID)
        {
            return API.get_sync_in_settings(deviceID, out sync_in_settings_t syncInSet) == Result.ok ?
                   base.SyncInSettings = syncInSet : new sync_in_settings_t();
        }

        public sync_out_settings_t GetSyncOutSettings(int deviceID)
        {
            return API.get_sync_out_settings(deviceID, out sync_out_settings_t syncOutSet) == Result.ok ?
                   base.SyncOutSettings = syncOutSet : new sync_out_settings_t();
        }

        public void SleepForMs(int ms)
        {
            API.msec_sleep(ms);
        }

        private static API.LoggingCallback callback;

        private static void MyLog(API.LogLevel loglevel, string message, IntPtr user_data)
        {
            //Console.WriteLine("MyLog {0}: {1}", loglevel, message);
        }

        /// <summary>
        /// Hazardous. Do not try to open more than one device at a given time. Perform a 
        /// Controller.ActuatorController.CloseDevice() method on the currently opened device before attempting to open any other <br/>
        ///Opens a device specified by deviceName string
        /// </summary>
        /// <param name="deviceName">identifies actuator, it specifies the interface name and port, of type string</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public int OpenDevice(string deviceName)
        {
            try
            {
                // Functie API
                if ((base.DeviceID = API.open_device(deviceName)) != -1)
                    // Functie de inregistrare
                    logController.LogActuatorInfo(Enums.ActuatorLog.OpenDevice, this);
            }
            // Tratae exceptii
            catch (Exception ex)
            {
                // Functie de inregistrare erori
                logController.LogActuatorError(Enums.ActuatorLog.OpenDevice, base.DeviceID, ex);
                // Metoda de apel in caz de eroare
                actuatorErrorHandler.HandlePhysicalErrors(Enums.PhysicalErrorType.APIError, "API error");
            }
            return base.DeviceID;
        }

        public Result SetAccessoriesSettings(int deviceID, ref accessories_settings_t accessoriesSettings)
        {
            base.AccessoriesSettings = accessoriesSettings;

            return API.set_accessories_settings(deviceID, ref accessoriesSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetBrakeSettings(int deviceID, ref brake_settings_t brakeSettings)
        {
            base.BrakeSettings = brakeSettings;

            return API.set_brake_settings(deviceID, ref brakeSettings) == Result.ok ?
                    base.Result : Result.error;
        }

        public Result SetCalibrationSetings(int deviceID, ref calibration_settings_t calibrationSettings)
        {
            base.CalibrationSettings = calibrationSettings;

            return API.set_calibration_settings(deviceID, ref calibrationSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetControlSettings(int deviceID, ref control_settings_t controlSettings)
        {
            base.ControlSettings = controlSettings;

            return API.set_control_settings(deviceID, ref controlSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetControllerName(int deviceID, ref controller_name_t controllerName)
        {
            base.ControllerName = controllerName;

            return API.set_controller_name(deviceID, ref controllerName) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetCtpSettings(int deviceID, ref ctp_settings_t ctpSettings)
        {
            base.CtpSettings = ctpSettings;

            return API.set_ctp_settings(deviceID, ref ctpSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetEdgesSettings(int deviceID, ref edges_settings_t edgesSettings)
        {
            base.EdgesSettings = edgesSettings;

            return API.set_edges_settings(deviceID, ref edgesSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetEncoderInformation(int deviceID, ref encoder_information_t encoderInformation)
        {
            base.EncoderInformation = encoderInformation;

            return API.set_encoder_information(deviceID, ref encoderInformation) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetEncoderSettings(int deviceID, ref encoder_settings_t encoderSettings)
        {
            base.EncoderSettings = encoderSettings;

            return API.set_encoder_settings(deviceID, ref encoderSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetEngineSettings(int deviceID, ref engine_settings_t engineSettings)
        {
            base.EngineSettings = engineSettings;

            return API.set_engine_settings(deviceID, ref engineSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetEntypeSettings(int deviceID, ref entype_settings_t entypeSettings)
        {
            base.EntypeSettings = entypeSettings;

            return API.set_entype_settings(deviceID, ref entypeSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetExtioSettings(int deviceID, ref extio_settings_t extioSettings)
        {
            base.ExtioSettings = extioSettings;

            return API.set_extio_settings(deviceID, ref extioSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetHomeSettings(int deviceID, ref home_settings_t homeSettings)
        {
            base.HomeSettings = homeSettings;

            return API.set_home_settings(deviceID, ref homeSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetMotorInformation(int deviceID, ref motor_information_t motorInformation)
        {
            base.MotorInformation = motorInformation;

            return API.set_motor_information(deviceID, ref motorInformation) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetMotorSettings(int deviceID, ref motor_settings_t motorSettings)
        {
            base.MotorSettings = motorSettings;

            return API.set_motor_settings(deviceID, ref motorSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetMoveSettings(int deviceID, ref move_settings_t moveSettings)
        {
            base.MoveSettings = moveSettings;

            return API.set_move_settings(deviceID, ref moveSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetPidSettings(int deviceID, ref pid_settings_t pidSettings)
        {
            base.PidSettings = pidSettings;

            return API.set_pid_settings(deviceID, ref pidSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public new Result SetPosition(int deviceID, ref set_position_t setPosition)
        {
            base.SetPosition = setPosition;

            return API.set_position(deviceID, ref setPosition) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetPowerSettings(int deviceID, ref power_settings_t powerSettings)
        {
            base.PowerSettings = powerSettings;

            return API.set_power_settings(deviceID, ref powerSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetSecureSettings(int deviceID, ref secure_settings_t secureSettings)
        {
            base.SecureSettings = secureSettings;

            return API.set_secure_settings(deviceID, ref secureSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public Result SetSyncInSettings(int deviceID, ref sync_in_settings_t syncInSettings)
        {
            base.SyncInSettings = syncInSettings;

            return API.set_sync_in_settings(deviceID, ref syncInSettings) == Result.ok ?
                    base.Result : Result.error;
        }

        public Result SetSyncOutSettings(int deviceID, ref sync_out_settings_t syncOutSettings)
        {
            base.SyncOutSettings = syncOutSettings;

            return base.Result = API.set_sync_out_settings(deviceID, ref syncOutSettings) == Result.ok ?
                   base.Result : Result.error;
        }

        public void SetAxis(int deviceID, Enums.Axis axis)
        {
            base.Axis = axis;
        }

        public Enums.Axis GetAxis(int deviceId)
        {
            return base.Axis;
        }

        /// <summary>
        /// Logs all available actuator information for the actuator specified by deviceID
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void GetActuatorInformation(int deviceID)
        {
            GetAccessoriesSettings(deviceID);
            GetAnalogData(deviceID);
            GetBrakeSettings(deviceID);
            GetCalibrationSettings(deviceID);
            GetChartData(deviceID);
            GetControllerName(deviceID);
            GetControllerSettings(deviceID);
            GetCtpSettings(deviceID);
            GetDeviceInformation(deviceID);
            GetEdgesSettings(deviceID);
            GetEncoderInformation(deviceID);
            GetEncoderSettings(deviceID);
            GetEngineSettings(deviceID);
            GetEntypeSettings(deviceID);
            GetExtioSettings(deviceID);
            GetHomeSettings(deviceID);
            GetMeasurements(deviceID);
            GetMotorInformation(deviceID);
            GetMotorSettings(deviceID);
            GetMoveSettings(deviceID);
            GetPidSettings(deviceID);
            GetPosition(deviceID);
            GetPowerSettings(deviceID);
            GetStatus(deviceID);
            base.Calibration = new calibration_t { A = 1, MicrostepMode = base.EngineSettings.MicrostepMode };
            GetStatusCalibration(DeviceID, base.Calibration);
            GetSyncInSettings(deviceID);
            GetSyncOutSettings(deviceID);

            logController.LogAllSettings(this);
        }

        public int GetCurrentSpeedSteps(int deviceID)
        {
            return GetStatus(deviceID).CurSpeed;
        }

        public int GetCurrentSpeedMicrosteps(int deviceID)
        {
            return GetStatus(deviceID).uCurSpeed;
        }

        public int GetCurrentPositionSteps(int deviceID)
        {
            return GetStatus(deviceID).CurPosition;
        }

        public int GetCurrentPositionMicroSteps(int deviceID)
        {
            return GetStatus(deviceID).uCurPosition;
        }

        public bool IsActuatorMoving(int deviceID)
        {
            if (GetCurrentSpeedSteps(deviceID) == 0 && GetCurrentSpeedMicrosteps(deviceID) == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Unsafe. Could physically damage actuators <br/>
        ///Perform a calibration of the actuator specified by deviceID. The actuator is moved to both edges and a middle (reference)
        /// value is set, which will represent the new zero position
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns>Result.ok if operation is successful, otherwise either Result.error, Result.value_error, Result.no_device, or Result.not_implemented</returns>
        public List<int> CalibrateMovement(int deviceID)
        {
            // Set move settings
            int minEdge = 0;
            int maxEdge = 0;

            move_settings_t moveSettings = new move_settings_t { Accel = 1000, Decel = 1000, Speed = 400 };
            SetMoveSettings(deviceID, ref moveSettings);

            // Move left until edge and remember position
            MoveContinuouslyLeft(deviceID);
            if (WaitForStopWhile(deviceID, 0) == Result.ok)
                minEdge = GetCurrentPositionSteps(deviceID);

            // Move right until edge and remember position
            MoveContinuouslyRight(deviceID);
            if (WaitForStopWhile(deviceID, 0) == Result.ok)
                maxEdge = GetCurrentPositionSteps(deviceID);

            Int32 zero = (maxEdge + minEdge) / 2;
            // Do maxEdge - minEdge and move to the position (middle)
            MoveToPosition(deviceID, zero, 0);
            if (WaitForStopWhile(deviceID, 0) == Result.ok)
                // Set this position as the new home position
                SetZeroPosition(deviceID);

            List<int> edgeValues = new List<int> { minEdge, maxEdge };

            return edgeValues;
        }

        /// <summary>
        /// Index in actuator list, getter and setter
        /// </summary>
        public int ListIndex { get => listIndex; set => listIndex = value; }

    }
}
