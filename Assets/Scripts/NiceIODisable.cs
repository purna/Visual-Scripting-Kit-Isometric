using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class NiceIODisable : MonoBehaviour
{

    [InitializeOnEnterPlayMode]
    public static void DisableCodebaseWarnings()
    {
        Debug.unityLogger.logEnabled = false;
        var _ = Codebase.assemblies;
        Debug.unityLogger.logEnabled = true;
    }
}
