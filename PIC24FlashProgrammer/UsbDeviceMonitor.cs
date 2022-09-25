using System.Management;

namespace PIC24FlashProgrammer
{
    internal class UsbDeviceMonitor
    {
        public event EventHandler<UsbDeviceMonitorArgs>? ConnectionChanged;
        private ManagementEventWatcher? arriveEvent;
        private ManagementEventWatcher? removeEvent;

        public void Start()
        {
            try
            {
                this.arriveEvent = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'"));
                this.arriveEvent.EventArrived += OnDeviceConnected;
                this.arriveEvent.Start();
                this.removeEvent = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'"));
                this.removeEvent.EventArrived += OnDeviceDisconnected;
                this.removeEvent.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private void OnDeviceConnected(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var usbDevice = GetUsbDevice((ManagementBaseObject)e.NewEvent["TargetInstance"]);
                NotifyConnectionChanged(usbDevice);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void OnDeviceDisconnected(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var usbDevice = GetUsbDevice((ManagementBaseObject)e.NewEvent["TargetInstance"]);
                NotifyConnectionChanged(usbDevice);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void NotifyConnectionChanged(UsbDevice? usbDevice)
        {
            if (usbDevice != null)
            {
                this.ConnectionChanged?.Invoke(this, new UsbDeviceMonitorArgs(usbDevice));
            }
        }

        private static UsbDevice? GetUsbDevice(ManagementBaseObject instance, bool connected = false)
        {
            try
            {
                var deviceId = (string)instance.GetPropertyValue("DeviceID");
                var description = (string)instance.GetPropertyValue("Description");
                var caption = (string)instance.GetPropertyValue("Caption");
                Logger.Debug($"USB {(connected ? "C" : "Disc")}onnect: {description}, {deviceId}");
                return new UsbDevice(deviceId, description, caption, connected);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return null;
        }
    }

    internal class UsbDeviceMonitorArgs : EventArgs
    {
        public UsbDevice Device { get; }

        public UsbDeviceMonitorArgs(UsbDevice device)
        {
            Device = device;
        }
    }
}
