namespace ThermalRight.AIOMonitor.Core;

public enum Wire
{
    Scsi,
    Bulk,
    HidType2,
    F5,
    Ly,
    Ly1,
}

public readonly record struct UsbId(ushort Vid, ushort Pid)
{
    public override string ToString() => $"{Vid:x4}:{Pid:x4}";
}
