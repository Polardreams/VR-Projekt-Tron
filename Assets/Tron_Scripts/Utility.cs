using UnityEngine;

public static class Utility
{
    /// <summary>
    /// Get a point on a curve between 3 points. Uses quadratic bezier calculation.
    /// </summary>
    /// <param name="p0">Point 00 in world space.</param>
    /// <param name="p1">Point 01 in world space.</param>
    /// <param name="p2">Point 02 in world space.</param>
    /// <param name="t">Position of the requested point on curve with 0 == Start and 1 == End.</param>
    /// <returns>Returns a point on the curve created by the three points passed as argument.</returns>
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }
}
