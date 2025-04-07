using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace SWPS.Common;
public static class ObjParser
{
    public static ObjModel Load(string path)
    {
        var model = new ObjModel();
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
                    model.Vertices.AddRange([x, y, z]);
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

public class ObjModel
{
    public List<float> Vertices { get; private set; } = [];
    public List<uint> Indices { get; private set; } = [];
}

   