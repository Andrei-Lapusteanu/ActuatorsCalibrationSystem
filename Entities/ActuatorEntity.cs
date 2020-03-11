using System;
using ximc;

/// <summary>
/// Namespace reponsible for maintaining the current state of the entities in the application
/// </summary>
namespace Entities
{
    /// <summary>
    /// This class has a substantial number of members, which completely represent the parameters of
    /// an actuator. Mist important are related to acutator ID, name, position, speed, 
    /// calibration settings, control, motor, status, power management.
    /// </summary>
    public class ActuatorEntity
    {
        Result result;
        int deviceID;
        string deviceName;
        string uiName;
        int positionSteps;
        int positionMicrosteps;
        int deltaSteps;
        int deltaMicrosteps;
        int waitForStopMs;
        int controllerSleepMs;
        IntPtr deviceEnumeration;
        accessories_settings_t accessoriesSettings;
        analog_data_t analogData;
        brake_settings_t brakeSettings;
        calibration_settings_t calibrationSettings;
        calibration_t calibration;
        chart_data_t chartData;
        control_settings_t controlSettings;
        controller_name_t controllerName;
        ctp_settings_t ctpSettings;
        device_information_t deviceInformation;
        edges_settings_t edgesSettings;
        encoder_information_t encoderInformation;
        encoder_settings_t encoderSettings;
        engine_settings_t engineSettings;
        entype_settings_t entypeSettings;
        extio_settings_t extioSettings;
        home_settings_t homeSettings;
        measurements_t measurementsData;
        motor_information_t motorInformation;
        motor_settings_t motorSettings;
        move_settings_t moveSettings;
        pid_settings_t pidSettings;
        get_position_t getPositionData;
        power_settings_t powerSettings;
        secure_settings_t secureSettings;
        set_position_t setPosition;
        status_t status;
        status_calb_t statusCalibration;
        sync_in_settings_t syncInSettings;
        sync_out_settings_t syncOutSettings;
        Enums.Axis axis;

        public Result Result { get => result; set => result = value; }
        public int DeviceID { get => deviceID; set => deviceID = value; }
        public string DeviceName { get => deviceName; set => deviceName = value; }
        public string UIName { get => uiName; set => uiName = value; }
        public int PositionSteps { get => positionSteps; set => positionSteps = value; }
        public int PositionMicrosteps { get => positionMicrosteps; set => positionMicrosteps = value; }
        public int DeltaSteps { get => deltaSteps; set => deltaSteps = value; }
        public int DeltaMicrosteps { get => deltaMicrosteps; set => deltaMicrosteps = value; }
        public int WaitForStopMs { get => waitForStopMs; set => waitForStopMs = value; }
        public int ControllerSleepMs { get => controllerSleepMs; set => controllerSleepMs = value; }
        public IntPtr DeviceEnumeration { get => deviceEnumeration; set => deviceEnumeration = value; }
        public accessories_settings_t AccessoriesSettings { get => accessoriesSettings; set => accessoriesSettings = value; }
        public analog_data_t AnalogData { get => analogData; set => analogData = value; }
        public brake_settings_t BrakeSettings { get => brakeSettings; set => brakeSettings = value; }
        public calibration_settings_t CalibrationSettings { get => calibrationSettings; set => calibrationSettings = value; }
        public calibration_t Calibration { get => calibration; set => calibration = value; }
        public chart_data_t ChartData { get => chartData; set => chartData = value; }
        public control_settings_t ControlSettings { get => controlSettings; set => controlSettings = value; }
        public controller_name_t ControllerName { get => controllerName; set => controllerName = value; }
        public ctp_settings_t CtpSettings { get => ctpSettings; set => ctpSettings = value; }
        public device_information_t DeviceInformation { get => deviceInformation; set => deviceInformation = value; }
        public edges_settings_t EdgesSettings { get => edgesSettings; set => edgesSettings = value; }
        public encoder_information_t EncoderInformation { get => encoderInformation; set => encoderInformation = value; }
        public encoder_settings_t EncoderSettings { get => encoderSettings; set => encoderSettings = value; }
        public engine_settings_t EngineSettings { get => engineSettings; set => engineSettings = value; }
        public entype_settings_t EntypeSettings { get => entypeSettings; set => entypeSettings = value; }
        public extio_settings_t ExtioSettings { get => extioSettings; set => extioSettings = value; }
        public home_settings_t HomeSettings { get => homeSettings; set => homeSettings = value; }
        public measurements_t MeasurementsData { get => measurementsData; set => measurementsData = value; }
        public motor_information_t MotorInformation { get => motorInformation; set => motorInformation = value; }
        public motor_settings_t MotorSettings { get => motorSettings; set => motorSettings = value; }
        public move_settings_t MoveSettings { get => moveSettings; set => moveSettings = value; }
        public pid_settings_t PidSettings { get => pidSettings; set => pidSettings = value; }
        public get_position_t GetPositionData { get => getPositionData; set => getPositionData = value; }
        public power_settings_t PowerSettings { get => powerSettings; set => powerSettings = value; }
        public secure_settings_t SecureSettings { get => secureSettings; set => secureSettings = value; }
        public set_position_t SetPosition { get => setPosition; set => setPosition = value; }
        public status_t Status { get => status; set => status = value; }
        public status_calb_t StatusCalibration { get => statusCalibration; set => statusCalibration = value; }
        public sync_in_settings_t SyncInSettings { get => syncInSettings; set => syncInSettings = value; }
        public sync_out_settings_t SyncOutSettings { get => syncOutSettings; set => syncOutSettings = value; }
        public Enums.Axis Axis { get => axis; set => axis = value; }
    }
}
