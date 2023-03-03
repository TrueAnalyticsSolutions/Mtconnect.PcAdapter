using Mtconnect.AdapterInterface.Contracts.Attributes;
using Mtconnect.AdapterInterface.DataItems;
using System;

namespace Mtconnect.PCAdapter
{
    public class PCModel : IAdapterDataModel
    {
        [Event("avail")]
        public string Availability { get; set; }

        [Sample("xPos", "user32.dll#GetCursorPos().X")]
        public int? XPosition { get; set; }
        [Timestamp("xPos")]
        public DateTime? XPosition_Time { get; set; }

        [Sample("yPos", "user32.dll#GetCursorPos().Y")]
        public int? YPosition { get; set; }

        [Event("prog", "user32.dll#GetWindowText(user32.dll#GetForegroundWindow(), StringBuilder(256), 256)")]
        public string WindowTitle { get; set; }

        [Event("foobar")]
        public string FooBar { get; set; } = null;

        [Event("ac")]
        public bool? ACConnected { get; set; } = null;
        [Condition("acState")]
        public Condition ACCondition { get; set; } = new Condition("acState");

        [Sample("battery")]
        public int? BatteryRemaining { get; set; } = null;
        [Condition("batteryState")]
        public Condition BatteryCondition { get; set; } = new Condition("batteryState");

        [Condition("access")]
        public Condition SystemAccess { get; set; } = new Condition("access");
    }
}
