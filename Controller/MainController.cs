using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using ximc;

/*
 * Order:
 * Enumerate Devices into IntPtr                  
 * Device Count into int                          
 * IF device count positive, select which to open 
 *      Device name into string
 *      Close all devices
 *      Open device into int - deviceID
 * Get stats for UI and poll for errors constantly
 * */

/*
 * NOTE:
 * Not sure if making actuatorInContext public (property) is a good idea
 * It's easier this way because in View you can directly access parameters for the UI
 * But becase of this I can (if I really want to) use the methods from ActuatorController in View, 
 * which I do not want to do because View should have minimal to no logic
 */

/* NOTE
 * When we open a device, should we automatically close all other and then open the selected one
 * or is it a good idea to close all devices in the back-end when we call OpenDevice(string deviceName)?
 * Is there any case in which we want to call OpenDevice(string deviceName) without closing all other beforehand?
 */

/// <summary>
/// Comtains the logic of the application. Executes methods raised by events from the user interface
/// </summary>
namespace Controller
{
    /// <summary>
    /// This class is responsible for delegating operations for the other classes
    /// containted within the Controller namespace, thus acting as a master class.
    /// It is the main class which links with the user interface, executes events 
    /// raised by it, or exposes values to be printed.
    /// </summary>
    public class MainController
    {
        Actuators actuators;
        ActuatorController actuatorInContext;
        ActuatorErrorHandler actuatorErrorHandler;
        LogController logController;
        BackgroundWorker bgWorkerErrorPolling;
        BackgroundWorker bgWorkerPollForActuators;
        List<Notification> notifList;
        bool isFirstTimeLaunch;

        static readonly object _lockerEnum = new object();
        static readonly object _lockerGetCount = new object();

        /// <summary>
        /// Unused, to be deleted
        /// </summary>
        public void Metoda()
        {
            // Aplicare lacatuire
            Monitor.Enter(_lockerEnum);

            Console.WriteLine("Cod lacatuit");

            // Eliberare lacat
            Monitor.Exit(_lockerEnum);
        }

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public MainController()
        {
            Actuators = new Actuators();
            ActuatorInContext = new ActuatorController();
            actuatorErrorHandler = new ActuatorErrorHandler(this);
            logController = new LogController();
            notifList = new List<Notification>();
            isFirstTimeLaunch = true;

            InitActuators();
            InitBgWorkers();
        }

