using System.IO;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ObjMeshImporter : MonoBehaviour
{
    [Header("Data Pipeline Configuration")]
    public string objFilePath = "Assets/TumorMesh.obj"; 
    public Material tumorMaterial;

    void Start()
    {
        ImportWavefrontObj(objFilePath);
    }

    void ImportWavefrontObj(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[Data Error] Target mesh file not located at: {path}");
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] tokens = line.Split(' ');
            if (tokens[0] == "v")
            {
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                float z = float.Parse(tokens[3]);
                vertices.Add(new Vector3(x, y, z));
            }
            else if (tokens[0] == "vn")
            {
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                float z = float.Parse(tokens[3]);
                normals.Add(new Vector3(x, y, z));
            }
            else if (tokens[0] == "f")
            {
                for (int i = 1; i <= 3; i++)
                {
                    string[] vertexData = tokens[i].Split('/');
                    triangles.Add(int.Parse(vertexData[0]) - 1);
                }
            }
        }

        Mesh generatedMesh = new Mesh();
        generatedMesh.name = "AI_Generated_Tumor_Volume";
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();

        if (normals.Count == vertices.Count)
        {
            generatedMesh.normals = normals.ToArray();
        }
        else
        {
            generatedMesh.RecalculateNormals();
        }

        generatedMesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = generatedMesh;
        
        if (tumorMaterial != null)
        {
            GetComponent<MeshRenderer>().material = tumorMaterial;
        }

        // Standard scaling adjustment to prevent the tumor from spawning too large in scene space
        transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        Debug.Log($"[Pipeline Success] Implemented mesh with {vertices.Count} vertices and {triangles.Count / 3} faces.");
    }
}