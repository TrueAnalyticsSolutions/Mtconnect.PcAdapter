using Mtconnect.AdapterInterface.DataItems;
using System;
using System.Drawing;
using System.Threading;

namespace Mtconnect.PCAdapter
{
    /// <summary>
    /// Contains an internal timer that periodically gets the current mouse position and active window title then publishes the values to an Adapter.
    /// </summary>
    public class PCAdapterSource : IAdapterSource, IDisposable
    {
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
            Model.Availability = "AVAILABLE";


            try
            {
                Point lpPoint;
                if (WindowHandles.GetCursorPos(out lpPoint))
                {
                    Model.XPosition = lpPoint.X;
                    Model.XPosition_Time = new DateTime(2002, 01, 01); // Birthdate of C#
                    Model.YPosition = lpPoint.Y;
                }
                else
                {
                    Model.XPosition = null;
                    Model.XPosition_Time = null;
                    Model.YPosition = null;
                }

                try
                {
                    string activeWindowTitle = WindowHandles.GetActiveWindowTitle();
                    if (!string.IsNullOrEmpty(activeWindowTitle))
                    {
                        Model.WindowTitle = activeWindowTitle;
                    }
                    else
                    {
                        Model.WindowTitle = null;
                    }
                }
                catch (Exception ex)
                {
                    Model.WindowTitle = null;
                }

                try
                {
                    WindowHandles.SystemPower.SystemPowerStatus sps = new WindowHandles.SystemPower.SystemPowerStatus();
                    WindowHandles.SystemPower.GetSystemPowerStatus(out sps);
                    if (sps.flgBattery == WindowHandles.SystemPower.BatteryFlag.Unknown || sps.flgBattery == WindowHandles.SystemPower.BatteryFlag.NoSystemBattery)
                    {
                        Model.BatteryCondition.Add(Condition.Level.WARNING, sps.flgBattery.ToString(), ((int)sps.flgBattery).ToString(), string.Empty, string.Empty);
                        Model.BatteryRemaining = null;
                    }
                    else
                    {
                        Model.BatteryCondition.Normal();
                        Model.BatteryRemaining = (int)sps.BatteryLifePercent;
                    }


                    if (sps.LineStatus == WindowHandles.SystemPower.ACLineStatus.Unknown)
                    {
                        Model.ACCondition.Add(Condition.Level.WARNING, sps.LineStatus.ToString(), ((int)sps.LineStatus).ToString(), string.Empty, string.Empty);
                        Model.ACConnected = null;
                    }
                    else
                    {
                        Model.ACCondition.Normal();
                        Model.ACConnected = sps.LineStatus == WindowHandles.SystemPower.ACLineStatus.Online;
                    }
                }
                catch (Exception ex)
                {
                    Model.BatteryCondition.Add(Condition.Level.FAULT, ex.Message, ex.TargetSite.Name, string.Empty, string.Empty);
                    Model.ACCondition.Add(Condition.Level.FAULT, ex.Message, ex.TargetSite.Name, string.Empty, string.Empty);
                }

                // Comment out for testing * DATAITEM_VALUE foobar
                if (_loopCount > 5000 / Timer.Interval)
                {
                    Model.FooBar = "foobar";
                }

                Model.SystemAccess.Normal();
            }
            catch (Exception ex)
            {
                Model.SystemAccess.Add(Condition.Level.FAULT, ex.Message, "access");
            }

            OnDataReceived?.Invoke(Model, new DataReceivedEventArgs());
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

        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
