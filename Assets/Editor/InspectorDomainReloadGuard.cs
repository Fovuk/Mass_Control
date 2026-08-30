using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
static class InspectorDomainReloadGuard
{
    static InspectorDomainReloadGuard()
    {
        AssemblyReloadEvents.beforeAssemblyReload += ClearUiSelectionBeforeReload;
    }

    static void ClearUiSelectionBeforeReload()
    {
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            return;
        }

        if (selected is GameObject gameObject &&
            gameObject.GetComponent<Image>() != null)
        {
            Selection.activeObject = null;
        }
    }
}
