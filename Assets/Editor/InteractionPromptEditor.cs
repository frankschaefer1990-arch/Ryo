using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractionPrompt))]
public class InteractionPromptEditor : Editor
{
    private void OnSceneGUI()
    {
        InteractionPrompt script = (InteractionPrompt)target;
        
        if (script == null) return;

        // Convert local offset to world position
        Vector3 worldPos = script.transform.TransformPoint(script.detectionOffset);

        EditorGUI.BeginChangeCheck();
        
        // Use a position handle to move the detection area
        Vector3 newWorldPos = Handles.PositionHandle(worldPos, script.transform.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Move Interaction Detection Offset");
            
            // Convert back to local space
            script.detectionOffset = script.transform.InverseTransformPoint(newWorldPos);
            
            EditorUtility.SetDirty(script);
        }
        
        // Label for clarity
        Handles.Label(worldPos + Vector3.up * 0.5f, "Interaction Detection Area");
    }
}