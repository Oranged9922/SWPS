using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace SWPS;

public static class Program
{
    private static void Main()
    {

        Dictionary<ShaderType, string> shader = new()
        {
            { ShaderType.VertexShader, "Shaders/shader.vert" },
            { ShaderType.FragmentShader, "Shaders/shader.frag" },
        };

    var nativeWindowSettings = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "SWPS",
        };

        using var window = new Window(GameWindowSettings.Default, nativeWindowSettings, shader);
        window.Run();
    }
}