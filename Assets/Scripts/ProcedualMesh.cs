using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;



struct VerticesParticle
{

    public int gloableIdx;
    public float mass;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 force;
    
    public bool pined;
    
}
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcedualMesh : MonoBehaviour
{
    //cloth property
    public float currentTime;
    public bool initialized = false;
    public int clothRes = 1;
    public float clothSize = 1;
    private float segmentLength;
    //spring property
    public float strechScale = 1,strechRl,  strechKs = -10000f, strechKd = -1000f;
    public float shearScale = 1,shearRl,  shearKs = -10000f , shearKd = -1000f;
    public float bendScale = 1,bendRl,  bendKs = -10000f, bendKd = -1000f;

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

         ComputeVertices();
         _mesh = CreateMesh("Procedual Mesh");
         GetComponent<MeshFilter>().mesh = _mesh;
         initialized = true;
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        currentTime += Time.deltaTime;
        
        if (currentTime >= 0.1f)
        {
            UpdateSimulation();
            _mesh.vertices = vertices;
            _mesh.RecalculateNormals();
            currentTime = 0;
        }
        
        /*
            UpdateSimulation();
            _mesh.vertices = vertices;
            _mesh.RecalculateNormals();
            currentTime = 0;
        */
    }

    public void ComputeVertices()
    {
        segmentLength = clothSize / (float)clothRes;
            strechRl = segmentLength;
            shearRl = segmentLength * Mathf.Sqrt(2.0f);
            bendRl = segmentLength * 2;
            vertices = new Vector3[(clothRes + 1) * (clothRes + 1)];
            verticesParticles = new VerticesParticle[clothRes + 1, clothRes + 1];
            for (int i = 0, z = 0; z <= clothRes; z++) //assign vertices
            {
                for (int x = 0; x <= clothRes; x++)
                {
                    vertices[i] = new Vector3(x * segmentLength, 0, z * segmentLength);
                    VerticesParticle vertParticle = new VerticesParticle();
                    vertParticle.gloableIdx = i;
                    vertParticle.mass = 1f;
                    vertParticle.position = vertices[i];
                    vertParticle.force = Vector3.zero;
                    vertParticle.velocity = Vector3.zero;
                    //check if this particle is pinned
                    Debug.Log("vertice pos:" + vertParticle.position + "idx:(" + z + "," + x + ")");
                    if (System.Array.Exists(pinedVertices, element => element == i))
                    {
                        vertParticle.pined = true;
                    }
                    else
                    {
                        vertParticle.pined = false;
                    }

                    verticesParticles[z, x] = vertParticle;
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
        if (initialized)
        { 
            SpringForce();
            //DragForce();
            Integrate();
        }
       
    }
    private void SpringForce()
    {
        for (int i = 0; i <= clothRes; i++)
        {
            for (int j = 0; j <=  clothRes; j++)
            {
                Vector2 id = new Vector2(i,j);
                Debug.Log("ID" + id);
                Debug.Log("ID_pos" + verticesParticles[i, j].position);
                Debug.Log("ID_force" + verticesParticles[i, j].force);
                verticesParticles[i, j].force += GetStretchForcesAtVert(id);
                verticesParticles[i, j].force += GetShearForcesAtVert(id);
                verticesParticles[i, j].force += GetBendForcesAtVert(id);
            }
        }
        
        
    }

    private bool isValidID(Vector2 id)
    {
        return !(id.x < 0 || id.x > ((int)clothRes) || id.y < 0 || id.y > ((int)clothRes));
    }

    private Vector3 GetSpringForce(Vector2 id1, Vector2 id2,float restlength, float ks, float kd)
    {
        //Debug.Log("id1:" + id1);
        //Debug.Log("id2:" + id2);
        //Debug.Log("id_2 ID_pos" + verticesParticles[(int)id2.x, (int)id2.y].position);
        //Debug.Log("id_2 ID_force" + verticesParticles[(int)id2.x, (int)id2.y].force);
        VerticesParticle a = verticesParticles[(int)id1.x, (int)id1.y];
        
        VerticesParticle b = verticesParticles[(int)id2.x, (int)id2.y];

        Vector3 aPos = a.position;
        Vector3 bPos = b.position;

        Vector3 aVel = a.velocity;
        Vector3 bVel = b.velocity;

        if (Vector3.Distance(aPos, bPos) < 0.00001f)
        {
            return Vector3.zero;
        }

        float distance = Vector3.Distance(aPos, bPos);
        Vector3 forceDir = Vector3.Normalize(bPos - aPos);
        float tmp1 = ks * (Vector3.Distance(aPos, bPos) - restlength);
        float tmp2 = kd * (Vector3.Dot(bVel - aVel, forceDir));
        
        Vector3 forceWithDamping = -(
            ks * (Vector3.Distance(aPos, bPos) - restlength) +
            kd * (Vector3.Dot(bVel-aVel,forceDir))) * 
            forceDir;
        return forceWithDamping;
    }
    //compute stretch Spring Force
    private Vector3 GetStretchForcesAtVert(Vector2 id)
    {
        Vector3 stretchForce = Vector3.zero;
        //up
        Vector2 id_u = id + new Vector2(0, 1);
        Debug.Log("id up" + id_u);
        if (isValidID(id_u))
        {
            Debug.Log("up is valid");
            stretchForce += GetSpringForce(id, id_u, strechRl, strechKs, strechKd);
        }
        //below
        
        Vector2 id_b = id + new Vector2(0, -1);Debug.Log("id below" + id_b);
        if (isValidID(id_b))
        {
            Debug.Log("below is valid");
            stretchForce += GetSpringForce(id, id_b, strechRl, strechKs, strechKd);
        }
        //left
        
        Vector2 id_l = id + new Vector2(-1, 0);Debug.Log("id left" + id_l);
        if (isValidID(id_l))
        {
            Debug.Log("left is valid");
            stretchForce += GetSpringForce(id, id_l, strechRl, strechKs, strechKd);
        }
        //right
        Vector2 id_r = id + new Vector2(1, 0);Debug.Log("id right" + id_r);
        if (isValidID(id_r))
        {
            Debug.Log("right is valid");
            
            stretchForce += GetSpringForce(id, id_r, strechRl, strechKs, strechKd);
        }
        return stretchForce;
    }
    private Vector3 GetShearForcesAtVert(Vector2 id)
    {
        Vector3 shearForce = Vector3.zero;
        //ul
        Vector2 id_ul = id + new Vector2(-1, 1);

        if (isValidID(id_ul))
        {
            shearForce += GetSpringForce(id, id_ul, shearRl, shearKs, shearKd);
        }
        //ur
        Vector2 id_ur = id + new Vector2(1, 1);
        if (isValidID(id_ur))
        {
            shearForce += GetSpringForce(id, id_ur, shearRl, shearKs, shearKd);
        }
        //bl
        Vector2 id_bl = id + new Vector2(-1, -1);
        if (isValidID(id_bl))
        {
            shearForce += GetSpringForce(id, id_bl, shearRl, shearKs, shearKd);
        }
        //br
        Vector2 id_br = id + new Vector2(1, -1);
        if (isValidID(id_br))
        {
            shearForce += GetSpringForce(id, id_br, shearRl, shearKs, shearKd);
        }
        return shearForce;
    }
    private Vector3 GetBendForcesAtVert(Vector2 id)
    {
        Vector3 bendForce = Vector3.zero;

        
        //up
        Vector2 id_ub = id + new Vector2(0, 2);
        if (isValidID(id_ub))
        {
            bendForce += GetSpringForce(id, id_ub, bendRl, bendKs, bendKd);
        }
        //below
        Vector2 id_bb = id + new Vector2(0, -2);
        if (isValidID(id_bb))
        {
            bendForce += GetSpringForce(id, id_bb, bendRl, bendKs, bendKd);
        }
        //left
        Vector2 id_lb = id + new Vector2(-2, 0);
        if (isValidID(id_lb))
        {
            bendForce += GetSpringForce(id, id_lb, bendRl, bendKs, bendKd);
        }
        //right
        Vector2 id_rb = id + new Vector2(2, 0);
        if (isValidID(id_rb))
        {
            bendForce += GetSpringForce(id, id_rb, bendRl, bendKs, bendKd);
        }
        
        return bendForce;
    }
    private void DragForce()
    {
        
    }

    private void Integrate()
    {
        for (int i = 0; i <= clothRes; i++)
        {
            for (int j = 0; j <= clothRes; j++)
            {
                VerticesParticle vp = verticesParticles[i, j];
                Vector3 g = new Vector3(0, -9.81f, 0);
                Vector3 a = g + (vp.force/ vp.mass);
                if (!vp.pined)
                {
                    verticesParticles[i, j].position += vp.velocity * Time.fixedDeltaTime;
                    verticesParticles[i, j].velocity += a * Time.fixedDeltaTime;
                    vertices[vp.gloableIdx] = verticesParticles[i, j].position;
                }
            }
        }
    }

    public bool WasSuccessfullyInitialized()
    {
        return initialized;
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
