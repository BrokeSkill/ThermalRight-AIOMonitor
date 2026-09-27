namespace FrostPanel.Core;

public static class KnownDevices
{
    private static readonly Dictionary<UsbId, Wire> Table = new()
    {
        [new UsbId(0x87CD, 0x70DB)] = Wire.Scsi,
        [new UsbId(0x87AD, 0x70DB)] = Wire.Bulk,
        [new UsbId(0x0402, 0x3922)] = Wire.Scsi,
        [new UsbId(0x0416, 0x5406)] = Wire.F5,
        [new UsbId(0x0416, 0x5302)] = Wire.HidType2,
        [new UsbId(0x0418, 0x5303)] = Wire.F5,
        [new UsbId(0x0418, 0x5304)] = Wire.F5,
        [new UsbId(0x0416, 0x5408)] = Wire.Ly,
        [new UsbId(0x0416, 0x5409)] = Wire.Ly1,
    };

    public static IReadOnlyDictionary<UsbId, Wire> All => Table;

    public static Wire? Resolve(UsbId id) => Table.TryGetValue(id, out var wire) ? wire : null;
}
