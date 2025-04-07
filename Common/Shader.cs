using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SWPS.Common
{
    /// <summary>
    /// A class that represents a shader program.
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// The handle to the shader program.
        /// </summary>
        public readonly int Handle;

        /// <summary>
        /// A dictionary that holds the locations of all the uniforms in this shader.
        /// </summary>
        private readonly Dictionary<string, int> _uniformLocations;

        /// <summary>
        /// Create a new shader program from the given vertex and fragment shader source files.
        /// </summary>
        /// <param name="vertPath">The path to the vertex shader source file.</param>
        /// <param name="fragPath">The path to the fragment shader source file.</param>
        public Shader(Dictionary<ShaderType, string> shaderFiles)
        {
            Dictionary<ShaderType, int> shaders = [];
            foreach (var kvp in shaderFiles) 
            {
                var shaderSource = File.ReadAllText(kvp.Value);
                var shader = GL.CreateShader(kvp.Key);
                GL.ShaderSource(shader, shaderSource);
                CompileShader(shader);
                shaders.Add(kvp.Key, shader);
            }

            Handle = GL.CreateProgram();

            foreach (var kvp in shaders)
            {
                GL.AttachShader(Handle, kvp.Value);
            }
            
            LinkProgram(Handle);

            foreach (var kvp in shaders)
            {
                GL.DetachShader(Handle, kvp.Value);
                GL.DeleteShader(kvp.Value);
            }

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = [];

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        /// <summary>
        /// Use this shader program.
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Get the location of a uniform in this shader.
        /// </summary>
        /// <param name="attribName"> The name of the uniform</param>
        /// <returns> The location of the uniform</returns>
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}