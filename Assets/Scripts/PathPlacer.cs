using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Path))]
public class PathPlacer : MonoBehaviour
{
    public float spacing = .1f;
    public float resolution = 1f;

    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] float scale = 0.2f;

    private void Update()
    {
        Matrix4x4[] points = gameObject.GetComponent<Path>().CalculateEvenlySpacedPointsMatrix4x4(spacing, Vector3.one * scale, resolution);
        Graphics.DrawMeshInstanced(mesh, 0, material, points);
    }

   
}
