using System.Numerics;

namespace Engine.Math;

public struct Frustum
{
    private Plane _left, _right, _top, _bottom, _near, _far;

    public static Frustum FromViewProjection(Matrix4x4 vp)
    {
        var f = new Frustum();

        // Left
        f._left = new Plane(
            vp.M14 + vp.M11, vp.M24 + vp.M21,
            vp.M34 + vp.M31, vp.M44 + vp.M41);
        // Right
        f._right = new Plane(
            vp.M14 - vp.M11, vp.M24 - vp.M21,
            vp.M34 - vp.M31, vp.M44 - vp.M41);
        // Bottom
        f._bottom = new Plane(
            vp.M14 + vp.M12, vp.M24 + vp.M22,
            vp.M34 + vp.M32, vp.M44 + vp.M42);
        // Top
        f._top = new Plane(
            vp.M14 - vp.M12, vp.M24 - vp.M22,
            vp.M34 - vp.M32, vp.M44 - vp.M42);
        // Near
        f._near = new Plane(
            vp.M13, vp.M23, vp.M33, vp.M43);
        // Far
        f._far = new Plane(
            vp.M14 - vp.M13, vp.M24 - vp.M23,
            vp.M34 - vp.M33, vp.M44 - vp.M43);

        f._left = Plane.Normalize(f._left);
        f._right = Plane.Normalize(f._right);
        f._top = Plane.Normalize(f._top);
        f._bottom = Plane.Normalize(f._bottom);
        f._near = Plane.Normalize(f._near);
        f._far = Plane.Normalize(f._far);

        return f;
    }

    public bool ContainsPoint(Vector3 point)
    {
        return DistanceToPlane(_left, point) >= 0 &&
               DistanceToPlane(_right, point) >= 0 &&
               DistanceToPlane(_top, point) >= 0 &&
               DistanceToPlane(_bottom, point) >= 0 &&
               DistanceToPlane(_near, point) >= 0 &&
               DistanceToPlane(_far, point) >= 0;
    }

    public bool IntersectsBox(BoundingBox box)
    {
        return TestPlaneBox(_left, box) &&
               TestPlaneBox(_right, box) &&
               TestPlaneBox(_top, box) &&
               TestPlaneBox(_bottom, box) &&
               TestPlaneBox(_near, box) &&
               TestPlaneBox(_far, box);
    }

    private static float DistanceToPlane(Plane plane, Vector3 point)
    {
        return plane.Normal.X * point.X + plane.Normal.Y * point.Y +
               plane.Normal.Z * point.Z + plane.D;
    }

    private static bool TestPlaneBox(Plane plane, BoundingBox box)
    {
        var pVertex = new Vector3(
            plane.Normal.X >= 0 ? box.Max.X : box.Min.X,
            plane.Normal.Y >= 0 ? box.Max.Y : box.Min.Y,
            plane.Normal.Z >= 0 ? box.Max.Z : box.Min.Z
        );
        return DistanceToPlane(plane, pVertex) >= 0;
    }
}
