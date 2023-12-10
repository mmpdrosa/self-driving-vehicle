using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Path = System.Collections.Generic.List<PathSegment>;

public class ReedsShepp : MonoBehaviour
{
    [SerializeField]
    private Car startCar, endCar;

    PathDrawer drawer;

    void Awake()
    {
        drawer = GetComponent<PathDrawer>();    
    }

    void Start()
    {
        Path path = GetOptimalPath(startCar, endCar);

        Debug.Log(startCar.rearWheelPosition + " " + startCar.headingAngle + " " + endCar.rearWheelPosition);

        drawer.Draw(
            startCar.rearWheelPosition.x,
            startCar.rearWheelPosition.z,
            startCar.headingAngle,
            endCar.rearWheelPosition.x,
            endCar.rearWheelPosition.z,
            endCar.headingAngle,
            path
        );
    }

    void Update()
    {
        
    }

    private float PathLength(Path path)
    {
        return path.Sum(segment => segment.distance);
    }


    public Path GetOptimalPath(Car startCar, Car endCar)
    {
        List<Path> paths = GetAllPaths(startCar, endCar);

        Path optimalPath = paths.OrderBy(PathLength).First();

        return optimalPath;
    }

    public List<Path> GetAllPaths(Car startCar, Car endCar)
    {
        List<System.Func<float, float, float, Path>> pathFuncs = new List<System.Func<float, float, float, Path>> 
        { Path1, Path2, Path3, Path4, Path5, Path6, Path7, Path8, Path9, Path10, Path11, Path12 };

        List<Path> paths = new List<Path>();

        // somar 90 no ângulo pq 0 graus do algoritmo é 90 graus no unity
        (float x, float y, float theta) = ChangeOfBasis(
            startCar.rearWheelPosition.x,
            startCar.rearWheelPosition.z,
            90f - startCar.headingAngle,
            endCar.rearWheelPosition.x,
            endCar.rearWheelPosition.z,
            90f - endCar.headingAngle,
            5.9f
        );

        foreach (var getPath in pathFuncs)
        {
            paths.Add(getPath(x, y, theta));
            paths.Add(Timeflip(getPath(-x, y, -theta)));
            paths.Add(Reflect(getPath(x, -y, -theta)));
            paths.Add(Reflect(Timeflip(getPath(-x, -y, theta))));
        }

        for (int i = 0; i < paths.Count; i++)
        {
            paths[i] = paths[i].FindAll(e => e.distance != 0);
        }

        paths.RemoveAll(p => p.Count == 0);

        return paths;

    }

    private Path Timeflip(Path path)
    {
        foreach (var segment in path) 
        {
            segment.ReverseGear();
        }

        return path;
    }

    private Path Reflect(Path path)
    {
        foreach (var segment in path)
        {
            segment.ReverseSteering();
        }

        return path;
    }

    private (float x, float y, float theta) ChangeOfBasis(float x1, float y1, float theta1, float x2, float y2, float theta2, float turningRadius = 1f)
    {
        float dx = (x2 - x1) / turningRadius; 
        float dy = (y2 - y1) / turningRadius;

        float theta = theta2 - theta1;

        theta1 *= Mathf.Deg2Rad;

        float x = dx * Mathf.Cos(theta1) + dy * Mathf.Sin(theta1);
        float y = -dx * Mathf.Sin(theta1) + dy * Mathf.Cos(theta1);


        return (x, y, theta);
    }

