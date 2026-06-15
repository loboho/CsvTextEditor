using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        // Register code pages (GBK, Shift-JIS, etc.) for encoding detection
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
