using System.Reflection;

namespace eBEST.OpenApi.DevCenter.Helpers
{
    internal static class ModelsHelper
    {
        public static List<Type> GetModelClasses()
        {
            var entry = Assembly.GetEntryAssembly()!;

            var refModels = entry.GetReferencedAssemblies().First(t => t.Name!.StartsWith("eBEST.OpenApi.Models"));
            return Assembly.Load(refModels).GetTypes().Where(t => t.IsClass && string.Equals(t.Namespace, "eBEST.OpenApi.Models", StringComparison.Ordinal)).ToList();
        }
    }
}
