namespace Content.Client.Stylesheets.Palette;

/// <summary>
///     Stores all style palettes in one accessible location
/// </summary>
/// <remarks>
///     Technically not limited to only colors, can store like, standard padding amounts, and font sizes, maybe?
/// </remarks>
public static class Palettes
{
    // muted tones
    public static readonly ColorPalette Navy = ColorPalette.FromHexBase("#4f5376", lightnessShift: 0.05f, chromaShift: 0.0045f);
    public static readonly ColorPalette Cyan = ColorPalette.FromHexBase("#42586a", lightnessShift: 0.05f, chromaShift: 0.0045f);
    public static readonly ColorPalette Slate = ColorPalette.FromHexBase("#545562");
    public static readonly ColorPalette Neutral = ColorPalette.FromHexBase("#555555");

    // status tones
    public static readonly ColorPalette Red = ColorPalette.FromHexBase("#b62124", chromaShift: 0.02f);
    public static readonly ColorPalette Amber = ColorPalette.FromHexBase("#c18e36");
    public static readonly ColorPalette Green = ColorPalette.FromHexBase("#3c854a");
    public static readonly StatusPalette Status = new([Red.Base, Amber.Base, Green.Base]);

    // highlight tones
    public static readonly ColorPalette Gold = ColorPalette.FromHexBase("#a88b5e");
    public static readonly ColorPalette Maroon = ColorPalette.FromHexBase("#9b2236");

    // Intended to be used with `ModulateSelf` to darken / lighten something
    public static readonly ColorPalette AlphaModulate = ColorPalette.FromHexBase("#ffffff");

    // begin starcup: department color palettes
    public static readonly ColorPalette DepartmentCommand = ColorPalette.FromHexBase("#55203f");
    public static readonly ColorPalette DepartmentEngineering = ColorPalette.FromHexBase("#77684B");
    public static readonly ColorPalette DepartmentScience = ColorPalette.FromHexBase("#6F5973");
    public static readonly ColorPalette DepartmentLogistics = ColorPalette.FromHexBase("#61503A");
    public static readonly ColorPalette DepartmentMedical = ColorPalette.FromHexBase("#49687D");
    public static readonly ColorPalette DepartmentSecurity = ColorPalette.FromHexBase("#724449");
    public static readonly ColorPalette DepartmentService = ColorPalette.FromHexBase("#607952");
    // end starcup

}
