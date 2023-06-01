using Mtconnect.AdapterSdk.Contracts.Attributes;
using Mtconnect.AdapterSdk.DataItems;
using Mtconnect.AdapterSdk.DataItemTypes;
using Mtconnect.AdapterSdk.DataItemValues;
using System;

namespace Mtconnect.PCAdapter
{
    public class PCModel : IAdapterDataModel
    {
        [Event("avail")]
        public Availability Availability { get; set; }

        private Mouse _mouse;
        [DataItemPartial("mouse_")]
        public Mouse Mouse => _mouse ?? (_mouse = new Mouse());

        private PCController _ctrlr;
        [DataItemPartial("ctrlr_")]
        public PCController Controller => _ctrlr ?? (_ctrlr = new PCController());
    }
    public class MouseAxis : Axis
    {
        [Sample("pos", "user32.dll#GetCursorPos()", Units = "PIXEL")]
        public Position.ACTUAL ActualPosition { get; set; }

        /* NOTE: Here we are providing the opportunity to override the timestamp of the DataItem.
         * This is a useful feature to provide more accurate timestamps for your data either by referencing a timestamp from the source
         * or, by calculating the accurate timestamp based on conditions of timestamp drift.
         * */
        //[Timestamp("pos")]
        //public DateTime? ActualPosition_Time { get; set;}
    }
    public class Mouse : Axes
    {
        [DataItemPartial("x_")]
        public MouseAxis X => GetOrAddAxis<MouseAxis>(nameof(X));

        [DataItemPartial("y_")]
        public MouseAxis Y => GetOrAddAxis<MouseAxis>(nameof(Y));
    }
    public class PCController : Controller
    {
        [DataItemPartial("p_")]
        public PCPath Path => GetOrAddPath<PCPath>(nameof(Path));
    }
    public class PCPath : Path {
        [Event("prog", "user32.dll#GetWindowText(user32.dll#GetForegroundWindow(), StringBuilder(256), 256)")]
        public Program WindowTitle { get; set; }

        [Event("ac", "Indicates whether the AC charger is currently connected")]
        public bool? ACConnected { get; set; } = null;
        [Condition("acState")]
        public Condition ACCondition { get; set; } = new Condition("acState");

        [Sample("battery", "Percentage of battery remaining", Units = "PERCENT")]
        public BatteryCharge BatteryRemaining { get; set; } = null;

        [Condition("batteryState")]
        public Condition BatteryCondition { get; set; } = new Condition("batteryState");

        [Condition("access", "State of the adapter's access to system information.")]
        public Condition SystemAccess { get; set; } = new Condition("access");
    }
}
