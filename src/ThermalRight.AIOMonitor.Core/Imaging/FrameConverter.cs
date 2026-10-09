namespace ThermalRight.AIOMonitor.Core.Imaging;

public static class FrameConverter
{
    public static byte[] ToPanelPayload(
        byte[] bgra,
        int sourceWidth,
        int sourceHeight,
        int panelWidth,
        int panelHeight,
        int rotationDegrees,
        bool bigEndian)
    {
        var (rotated, rotatedWidth, rotatedHeight) =
            BgraOps.Rotate(bgra, sourceWidth, sourceHeight, rotationDegrees);
        var scaled = BgraOps.Resample(rotated, rotatedWidth, rotatedHeight, panelWidth, panelHeight);
        return Rgb565.Encode(scaled, bigEndian);
    }

    public static byte[] ToJpegPayload(
        byte[] bgra,
        int sourceWidth,
        int sourceHeight,
        int panelWidth,
        int panelHeight,
        int rotationDegrees,
        int quality)
    {
        var (rotated, rotatedWidth, rotatedHeight) =
            BgraOps.Rotate(bgra, sourceWidth, sourceHeight, rotationDegrees);
        var scaled = BgraOps.Resample(rotated, rotatedWidth, rotatedHeight, panelWidth, panelHeight);
        return JpegEncoder.Encode(scaled, panelWidth, panelHeight, quality);
    }
}
