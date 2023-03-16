using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcedualMesh : MonoBehaviour
{
    //world size
    public int clothRes = 1;
    public float clothSize = 1;
    private float segmentLength;
    //generated mesh
    private Mesh _mesh;
    //initilize array to store mesh info
    [SerializeField] private int[] triangles;
    [SerializeField] private Vector3[] vertices;
    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    void Start()
    {
        _mesh = CreateMesh("Procedual Mesh");
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ComputeVertices()
    {
        segmentLength = clothSize / (float)clothRes;
        vertices = new Vector3[(clothRes+1)*(clothRes+1)];
        for(int i = 0, z = 0; z <= clothRes; z++)//assign vertices
        {
            for(int x = 0; x <= clothRes; x++)
            {
                vertices[i] = new Vector3(x*segmentLength, 0, z*segmentLength);
                i++;
            }
        }
    }
    Mesh CreateMesh(string name)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        triangles = new int[clothRes * clothRes * 6];
        
        int tris = 0;
        int vertIdx = 0;
        for (int z = 0; z < clothRes; z++)//assign vertices
        {
            for (int x = 0; x < clothRes; x++) {
                //tri 012
                triangles[tris + 0] = vertIdx + 0;
                triangles[tris + 1] = vertIdx + clothRes + 1;
                triangles[tris + 2] = vertIdx + 1;
                //tri 132
                triangles[tris + 3] = vertIdx + 1;
                triangles[tris + 4] = vertIdx + clothRes + 1;
                triangles[tris + 5] = vertIdx + clothRes + 1 + 1;

                vertIdx++;
                tris += 6;
            }
            vertIdx++;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    
    
    void OnDrawGizmos() { 
        if (vertices != null) {
            for (int i = 0; i < vertices.Length; i++){
                Gizmos.color = Color.white;
                    Gizmos.DrawSphere(vertices[i], segmentLength*0.125f);
            }
        }
    }
}
