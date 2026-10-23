using System.Reflection;

namespace OpenCredential.Service.Impl
{
    // Isolated from Service: formatting tests must never initialize Framework, plugins or registry settings.
    internal static class MotdVersion
    {
        internal static string Expand(string text)
        {
            if (!text.Contains("%v")) return text;
            var assembly = Assembly.GetExecutingAssembly();
            var release = (AssemblyInformationalVersionAttribute)System.Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute));
            var file = (AssemblyFileVersionAttribute)System.Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
            string version = release != null && !string.IsNullOrWhiteSpace(release.InformationalVersion) ? release.InformationalVersion :
                file != null && !string.IsNullOrWhiteSpace(file.Version) ? file.Version : assembly.GetName().Version.ToString();
            return text.Replace("%v", version);
        }
    }
}
