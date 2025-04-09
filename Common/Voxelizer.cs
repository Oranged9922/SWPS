using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWPS.Common
{
    public static class Voxelizer
    {
        /// <summary>
        /// Calculates all points (voxels) inside the given mesh, spaced by 'spacing' (e.g., 0.01 for 1 cm).
        /// </summary>
        /// <param name="mesh">The input mesh (assumed closed and manifold).</param>
        /// <param name="spacing">Distance between grid points (in mesh units).</param>
        /// <returns>List of points that are inside the mesh.</returns>
        public static List<Vector3> CalculatePointsInsideMesh(Mesh mesh, float spacing)
        {
            // Calculate bounding box of the mesh.
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var vertex in mesh.Vertices)
            {
                min = Vector3.ComponentMin(min, vertex);
                max = Vector3.ComponentMax(max, vertex);
            }

            List<Vector3> insidePoints = new List<Vector3>();

            for (float x = min.X; x <= max.X; x += spacing)
            {
                for (float y = min.Y; y <= max.Y; y += spacing)
                {
                    for (float z = min.Z; z <= max.Z; z += spacing)
                    {
                        Vector3 point = new Vector3(x, y, z);

                        if (IsPointInsideMesh(point, mesh))
                        {
                            insidePoints.Add(point);
                        }
                    }
                }
            }

            return insidePoints;
        }

        /// <summary>
        /// Determines if the given point is inside the mesh using a ray-casting (even-odd) method.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="mesh">The mesh being tested.</param>
        /// <returns>true if inside; otherwise, false.</returns>
        private static bool IsPointInsideMesh(Vector3 point, Mesh mesh)
        {
            // Cast a ray from the point in the positive X direction.
            Vector3 rayOrigin = point;
            Vector3 rayDirection = new Vector3(1, 0, 0);  // Positive X

            int intersectionCount = 0;
            int triangleCount = mesh.Indices.Count / 3;

            // For every triangle in the mesh:
            for (int i = 0; i < triangleCount; i++)
            {
                int index0 = (int)mesh.Indices[i * 3];
                int index1 = (int)mesh.Indices[i * 3 + 1];
                int index2 = (int)mesh.Indices[i * 3 + 2];

                Vector3 v0 = mesh.Vertices[index0];
                Vector3 v1 = mesh.Vertices[index1];
                Vector3 v2 = mesh.Vertices[index2];

                // If the ray intersects the triangle, increment the count.
                if (RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2, out float t))
                {
                    // Only count intersections in the forward direction.
                    if (t >= 0)
                        intersectionCount++;
                }
            }
            // If the number of intersections is odd, the point is inside.
            return (intersectionCount % 2) == 1;
        }

        /// <summary>
        /// Implements the Möller–Trumbore ray-triangle intersection algorithm.
        /// </summary>
        /// <param name="rayOrigin">Origin of the ray.</param>
        /// <param name="rayDirection">Normalized direction of the ray.</param>
        /// <param name="v0">First vertex of the triangle.</param>
        /// <param name="v1">Second vertex of the triangle.</param>
        /// <param name="v2">Third vertex of the triangle.</param>
        /// <param name="t">Distance along the ray to the intersection point.</param>
        /// <returns>true if the ray intersects the triangle; otherwise, false.</returns>
        private static bool RayIntersectsTriangle(Vector3 rayOrigin, Vector3 rayDirection,
                                                  Vector3 v0, Vector3 v1, Vector3 v2,
                                                  out float t)
        {
            t = 0;
            // Use a small epsilon for float comparisons.
            const float epsilon = 1e-6f;
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 pvec = Vector3.Cross(rayDirection, edge2);
            float det = Vector3.Dot(edge1, pvec);

            // If the determinant is near zero, the ray is parallel to the triangle plane.
            if (det > -epsilon && det < epsilon)
                return false;

            float invDet = 1.0f / det;
            Vector3 tvec = rayOrigin - v0;
            float u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1)
                return false;

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(rayDirection, qvec) * invDet;
            if (v < 0 || u + v > 1)
                return false;

            t = Vector3.Dot(edge2, qvec) * invDet;
            return t > epsilon;
        }
    }
}
