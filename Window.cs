using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using SWPS.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SWPS;

public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Dictionary<string, Dictionary<ShaderType, string>> shaders) : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    MeshBuffer room;
    PointCloudBuffer pointCloudBuffer;

    private Stopwatch? _timer;
    private int _elementBufferObject;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private Dictionary<string, Shader> _shaders = [];
    private Camera _camera;
    private bool _firstMove = true;
    private bool _isWireframe = false;
    private Vector2 _lastPos;
    private double _time;

    private const int texWidth = 128;
    private const int texHeight = 128;
    private const int texDepth = 128;
    private int volumeTexture;

    protected override void OnLoad()
    {
        base.OnLoad();

#if DEBUG
        WriteGPUInfo();
#endif
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        this.CursorState = CursorState.Hidden;
        CreateShaders();
        var roomMesh = ObjParser.Load("ObjFiles/Room.obj");
        room = new MeshBuffer(roomMesh);
        room.Unbind();
        var pointsInsideRoom = Voxelizer.CalculatePointsInsideMesh(roomMesh, 0.1f).SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray();
        pointCloudBuffer = new PointCloudBuffer(pointsInsideRoom);
        pointCloudBuffer.Unbind();

        GL.GenTextures(1, out volumeTexture);
        GL.BindTexture(TextureTarget.Texture3D, volumeTexture);

        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgba32f,
                      texWidth, texHeight, texDepth, 0,
                      PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GL.BindTexture(TextureTarget.Texture3D, 0);




        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        _timer = new Stopwatch();
        _timer.Start();
    }

    private void CreateShaders()
    {
        foreach (var kvp in shaders)
        {
            var shader = new Shader(kvp.Value);
            _shaders.Add(kvp.Key, shader);
        }
    }

    private static void WriteGPUInfo()
    {
        GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
        Console.WriteLine($"Graphics card used: {GL.GetString(StringName.Vendor)},{GL.GetString(StringName.Renderer)} , GL version: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");
        Console.WriteLine("Compute shader work group info:");
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out var gc0);
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 1, out var gc1);
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 2, out var gc2);
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 0, out var w0);
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 1, out var w1);
        GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 2, out var w2);
        Console.WriteLine($"Max work group count: x:{gc0}, y:{gc1}, z:{gc2}");
        Console.WriteLine($"Max work group size: x:{w0}, y:{w1}, z:{w2}");
        Console.WriteLine($"Max work group invocations: {GL.GetInteger(GetPName.MaxComputeWorkGroupInvocations)}");
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.CullFace(TriangleFace.Back);
        UseComputeShaderProgram();
        UseVertFragShaderProgram();

        SwapBuffers();
    }

    private void UseComputeShaderProgram()
    {
        _shaders["compute"].Use();
        GL.BindImageTexture(0, volumeTexture, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

        int groupCountX = (texWidth + 7) / 8;
        int groupCountY = (texHeight + 7) / 8;
        int groupCountZ = (texDepth + 7) / 8;
        GL.DispatchCompute(groupCountX, groupCountY, groupCountZ);

        GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
    }

    private void UseVertFragShaderProgram()
    {
        var roomShader = _shaders["roomMesh"];
        roomShader.Use();
        room.Bind();
        double timeValue = _timer!.Elapsed.TotalSeconds;

        var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));

        int vertexColorLocation = GL.GetUniformLocation(roomShader.Handle, "clr");

        roomShader.SetMatrix4("model", model);
        roomShader.SetMatrix4("view", _camera.GetViewMatrix());
        roomShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        GL.Uniform4(vertexColorLocation, 0.5f, 0.1f, 0.1f, 1.0f);
        room.Draw();

        var pointCloudShader = _shaders["pointCloud"];
        pointCloudShader.Use();
        pointCloudBuffer.Bind();

        pointCloudShader.SetMatrix4("model", model);
        pointCloudShader.SetMatrix4("view", _camera.GetViewMatrix());
        pointCloudShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        pointCloudShader.SetInt("volumeTexture", 0);
        // minBound and maxBound
        pointCloudShader.SetVector3("minBound", new Vector3(-5));
        pointCloudShader.SetVector3("maxBound", new Vector3(5));
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture3D, volumeTexture);
        GL.Enable(EnableCap.ProgramPointSize);
        pointCloudBuffer.Draw();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) // Check to see if the window is focused
        {
            return;
        }

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
        }

        if (input.IsKeyDown(Keys.S))
        {
            _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
        }
        if (input.IsKeyDown(Keys.F))
        {
            ToggleWireframe();
        }

        var mouse = MouseState;

        if (_firstMove)
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity;
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _camera.Fov -= e.OffsetY;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _camera.AspectRatio = Size.X / (float)Size.Y;
    }

    private void ToggleWireframe()
    {
        if (_isWireframe)
        {
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            _isWireframe = false;
        }
        else
        {
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            _isWireframe = true;
        }
    }
}