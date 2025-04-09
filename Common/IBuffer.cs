using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWPS.Common;

/// <summary>
/// An interface that defines the basic operations for a buffer.
/// </summary>
public interface IBuffer
{
    /// <summary>
    /// Binds the buffer.
    /// </summary>
    void Bind();
    /// <summary>
    /// Unbinds the buffer.
    /// </summary>
    void Unbind();
    /// <summary>
    /// Draws the buffer.
    /// </summary>
    void Draw(PrimitiveType mode);

    /// <summary>
    /// Disposes the buffer.
    /// </summary>
    void Dispose();
}
