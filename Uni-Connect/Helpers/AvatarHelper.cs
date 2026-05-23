namespace Uni_Connect.Helpers;

public static class AvatarHelper
{
    // Background colour pairs (light-pastel) that look great on the notionists style
    private static readonly string[] BgPalette =
    {
        "b6e3f4,c0aede",   // blue-lavender
        "d1d4f9,ffd5dc",   // periwinkle-pink
        "ffdfbf,c0aede",   // peach-lavender
        "b6e3f4,d1d4f9",   // blue-periwinkle
        "c0aede,ffdfbf",   // lavender-peach
        "ffd5dc,d1d4f9",   // pink-periwinkle
        "d1fae5,b6e3f4",   // mint-blue
        "fef3c7,ffdfbf",   // cream-peach
    };

    // Fallback initials palette (used only as onerror data-URI)
    private static readonly string[] FallbackColors =
    {
        "3D52A0", "7C3AED", "059669", "D97706",
        "0891B2", "BE185D", "C2410C", "0F766E"
    };

    private static int Hash(string? name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        return Math.Abs(name.Sum(c => (int)c));
    }

    public static string Color(string? name) =>
        FallbackColors[Hash(name) % FallbackColors.Length];

    private static string GetInitials(string name)
    {
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "U";
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    // Returns a DiceBear notionists URL — illustrated, consistent per seed, no external JS needed
    public static string Url(string? name, int size = 40)
    {
        var seed = Uri.EscapeDataString(name ?? "Student");
        var bg   = BgPalette[Hash(name) % BgPalette.Length];
        return $"https://api.dicebear.com/9.x/notionists/svg?seed={seed}&backgroundColor={bg}&size={size}";
    }

    // Inline SVG data-URI for onerror fallback (initials, no network needed)
    public static string FallbackUrl(string? name, int size = 40)
    {
        var initials = GetInitials(name ?? "User");
        var color    = Color(name);
        var fs       = (int)(size * 0.4);
        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='{size}' height='{size}' viewBox='0 0 {size} {size}'>" +
                  $"<rect width='{size}' height='{size}' rx='{size / 2}' fill='#{color}'/>" +
                  $"<text x='50%' y='50%' dominant-baseline='central' text-anchor='middle' fill='white' font-size='{fs}' font-weight='700' font-family='system-ui,sans-serif'>{initials}</text>" +
                  "</svg>";
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{b64}";
    }
}
