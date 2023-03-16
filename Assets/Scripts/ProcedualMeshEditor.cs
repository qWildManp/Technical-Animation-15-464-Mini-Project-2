using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProcedualMesh))]
public class ProcedualMeshEditor : Editor
{
    private bool showMeshProperties = false;
    private SerializedProperty vertices;
    // Start is called before the first frame update
    void OnEnable()
    {
        vertices = serializedObject.FindProperty("vertices");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        ProcedualMesh procedualMesh = (ProcedualMesh)target;
        EditorGUILayout.Space();
        showMeshProperties = EditorGUILayout.Foldout(showMeshProperties, "Mesh Properties", true);
        if (showMeshProperties)
        {
            procedualMesh.clothRes = (int)Mathf.Max(EditorGUILayout.IntField("Cloth Resolution", procedualMesh.clothRes), 1.0f);
            procedualMesh.clothSize = Mathf.Max(EditorGUILayout.FloatField("Cloth Size", procedualMesh.clothSize), 1.0f);
            EditorGUILayout.PropertyField(vertices, true);
            serializedObject.ApplyModifiedProperties();
        }

        procedualMesh.ComputeVertices();
    }
}
