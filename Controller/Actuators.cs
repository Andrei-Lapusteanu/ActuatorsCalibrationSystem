using Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using ximc;

namespace Controller
{
    /// <summary>
    /// This class is responsible for detecting and initializing connected actuators
    /// </summary>
    public class Actuators
    {
        private static List<ActuatorController> list;
        private int count;
        private IntPtr actuatorEnumeration;
        private LogController logController;

        static readonly object _lockerEnum = new object();
        static readonly object _lockerGetCount = new object();

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public Actuators()
        {
            this.count = -1;
            this.ActuatorEnumeration = new IntPtr();
            logController = new LogController();
        }

        /// <summary>
        /// Contains all method calls necessary in order to properly initialize connected actuators
        /// </summary>
        public void InitializeActuators()
        {
            FreeEnumeratedActuators();
            EnumerateActuators();
            GetDeviceCount();
            AddActuatorsToList();
            GetActuatorNames();
            TryOpenActuatorsAndGetID();
            GetActuatorInformation();
            CloseActuators();
        }

        /// <summary>
        /// Free from memory enumerated actuators
        /// </summary>
        public void FreeEnumeratedActuators()
        {
            try
            {
                if (API.free_enumerate_devices(ActuatorEnumeration) == Result.ok)
                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.FreeEnumeratedActuators);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error freeing enumerated actuators. It may be that actuators have never been enumerated before -" + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.FreeEnumeratedActuators, -1, ex);
            };
        }

