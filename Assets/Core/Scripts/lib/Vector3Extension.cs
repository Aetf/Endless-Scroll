using UnityEngine;
using System.Collections;

public static class Extensions {
    /// <summary>
    /// Construct a Vector3 from a Vector2 and an optional z value
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(this Vector2 vec, float z = 0f) {
        return new Vector3(vec.x, vec.y, z);
    }

    public static Vector3 Translate(this Vector3 vec, float x, float y, float z) {
        return vec + new Vector3(x, y, z);
    }

    public static Vector2 Translate(this Vector2 vec, float x, float y) {
        return vec + new Vector2(x, y);
    }

    public static void Reset(this Transform trans) {
        trans.position = Vector3.zero;
        trans.rotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }
}
