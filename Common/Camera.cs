using OpenTK.Mathematics;

namespace SWPS.Common;

/// <summary>
/// Construct a new camera object.
/// </summary>
/// <param name="position"> The position of the camera in world space.</param>
/// <param name="aspectRatio"> The aspect ratio of the viewport.</param>
public class Camera(Vector3 position, float aspectRatio)
{
    private Vector3 _front = -Vector3.UnitZ;

    private Vector3 _up = Vector3.UnitY;

    private Vector3 _right = Vector3.UnitX;

    private float _pitch;

    private float _yaw = -MathHelper.PiOver2;

    private float _fov = MathHelper.PiOver2;

    /// <summary>
    /// The position of the camera in world space.
    /// </summary>
    public Vector3 Position { get; set; } = position;

    /// <summary>
    /// The aspect ratio of the viewport.
    /// </summary>
    public float AspectRatio { private get; set; } = aspectRatio;

    /// <summary>
    /// The front vector of the camera.
    /// </summary>
    public Vector3 Front => _front;

    /// <summary>
    /// The up vector of the camera.
    /// </summary>
    public Vector3 Up => _up;

    /// <summary>
    /// The right vector of the camera.
    /// </summary>
    public Vector3 Right => _right;

    /// <summary>
    /// The pitch of the camera in degrees.
    /// </summary>
    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            var angle = MathHelper.Clamp(value, -89f, 89f);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    /// <summary>
    /// The yaw of the camera in degrees.
    /// </summary>
    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    /// <summary>
    /// The field of view (FOV) of the camera in degrees.
    /// </summary>
    public float Fov
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            var angle = MathHelper.Clamp(value, 1f, 90f);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    /// <summary>
    /// Get the view matrix of the camera.
    /// </summary>
    /// <returns> The view matrix of the camera.</returns>
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    /// <summary>
    /// Get the projection matrix of the camera.
    /// </summary>
    /// <returns> The projection matrix of the camera.</returns>
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
    }

    /// <summary>
    /// Update the camera vectors based on the current pitch and yaw.
    /// </summary>
    private void UpdateVectors()
    {
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

        _front = Vector3.Normalize(_front);

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}