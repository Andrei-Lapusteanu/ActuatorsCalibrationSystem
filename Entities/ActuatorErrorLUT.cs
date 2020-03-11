using System.Collections.Generic;

namespace Entities
{
    /// <summary>
    /// This class contains a LUT (Look Up Table), implemented using a dictionary,
    /// of Entities.Enums.ControllerError and string
    /// </summary>
    public class ActuatorErrorLUT
    {
        private Dictionary<Enums.ControllerError, string> messages;
        private string critStr = "Critical error: ";
        private string warnStr = "Warning: ";

        /// <summary>
        /// Implicit constructor, fills the dicitonary with data
        /// </summary>
        public ActuatorErrorLUT()
        {
            Messages = new Dictionary<Enums.ControllerError, string>
            {
                // Critical
                { Enums.ControllerError.StateAlarm, critStr + "Controller is in alarm state indicating that something dangerous had happened. Most commands are ignored in this state" },
                { Enums.ControllerError.PowerDriverOverheat, critStr + "Power driver overheat" },
                { Enums.ControllerError.PowerControllerOverheat, critStr + "Controller overheat" },
                { Enums.ControllerError.PowerVolatageOverload, critStr + "Power voltage exceeds safe limit" },
                { Enums.ControllerError.PowerCurrentOverload, critStr + "Power current exceeds safe limit" },
                { Enums.ControllerError.UsbVoltageOverload, critStr + "USB voltage exceeds safe limit" },
                { Enums.ControllerError.UsbCurrentOverload, critStr + "USB current exceeds safe limit" },
                { Enums.ControllerError.CurrentLimitExceeded, critStr + "Current limit exceeded" },
                { Enums.ControllerError.PowerOff, critStr + "Power supply voltage is too low" },
                { Enums.ControllerError.NoVoltage, critStr + "No voltage detected, actuator may be disconnected" },

                // Warning (recoverable)
                { Enums.ControllerError.EncoderStateMalfunction, warnStr + "Encoder is connected and malfunctioning" },
                { Enums.ControllerError.CommandError, warnStr + "Command error encountered" },
                { Enums.ControllerError.DataIntegrityError, warnStr + "Data integrity error encountered" },
                { Enums.ControllerError.ValueError, warnStr + "Value error encountered" },
                { Enums.ControllerError.LeftLimitReached, warnStr + "Engine stuck at the left edge" },
                { Enums.ControllerError.RightLimitReached, warnStr + "Engine stuck at the right edge" },
                { Enums.ControllerError.TimeoutError, warnStr + "Device is not responding" },

                // Redundant
                { Enums.ControllerError.TemperatureOutsideLimits, "Warning: Temperature is outside safe limits" }
            };
        }

        /// <summary>
        /// Dictionary<Entities.Enums.ControllerError, string> which contains entries for all
        /// possible controller error types
        /// </summary>
        public Dictionary<Enums.ControllerError, string> Messages { get => messages; set => messages = value; }
    }
}
