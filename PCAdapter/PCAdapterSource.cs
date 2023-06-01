using Mtconnect.AdapterSdk.DataItems;
using Mtconnect.AdapterSdk.DataItemValues;
using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mtconnect.PCAdapter
{
    /// <summary>
    /// Contains an internal timer that periodically gets the current mouse position and active window title then publishes the values to an Adapter.
    /// </summary>
    public class PCAdapterSource : IAdapterSource, IDisposable
    {
        private string _deviceUuid;
        public string DeviceUuid => _deviceUuid ?? (_deviceUuid = GenerateUUID(DeviceName));

        public string DeviceName => Environment.MachineName;

        public string StationId => Environment.MachineName;

        public string SerialNumber => throw new NotImplementedException();

        public string Manufacturer => throw new NotImplementedException();

        /// <inheritdoc />
        public event DataReceivedHandler OnDataReceived;
        /// <inheritdoc />
        public event AdapterSourceStartedHandler OnAdapterSourceStarted;
        /// <inheritdoc />
        public event AdapterSourceStoppedHandler OnAdapterSourceStopped;

        /// TODO: Change the type to your data model or implement some other form of managing a IAdapterDataModel
        public PCModel Model { get; private set; } = new PCModel();

        private int _loopCount = 0;
        private System.Timers.Timer Timer = new System.Timers.Timer();

        /// <summary>
        /// Constructs a new instance of the PC monitor.
        /// </summary>
        /// <param name="sampleRate">The frequency for which the current states of the PC are collected (in milliseconds).</param>
        public PCAdapterSource(int sampleRate = 50)
        {
            // NOTE: You MUST have at least one constructor with a signature containing ONLY primitive types.

            Timer.Interval = sampleRate;
            Timer.Elapsed += Timer_Elapsed;
        }

        // NOTE: This could be tied to a custom egress event, an asynchronous loop, or a timer.
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Model.Availability = Availability.AVAILABLE;


            try
            {
                Point lpPoint;
                if (WindowHandles.GetCursorPos(out lpPoint))
                {
                    Model.Mouse.X.ActualPosition = lpPoint.X;
                    //Model.Mouse.X.ActualPosition_Time = new DateTime(2002, 01, 01); // Birthdate of C#
                    Model.Mouse.Y.ActualPosition = lpPoint.Y;
                }
                else
                {
                    Model.Mouse?.X?.ActualPosition?.Unavailable();
                    Model.Mouse?.Y?.ActualPosition?.Unavailable();
                }

                try
                {
                    string activeWindowTitle = WindowHandles.GetActiveWindowTitle();
                    if (!string.IsNullOrEmpty(activeWindowTitle))
                    {
                        Model.Controller.Path.WindowTitle = activeWindowTitle;
                    }
                    else
                    {
                        Model.Controller.Path.WindowTitle?.Unavailable();
                    }
                }
                catch (Exception ex)
                {
                    Model.Controller.Path.WindowTitle?.Unavailable();
                }

                try
                {
                    WindowHandles.SystemPower.SystemPowerStatus sps = new WindowHandles.SystemPower.SystemPowerStatus();
                    WindowHandles.SystemPower.GetSystemPowerStatus(out sps);
                    if (sps.flgBattery == WindowHandles.SystemPower.BatteryFlag.Unknown || sps.flgBattery == WindowHandles.SystemPower.BatteryFlag.NoSystemBattery)
                    {
                        Model.Controller.Path.BatteryCondition.Add(Condition.Level.WARNING, sps.flgBattery.ToString(), ((int)sps.flgBattery).ToString(), string.Empty, string.Empty);
                        Model.Controller.Path.BatteryRemaining?.Unavailable();
                    }
                    else
                    {
                        Model.Controller.Path.BatteryCondition.Normal();
                        Model.Controller.Path.BatteryRemaining = (int)sps.BatteryLifePercent;
                    }


                    if (sps.LineStatus == WindowHandles.SystemPower.ACLineStatus.Unknown)
                    {
                        Model.Controller.Path.ACCondition.Add(Condition.Level.WARNING, sps.LineStatus.ToString(), ((int)sps.LineStatus).ToString(), string.Empty, string.Empty);
                        Model.Controller.Path.ACConnected = null;
                    }
                    else
                    {
                        Model.Controller.Path.ACCondition.Normal();
                        Model.Controller.Path.ACConnected = sps.LineStatus == WindowHandles.SystemPower.ACLineStatus.Online;
                    }
                }
                catch (Exception ex)
                {
                    Model.Controller.Path.BatteryCondition.Add(Condition.Level.FAULT, ex.Message, ex.TargetSite.Name, string.Empty, string.Empty);
                    Model.Controller.Path.ACCondition.Add(Condition.Level.FAULT, ex.Message, ex.TargetSite.Name, string.Empty, string.Empty);
                }

                Model.Controller.Path.SystemAccess.Normal();
            }
            catch (Exception ex)
            {
                Model.Controller.Path.SystemAccess.Add(Condition.Level.FAULT, ex.Message, "access");
            }

            OnDataReceived?.Invoke(this, new DataReceivedEventArgs(Model));
        }

        /// <inheritdoc />
        public void Start(CancellationToken token = default)
        {
            // NOTE: Start any timers, loops, or attach to any egress events from here.
            Timer.Start();

            OnAdapterSourceStarted?.Invoke(this, new AdapterSourceStartedEventArgs());
        }

        /// <inheritdoc />
        public void Stop(Exception ex = null)
        {
            // NOTE: Stop any timers or loops, or detatch from any egress events from here.
            Timer.Stop();

            OnAdapterSourceStopped?.Invoke(this, new AdapterSourceStoppedEventArgs(ex));
        }

        private string GenerateUUID(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Set the version (4 bits) and variant (2 bits) according to the UUID specification
                hashBytes[7] = (byte)((hashBytes[7] & 0x0F) | 0x30); // version 3 (MD5)
                hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80); // variant 1

                // Convert the hash bytes to a Guid
                return new Guid(hashBytes).ToString();
            }
        }

        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
