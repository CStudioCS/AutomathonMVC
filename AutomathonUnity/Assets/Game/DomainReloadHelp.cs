#if UNITY_EDITOR
using NetMQ;
using UnityEditor;
public static class DomainReloadHandler
{
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeDomainReload;
    }

    private static void OnBeforeDomainReload()
    {
        NetMQConfig.Cleanup(false);

        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeDomainReload;
    }
}

#endif