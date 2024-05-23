namespace ToolkitAPI.Device {
    //REVIEW: [CRT-4039] Review the name of this. We need a standard term for a template containing both "Calibration" and default "QuiltSettings".
    //  It was loosely equivalent as DeviceSettings (struct) previously in the UnityPlugin
    public class LKGDeviceTemplate {
        public Calibration calibration;
        public QuiltSettings defaultQuilt;

        public LKGDeviceTemplate(Calibration calibration, QuiltSettings defaultQuilt) {
            this.calibration = calibration;
            this.defaultQuilt = defaultQuilt;
        }
    }
}
