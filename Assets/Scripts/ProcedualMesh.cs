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
    public Vector3 pre_position;
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
    public float strechScale = 1,strechRl,  strechKs, strechKd;
    public float shearScale = 1,shearRl,  shearKs, shearKd;
    public float bendScale = 1,bendRl,  bendKs, bendKd;
    //integrator property
    public bool is_euler;
    public bool is_semiimplecit_euler;
    public bool is_verlet ;
    //generated mesh
    private Mesh _mesh;
    //initilize array to store mesh info
    [SerializeField] private int[] triangles;
    [SerializeField] private Vector3[] vertices;
    [SerializeField] private int[] pinedVertices;

    private int current_loop = 0;
    private int max_loop = 500;
    // property for simulation
    private int[] triangleArray;
    private VerticesParticle[,] verticesParticles;
    private void Awake()
    {

         ComputeVertices();
         _mesh = CreateMesh("Procedural Mesh");
         GetComponent<MeshFilter>().mesh = _mesh;
         initialized = true;
    }

    private void Start()
    {
        //InvokeRepeating("UpdateSimulation",.01f,0.1f);
        //StartCoroutine(UpdateSimulateCo());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSimulation();
            //currentTime = 0;
        
    }

    IEnumerator UpdateSimulateCo()
    {
        while (true)
        {
            SpringForce();
            Integrate();
            current_loop += 1;
            if (current_loop > 10)
            {
                _mesh.vertices = vertices;
                _mesh.RecalculateNormals();
                yield return null;
                current_loop = 0;
            }
           
        }
        
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
                    vertParticle.pre_position = vertices[i];
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
        SpringForce();
        //DragForce();
        Integrate();
        _mesh.vertices = vertices;
        _mesh.RecalculateNormals();
    }
    private void SpringForce()//compute all the spring force
    {
        for (int i = 0; i <= clothRes; i++)
        {
            for (int j = 0; j <=  clothRes; j++)
            {
                ;
                Vector2 id = new Vector2(i,j);
                //Debug.Log("ID" + id);
                Vector3 new_force = GetStretchForcesAtVert(i, j)
                                    + GetShearForcesAtVert(i, j) 
                                    + GetBendForcesAtVert(i, j);
                //Debug.Log("ID_pos" + verticesParticles[i, j].position);
                //Debug.Log("ID_force" + verticesParticles[i, j].force);
                verticesParticles[i, j].force = new_force;
                //IntegrateSingle(i,j);
            }
        }
        
        
    }

    private bool isValidID(int p_x,int p_z)
    {
        return !(p_x < 0 || p_x > ((int)clothRes) || p_z < 0 || p_z > ((int)clothRes));
    }

    private Vector3 GetSpringForce(int pa_x,int pa_z,int pb_x,int pb_z,float restlength, float ks, float kd)//get string force of node a from spring node:ab
    {
        VerticesParticle a = verticesParticles[pa_x, pa_z];
        
        VerticesParticle b = verticesParticles[pb_x, pb_z];

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
        float tmp0 = restlength;
        float tmp = segmentLength;
        float tmp1 = ks * (Vector3.Distance(aPos, bPos) - restlength);
        Debug.Log("Spring Force:" + tmp1);
        float tmp2 = kd * (Vector3.Dot(bVel - aVel, forceDir));
        Debug.Log("Spring Damp:" + tmp2);
        Vector3 forceWithDamping = -(
            ks * (Vector3.Distance(aPos, bPos) - restlength) +
            kd * (Vector3.Dot(bVel,forceDir) - Vector3.Dot(aVel,forceDir))) * 
            forceDir;
        Debug.Log("Force Damp:" + forceWithDamping);
        return forceWithDamping;
    }
    //compute stretch Spring Force
    private Vector3 GetStretchForcesAtVert(int pa_x,int pa_z)//get all stretch spring forces
    {
        Vector3 stretchForce = Vector3.zero;
        //up
        int pu_x = pa_x;
        int pu_z = pa_z + 1;
        if (isValidID(pu_x,pu_z))
        {
            //Debug.Log("Force up is valid");
            stretchForce += GetSpringForce(pa_x, pa_z,pu_x,pu_z, strechRl, strechKs, strechKd);
        }
        //below
        
        int pb_x = pa_x;
        int pb_z = pa_z - 1;
        if (isValidID(pb_x,pb_z))
        {
            //Debug.Log("Force below is valid");
            stretchForce += GetSpringForce(pa_x, pa_z, pb_x, pb_z, strechRl, strechKs, strechKd);
        }
        //left
        int pl_x = pa_x - 1;
        int pl_z = pa_z;
        if (isValidID(pl_x,pl_z))
        {
            //Debug.Log("Force left is valid");
            stretchForce += GetSpringForce(pa_x,pa_z,pl_x,pl_z, strechRl, strechKs, strechKd);
        }
        //right
        int pr_x = pa_x + 1;
        int pr_z = pa_z;
        if (isValidID(pr_x,pr_z))
        {
            //Debug.Log("Force right is valid");
            
            stretchForce += GetSpringForce(pa_x,pa_z,pr_x,pr_z, strechRl, strechKs, strechKd);
        }
        return stretchForce;
    }
    
    private Vector3 GetShearForcesAtVert(int pa_x,int pa_z)//get all shear spring forces
    {
        Vector3 shearForce = Vector3.zero;
        //ul
        int pul_x = pa_x - 1;
        int pul_z = pa_z + 1;
        if (isValidID(pul_x,pul_z))
        {
            shearForce += GetSpringForce(pa_x,pa_z,pul_x,pul_z, shearRl, shearKs, shearKd);
        }
        //ur
        int pur_x = pa_x + 1;
        int pur_z = pa_z + 1;
        if (isValidID(pur_x,pur_z))
        {
            shearForce += GetSpringForce(pa_x,pa_z,pur_x,pur_z, shearRl, shearKs, shearKd);
        }
        //bl
        int pbl_x = pa_x - 1;
        int pbl_z = pa_z - 1;
        if (isValidID(pbl_x,pbl_z))
        {
            shearForce += GetSpringForce(pa_x,pa_z,pbl_x,pbl_z, shearRl, shearKs, shearKd);
        }
        //br
        int pbr_x = pa_x - 1;
        int pbr_z = pa_z + 1;
        if (isValidID(pbr_x,pbr_z))
        {
            shearForce += GetSpringForce(pa_x,pa_z,pbr_x,pbr_z, shearRl, shearKs, shearKd);
        }
        return shearForce;
    }
    private Vector3 GetBendForcesAtVert(int pa_x,int pa_z)//get all bend spring forces
    {
        Vector3 bendForce = Vector3.zero;
        //up
        int pu_x = pa_x;
        int pu_z = pa_z + 2;
        if (isValidID(pu_x,pu_z))
        {
            //Debug.Log("Force up is valid");
            bendForce += GetSpringForce(pa_x, pa_z,pu_x,pu_z, bendRl, bendKs, bendKd);
        }
        //below
        
        int pb_x = pa_x;
        int pb_z = pa_z - 2;
        if (isValidID(pb_x,pb_z))
        {
            //Debug.Log("Force below is valid");
            bendForce += GetSpringForce(pa_x, pa_z,pb_x,pb_z, bendRl, bendKs, bendKd);
        }
        //left
        int pl_x = pa_x - 2;
        int pl_z = pa_z;
        if (isValidID(pl_x,pl_z))
        {
            //Debug.Log("Force left is valid");
            bendForce += GetSpringForce(pa_x,pa_z,pl_x,pl_z, bendRl, bendKs, bendKd);
        }
        //right
        int pr_x = pa_x + 2;
        int pr_z = pa_z;
        if (isValidID(pr_x,pr_z))
        {
            //Debug.Log("Force right is valid");
            
            bendForce += GetSpringForce(pa_x,pa_z,pr_x,pr_z, bendRl, bendKs, bendKd);
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
                    if (is_euler)
                    {
                         verticesParticles[i, j].position += vp.velocity * (Time.deltaTime/10);
                         verticesParticles[i, j].velocity += a * (Time.deltaTime/10);
                         //Debug.Log("Calculated location" +  "(" +i + j +")"+verticesParticles[i, j].position);
                         //Debug.Log("Calculated velocity" + verticesParticles[i, j].velocity);
                    }
                    else if (is_semiimplecit_euler)
                    {
                        verticesParticles[i, j].velocity += a * (Time.deltaTime/10);
                        verticesParticles[i, j].position += vp.velocity * (Time.deltaTime/10);
                    }
                    else if (is_verlet)
                    {

                        verticesParticles[i, j].position = 2 * verticesParticles[i, j].position -
                            verticesParticles[i, j].pre_position + a * (Time.deltaTime/2 ) * (Time.deltaTime/2 );
                        verticesParticles[i, j].pre_position = verticesParticles[i, j].position;
                    }
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
                    Gizmos.DrawSphere(vertices[i] + transform.position, segmentLength*0.25f);
                }
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(vertices[i] +  transform.position, segmentLength*0.125f);
            }
        }
        
    }
    
}
