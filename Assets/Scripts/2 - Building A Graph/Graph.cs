using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    Transform[] points;

    [Range(10, 200)] public int resolution = 10;

    public GraphFunctionName function;

    static GraphFunction[] functions = { SineFunction, Sine2DFunction, MultiSineFunction, MultiSine2DFunction, Ripple, Cylinder, Sphere, Torus };

    void Awake()
    {
        points = new Transform[resolution * resolution];

        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;

        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
    }

    void Update()
    {
        float time = Time.time;
        GraphFunction f = functions[(int)function];
        float step = 2f / resolution;

        for (int i = 0, z = 0; z < resolution; z++)
        {
            float v = (z + 0.5f) * step - 1f;

            for (int x = 0; x < resolution; x++, i++)
            {
                float u = (x + 0.5f) * step - 1f;
                points[i].localPosition = f(u, v, time);
            }
        }
    }

    const float pi = Mathf.PI;

    static Vector3 SineFunction(float _x, float _z, float _t)
    {
        Vector3 p;
        p.x = _x;
        p.y = Mathf.Sin(pi * (_x + _t));
        p.z = _z;
        return p;
    }

    static Vector3 MultiSineFunction(float _x, float _z, float _t)
    {
        Vector3 p;
        p.x = _x;
        p.y = Mathf.Sin(pi * (_x + _t));
        p.y += Mathf.Sin(2f * pi * (_x + _t)) / 2f;
        p.y *= 2f / 3f;
        p.z = _z;
        return p;
    }

    static Vector3 Sine2DFunction(float _x, float _z, float _t)
    {
        Vector3 p;
        p.x = _x;
        p.y = Mathf.Sin(pi * (_x + _t));
        p.y += Mathf.Sin(pi * (_z + _t));
        p.y *= 0.5f;
        p.z = _z;
        return p;
    }

    static Vector3 MultiSine2DFunction(float _x, float _z, float _t)
    {
        Vector3 p;
        p.x = _x;
        p.y = 4f * Mathf.Sin(pi * (_x + _z + _t * 0.5f));
        p.y += Mathf.Sin(pi * (_x + _t));
        p.y += Mathf.Sin(2f * pi * (_z + _t)) * 0.5f;
        p.y *= 1f / 5.5f;
        p.z = _z;
        return p;
    }

    static Vector3 Ripple(float _x, float _z, float _t)
    {
        Vector3 p;
        float d = Mathf.Sqrt(_x * _x + _z * _z);
        p.x = _x;
        p.y = Mathf.Sin(pi * (4f * d - _t));
        p.y /= 1f + 10f * d;
        p.z = _z;
        return p;
    }

    static Vector3 Cylinder(float _u, float _v, float _t)
    {
        Vector3 p;
        float r = 0.8f + Mathf.Sin(pi * (6f * _u + 2f * _v + _t)) * 0.2f;
        p.x = r * Mathf.Sin(pi * _u);
        p.y = _v;
        p.z = r * Mathf.Cos(pi * _u);
        return p;
    }

    static Vector3 Sphere(float _u, float _v, float _t)
    {
        Vector3 p;
        float r = 0.8f + Mathf.Sin(pi * (6f * _u + _t)) * 0.1f;
        r += Mathf.Sin(pi * (4f * _v + _t)) * 0.1f;
        float s = r * Mathf.Cos(pi * 0.5f * _v);
        p.x = s * Mathf.Sin(pi * _u);
        p.y = r * Mathf.Sin(pi * 0.5f * _v);
        p.z = s * Mathf.Cos(pi * _u);
        return p;
    }

    static Vector3 Torus(float _u, float _v, float _t)
    {
        Vector3 p;
        float r1 = 0.65f + Mathf.Sin(pi * (6f * _u + _t)) * 0.1f;
        float r2 = 0.2f + Mathf.Sin(pi * (4f * _v + _t)) * 0.05f;
        float s = r2 * Mathf.Cos(pi * _v) + r1;
        p.x = s * Mathf.Sin(pi * _u);
        p.y = r2 * Mathf.Sin(pi * _v);
        p.z = s * Mathf.Cos(pi * _u);
        return p;
    }
}