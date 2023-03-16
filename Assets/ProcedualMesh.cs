using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedualMesh : MonoBehaviour
{
    //world size
    public int Worldx;
    public int Worldz;
    //generated mesh
    private Mesh mesh;
    //initilize array to store mesh info
    [SerializeField] private int[] triangles;
    [SerializeField] private Vector3[] vertices;
    // Start is called before the first frame update
    void Start()
    {
        mesh = CreateMesh("Procedual Mesh");
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Mesh CreateMesh(string name)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        triangles = new int[Worldx * Worldz * 6];
        vertices = new Vector3[(Worldx+1)*(Worldz*1)];

        for(int i = 0, z = 0; z < Worldz; z++)//assign vertices
        {
            for(int x =0; x < Worldx; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }
        int tris = 0;
        int vert_idx = 0;
        for (int z = 0; z < Worldz; z++)//assign vertices
        {
            for (int x = 0; x < Worldx; x++) {
                //tri 012
                triangles[tris + 0] = vert_idx + 0;
                triangles[tris + 1] = vert_idx + Worldz + 1;
                triangles[tris + 2] = vert_idx + 1;
                //tri 132
                triangles[tris + 3] = vert_idx + 1;
                triangles[tris + 4] = vert_idx + Worldz + 1;
                triangles[tris + 5] = vert_idx + Worldz + 1 + 1;

                vert_idx++;
                tris += 6;
            }
            vert_idx++;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
