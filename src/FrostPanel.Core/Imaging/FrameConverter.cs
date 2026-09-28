namespace FrostPanel.Core.Imaging;

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
}
