using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProcedualMesh))]
public class ProcedualMeshEditor : Editor
{
    
    private bool showMeshProperties = false;
    private bool showSimulationProperties = false;
    private SerializedProperty vertices;
    private SerializedProperty pinedVertices;
    // Start is called before the first frame update
    void OnEnable()
    {
        vertices = serializedObject.FindProperty("vertices");
        pinedVertices = serializedObject.FindProperty("pinedVertices");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        ProcedualMesh procedualMesh = (ProcedualMesh)target;
        EditorGUILayout.Space();
        showMeshProperties = EditorGUILayout.Foldout(showMeshProperties, "Mesh Properties", true);
        if (showMeshProperties)
        {
            procedualMesh.currentTime = Mathf.Max(EditorGUILayout.FloatField("current time", procedualMesh.currentTime), 1.0f);
            procedualMesh.clothRes = (int)Mathf.Max(EditorGUILayout.IntField("Cloth Resolution", procedualMesh.clothRes), 1.0f);
            procedualMesh.clothSize = Mathf.Max(EditorGUILayout.FloatField("Cloth Size", procedualMesh.clothSize), 1.0f);
            EditorGUILayout.PropertyField(vertices, true);
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.PropertyField(pinedVertices, true);
            serializedObject.ApplyModifiedProperties();
            
            if (procedualMesh.WasSuccessfullyInitialized()) {
                GUI.enabled = false;
            }
        }
        
        showSimulationProperties = EditorGUILayout.Foldout(showSimulationProperties, "Spring Properties", true);
        if (showSimulationProperties)
        {

            procedualMesh.strechScale = EditorGUILayout.Slider("Stretch Spring Contribution", procedualMesh.strechScale, 0, 1f);
            procedualMesh.shearScale =    EditorGUILayout.Slider("Shear Spring Contribution", procedualMesh.shearScale, 0, 1f);
            procedualMesh.bendScale =    EditorGUILayout.Slider("Bend Spring Contribution", procedualMesh.bendScale, 0, 1f);
            EditorGUILayout.Space();
            EditorGUILayout.Foldout(true, "Stretch Spring Properties", true);
            procedualMesh.strechKs = Mathf.Min(EditorGUILayout.FloatField("Stretch Spring Constant (ks)", procedualMesh.strechKs), 10000f);
            procedualMesh.strechKd = Mathf.Min(EditorGUILayout.FloatField("Damping Constant (kd)", procedualMesh.strechKd), 1000f);
            
            EditorGUILayout.Foldout(true, "Shear Spring Properties", true);
            procedualMesh.shearKs = Mathf.Min(EditorGUILayout.FloatField("ShearSpring Constant (ks)", procedualMesh.shearKs), 10000f);
            procedualMesh.shearKd = Mathf.Min(EditorGUILayout.FloatField("Damping Constant (kd)", procedualMesh.shearKd), 1000f);
            
            EditorGUILayout.Foldout(true, "Bend Spring Properties", true);
            procedualMesh.bendKs = Mathf.Min(EditorGUILayout.FloatField("Bend Spring Constant (ks)", procedualMesh.bendKs), 10000f);
            procedualMesh.bendKd = Mathf.Min(EditorGUILayout.FloatField("Damping Constant (kd)", procedualMesh.bendKd), 1000f);

            procedualMesh.is_euler = EditorGUILayout.Toggle("Euler",procedualMesh.is_euler);
            procedualMesh.is_semiimplecit_euler = EditorGUILayout.Toggle("Semi Implicit Euler",procedualMesh.is_semiimplecit_euler);
            procedualMesh.is_verlet = EditorGUILayout.Toggle("Verlet",procedualMesh.is_verlet);
        }


        //procedualMesh.ComputeVertices();

        
    }
}
