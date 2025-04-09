using OpenTK.Mathematics;

namespace SWPS.Common;

/// <summary>
/// A class that represents a 3D mesh.
/// </summary>
public class Mesh
{
    /// <summary>
    /// List of vertices in the mesh.
    /// </summary>
    public List<Vector3> Vertices { get; set; } = [];
    /// <summary>
    /// List of indices that define the mesh's triangles.
    /// </summary>
    public List<uint> Indices { get; set; } = [];


    /// <summary>
    /// Array of vertices in the mesh.
    /// </summary>
    public float[] VerticesArray => [.. Vertices.SelectMany(v => new[] { v.X, v.Y, v.Z })];
    /// <summary>
    /// Array of indices that define the mesh's triangles.
    /// </summary>
    public uint[] IndicesArray => [.. Indices];
}
