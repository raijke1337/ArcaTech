// RemapSmrEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoRemapSkinnedMesh))]
public class RemapSmrEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoRemapSkinnedMesh comp = (AutoRemapSkinnedMesh)target;
        if (GUILayout.Button("Remap Bones Now"))
        {
            comp.RemapBones();
        }

        if (comp.GetComponent<SkinnedMeshRenderer>() != null)
        {
            var smr = comp.GetComponent<SkinnedMeshRenderer>();
            if (smr.bones != null && smr.bones.Length > 0)
            {
                EditorGUILayout.LabelField("Source bone names (first 20):");
                int count = Mathf.Min(20, smr.bones.Length);
                for (int i = 0; i < count; ++i)
                {
                    var b = smr.bones[i];
                    EditorGUILayout.LabelField(i + ": " + (b ? b.name : "NULL"));
                }
            }
        }
    }
}
#endif
