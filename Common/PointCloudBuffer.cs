using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWPS.Common
{
    /// <summary>
    /// A wrapper class for a point cloud, encapsulating VAO and VBO.
    /// Each point is assumed to be three floats (x, y, z).
    /// </summary>
    public class PointCloudBuffer : IDisposable, IBuffer
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
        /// The number of vertices in the point cloud.
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// Constructs a new PointCloudBuffer.
        /// </summary>
        /// <param name="points">A float array containing point positions (x, y, z for each point).</param>
        public PointCloudBuffer(float[] points)
        {
            VertexCount = points.Length / 3;

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          points.Length * sizeof(float),
                          points,
                          BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Binds this point cloud's VAO.
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(VertexArrayObject);
        }

        /// <summary>
        /// Unbinds any VAO.
        /// </summary>
        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws the point cloud using GL.DrawArrays with PrimitiveType.Points.
        /// </summary>
        public void Draw(PrimitiveType mode = PrimitiveType.Points)
        {
            Bind();
            GL.DrawArrays(mode, 0, VertexCount);
            Unbind();
        }

        /// <summary>
        /// Releases the OpenGL resources held by this point cloud.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            GC.SuppressFinalize(this);
        }
    }
}
