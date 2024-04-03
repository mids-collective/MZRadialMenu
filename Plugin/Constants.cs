using System.Text.RegularExpressions;

namespace Plugin;
public static partial class Constants {
    public static Regex mzri = MZRIRegex();
    [GeneratedRegex(@"MZRI_\((?<import>.*)\)")]
    private static partial Regex MZRIRegex();
}