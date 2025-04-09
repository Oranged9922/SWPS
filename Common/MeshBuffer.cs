using System;
using OpenTK.Graphics.OpenGL4;

namespace SWPS.Common;

/// <summary>
/// A wrapper class for an OpenGL mesh that encapsulates a VAO, VBO, and EBO.
/// </summary>
public class MeshBuffer : IDisposable, IBuffer
{
    /// <summary>
    /// The OpenGL Vertex Array Object (VAO) ID.
    /// </summary>
    public int VertexArrayObject { get; private set; }

    /// <summary>
    /// The OpenGL Vertex Buffer Object (VBO) ID.
    /// </summary>
    public int VertexBufferObject { get; private set; }
    /// <summary>
    /// The OpenGL Element Buffer Object (EBO) ID.
    /// </summary>
    public int ElementBufferObject { get; private set; }
    /// <summary>
    /// The number of indices in the mesh.
    /// </summary>
    public int IndicesCount { get; private set; }

    /// <summary>
    /// Creates a new MeshBuffer.
    /// </summary>
    /// <param name="vertices">
    /// The vertex data array. It is assumed that vertex attributes are tightly packed.
    /// For example, if vertices only contain positions, each vertex might be 3 floats.
    /// </param>
    /// <param name="indices">
    /// The indices array (each group of 3 indices represents a triangle).
    /// </param>
    public MeshBuffer(Mesh mesh)
    {
        IndicesCount = mesh.IndicesArray.Length;

        VertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(VertexArrayObject);

        VertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      mesh.VerticesArray.Length * sizeof(float),
                      mesh.VerticesArray,
                      BufferUsageHint.StaticDraw);

        ElementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer,
                      mesh.IndicesArray.Length * sizeof(uint),
                      mesh.IndicesArray,
                      BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Binds the mesh's VAO.
    /// </summary>
    public void Bind()
    {
        GL.BindVertexArray(VertexArrayObject);
    }

    /// <summary>
    /// Unbinds the current VAO.
    /// </summary>
    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Draws the mesh using the specified primitive type.
    /// The VAO must have the appropriate vertex attribute pointers configured.
    /// </summary>
    /// <param name="mode">The type of primitives to render (e.g., Triangles, Lines).</param>
    public void Draw(PrimitiveType mode = PrimitiveType.Triangles)
    {
        Bind();
        GL.DrawElements(mode, IndicesCount, DrawElementsType.UnsignedInt, 0);
        Unbind();
    }

    /// <summary>
    /// Releases the OpenGL resources held by this mesh.
    /// </summary>
    public void Dispose()
    {
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GL.DeleteVertexArray(VertexArrayObject);
        GC.SuppressFinalize(this);
    }
}
