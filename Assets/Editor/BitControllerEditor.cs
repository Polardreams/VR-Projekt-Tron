using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to add toggle buttons for bit states to inspector.
/// </summary>
[CustomEditor(typeof(BitController))]
public class BitControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BitController myScript = (BitController)target;

        // Add button to trigger True state
        if (GUILayout.Button("Trigger TRUE"))
            myScript.TriggerState(true);

        // Add button to trigger False state
        if (GUILayout.Button("Trigger FALSE"))
            myScript.TriggerState(false);
    }
}
