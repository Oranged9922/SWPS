using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace SWPS;

public static class Program
{
    private static void Main()
    {

        Dictionary<string,Dictionary<ShaderType, string>> shader = new()
        {
            { "vertfrag", new Dictionary<ShaderType, string>
                {
                    { ShaderType.VertexShader, "Shaders/shader.vert" },
                    { ShaderType.FragmentShader, "Shaders/shader.frag" },
                }
            },
            { "compute", new Dictionary<ShaderType, string>
                {
                    { ShaderType.ComputeShader, "Shaders/shader.comp" },
                }
            },
        };

    var nativeWindowSettings = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "SWPS",
            APIVersion = new Version(4, 5),
    };

        using var window = new Window(GameWindowSettings.Default, nativeWindowSettings, shader);
        window.Run();
    }
}