namespace Uni_Connect.Helpers;

public static class AvatarHelper
{
    private static readonly string[] Palette =
    {
        "3D52A0", "7C3AED", "059669", "D97706",
        "0891B2", "BE185D", "C2410C", "0F766E"
    };

    public static string Color(string? name)
    {
        if (string.IsNullOrEmpty(name)) return "3D52A0";
        int hash = Math.Abs(name.Sum(c => (int)c));
        return Palette[hash % Palette.Length];
    }

    private static string GetInitials(string name)
    {
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "U";
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    public static string Url(string? name, int size = 40)
    {
        var initials = GetInitials(name ?? "User");
        var color = Color(name);
        var fontSize = (int)(size * 0.4);
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="{size}" height="{size}" viewBox="0 0 {size} {size}">
              <rect width="{size}" height="{size}" rx="{size / 2}" fill="#{color}"/>
              <text x="50%" y="50%" dominant-baseline="central" text-anchor="middle" fill="white" font-size="{fontSize}" font-weight="700" font-family="system-ui,sans-serif">{initials}</text>
            </svg>
            """;
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }
}
