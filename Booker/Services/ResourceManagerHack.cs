using System;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Booker.Services;

public static class ResourceManagerHack
{
    /// <summary>
    /// If the server doesn't have .NET language packs installed then no matter what CurrentUICulture is set to, you'll always get English in 
    /// DataAnnotations validation messages. Here we override DataAnnotationsResources to use a ResourceManager that uses language .resources 
    /// files embedded in this assembly.
    /// </summary>
    public static void OverrideComponentModelAnnotationsResourceManager()
    {
        EnsureAssemblyIsLoaded();

        FieldInfo? resourceManagerFieldInfo = GetResourceManagerFieldInfo();
        ResourceManager resourceManager = GetNewResourceManager();
        resourceManagerFieldInfo?.SetValue(null, resourceManager);
    }

    private static FieldInfo? GetResourceManagerFieldInfo()
    {
        var srAssembly = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .First(assembly => assembly.FullName?.StartsWith("System.ComponentModel.Annotations,") ?? false);
        var srType = srAssembly.GetType("System.SR");
        return srType?.GetField("s_resourceManager", BindingFlags.Static | BindingFlags.NonPublic);
    }
    internal static ResourceManager GetNewResourceManager()
    {
        return new ResourceManager(typeof(DataAnnotations).ToString(), typeof(DataAnnotations).Assembly);
    }
    private static void EnsureAssemblyIsLoaded()
    {
        var _ = typeof(System.ComponentModel.DataAnnotations.RequiredAttribute);
    }
}