        /// <summary>
        /// Thread used to delegate work for error polling, sets a delay in miliseconds for the rate of polling
        /// </summary>
        /// <param name="obj">method necessary, of type System.object</param>
        /// <param name="e">event argument, of type System.ComponentModel.DoWorkEventArgs</param>
        void bgWorkerErrorPollingDoWork(object obj, DoWorkEventArgs e)
        {
            while (true)
            {
                bgWorkerErrorPolling.ReportProgress(0);
                Thread.Sleep(50);
            }
        }
        /// <summary>
        /// Deprecated. Thread was tasked with delegating work used for checking for newly connected actuators.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        /*TO DO - turned out not to work, to delete or rethink*/
        void bgWorkerPollForActuatorsDoWork(object obj, DoWorkEventArgs e)
        {
            while (true)
            {
                bgWorkerPollForActuators.ReportProgress(0);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Deprecated. Thread was tasked with executing work used for checking for newly connected actuators. 
        /// by calling Controller.ActuatorErrorHandler.CheckForActuatorErrors()
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        // TO DELETE
        void bgWorkerErrorPollingProgressChanged(object obj, ProgressChangedEventArgs e)
        {
            if (ActuatorInContext.DeviceID != -1)
                actuatorErrorHandler.CheckForActuatorErrors(ActuatorInContext.GetStatus(ActuatorInContext.DeviceID));

            // If device is -1, meaning that actuator got disconncted
            else
            {
                // Try to get another actuator in context, the first one found in the list
                if (Actuators.Count > 0)
                    ChangeContext(Actuators.List[0].DeviceID);

                // Await actuator connection
                else
                { }
            }
        }

        /// <summary>
        /// Deprecated. Thread used to check for the disconnection of actuators during runtime
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        // TO DO - CHANGE
        void bgWorkerPollForActuatorsChanged(object obj, ProgressChangedEventArgs e)
        {
            Actuators.EnumerateActuators();

            // If new actuator detected
            if (Actuators.Count < Actuators.GetDeviceCountNoOverwrite())
            {
                //gWorkerErrorPolling.CancelAsync();

                Actuators = new Actuators();
                Actuators.InitializeActuators();

                API.open_device(API.get_device_name(API.enumerate_devices(0, ""), 0));

                Actuators.EnumerateActuators();

                // Update new device count and get list index value
                int newIndex = Actuators.GetDeviceCount() - 1;

                // Add to list new actuator
                Actuators.List.Add(new ActuatorController(newIndex));

                // Get new actuator's name
                Actuators.List[newIndex].DeviceName = Actuators.List[newIndex].GetDeviceName(Actuators.ActuatorEnumeration, newIndex);

                // Get new actuator's ID & open
                Actuators.List[newIndex].DeviceID = Actuators.List[newIndex].OpenDevice(Actuators.List[newIndex].DeviceName);

                // Get all of its info
                Actuators.List[newIndex].GetActuatorInformation(Actuators.List[newIndex].DeviceID);

                // Close new actuator
                Actuators.List[newIndex].CloseDevice(ref newIndex);

                //actuators.ProcessNewActuator();

                //bgWorkerErrorPolling.RunWorkerAsync();
                // Update UI somehow
            }
        }

        /// <summary>
        /// Used for setting the currently controllable actuator <br/>
        ///As only one actuator can be commanded at any given time (see Controller.ActuatorController.OpenDevice())
        /// this method provides a techinque to give to the user the impression that he can have control of all
        /// of the actuators at the same time. When the user issues a command on the interface, the software registers to which
        /// actuator the command is issued, closes all connections to other actuators using Controller.MainController.CloseAllActuators(),
        /// opens with Controller.ActuatorController.OpenDevice() only the device needed and executes the command. This implies a level
        /// of transparency, the user not needing to know this happens in the background
        /// </summary>
        /// <param name="listIndex">identifies the index of the actuator (Entities.Actuator) in the actuator list, of type int </param>
        public void ChangeContext(int listIndex = 0)
        {
            // Engage thread lock
            Monitor.Enter(_lockerEnum);

            if (isFirstTimeLaunch == true)
            {
                Actuators.List[listIndex].OpenDevice(Actuators.List[listIndex].DeviceName);
                ActuatorInContext = Actuators.List[listIndex];
                isFirstTimeLaunch = false;
                logController.LogActuatorInfo(Enums.ActuatorLog.NewActuatorInContext, ActuatorInContext);
            }
            else

            // Only change the context if the new device is different from the one already opened
            if (Actuators.List[listIndex].DeviceID != ActuatorInContext.DeviceID)
            {
                CloseAllActuators();

                Actuators.List[listIndex].OpenDevice(Actuators.List[listIndex].DeviceName);

                // Set the new actuator in context (which is currently in use by the controller)
                ActuatorInContext = Actuators.List[listIndex];
                logController.LogActuatorInfo(Enums.ActuatorLog.NewActuatorInContext, ActuatorInContext);
            }

            // Disengage thread lock
            Monitor.Exit(_lockerEnum);
        }

        /// <summary>
        /// Used for setting the currently controllable actuator by its assigned axis of motion<br/>
        /// See Controller.MainController.ChangeContext() for addition information
        /// </summary>
        /// <param name="axis">identifies assigned axis of an actuator, of type Entities.Enums.Axis</param>
        public void ChangeContextByAxis(Enums.Axis axis)
        {
            // Engage thread lock
            Monitor.Enter(_lockerEnum);

            int listIndex = -1;

            CloseAllActuators();

            foreach (ActuatorController actuator in Actuators.List)
                if (actuator.Axis == axis)
                    listIndex = actuator.ListIndex;

            Actuators.List[listIndex].OpenDevice(Actuators.List[listIndex].DeviceName);

            ActuatorInContext = Actuators.List[listIndex];

            // Disengage thread lock
            Monitor.Exit(_lockerEnum);
        }

        public int GetActuatorPositionByAxis(Enums.Axis axis)
        {
            switch (axis)
            {
                case Enums.Axis.X:
                    ChangeContextByAxis(Enums.Axis.X);
                    break;

                case Enums.Axis.Y:
                    ChangeContextByAxis(Enums.Axis.Y);
                    break;
            }

            return ActuatorInContext.GetCurrentPositionSteps(ActuatorInContext.DeviceID);
        }

        /// <summary>
        /// Iterates through all the connected actuators, and calls Controller.ActuatorController.CloseDevice() for each
        /// </summary>
        public void CloseAllActuators()
        {
            Random rand = new Random();

            for (int i = 0; i < Actuators.Count; i++)
            {
                int refID = Actuators.List[i].DeviceID;

                // Very dirty
                Actuators.List[i].DeviceID = rand.Next(-999999999, 999999999);

                Actuators.List[i].CloseDevice(ref refID);
            }
        }

        /// <summary>
        /// Closes the connection to an actuator <br/>
        /// Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.CloseDevice()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void ActuatorCloseDevice(int deviceID)
        {
            ActuatorInContext.CloseDevice(ref deviceID);
        }

        /// <summary>
        /// Hazardous. Calibrates the position of connected actuators <br/>
        /// Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.CalibrateMovement()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <returns></returns>
        public List<int> ActuatorCalibrateMovement(int deviceID)
        {
            return ActuatorInContext.CalibrateMovement(deviceID);
        }

        /// <summary>
        /// Moves the actuator to its home position <br/>
        /// Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveToHomePosition()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void MoveHome(int deviceID)
        {
            if (ActuatorInContext.MoveToHomePosition(deviceID) == Result.ok)
            {
                //Task asyncTask = Task.Run(() =>
                //{
                //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                //        logController.LogActuatorInfo(Enums.ActuatorLog.MoveHome, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                //});
            }
        }

        /// <summary>
        /// Moves the actuator to its home position <br/>
        /// Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveToHomePosition()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void ActuatorMoveHome(int deviceID)
        {
            if (ActuatorInContext.MoveToHomePosition(deviceID) == Result.ok)
                if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                {
                    //Task asyncTask = Task.Run(() =>
                    //{
                    //    logController.LogActuatorInfo(Enums.ActuatorLog.MoveHome, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                    //});
                }
        }

        /// <summary>
        /// Move the actuator continuously to the left (retracting motion) <br/>
        ///Note that Controller.ActuatorController.MoveContinuouslyLeft() is not called because it is marked as unsafe <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveToPosition()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="minEdgeSoftware">represents minimum steps value to which an actuator can move, refers to 
        /// View.ActuatorPositionSoftwareLimits.MinEdgePositionStepsAllDevices, of type int</param>
        // MoveContinuouslyLeft is unsafe! Using MoveToPosition instead
        public void ActuatorMoveContinuouslyLeft(int deviceID, int minEdgeSoftware)
        {
            if (ActuatorInContext.MoveToPosition(deviceID, minEdgeSoftware, 0) == Result.ok)
            {
                //Task asyncTask = Task.Run(() =>
                //{
                //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                //        logController.LogActuatorInfo(Enums.ActuatorLog.MoveToPosition, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                //});
            }
        }

        /// <summary>
        /// Moves actuator to a specified position in steps and microsteps <br/>
        /// Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveToPosition() <br/>
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="positionSteps">absolute value in actuator steps, of type int</param>
        /// <param name="positionMicrosteps">absolute value in actuator microsteps, of type int</param>
        public void ActuatorMoveToPosition(int deviceID, int positionSteps, int positionMicrosteps)
        {
            if (ActuatorInContext.MoveToPosition(deviceID, positionSteps, positionMicrosteps) == Result.ok)
            {
                //Task asyncTask = Task.Run(() =>
                //{
                //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                //        logController.LogActuatorInfo(Enums.ActuatorLog.MoveToPosition, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                //});
            }
        }

        /// <summary>
        /// Moves the actuator by a specified delta in steps and microsteps <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveRelatively()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="deltaSteps">relative actuator steps value, of type int</param>
        /// <param name="deltaMicrosteps">relative actuator microsteps value, of type int</param>
        public void ActuatorMoveRelatively(int deviceID, int deltaSteps, int deltaMicrosteps)
        {
            if (ActuatorInContext.MoveRelatively(deviceID, deltaSteps, deltaMicrosteps) == Result.ok)
            {
                //Task asyncTask = Task.Run(() =>
                //{
                //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                //        logController.LogActuatorInfo(Enums.ActuatorLog.MoveRelatively, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                //});
            }
        }

        /// <summary>
        /// Powers off selected actuator  <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.PowerOff()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void ActuatorPowerOff(int deviceID)
        {
            ActuatorInContext.PowerOff(deviceID);
        }

        /// <summary>
        /// Move the actuator continuously to the right (expanding motion) <br/>
        ///Note that Controller.ActuatorController.MoveContinuouslyRight() is not called because it is marked as unsafe <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.MoveToPosition()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        /// <param name="maxEdgeSoftware">represents maximum steps value to which an actuator can move, refers to 
        /// View.ActuatorPositionSoftwareLimits.MaxEdgePositionStepsAllDevices, of type int</param>
        // MoveContinuouslyRight is unsafe! Using MoveToPosition instead
        public void ActuatorMoveContinuouslyRight(int deviceID, int maxEdgeSoftware)
        {
            if (ActuatorInContext.MoveToPosition(deviceID, maxEdgeSoftware, 0) == Result.ok)
            {
                //Task asyncTask = Task.Run(() =>
                //{
                //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                //        logController.LogActuatorInfo(Enums.ActuatorLog.MoveToPosition, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                //});
            }
        }

        /// <summary>
        /// Performs a decelearing stopping of the actuator <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.SoftStop()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void ActuatorSoftStop(int deviceID)
        {
            if (ActuatorInContext.GetStatus(ActuatorInContext.DeviceID).CurSpeed != 0 || ActuatorInContext.GetStatus(ActuatorInContext.DeviceID).uCurSpeed != 0)
                if (ActuatorInContext.SoftStop(deviceID) == Result.ok)
                {
                    //Task asyncTask = Task.Run(() =>
                    //{
                    //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                    //        logController.LogActuatorInfo(Enums.ActuatorLog.SoftStop, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                    //});
                }
        }

        /// <summary>
        /// Performs an instant stop of the actuator <br/>
        ///Not recommended because of potential hardware stress. Consider using Controller.MainController.ActuatorSoftStop() instead <br/>
        ///Higher level wrapper method, called from the user interface. Calls Controller.ActuatorController.HardStop()
        /// </summary>
        /// <param name="deviceID">identifies actuator, of type int</param>
        public void ActuatorHardStop(int deviceID)
        {
            if (ActuatorInContext.GetStatus(ActuatorInContext.DeviceID).CurSpeed != 0 || ActuatorInContext.GetStatus(ActuatorInContext.DeviceID).uCurSpeed != 0)
                if (ActuatorInContext.HardStop(deviceID) == Result.ok)
                {
                    //Task asyncTask = Task.Run(() =>
                    //{
                    //    if (ActuatorInContext.WaitForStopWhile(deviceID, 0) == Result.ok)
                    //        logController.LogActuatorInfo(Enums.ActuatorLog.HardStop, ActuatorInContext, Enums.ActuatorLogTiming.Finish);
                    //});
                }
        }

        public void ActuatorSetZero(int deviceID)
        {
            ActuatorInContext.SetZeroPosition(deviceID);
        }

        public int GetMaxStepsPosFromAllActuators()
        {
            return GetMinMaxPosFromAll(Enums.CtrlEgdeType.Max, Enums.StepsType.Steps);
        }

        public int GetMinStepsPosFromAllActuators()
        {
            return GetMinMaxPosFromAll(Enums.CtrlEgdeType.Min, Enums.StepsType.Steps);
        }

        public int GetMaxMicroStepsPosFromAllActuators()
        {
            return GetMinMaxPosFromAll(Enums.CtrlEgdeType.Max, Enums.StepsType.MicroSteps);
        }

        public int GetMinMicroStepsPosFromAllActuators()
        {
            return GetMinMaxPosFromAll(Enums.CtrlEgdeType.Min, Enums.StepsType.MicroSteps);
        }

        private int GetMinMaxPosFromAll(Enums.CtrlEgdeType ctrlEgdeType, Enums.StepsType stepsType)
        {
            int retValue = 0, pos = 0;
            int min = int.MaxValue;
            int max = int.MinValue;

            switch (ctrlEgdeType)
            {
                case Enums.CtrlEgdeType.Min:
                    for (int i = 0; i < Actuators.Count; i++)
                    {
                        ChangeContext(i);

                        if (stepsType == Enums.StepsType.Steps)
                            pos = ActuatorInContext.GetCurrentPositionSteps(ActuatorInContext.DeviceID);
                        else if (stepsType == Enums.StepsType.MicroSteps)
                            pos = ActuatorInContext.GetCurrentPositionMicroSteps(ActuatorInContext.DeviceID);

                        if (min > pos)
                            min = pos;
                    }
                    retValue = pos;
                    break;

                case Enums.CtrlEgdeType.Max:
                    for (int i = 0; i < Actuators.Count; i++)
                    {
                        ChangeContext(i);

                        if (stepsType == Enums.StepsType.Steps)
                            pos = ActuatorInContext.GetCurrentPositionSteps(ActuatorInContext.DeviceID);
                        else if (stepsType == Enums.StepsType.MicroSteps)
                            pos = ActuatorInContext.GetCurrentPositionMicroSteps(ActuatorInContext.DeviceID);

                        if (pos > max)
                            max = pos;
                    }
                    retValue = pos;
                    break;

                default:
                    break;
            }

            return retValue;
        }

        public bool IsActuatorMoving(int listIndex)
        {
            if (Actuators.List[listIndex].GetCurrentSpeedSteps(Actuators.List[listIndex].DeviceID) == 0 && Actuators.List[listIndex].GetCurrentSpeedMicrosteps(Actuators.List[listIndex].DeviceID) == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <returns></returns>
        // TO DELETE
        public bool HasActuatorCountReduced()
        {
            Actuators.EnumerateActuators(true);

            if (Actuators.Count > Actuators.GetDeviceCountNoOverwrite())
                return true;
            else return false;
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <returns></returns>
        // TO DELETE
        public int GetNewActuatorCount()
        {
            return Actuators.GetDeviceCount();
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        // TO DELETE
        public void DisconnectActuator()
        {
            Actuators.RemoveActuatorFromList();
            ActuatorInContext.DeviceID = -1;
        }

        /// <summary>
        /// Calls Controller.Actuators.InitializeActuators() from constructor Controller.MainController() in order to initialize actuators
        /// </summary>
        public void InitActuators()
        {
            Actuators.InitializeActuators();

            if (Actuators.Count > 0)
                ChangeContext();
            else
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.NoDevicesFound);
        }

        /// <summary>
        /// Initializes the background workers (threads)
        /// </summary>
        public void InitBgWorkers()
        {
            bgWorkerErrorPolling = new BackgroundWorker();
            bgWorkerPollForActuators = new BackgroundWorker();

            bgWorkerErrorPolling.DoWork += new DoWorkEventHandler(bgWorkerErrorPollingDoWork);
            bgWorkerErrorPolling.ProgressChanged += new ProgressChangedEventHandler(bgWorkerErrorPollingProgressChanged);
            bgWorkerErrorPolling.WorkerSupportsCancellation = true;
            bgWorkerErrorPolling.WorkerReportsProgress = true;

            bgWorkerPollForActuators.DoWork += new DoWorkEventHandler(bgWorkerPollForActuatorsDoWork);
            bgWorkerPollForActuators.ProgressChanged += new ProgressChangedEventHandler(bgWorkerPollForActuatorsChanged);
            bgWorkerPollForActuators.WorkerSupportsCancellation = true;
            bgWorkerPollForActuators.WorkerReportsProgress = true;

            //bgWorkerErrorPolling.RunWorkerAsync();
            //bgWorkerPollForActuators.RunWorkerAsync();
        }

        /// <summary>
        /// Actuator currently in context, getter and setter
        /// </summary>
        public ActuatorController ActuatorInContext { get => actuatorInContext; set => actuatorInContext = value; }

        /// <summary>
        /// Actuators object property, getter and setter
        /// </summary>
        public Actuators Actuators { get => actuators; set => actuators = value; }

        /// <summary>
        /// Notification list, getter and setter
        /// </summary>
        public List<Notification> NotifList { get => notifList; set => notifList = value; }
    }
}
