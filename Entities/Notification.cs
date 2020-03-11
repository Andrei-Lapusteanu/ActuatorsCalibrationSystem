using System.Collections.Generic;

namespace Entities
{
    /// <summary>
    /// This class contains members which mode the structure of a notification on the
    /// user interface and its message. Contains a notification list to keep a history
    /// of past events
    /// </summary>
    public class Notification
    {
        private Enums.NotificationType notifType;
        private string notifString;
        private bool wasDisplayed;
        static List<Notification> notifList;

        /// <summary>
        /// Static constructor
        /// </summary>
        static Notification()
        {
            notifList = new List<Notification>();
        }

        /// <summary>
        /// Impicit constructor
        /// </summary>
        public Notification()
        {
            this.NotifType = new Enums.NotificationType();
            this.NotifString = "";
            this.WasDisplayed = false;
            notifList.Add(this);
        }

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="notifType">identifies notification type, of type Entities.Enums.NotificationType</param>
        /// <param name="notifString">notification message, of type string</param>
        public Notification(Enums.NotificationType notifType, string notifString)
        {
            this.notifType = notifType;
            this.notifString = notifString;
            this.wasDisplayed = false;
        }

        /// <summary>
        /// Notification type, getter and settter
        /// </summary>
        public Enums.NotificationType NotifType { get => notifType; set => notifType = value; }

        /// <summary>
        /// Notification string, getter and setter
        /// </summary>
        public string NotifString { get => notifString; set => notifString = value; }

        /// <summary>
        /// Was notification displayed, getter and setters
        /// </summary>
        public bool WasDisplayed { get => wasDisplayed; set => wasDisplayed = value; }

        /// <summary>
        /// Static notification list, getter and setter
        /// </summary>
        public static List<Notification> NotifList { get => notifList; set => notifList = value; }
    }
}
