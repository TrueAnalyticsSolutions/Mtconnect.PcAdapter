using Mtconnect.AdapterSdk.Contracts.Attributes;
using Mtconnect.AdapterSdk.DataItems;
using Mtconnect.AdapterSdk.DataItemValues;
using System;

namespace Mtconnect.PCAdapter
{
    public class PCModel : IAdapterDataModel
    {
        [Event("avail")]
        public Availability Availability { get; set; }

        [Sample("xPos", "user32.dll#GetCursorPos().X")]
        public Position.ACTUAL XPosition { get; set; }
        [Timestamp("xPos")]
        public DateTime? XPosition_Time { get; set; }

        [Sample("yPos", "user32.dll#GetCursorPos().Y")]
        public Position.ACTUAL YPosition { get; set; }

        [Event("prog", "user32.dll#GetWindowText(user32.dll#GetForegroundWindow(), StringBuilder(256), 256)")]
        public Program WindowTitle { get; set; }

        [Event("ac")]
        public bool? ACConnected { get; set; } = null;
        [Condition("acState")]
        public Condition ACCondition { get; set; } = new Condition("acState");

        [Sample("battery", "Percentage of battery remaining")]
        public BatteryCharge BatteryRemaining { get; set; } = null;
        [Condition("batteryState")]
        public Condition BatteryCondition { get; set; } = new Condition("batteryState");

        [Condition("access")]
        public Condition SystemAccess { get; set; } = new Condition("access");
    }
}
