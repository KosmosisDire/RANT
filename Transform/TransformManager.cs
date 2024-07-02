namespace RantCore;


public static class TransformManager
{
    private static readonly List<Transform> transforms = new List<Transform>();

    internal static void AddTransform(Transform transform)
    {
        transforms.Add(transform);
    }
}