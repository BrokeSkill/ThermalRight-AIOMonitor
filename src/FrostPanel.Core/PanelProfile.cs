namespace FrostPanel.Core;

public sealed record PanelProfile(
    int Width,
    int Height,
    bool Jpeg = false,
    bool BigEndian = false,
    bool Rotate = false,
    bool Widescreen = false,
    IReadOnlySet<int>? Rgb565Subs = null)
{
    public bool EncodesJpeg(int sub) => Jpeg && (Rgb565Subs is null || !Rgb565Subs.Contains(sub));
}

public static class Profiles
{
    public static readonly PanelProfile Default = new(320, 320, BigEndian: true);

    private static readonly IReadOnlySet<int> FanHubSubs = new HashSet<int> { 4 };

    public static readonly IReadOnlyDictionary<int, PanelProfile> ByFbl = new Dictionary<int, PanelProfile>
    {
        [36] = new(240, 240),
        [37] = new(240, 240),
        [49] = new(320, 240, BigEndian: true, Rotate: true),
        [50] = new(320, 240, Rotate: true),
        [51] = new(320, 240, Rotate: true),
        [52] = new(320, 240, Rotate: true),
        [53] = new(320, 240, Rotate: true),
        [54] = new(360, 360, Jpeg: true, Rgb565Subs: FanHubSubs),
        [58] = new(320, 240, Rotate: true),
        [60] = new(320, 176, Rotate: true),
        [64] = new(640, 480, Rotate: true),
        [72] = new(480, 480, Jpeg: true),
        [100] = new(320, 320, BigEndian: true),
        [101] = new(320, 320, BigEndian: true),
        [102] = new(320, 320, BigEndian: true),
        [114] = new(1600, 720, Jpeg: true, Rotate: true, Widescreen: true),
        [128] = new(1280, 480, Jpeg: true, Rotate: true, Widescreen: true),
        [129] = new(480, 480),
        [192] = new(1920, 462, Jpeg: true, Rotate: true, Widescreen: true),
        [224] = new(854, 480, Jpeg: true, Rotate: true, Widescreen: true),
    };

    private static readonly Dictionary<int, int> PmToFblOverrides = new()
    {
        [5] = 50,
        [7] = 64,
        [9] = 224,
        [10] = 224,
        [11] = 224,
        [12] = 224,
        [13] = 224,
        [14] = 64,
        [15] = 224,
        [16] = 224,
        [17] = 224,
        [18] = 224,
        [20] = 224,
        [32] = 100,
        [50] = 50,
        [63] = 114,
        [64] = 114,
        [65] = 192,
        [66] = 192,
        [68] = 192,
        [69] = 192,
    };

    private static readonly Dictionary<int, (int W, int H)> Fbl224ByPm = new()
    {
        [9] = (854, 480),
        [10] = (960, 540),
        [11] = (854, 480),
        [12] = (800, 480),
        [13] = (960, 320),
        [15] = (640, 172),
        [16] = (960, 540),
        [17] = (960, 320),
        [18] = (960, 320),
        [20] = (854, 480),
    };

    private static readonly Dictionary<int, (int W, int H)> Fbl192ByPm = new()
    {
        [1] = (1920, 462),
        [65] = (1920, 462),
        [66] = (1920, 462),
        [68] = (1280, 480),
        [69] = (1920, 440),
    };

    private static readonly Dictionary<(int Pm, int Sub), int> PmSubToFbl = new()
    {
        [(1, 48)] = 114,
        [(1, 49)] = 192,
    };

    public static int PmToFbl(int pm, int sub) =>
        PmSubToFbl.TryGetValue((pm, sub), out var named) ? named
        : PmToFblOverrides.TryGetValue(pm, out var over) ? over
        : pm;

    public static bool InPmLadder(int pm, int sub) =>
        PmSubToFbl.ContainsKey((pm, sub)) || PmToFblOverrides.ContainsKey(pm);

    public static PanelProfile Get(int fbl, int pm = 0, int sub = 0)
    {
        var profile = ByFbl.TryGetValue(fbl, out var found) ? found : Default;
        if (fbl == 192)
        {
            var (w, h) = Fbl192ByPm.TryGetValue(pm, out var r) ? r : (1920, 462);
            profile = profile with { Width = w, Height = h };
        }
        else if (fbl == 224)
        {
            var (w, h) = Fbl224ByPm.TryGetValue(pm, out var r) ? r : (854, 480);
            profile = profile with { Width = w, Height = h };
        }
        return profile with { Jpeg = profile.EncodesJpeg(sub) };
    }

    public static PanelProfile ForScsi(int fbl) => Get(fbl, fbl, 0);

    public static PanelProfile ForBulk(int pm, int sub) =>
        InPmLadder(pm, sub) ? Get(PmToFbl(pm, sub), pm, sub) : Get(72, pm, sub);

    public static PanelProfile ForHidType2(int pm, int sub) => Get(PmToFbl(pm, sub), pm, sub);
}
