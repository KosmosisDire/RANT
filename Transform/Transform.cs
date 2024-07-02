using System.Numerics;
namespace RantCore;


public class Transform
{
    public Transform? Parent { get; private set; }
    private Vector3 _position;
    private Quaternion _rotation;
    private Vector3 _scale;
    public Vector3 Position { get => _position; set => _position = value; }
    public Quaternion Rotation { get => _rotation; set => _rotation = value; }
    private Vector3 Scale { get => _scale; set => _scale = value; }


    public Transform(Vector3 position, Quaternion rotation = default, Vector3 scale = default)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public void SetParent(Transform? parent)
    {
        Parent = parent;
    }

    private Matrix4x4 ToWorldMatrix()
    {
        var matrix = ToLocalMatrix();
        Matrix4x4.Invert(matrix, out matrix);
        return matrix;
    }

    public Matrix4x4 ToLocalMatrix()
    {
        Matrix4x4 matrix = Matrix4x4.CreateScale(Scale);
        matrix *= Matrix4x4.CreateFromQuaternion(Rotation);
        matrix *= Matrix4x4.CreateTranslation(Position);
        matrix *= Parent?.ToLocalMatrix() ?? Matrix4x4.Identity;
        return matrix;
    }


    public Vector3 TransformPoint(Vector3 point)
    {
        return Vector3.Transform(point, ToLocalMatrix());
    }

    public Vector3 InverseTransformPoint(Vector3 point)
    {
        return Vector3.Transform(point, ToWorldMatrix());
    }

    public Vector3 TransformVector(Vector3 vector)
    {
        return Vector3.TransformNormal(vector, ToLocalMatrix());
    }

    public Vector3 InverseTransformVector(Vector3 vector)
    {
        return Vector3.TransformNormal(vector, ToWorldMatrix());
    }

    public Vector3 TransformDirection(Vector3 direction)
    {
        Matrix4x4.Decompose(ToLocalMatrix(), out _, out var rot, out _);
        var mat = Matrix4x4.CreateFromQuaternion(rot);
        return Vector3.TransformNormal(direction, mat);
    }

    public Vector3 InverseTransformDirection(Vector3 direction)
    {
        Matrix4x4.Decompose(ToWorldMatrix(), out _, out var rot, out _);
        var mat = Matrix4x4.CreateFromQuaternion(rot);
        return Vector3.TransformNormal(direction, mat);
    }

    public static Matrix4x4 FromToMatrix(Transform from, Transform to)
    {
        return from.ToWorldMatrix() * to.ToLocalMatrix();
    }


}