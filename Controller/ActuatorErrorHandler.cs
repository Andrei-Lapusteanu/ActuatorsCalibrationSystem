using Entities;
using ximc;

namespace Controller
{
    /// <summary>
    /// This class contains methods which can identify and differentiate the types
    /// of exceptions and errors raised, and methods which are able to handle these
    /// types of occurences. These methods make use of the libximc libraries' error 
    /// flags.<br/>
    ///These are two types of exceptions that can be raised: Warning and Critical 
    /// </summary>
    class ActuatorErrorHandler
    {
        MainController mainController;
        LogController logController;
        ActuatorErrorLUT LUT;

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public ActuatorErrorHandler()
        {
            logController = new LogController();
            LUT = new ActuatorErrorLUT();
        }

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="mainController">parameter is defunct, to be removed</param>
        public ActuatorErrorHandler(MainController mainController)
        {
            logController = new LogController();
            LUT = new ActuatorErrorLUT();
            this.mainController = mainController;
        }

        /// <summary>
        /// Checks for a wide variety of possible errors, handles, logs, and prints them on the user interface notification area
        /// <br/>See Entities.Enums.ControllerError for all possible error types
        /// <br/>See Entities.Enums.NotificationType for all possible notification types
        /// </summary>
        /// <param name="status">object which holds relevant data regarding to the actuators status, of type status_t</param>
        public void CheckForActuatorErrors(status_t status)
        {
            string errorMessage;

            switch (status.EncSts)
            {
                case 0x2:
                    LUT.Messages.TryGetValue(Enums.ControllerError.EncoderStateMalfunction, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.EncoderStateMalfunction, status.EncSts, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Warning, errorMessage);
                    break;

                default:
                    break;
            }
            switch (status.Flags ^ 0x40)
            {
                case 0x1:
                    LUT.Messages.TryGetValue(Enums.ControllerError.CommandError, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.CommandError, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Warning, errorMessage);
                    break;

                case 0x2:
                    LUT.Messages.TryGetValue(Enums.ControllerError.DataIntegrityError, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.DataIntegrityError, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Warning, errorMessage);
                    break;

                case 0x4:
                    LUT.Messages.TryGetValue(Enums.ControllerError.ValueError, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.ValueError, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Warning, errorMessage);
                    break;

                case 0x100:
                    LUT.Messages.TryGetValue(Enums.ControllerError.PowerDriverOverheat, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.PowerDriverOverheat, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x200:
                    LUT.Messages.TryGetValue(Enums.ControllerError.PowerControllerOverheat, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.PowerControllerOverheat, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x400:
                    LUT.Messages.TryGetValue(Enums.ControllerError.PowerVolatageOverload, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.PowerVolatageOverload, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x800:
                    LUT.Messages.TryGetValue(Enums.ControllerError.PowerCurrentOverload, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.PowerCurrentOverload, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x1000:
                    LUT.Messages.TryGetValue(Enums.ControllerError.UsbVoltageOverload, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.UsbVoltageOverload, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x4000:
                    LUT.Messages.TryGetValue(Enums.ControllerError.UsbCurrentOverload, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.UsbCurrentOverload, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                case 0x400000:
                    LUT.Messages.TryGetValue(Enums.ControllerError.CurrentLimitExceeded, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.CurrentLimitExceeded, status.Flags ^ 0x40, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
                    break;

                default:
                    break;
            }

            switch (status.GPIOFlags)
            {
                case 0x1:
                    LUT.Messages.TryGetValue(Enums.ControllerError.LeftLimitReached, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.LeftLimitReached, status.GPIOFlags, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.LeftEdge, errorMessage);
                    break;

                case 0x2:
                    LUT.Messages.TryGetValue(Enums.ControllerError.RightLimitReached, out errorMessage);
                    logController.LogControllerError(Enums.ControllerError.RightLimitReached, status.GPIOFlags, errorMessage);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, errorMessage));
                    HandlePhysicalErrors(Enums.PhysicalErrorType.RightEdge, errorMessage);
                    break;

                default:
                    break;
            }

            if (status.Upwr <= 0)
            {
                LUT.Messages.TryGetValue(Enums.ControllerError.NoVoltage, out errorMessage);
                logController.LogControllerError(Enums.ControllerError.NoVoltage, status.GPIOFlags, errorMessage);
                Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, errorMessage));
                HandlePhysicalErrors(Enums.PhysicalErrorType.Critical, errorMessage);
            }
        }

        /// <summary>
        /// These is no implementation as of now <br/>
        ///Handles errors caught by Controller.ActuatorErrorHandler.CheckForActuatorErrors()
        /// </summary>
        /// <param name="errorType">type of error, of type Enitites.Enums.PhysicalErrorType</param>
        /// <param name="errorMessage">error message, of type string</param>
        public void HandlePhysicalErrors(Enums.PhysicalErrorType errorType, string errorMessage)
        {
            switch (errorType)
            {
                case Enums.PhysicalErrorType.LeftEdge:
                    break;

                case Enums.PhysicalErrorType.RightEdge:
                    break;

                case Enums.PhysicalErrorType.Warning:
                    break;

                case Enums.PhysicalErrorType.Critical:
                    CheckForConnectedActuators();
                    break;

                case Enums.PhysicalErrorType.Timeout:
                    break;

                case Enums.PhysicalErrorType.APIError:
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Deprecated and useless - to delete
        /// </summary>
        // TO DO - unused
        public void CheckForConnectedActuators()
        {
            if (mainController.HasActuatorCountReduced() == true)
                mainController.DisconnectActuator();

            else if (mainController.GetNewActuatorCount() == 0)
                mainController.ActuatorInContext.DeviceID = -1;

        }

    }
}
