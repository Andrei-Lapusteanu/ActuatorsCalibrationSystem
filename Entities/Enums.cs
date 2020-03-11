namespace Entities
{
    /// <summary>
    /// Contains enumerators used to uniquely implement helper data structures
    /// </summary>
    public class Enums
    {
        public enum ActuatorLog
        {
            CloseDevice,
            MoveHome,
            MoveLeft,
            MoveToPosition,
            MoveRelatively,
            PowerOff,
            MoveRight,
            SoftStop,
            HardStop,
            SetZero,
            WaitForStop,
            OpenDevice,
            NewActuatorInContext,
        }

        public enum MultipleActuatorsLog
        {
            FreeEnumeratedActuators,
            EnumerateActuators,
            GetDeviceCount,
            AddActuatorsToList,
            GetActuatorNames,
            TryOpenActuators,
            GetActuatorInformation,
            CloseActuators,
            RemovedActuator,
            ProcessNewActuator,
            NoDevicesFound
        }

        public enum ActuatorLogTiming
        {
            Start,
            Finish,
            DontCare
        }

        public enum PhysicalErrorType
        {
            LeftEdge,
            RightEdge,
            Timeout,
            Critical,
            Warning,
            APIError
        }

        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            CriticalError,
            Camera
        }

        public enum ControllerError
        {
            StatusOK,
            EncoderStateMalfunction,    // EncSts, 	  0x2 - ENC_STATE_MALFUNC
            CommandError,               // Flags,     0x1 - STATE_ERRC	
            DataIntegrityError,         // Flags,     0x2 - STATE_ERRD	
            ValueError,                 // Flags,     0x4 - STATE_ERRV
            StateAlarm,                 // Flags,     0x40 - STATE_ALARM
            PowerDriverOverheat,        // Flags,     0x100 - STATE_POWER_OVERHEAT
            PowerControllerOverheat,    // Flags,     0x200 - STATE_CONTROLLER_OVERHEAT
            PowerVolatageOverload,      // Flags,     0x400 - STATE_OVERLOAD_POWER_VOLTAGE
            PowerCurrentOverload,       // Flags,     0x800 - STATE_OVERLOAD_POWER_CURRENT
            UsbVoltageOverload,         // Flags,     0x1000 - STATE_OVERLOAD_USB_VOLTAGE
            UsbCurrentOverload,         // Flags,     0x4000 - STATE_OVERLOAD_USB_CURRENT
            CurrentLimitExceeded,       // Flags,     0x400000 - STATE_MOTOR_CURRENT_LIMIT
            LeftLimitReached,           // GPIOFlags, 0x2 - STATE_LEFT_EDGE
            RightLimitReached,          // GPIOFlags, 0x1 - STATE_RIGHT_EDGE
            PowerOff,                   // Upwr == 0
            TemperatureOutsideLimits,   // CurTdcw
            TimeoutError,
            NoVoltage
        }

        public enum TextBoxValueSafetyType
        {
            MoveTo,
            ShiftOn,
        }

        public enum TextBoxActuatorSettingsType
        {
            Speed,
            MicroSpeed,
            Acceleration,
            Deceleration,
            MinEgde,
            MaxEdge
        }

        public enum TextBoxState
        {
            Warning,
            Default
        }

        public enum Axis
        {
            X,
            Y,
            Z,
            DontCare
        }

        public enum ScrollDirection
        {
            Up,
            Down
        }

        public enum HistogramCalculation
        {
            Normal,
            Reversed
        }

        public enum ImgProcAlgorithm
        {
            None,
            StaticThresh,
            DynamicThresh,
            OstuThresh,
        }

        public enum CalibrationAlgorithm
        {
            None,
            ImageDistance,
            BoundingBox
        }

        public enum TrackingAlgoirthm
        {
            None,
            ImageDistance,
            BoundingBox
        }

        public enum UIDevice
        {
            One,
            Two,
            Three
        }

        public enum BoundingBoxCorner
        {
            TopLeft,
            BottomRight
        }

        public enum ApslAxis
        {
            Axis_X_MinEdgePos,
            Axis_X_MaxEdgePos,
            Axis_X_HomeEdgePos,
            Axis_Y_MinEdgePos,
            Axis_Y_MaxEdgePos,
            Axis_Y_HomeEdgePos,
            Axis_Z_MinEdgePos,
            Axis_Z_MaxEdgePos,
            Axis_Z_HomeEdgePos
        }

        public enum ApslEdgeType
        {
            Min,
            Max
        }

        public enum CtrlEgdeType
        {
            Min,
            Max
        }

        public enum StepsType
        {
            Steps,
            MicroSteps
        }

        public enum AxisDeviceType
        {
            DeviceOne,
            DeviceTwo,
            DeviceThree,
            MoveTo,
            Shift,
        }

        public enum ToggleType
        {
            Enabled,
            Disabled
        }
    }
}
