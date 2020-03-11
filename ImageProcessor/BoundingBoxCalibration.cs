using Entities;

namespace ImageProcessor
{
    public class ActuatorPos
    {
        private int x;
        private int y;

        public ActuatorPos()
        {
            this.X = new int();
            this.Y = new int();
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
    }

    public class PixelPos
    {
        private int x;
        private int y;

        public PixelPos()
        {
            this.X = new int();
            this.Y = new int();
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
    }

    public class BoundingBoxCalibration
    {
        private ActuatorPos topLeftActuatorPos;
        private ActuatorPos bottomRightActuatorPos;
        private PixelPos topLeftPixelPos;
        private PixelPos bottomRightPixelPos;
        private bool hasTopLeftBeenSet;
        private bool hasBottomRightBeenSet;
        private bool hasBoundingBoxBeenSet;

        public BoundingBoxCalibration()
        {
            this.topLeftActuatorPos = new ActuatorPos();
            this.bottomRightActuatorPos = new ActuatorPos();
            this.topLeftPixelPos = new PixelPos();
            this.bottomRightPixelPos = new PixelPos();

            this.hasTopLeftBeenSet = false;
            this.hasBottomRightBeenSet = false;
            this.hasBoundingBoxBeenSet = false;
        }

        public void SetTopLeft(System.Drawing.Point pixelPostion, int actuator_X, int actuator_Y)
        {
            this.topLeftPixelPos = new PixelPos() { X = pixelPostion.X, Y = pixelPostion.Y };
            this.topLeftActuatorPos = new ActuatorPos() { X = actuator_X, Y = actuator_Y };

            this.hasTopLeftBeenSet = true;
            SetBoundingBoxBool();
        }

        public void SetBottomRight(System.Drawing.Point pixelPostion, int actuator_X, int actuator_Y)
        {
            this.bottomRightPixelPos = new PixelPos() { X = pixelPostion.X, Y = pixelPostion.Y };
            this.bottomRightActuatorPos = new ActuatorPos() { X = actuator_X, Y = actuator_Y };

            this.hasBottomRightBeenSet = true;
            SetBoundingBoxBool();
        }


        public void ResetBoundingBox()
        {
            this.topLeftActuatorPos = new ActuatorPos();
            this.bottomRightActuatorPos = new ActuatorPos();
            this.topLeftPixelPos = new PixelPos();
            this.bottomRightPixelPos = new PixelPos();

            this.hasTopLeftBeenSet = false;
            this.hasBottomRightBeenSet = false;
            this.hasBoundingBoxBeenSet = false;
        }

        private void SetBoundingBoxBool()
        {
            if (this.hasTopLeftBeenSet == true || this.hasBottomRightBeenSet == true)
            {
                if (Notification.NotifList[Notification.NotifList.Count - 2].NotifString != "Manual edge setting disabled" &&
                    Notification.NotifList[Notification.NotifList.Count - 1].NotifString != "Reset bounding box calibration to enable it")
                {
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Warning, "Manual edge setting disabled"));
                    Notification.NotifList.Add(new Notification(Enums.NotificationType.Info, "Reset bounding box calibration to enable it"));
                }
            }

            if (this.hasTopLeftBeenSet == true && this.hasBottomRightBeenSet == true)
                this.hasBoundingBoxBeenSet = true;
            else
                this.hasBoundingBoxBeenSet = false;
        }

        public ActuatorPos TopLeftActuatorPos { get => topLeftActuatorPos; set => topLeftActuatorPos = value; }
        public ActuatorPos BottomRightActuatorPos { get => bottomRightActuatorPos; set => bottomRightActuatorPos = value; }
        public PixelPos TopLeftPixelPos { get => topLeftPixelPos; set => topLeftPixelPos = value; }
        public PixelPos BottomRightPixelPos { get => bottomRightPixelPos; set => bottomRightPixelPos = value; }
        public bool HasTopLeftBeenSet { get => hasTopLeftBeenSet; set => hasTopLeftBeenSet = value; }
        public bool HasBottomRightBeenSet { get => hasBottomRightBeenSet; set => hasBottomRightBeenSet = value; }
        public bool HasBoundingBoxBeenSet { get => hasBoundingBoxBeenSet; set => hasBoundingBoxBeenSet = value; }
    }
}
