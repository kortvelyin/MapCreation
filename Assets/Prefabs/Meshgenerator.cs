using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meshgenerator : MonoBehaviour
{
    public Material mat;
    float width = 1;
    float height = 1;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(-width, -height);
        vertices[1] = new Vector3(width, -height);
        vertices[2] = new Vector3(-width, height);

        mesh.vertices = vertices;
       mesh.triangles = new int[] { 0, 2, 1 };

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
