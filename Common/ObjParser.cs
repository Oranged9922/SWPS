using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace SWPS.Common;

/// <summary>
/// A class for parsing OBJ files.
/// </summary>
public static class ObjParser
{
    /// <summary>
    /// Loads a 3D model from an OBJ file.
    /// </summary>
    /// <param name="path"> The path to the OBJ file.</param>
    /// <returns> A <see cref="Mesh"/> object containing the vertices and indices of the model.</returns>
    public static Mesh Load(string path)
    {
        var model = new Mesh();
        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("v "))
            {
                string[] parts = trimmedLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    model.Vertices.Add(new(x, y, z));
                }
            }
            else if (trimmedLine.StartsWith("f "))
            {
                string[] parts = trimmedLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    List<uint> faceIndices = [];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] vertexData = parts[i].Split('/');
                        uint vertexIndex = uint.Parse(vertexData[0]) - 1;
                        faceIndices.Add(vertexIndex);
                    }
                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        model.Indices.Add(faceIndices[0]);
                        model.Indices.Add(faceIndices[i]);
                        model.Indices.Add(faceIndices[i + 1]);
                    }
                }
            }
        }
        return model;
    }
}


   