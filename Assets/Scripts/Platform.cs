using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Platform : MonoBehaviour
{
    [SerializeField] private List<Vector2> points = new List<Vector2>();
    [SerializeField] private float platformThickness = 0.2f;
    [SerializeField] private Material platformMaterial;
    
    private EdgeCollider2D edgeCollider;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (platformMaterial != null)
        {
            meshRenderer.material = platformMaterial;
        }
        
        UpdateCollider();
        GenerateMesh();
    }

    public void SetPoints(List<Vector2> newPoints)
    {
        points = new List<Vector2>(newPoints);
        UpdateCollider();
        GenerateMesh();
    }

    public List<Vector2> GetPoints()
    {
        return new List<Vector2>(points);
    }

    private void UpdateCollider()
    {
        if (points.Count >= 2)
        {
            edgeCollider.points = points.ToArray();
        }
    }

    private void GenerateMesh()
    {
        if (points.Count < 2) return;

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        // Get gravity direction for each point to determine thickness direction
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[i + 1];
            
            Vector2 gravityDir1 = CircularWorldController.Instance.GetGravityDirection(current);
            Vector2 gravityDir2 = CircularWorldController.Instance.GetGravityDirection(next);
            
            // Add four corners of this segment
            vertices.Add(current);
            vertices.Add(next);
            vertices.Add(current + gravityDir1 * platformThickness);
            vertices.Add(next + gravityDir2 * platformThickness);
            
            // Create two triangles for this quad segment
            int baseIndex = i * 4;
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 2);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCollider();
            GenerateMesh();
        }
    }
}
