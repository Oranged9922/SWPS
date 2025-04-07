using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using SWPS.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SWPS;

public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Dictionary<ShaderType, string> shader) : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    private float[] vertices { get; set; } = [];
    private uint[] indices { get; set; } = [];


    private Stopwatch? _timer;

    private int _elementBufferObject;

    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private Shader? _shader;

    private Camera _camera;

    private bool _firstMove = true;
    private bool _isWireframe = false;
    private Vector2 _lastPos;

    private double _time;
    protected override void OnLoad()
    {
        base.OnLoad();
        LoadRoom();
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        _shader = new Shader(shader);
        _shader.Use();

        var vertexLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
        Console.WriteLine($"Graphics card used: {GL.GetString(StringName.Vendor)},{GL.GetString(StringName.Renderer)} , GL version: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        _timer = new Stopwatch();
        _timer.Start();
    }

    private void LoadRoom()
    {
        var room = ObjParser.Load("ObjFiles/Room.obj");
        vertices = [.. room.Vertices];
        indices = [.. room.Indices];
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.CullFace(TriangleFace.Back);
        _shader!.Use();

        double timeValue = _timer!.Elapsed.TotalSeconds;

        int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");

        GL.Uniform4(vertexColorLocation, 0.1f, 0.1f, 0.1f, 1.0f);

        GL.BindVertexArray(_vertexArrayObject);

        var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
        _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        SwapBuffers();
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