        /// <summary>
        /// Enumerate all valid connected actuators
        /// </summary>
        /// <param name="isErrorPolling">used for checking whether application is polling for errors, of type bool</param>
        public void EnumerateActuators(bool isErrorPolling = false)
        {
            // Code is locked for thread safety. It is possible for
            // multiple threads to access it concurrently
            try
            {
                // Engage thread lock
                Monitor.Enter(_lockerEnum);

                if ((ActuatorEnumeration = API.enumerate_devices(0, "")) != null)
                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.EnumerateActuators);

                // Disengage thread lock
                Monitor.Exit(_lockerEnum);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enumerating actuators - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.FreeEnumeratedActuators, -1, ex);
            }
        }

        /// <summary>
        /// Return device count - number of connected and valid actuators
        /// </summary>
        /// <param name="isErrorPolling">used for checking whether application is polling for errors, of type bool</param>
        /// <returns>device count, of type int</returns>
        public int GetDeviceCount(bool isErrorPolling = false)
        {
            // Code is locked for thread safety. It is possible for
            // multiple threads to access it concurrently
            try
            {
                // Engage thread lock
                Monitor.Enter(_lockerGetCount);

                Count = API.get_device_count(ActuatorEnumeration) + 3;
                if (isErrorPolling == false)
                    Logger.Log.Info("Found " + Count + " actuators connected");

                // Notify UI
                if (Count > 0)
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Found " + Count + " actuators connected"));
                else
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Found " + Count + " actuators connected"));

                // Disengage thread lock
                Monitor.Exit(_lockerGetCount);

                return Count;
            }
            catch (Exception ex)
            {
                // Notify UI
                Notification.NotifList.Add(new Notification(Enums.NotificationType.CriticalError, "Error getting device count - " + ex));

                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.GetDeviceCount, -1, ex);
                return -1;
            }
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        /// <returns></returns>
        public int GetDeviceCountNoOverwrite()
        {
            // Code is locked for thread safety. It is possible for
            // multiple threads to access it concurrently
            try
            {
                // Engage thread lock
                Monitor.Enter(_lockerGetCount);

                int count = API.get_device_count(ActuatorEnumeration) + 2;

                // Disengage thread lock
                Monitor.Exit(_lockerGetCount);

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting device count (no overwrite) - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.GetDeviceCount, -1, ex);
                return -1;
            }
        }

        /// <summary>
        /// Add a connected actuator to the list of actuators
        /// </summary>
        public void AddActuatorsToList()
        {
            try
            {
                list = new List<ActuatorController>();

                if (Count > 0)
                {
                    for (int i = 0; i < Count; i++)
                        List.Add(new ActuatorController(i));

                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.AddActuatorsToList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding actuators to list - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.AddActuatorsToList, -1, ex);
            }
        }

        /// <summary>
        /// Get actuator device names, used for printing on the user interface
        /// </summary>
        public void GetActuatorNames()
        {
            try
            {
                if (Count > 0)
                {
                    //for (int i = 0; i < Count; i++)
                    //{
                    //    //if (i == 0)
                    //    //{
                    //        List[i].DeviceName = List[i].GetDeviceName(ActuatorEnumeration, List[i].ListIndex);
                    //        List[i].UIName = List[i].DeviceName.Substring(11, 4);

                    //        // Notify UI
                    //        Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Found actuator with name " + List[i].DeviceName));
                    //    //}
                    //}

                    /* VIRTUAL */
                    List[0].DeviceName = "xi-emu:///8MT173V-30-VSS42-VIRT1.bin";
                    List[1].DeviceName = "xi-emu:///8MT173V-30-VSS42-VIRT2.bin";
                    List[2].DeviceName = "xi-emu:///8MT173V-30-VSS42-VIRT3.bin";

                    List[0].UIName = "VIRT1";
                    List[1].UIName = "VIRT2";
                    List[2].UIName = "VIRT3";

                    Result res = API.probe_device(List[0].DeviceName);

                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.GetActuatorNames);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting actuator names - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.GetActuatorNames, -1, ex);
            }
        }

        /// <summary>
        /// Open each actuator in actuator list and store corresponding device ID
        /// </summary>
        public void TryOpenActuatorsAndGetID()
        {
            try
            {
                foreach (ActuatorController actuator in List)
                {
                    actuator.DeviceID = actuator.OpenDevice(actuator.DeviceName); // asignare de full stack dev

                    // Notify UI
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Success, "Opened device with ID = " + actuator.DeviceID));
                }

                logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.TryOpenActuators);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to open actuators - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.TryOpenActuators);
            }
        }

        /// <summary>
        /// Call to Controller.ActuatorController.GetActuatorInformation()
        /// </summary>
        public void GetActuatorInformation()
        {
            try
            {
                foreach (ActuatorController actuator in List)
                {
                    actuator.GetActuatorInformation(actuator.DeviceID);
                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.GetActuatorInformation, actuator.DeviceID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to get actuator information - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.GetActuatorInformation);
            }
        }

        /// <summary>
        /// Close connections to all actuators in actuator list
        /// </summary>
        public void CloseActuators()
        {
            try
            {
                foreach (ActuatorController actuator in List)
                {
                    int refID = actuator.DeviceID;

                    if (actuator.CloseDevice(ref refID) == Result.ok)
                        logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.CloseActuators, actuator.DeviceID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to close actuator - " + ex);
                logController.LogMultipleActuatorsError(Enums.MultipleActuatorsLog.CloseActuators);
            }
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        // TO DELETE
        public void RemoveActuatorFromList()
        {
            int failIndex = -1;

            for (failIndex = 0; failIndex < List.Count; failIndex++)
                if (List[failIndex].ProbeDeviceeNoErrorHandling(List[failIndex].DeviceName) != Result.ok)
                {
                    Result res = API.command_power_off(List[failIndex].DeviceID);

                    // Remove item
                    List.RemoveAt(failIndex);

                    EnumerateActuators();

                    // Update device count
                    GetDeviceCount();

                    // Update deviceIDs just to be extra safe
                    TryOpenActuatorsAndGetID();

                    logController.LogMultipleActuatorsInfo(Enums.MultipleActuatorsLog.RemovedActuator, failIndex);
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Actuator with was released from memory and disconnected"));
                }
        }

        /// <summary>
        /// List which contains actuators, getter and setter
        /// </summary>
        public static List<ActuatorController> List { get => list; set => list = value; }

        /// <summary>
        /// Actuator count, getter and setter
        /// </summary>
        public int Count { get => count; set => count = value; }

        /// <summary>
        /// Actuator enumeration IntPtr, getter and setter
        /// </summary>
        public IntPtr ActuatorEnumeration { get => actuatorEnumeration; set => actuatorEnumeration = value; }
    }
}