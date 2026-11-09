/// <summary>
/// This Class Helps to give extra functionality to Vector Class
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class VectorExtention
{
	public static Vector2 DirToPoint (this Vector2 v, float degrees) {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin (radians);
        float cos = Mathf.Cos (radians);
        float tx = v.x;
        float ty = v.y;
        return new Vector2 (cos * tx - sin * ty, sin * tx + cos * ty);
    }

    public static bool IsXBetween (this Vector3 p, Vector3 p1, Vector3 p2) {
        if (p1.x < p2.x) {
            return (p.x > p1.x && p.x <= p2.x);
        } else {
            return (p.x > p2.x && p.x <= p1.x);

        }
    }

    public static Vector3 Float3ToVec3(this float3 val)
    {
        return new Vector3 (val.x, val.y, val.z);
    }

    public static float3 Vec3ToFloat3(this Vector3 vec3)
    {
        return new float3(vec3.x, vec3.y, vec3.z);
    }

    public static bool IsXBetween (this Vector3 p, Transform p1, Transform p2) {
        if (p1.position.x < p2.position.x) {
            return (p.x > p1.position.x && p.x <= p2.position.x);
        } else {
            return (p.x > p2.position.x && p.x <= p1.position.x);

        }
    }

    public static bool IsZBetween (this Vector3 p, Vector3 p1, Vector3 p2) {
        if (p1.z < p2.z) {
            return (p.z > p1.z && p.z < p2.z);
        } else {
            return (p.z > p2.z && p.z < p1.z);
        }
    }

    public static bool IsZBetween (this Vector3 p, Transform p1, Transform p2) {
        if (p1.position.z < p2.position.z) {
            return (p.z > p1.position.z && p.z < p2.position.z);
        } else {
            return (p.z > p2.position.z && p.z < p1.position.z);
        }
    }

    public static List<Vector3> GeneratePointsBetween (Vector3 from, Vector3 to, int chunkAmount) {
        //divider must be between 0 and 1
        float divider = 1f / chunkAmount;
        float linear = 0f;
        List<Vector3> result = new List<Vector3> ();
        if (chunkAmount == 0) {
            Debug.LogError ("chunkAmount Distance must be > 0 instead of " + chunkAmount);
            return null;
        }

        if (chunkAmount == 1) {
            result.Add (Vector3.Lerp (from, to, 0.5f)); //Return half/middle point
        }

        for (int i = 0; i < chunkAmount; i++) {
            if (i == 0) {
                linear = divider / 2;
            } else {
                linear += divider; //Add the divider to it to get the next distance
            }
            result.Add (Vector3.Lerp (from, to, linear));
        }

        return result;
    }

    public static float GetDefenderSeparationDistance (Vector3 from, Vector3 to, int chunkAmount) {
        //divider must be between 0 and 1
        float divider = 1f / chunkAmount;
        float linear = 0f;
        List<Vector3> result = new List<Vector3> ();
        if (chunkAmount == 0) {
            Debug.LogError ("chunkAmount Distance must be > 0 instead of " + chunkAmount);
            return 0;
        }

        if (chunkAmount == 1) {
            result.Add (Vector3.Lerp (from, to, 0.5f)); //Return half/middle point
        }

        for (int i = 0; i < chunkAmount; i++) {
            if (i == 0) {
                linear = divider / 2;
            } else {
                linear += divider; //Add the divider to it to get the next distance
            }
            result.Add (Vector3.Lerp (from, to, linear));
        }

        return Mathf.Abs (result[0].x - result[1].x);
    }
    /// <summary>
    /// Truncates the Vector3 (z value is removed)
    /// </summary>
    /// <returns>Vector2.</returns>
    /// <param name="vec3">Vector3 to truncate.</param>
    public static Vector2 ToVec2 (this Vector3 vec3) {
        return new Vector2 (vec3.x, vec3.y);
    }
    

    /// <summary>
    /// Converts a Vector2 to Vector3 with the required z value
    /// </summary>
    /// <returns>Vector3.</returns>
    /// <param name="vec2">Vector2 to convert to Vector 3.</param>
    /// <param name="zValue">z	 value.</param>
    public static Vector2 ToVec3 (this Vector2 vec2, float zValue) {
        return new Vector3 (vec2.x, vec2.y, zValue);
    }

    /// <summary>
    /// Replaces the z value of the of the Vector3
    /// </summary>
    /// <returns>Vector3 whose z value is to be changed.</returns>
    /// <param name="vec3">Vector3.</param>
    /// <param name="zVal">z value.</param>
    public static Vector3 ToVec3 (this Vector3 vec3, float zVal) {
        return new Vector3 (vec3.x, vec3.y, zVal);
    }

    public static Vector3 WithY (this Vector3 vec3, float yVal) {
        return new Vector3 (vec3.x, yVal, vec3.z);
    }

    public static float GetAngleWithXAxis (Vector3 t1, Vector3 t2) {
        Vector3 distance = t2 - t1;
        float angle = Mathf.Atan2 (distance.y, distance.x) * Mathf.Rad2Deg;
        if (angle < 0) {
            angle += 360;
        }

        return angle;
    }

    public static float GetAngle (Vector2 p1, Vector2 pMid, Vector2 p2) {
        Vector2 vBA = (pMid - p1);
        Vector2 vCA = (pMid - p2);
        return Vector2.Angle (vBA, vCA);
    }
    
    public static Vector3 GetRandomPositionInBounds(this Bounds b)
    {
        float x = UnityEngine.Random.Range(b.min.x, b.max.x);
        float y = UnityEngine.Random.Range(b.min.y, b.max.y);
        float z = UnityEngine.Random.Range(b.min.z, b.max.z);
        return new Vector3(x, y, z);
    }

    public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z)
        );
    }
    ///Interpolates between a and b by t without clamping
    ///the interpolant and makes sure the values interpolate
    ///correctly when they wrap aroud 360 degrees
    
}
