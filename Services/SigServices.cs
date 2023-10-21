using System.Reflection;
using Newtonsoft.Json;

namespace Plugin.Services;

public sealed class SigService : IService<SigService>
{
    public static SigService Instance => Service<SigService>.Instance;
    public static string GetSig(string glob) => Instance.GetSignature(glob);
    private Dictionary<string, string> Signatures = new();
    public string GetSignature(string Glob)
    {
        return Signatures[Glob];
    }
    private SigService()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        using (var stream = thisAssembly.GetManifestResourceStream($"{thisAssembly.GetName().Name}.sigs.json"))
        {
            if (stream != null)
                using (var reader = new StreamReader(stream))
                {
                    if (reader != null)
                        Signatures = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd()) ?? new();
                }
        }
    }
    public void Dispose()
    {
    }
}