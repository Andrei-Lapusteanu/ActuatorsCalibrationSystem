using Entities;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace View
{
    public class ActuatorPositionSoftwareLimits
    {
        private ApslLimitsBundle apslLimBundle;
        private ApslLimitsBundle copyApslLimBundle;
        private int minEdgePositionStepsAllDevices;
        private int maxEdgePositionStepsAllDevices;
        private int homePositionStepsAllDevices;
        private int minPositionMicroStepsAllDevices;
        private int maxPositionMicroStepsAllDevices;

        public ActuatorPositionSoftwareLimits()
        {
            this.apslLimBundle = new ApslLimitsBundle();
            this.copyApslLimBundle = new ApslLimitsBundle();
            this.minEdgePositionStepsAllDevices = new int();
            this.maxEdgePositionStepsAllDevices = new int();
            this.homePositionStepsAllDevices = new int();
            this.minPositionMicroStepsAllDevices = new int();
            this.maxEdgePositionStepsAllDevices = new int();
        }

        public ActuatorPositionSoftwareLimits(ApslLimitsBundle apslLimBundle, int minEdgePositionStepsAllDevices, int maxEdgePositionStepsAllDevices, int homePositionStepsAllDevices)
        {
            this.apslLimBundle = apslLimBundle;
            this.MinEdgePositionStepsAllDevices = minEdgePositionStepsAllDevices;
            this.MaxEdgePositionStepsAllDevices = maxEdgePositionStepsAllDevices;
            this.HomePositionStepsAllDevices = homePositionStepsAllDevices;
            this.MinPositionMicroStepsAllDevices = -255;
            this.MaxPositionMicroStepsAllDevices = 255;
        }

        public void SetGlobalEdgeLimits()
        {
            // Create a copy of the original axis egde limits values
            copyApslLimBundle = apslLimBundle.DeepClone<ApslLimitsBundle>();

            apslLimBundle.Axis_X_MinEdgeStepsPosition =
            apslLimBundle.Axis_Y_MinEdgeStepsPosition =
            apslLimBundle.Axis_Z_MinEdgeStepsPosition =
            apslLimBundle.GlobalMinEdgeStepsPosition;

            apslLimBundle.Axis_X_MaxEdgeStepsPosition =
            apslLimBundle.Axis_Y_MaxEdgeStepsPosition =
            apslLimBundle.Axis_Z_MaxEdgeStepsPosition =
            apslLimBundle.GlobalMaxEdgeStepsPosition;
        }

        public void SetEdgeLimitsPerAxis()
        {
            // Copy back the original values
            if (copyApslLimBundle != null)
                apslLimBundle = copyApslLimBundle;
        }

        public int GetActEdgeVal(Enums.Axis axis, Enums.ApslEdgeType edgeType)
        {
            int value = 0;

            switch (axis)
            {
                case Enums.Axis.X:
                    if (edgeType == Enums.ApslEdgeType.Min)
                        value = apslLimBundle.Axis_X_MinEdgeStepsPosition;//GetActEdgeValByAxis(Enums.ApslAxis.Axis_X_MinEdgePos);

                    else if (edgeType == Enums.ApslEdgeType.Max)
                        value = apslLimBundle.Axis_X_MaxEdgeStepsPosition; //GetActEdgeValByAxis(Enums.ApslAxis.Axis_X_MaxEdgePos);
                    break;

                case Enums.Axis.Y:
                    if (edgeType == Enums.ApslEdgeType.Min)
                        value = apslLimBundle.Axis_Y_MinEdgeStepsPosition; //GetActEdgeValByAxis(Enums.ApslAxis.Axis_Y_MinEdgePos);

                    else if (edgeType == Enums.ApslEdgeType.Max)
                        value = apslLimBundle.Axis_Y_MaxEdgeStepsPosition; //GetActEdgeValByAxis(Enums.ApslAxis.Axis_Y_MaxEdgePos);
                    break;

                case Enums.Axis.Z:
                    if (edgeType == Enums.ApslEdgeType.Min)
                        value = apslLimBundle.Axis_Z_MinEdgeStepsPosition; //GetActEdgeValByAxis(Enums.ApslAxis.Axis_Z_MinEdgePos);

                    else if (edgeType == Enums.ApslEdgeType.Max)
                        value = apslLimBundle.Axis_Z_MaxEdgeStepsPosition; //GetActEdgeValByAxis(Enums.ApslAxis.Axis_Z_MaxEdgePos);
                    break;

                case Enums.Axis.DontCare:
                    value = GetEdgeForDontCareAxis(edgeType);
                    break;
            }

            return value;
        }

        private int GetEdgeForDontCareAxis(Enums.ApslEdgeType edgeType)
        {
            int value = 0, temp = 0;

            switch (edgeType)
            {
                case Enums.ApslEdgeType.Min:
                    temp = Math.Max(ApslLimBundle.Axis_X_MinEdgeStepsPosition, ApslLimBundle.Axis_Y_MinEdgeStepsPosition);
                    value = Math.Max(temp, ApslLimBundle.Axis_Z_MinEdgeStepsPosition);
                    break;

                case Enums.ApslEdgeType.Max:
                    temp = Math.Min(ApslLimBundle.Axis_X_MaxEdgeStepsPosition, ApslLimBundle.Axis_Y_MaxEdgeStepsPosition);
                    value = Math.Min(temp, ApslLimBundle.Axis_Z_MaxEdgeStepsPosition);
                    break;

                default:
                    break;
            }

            return value;
        }

        // Patch, ugly
        int thisMinEdge = 12345;
        int thisMaxEdge = 12345;
        public void SaveDeviceOneTextBoxValues(int minEdge, int maxEdge)
        {
            thisMinEdge = minEdge;
            thisMaxEdge = maxEdge;
        }

        // Patch, ugly
        public string GetDeviceOneMinEgde()
        {
            return thisMinEdge.ToString();
        }

        // Patch, ugly
        public string GetDeviceOneMaxEgde()
        {
            return thisMaxEdge.ToString();
        }

        public ApslLimitsBundle ApslLimBundle { get => apslLimBundle; set => apslLimBundle = value; }
        public int MinEdgePositionStepsAllDevices { get => minEdgePositionStepsAllDevices; set => minEdgePositionStepsAllDevices = value; }
        public int MaxEdgePositionStepsAllDevices { get => maxEdgePositionStepsAllDevices; set => maxEdgePositionStepsAllDevices = value; }
        public int HomePositionStepsAllDevices { get => homePositionStepsAllDevices; set => homePositionStepsAllDevices = value; }
        public int MinPositionMicroStepsAllDevices { get => minPositionMicroStepsAllDevices; set => minPositionMicroStepsAllDevices = value; }
        public int MaxPositionMicroStepsAllDevices { get => maxPositionMicroStepsAllDevices; set => maxPositionMicroStepsAllDevices = value; }
    }
}

public static class ExtensionMethods
{
    // Deep clone
    public static T DeepClone<T>(this T a)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }
}