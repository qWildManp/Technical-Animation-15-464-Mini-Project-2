using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;



struct VerticesParticle
{
    private static int mass = 1;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 force;

    public bool pined;
    
    //spring property
    public float strechScale, strechRl, strechKs, strechKd;
    public float shearScale, shearRl, shearKs, shearKd;
    public float bendScale, bendRl, bendKs, bendKd;
}
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcedualMesh : MonoBehaviour
{
    //cloth property
    public int clothRes = 1;
    public float clothSize = 1;
    private float segmentLength;
    //spring property
    public float strechScale,  strechKs, strechKd;
    public float shearScale,  shearKs, shearKd;
    public float bendScale,  bendKs, bendKd;

    //generated mesh
    private Mesh _mesh;
    //initilize array to store mesh info
    [SerializeField] private int[] triangles;
    [SerializeField] private Vector3[] vertices;
    [SerializeField] private int[] pinedVertices;
    
    // property for simulation
    private int[] triangleArray;
    private VerticesParticle[,] verticesParticles;
    private void Awake()
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
        verticesParticles = new VerticesParticle[clothRes+1, clothRes+1];
        for(int i = 0, z = 0; z <= clothRes; z++)//assign vertices
        {
            for(int x = 0; x <= clothRes; x++)
            {
                vertices[i] = new Vector3(x*segmentLength, 0, z*segmentLength);
                VerticesParticle vertParticle = new VerticesParticle();
                vertParticle.position = vertices[i];
                vertParticle.force = Vector3.zero;
                vertParticle.velocity = Vector3.zero;
                
                vertParticle.strechScale = strechScale;
                vertParticle.strechRl = segmentLength;
                vertParticle.strechKd = strechKd;
                vertParticle.strechKs = strechKs;
                
                vertParticle.shearScale = shearScale;
                vertParticle.shearRl = segmentLength * Mathf.Sqrt(2.0f);
                vertParticle.shearKd = shearKd;
                vertParticle.shearKs = shearKs;

                vertParticle.bendScale = bendScale;
                vertParticle.bendRl = segmentLength * 2;
                vertParticle.bendKd = bendKd;
                vertParticle.bendKs = bendKs;
                
                
                verticesParticles[z, x] = new VerticesParticle();
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

    private void UpdateSimulation()
    {
        SpringForce();
        DragForce();
    }
    private void SpringForce()
    {
        for (int i = 0; i < clothRes ; i++)
        {
            for (int j = 0; j < clothRes; j++)
            {
                Vector2 id = new Vector2(i,j);
                verticesParticles[i, j].force += GetStrechForcesAtVert(id);
                verticesParticles[i, j].force += GetShearForcesAtVert(id);
                verticesParticles[i, j].force += GetBendForcesAtVert(id);
            }
        }
    }

    private bool isValidID(Vector2 id)
    {
        return !(id.x < 0 || id.x >= ((int)clothRes + 1) || id.y < 0 || id.y >= ((int)clothRes + 1));
    }
    private Vector3 GetStrechForcesAtVert(Vector2 id)
    {
        //up
        Vector2 id_u = id + new Vector2(0, 1);
        if (isValidID(id_u))
        {
            
        }
        //below
        Vector2 id_b = id + new Vector2(0, -1);
        if (isValidID(id_b))
        {
            
        }
        //left
        Vector2 id_l = id + new Vector2(-1, 0);
        if (isValidID(id_l))
        {
            
        }
        //right
        Vector2 id_r = id + new Vector2(1, 0);
        if (isValidID(id_r))
        {
            
        }
        return Vector3.zero;
    }
    private Vector3 GetShearForcesAtVert(Vector2 id)
    {
        //ul
        //ur
        //bl
        //br
        return Vector3.zero;
    }
    private Vector3 GetBendForcesAtVert(Vector2 id)
    {
        //up
        //below
        //left
        //right
        return Vector3.zero;
    }
    private void DragForce()
    {
        
    }
    
    void OnDrawGizmos() { 
        if (vertices != null) {
            for (int i = 0; i < vertices.Length; i++){
                if (pinedVertices != null && System.Array.Exists(pinedVertices, element => element == i)) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(vertices[i], segmentLength*0.25f);
                }
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(vertices[i], segmentLength*0.125f);
            }
        }
    }
}
