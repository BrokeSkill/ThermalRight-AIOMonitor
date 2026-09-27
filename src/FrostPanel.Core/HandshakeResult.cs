namespace FrostPanel.Core;

public sealed record HandshakeResult(
    int Width,
    int Height,
    int ModelId,
    int PmByte,
    int SubByte,
    int Fbl,
    byte[] RawResponse);