    private Path Path1(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        (float u, float t) = R(x - Mathf.Sin(phi), y - 1 + Mathf.Cos(phi));
        float v = M(phi - t);

        Path path = new Path 
        {
            new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
            new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Forward),
            new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Forward)
        };

        return path;
    }

    private Path Path2(float x, float y, float phi)
    {
        phi = M(phi * Mathf.Deg2Rad);

        (float rho, float t1) = R(x + Mathf.Sin(phi), y - 1 - Mathf.Cos(phi));

        float t, u, v;

        Path path = new Path();

        if (rho * rho >= 4)
        {
            u = Mathf.Sqrt(rho * rho - 4);
            t = M(t1 + Mathf.Atan2(2, u));
            v = M(t - phi);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Forward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Forward)
            };
        }

        return path;
    }

    private Path Path3(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x - Mathf.Sin(phi);
        float eta = y - 1 + Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho <= 4)
        {
            A = Mathf.Acos(rho / 4f);
            t = M(theta + Mathf.PI / 2f + A);
            u = M(Mathf.PI - 2f * A);
            v = M(phi - t - u);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Forward)
            };
        }

        return path;
    }

    private Path Path4(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x - Mathf.Sin(phi);
        float eta = y - 1 + Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho <= 4)
        {
            A = Mathf.Acos(rho / 4f);
            t = M(theta + Mathf.PI / 2f + A);
            u = M(Mathf.PI - 2f * A);
            v = M(t + u - phi);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Backward)
            };
        }

        return path;

    }

    private Path Path5(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x - Mathf.Sin(phi);
        float eta = y - 1 + Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho <= 4)
        {
            u = Mathf.Acos(1 - rho * rho / 8f);
            A = Mathf.Asin(2 * Mathf.Sin(u) / rho);
            t = M(theta + Mathf.PI / 2f - A);
            v = M(t - u - phi);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Right, PathSegment.Gear.Forward),
                new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path6(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x + Mathf.Sin(phi);
        float eta = y - 1 - Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho <= 4f)
        {
            if (rho <= 2f)
            {
                A = Mathf.Acos((rho + 2f) / 4f);
                t = M(theta + Mathf.PI / 2f + A);
                u = M(A);
                v = M(phi - t + 2f * u);
                
            } else
            {
                A = Mathf.Acos((rho - 2f) / 4f);
                t = M(theta + Mathf.PI / 2f - A);
                u = M(Mathf.PI - A);
                v = M(phi - t + 2f * u);
            }

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Right, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Left, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path7(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x + Mathf.Sin(phi);
        float eta = y - 1 - Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);
        float u1 = (20f - rho * rho) / 16f;

        float t, u, v, A;

        Path path = new Path();

        if (rho <= 6 && (u1 >= 0 && u1 <= 1f))
        {
            u = Mathf.Acos(u1);
            A = Mathf.Asin(2f * Mathf.Sin(u) / rho);
            t = M(theta + Mathf.PI / 2f + A);
            v = M(t - phi);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(u, PathSegment.Steering.Left, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Forward)
            };
        }

        return path;
    }

    private Path Path8(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x - Mathf.Sin(phi);
        float eta = y - 1 + Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho >= 2)
        {
            u = Mathf.Sqrt(rho * rho - 4f) - 2f;
            A = Mathf.Atan2(2, u + 2);
            t = M(theta + Mathf.PI / 2f + A);
            v = M(t - phi + Mathf.PI / 2f);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path9(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x - Mathf.Sin(phi);
        float eta = y - 1 + Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho >= 2)
        {
            u = Mathf.Sqrt(rho * rho - 4f) - 2f;
            A = Mathf.Atan2(u + 2f, 2f);
            t = M(theta + Mathf.PI / 2f - A);
            v = M(t - phi - Mathf.PI / 2f);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Forward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Right, PathSegment.Gear.Forward),
                new PathSegment(v, PathSegment.Steering.Left, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path10(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x + Mathf.Sin(phi);
        float eta = y - 1 - Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v;

        Path path = new Path();

        if (rho >= 2)
        {
            t = M(theta + Mathf.PI / 2f);
            u = rho - 2;
            v = M(phi - t - Mathf.PI / 2f);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path11(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x + Mathf.Sin(phi);
        float eta = y - 1 - Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v;

        Path path = new Path();

        if (rho >= 2)
        {
            t = M(theta);
            u = rho - 2;
            v = M(phi - t - Mathf.PI / 2f);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Forward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Backward)
            };
        }

        return path;
    }

    private Path Path12(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        float xi = x + Mathf.Sin(phi);
        float eta = y - 1 - Mathf.Cos(phi);

        (float rho, float theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new Path();

        if (rho >= 4)
        {
            u = Mathf.Sqrt(rho * rho - 4f) - 4f;
            A = Mathf.Atan2(2f, u + 4f);
            t = M(theta + Mathf.PI / 2f + A);
            v = M(t - phi);

            path = new Path
            {
                new PathSegment(t, PathSegment.Steering.Left, PathSegment.Gear.Forward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Right, PathSegment.Gear.Backward),
                new PathSegment(u, PathSegment.Steering.Straight, PathSegment.Gear.Backward),
                new PathSegment(Mathf.PI / 2f, PathSegment.Steering.Left, PathSegment.Gear.Backward),
                new PathSegment(v, PathSegment.Steering.Right, PathSegment.Gear.Forward)
            };
        }

        return path;
    }


    /// <summary>
    /// Returns the angle phi = theta mod (2 * pi) such that -pi <= theta < pi.
    /// </summary>
    /// <param name="theta">The input angle in radians.</param>
    /// <returns>The adjusted angle phi in radians.</returns>
    private float M(float theta)
    {
        float phi = theta % (2f * Mathf.PI);
        if (phi < -Mathf.PI) return phi + 2f * Mathf.PI;
        if (phi >= Mathf.PI) return phi - 2f * Mathf.PI;
        return theta;
    }

    /// <summary>
    /// Return the polar coordinates (r, theta) of the point (x, y).
    /// </summary>
    private (float, float) R(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float theta = Mathf.Atan2(y, x);
        return (r, theta);
    }
}
