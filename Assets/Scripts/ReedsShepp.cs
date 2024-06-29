using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class ReedsShepp
{
    private static float PathLength(Path path)
    {
        return path.Sum(movement => movement.Distance);
    }

    public static Path GetOptimalPath(float x1, float y1, float theta1, float x2, float y2, float theta2)
    {
        var paths = GetAllPaths(x1, y1, theta1, x2, y2, theta2);

        var optimalPath = paths.OrderBy(PathLength).First();

        return optimalPath;
    }

    public static List<Path> GetAllPaths(float x1, float y1, float theta1, float x2, float y2, float theta2)
    {
        var pathFuncs = new List<Func<float, float, float, Path>>
            { Path1, Path2, Path3, Path4, Path5, Path6, Path7, Path8, Path9, Path10, Path11, Path12 };

        var paths = new List<Path>();

        var (x, y, theta) = ChangeOfBasis(
            x1,
            y1,
            90f - theta1,
            x2,
            y2,
            90f - theta2,
            Constants.CarTurningRadius
        );

        foreach (var getPath in pathFuncs)
        {
            paths.Add(getPath(x, y, theta));
            paths.Add(TimeFlip(getPath(-x, y, -theta)));
            paths.Add(Reflect(getPath(x, -y, -theta)));
            paths.Add(Reflect(TimeFlip(getPath(-x, -y, theta))));
        }

        for (var i = 0; i < paths.Count; i++)
        {
            paths[i] = paths[i].FindAll(movement => movement.Distance != 0);

            paths[i].ForEach(movement => movement.Distance *= Constants.CarTurningRadius);
        }

        paths.RemoveAll(path => path.Count == 0);

        return paths;
    }

    private static Path TimeFlip(Path path)
    {
        foreach (var movement in path)
        {
            movement.ReverseGear();
        }

        return path;
    }

    private static Path Reflect(Path path)
    {
        foreach (var movement in path)
        {
            movement.ReverseSteering();
        }

        return path;
    }

    private static (float x, float y, float theta) ChangeOfBasis(float x1, float y1, float theta1, float x2, float y2,
        float theta2, float turningRadius = 1f)
    {
        var dx = (x2 - x1) / turningRadius;
        var dy = (y2 - y1) / turningRadius;

        var theta = theta2 - theta1;

        theta1 *= Mathf.Deg2Rad;

        var x = dx * Mathf.Cos(theta1) + dy * Mathf.Sin(theta1);
        var y = -dx * Mathf.Sin(theta1) + dy * Mathf.Cos(theta1);

        return (x, y, theta);
    }

    private static Path Path1(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var (u, t) = R(x - Mathf.Sin(phi), y - 1f + Mathf.Cos(phi));
        var v = M(phi - t);

        var path = new Path
        {
            new(t, Movement.Steering.Left, Movement.Gear.Forward),
            new(u, Movement.Steering.Straight, Movement.Gear.Forward),
            new(v, Movement.Steering.Left, Movement.Gear.Forward)
        };

        return path;
    }

    private static Path Path2(float x, float y, float phi)
    {
        phi = M(phi * Mathf.Deg2Rad);

        var (rho, t1) = R(x + Mathf.Sin(phi), y - 1f - Mathf.Cos(phi));

        Path path = new();

        if (rho * rho >= 4f)
        {
            var u = Mathf.Sqrt(rho * rho - 4f);
            var t = M(t1 + Mathf.Atan2(2f, u));
            var v = M(t - phi);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Straight, Movement.Gear.Forward),
                new(v, Movement.Steering.Right, Movement.Gear.Forward)
            };
        }

        return path;
    }

    private static Path Path3(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x - Mathf.Sin(phi);
        var eta = y - 1f + Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho <= 4f)
        {
            var A = Mathf.Acos(rho / 4f);
            var t = M(theta + Mathf.PI / 2f + A);
            var u = M(Mathf.PI - 2f * A);
            var v = M(phi - t - u);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Right, Movement.Gear.Backward),
                new(v, Movement.Steering.Left, Movement.Gear.Forward)
            };
        }

        return path;
    }

    private static Path Path4(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x - Mathf.Sin(phi);
        var eta = y - 1f + Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho <= 4f)
        {
            var A = Mathf.Acos(rho / 4f);
            var t = M(theta + Mathf.PI / 2f + A);
            var u = M(Mathf.PI - 2f * A);
            var v = M(t + u - phi);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Right, Movement.Gear.Backward),
                new(v, Movement.Steering.Left, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path5(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x - Mathf.Sin(phi);
        var eta = y - 1f + Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho <= 4f)
        {
            var u = Mathf.Acos(1f - rho * rho / 8f);
            var A = Mathf.Asin(2f * Mathf.Sin(u) / rho);
            var t = M(theta + Mathf.PI / 2f - A);
            var v = M(t - u - phi);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Right, Movement.Gear.Forward),
                new(v, Movement.Steering.Left, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path6(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x + Mathf.Sin(phi);
        var eta = y - 1f - Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        float t, u, v, A;

        Path path = new();

        if (rho <= 4f)
        {
            if (rho <= 2f)
            {
                A = Mathf.Acos((rho + 2f) / 4f);
                t = M(theta + Mathf.PI / 2f + A);
                u = M(A);
                v = M(phi - t + 2f * u);
            }
            else
            {
                A = Mathf.Acos((rho - 2f) / 4f);
                t = M(theta + Mathf.PI / 2f - A);
                u = M(Mathf.PI - A);
                v = M(phi - t + 2f * u);
            }

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Right, Movement.Gear.Forward),
                new(u, Movement.Steering.Left, Movement.Gear.Backward),
                new(v, Movement.Steering.Right, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path7(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x + Mathf.Sin(phi);
        var eta = y - 1f - Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);
        var u1 = (20f - rho * rho) / 16f;

        Path path = new();

        if (rho <= 6f && u1 >= 0 && u1 <= 1f)
        {
            var u = Mathf.Acos(u1);
            var A = Mathf.Asin(2f * Mathf.Sin(u) / rho);
            var t = M(theta + Mathf.PI / 2f + A);
            var v = M(t - phi);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Right, Movement.Gear.Backward),
                new(u, Movement.Steering.Left, Movement.Gear.Backward),
                new(v, Movement.Steering.Right, Movement.Gear.Forward)
            };
        }

        return path;
    }

    private static Path Path8(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x - Mathf.Sin(phi);
        var eta = y - 1f + Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho >= 2f)
        {
            var u = Mathf.Sqrt(rho * rho - 4f) - 2f;
            var A = Mathf.Atan2(2f, u + 2f);
            var t = M(theta + Mathf.PI / 2f + A);
            var v = M(t - phi + Mathf.PI / 2f);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(Mathf.PI / 2f, Movement.Steering.Right, Movement.Gear.Backward),
                new(u, Movement.Steering.Straight, Movement.Gear.Backward),
                new(v, Movement.Steering.Left, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path9(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x - Mathf.Sin(phi);
        var eta = y - 1f + Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho >= 2f)
        {
            var u = Mathf.Sqrt(rho * rho - 4f) - 2f;
            var A = Mathf.Atan2(u + 2f, 2f);
            var t = M(theta + Mathf.PI / 2f - A);
            var v = M(t - phi - Mathf.PI / 2f);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Straight, Movement.Gear.Forward),
                new(Mathf.PI / 2f, Movement.Steering.Right, Movement.Gear.Forward),
                new(v, Movement.Steering.Left, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path10(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x + Mathf.Sin(phi);
        var eta = y - 1f - Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho >= 2f)
        {
            var t = M(theta + Mathf.PI / 2f);
            var u = rho - 2f;
            var v = M(phi - t - Mathf.PI / 2f);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(Mathf.PI / 2f, Movement.Steering.Right, Movement.Gear.Backward),
                new(u, Movement.Steering.Straight, Movement.Gear.Backward),
                new(v, Movement.Steering.Right, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path11(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x + Mathf.Sin(phi);
        var eta = y - 1f - Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho >= 2f)
        {
            var t = M(theta);
            var u = rho - 2f;
            var v = M(phi - t - Mathf.PI / 2f);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(u, Movement.Steering.Straight, Movement.Gear.Forward),
                new(Mathf.PI / 2f, Movement.Steering.Left, Movement.Gear.Forward),
                new(v, Movement.Steering.Right, Movement.Gear.Backward)
            };
        }

        return path;
    }

    private static Path Path12(float x, float y, float phi)
    {
        phi *= Mathf.Deg2Rad;

        var xi = x + Mathf.Sin(phi);
        var eta = y - 1f - Mathf.Cos(phi);

        var (rho, theta) = R(xi, eta);

        Path path = new();

        if (rho >= 4f)
        {
            var u = Mathf.Sqrt(rho * rho - 4f) - 4f;
            var A = Mathf.Atan2(2f, u + 4f);
            var t = M(theta + Mathf.PI / 2f + A);
            var v = M(t - phi);

            path = new Path
            {
                new(t, Movement.Steering.Left, Movement.Gear.Forward),
                new(Mathf.PI / 2f, Movement.Steering.Right, Movement.Gear.Backward),
                new(u, Movement.Steering.Straight, Movement.Gear.Backward),
                new(Mathf.PI / 2f, Movement.Steering.Left, Movement.Gear.Backward),
                new(v, Movement.Steering.Right, Movement.Gear.Forward)
            };
        }

        return path;
    }


    /// <summary>
    /// Returns the angle phi = theta mod (2 * pi) such that -pi <= theta < pi.
    /// </summary>
    /// <param name="theta">The input angle in radians.</param>
    /// <returns>The adjusted angle phi in radians.</returns>
    private static float M(float theta)
    {
        theta %= (2f * Mathf.PI);
        if (theta < -Mathf.PI) return theta + 2f * Mathf.PI;
        if (theta >= Mathf.PI) return theta - 2f * Mathf.PI;
        return theta;
    }

    /// <summary>
    /// Return the polar coordinates (r, theta) of the point (x, y).
    /// </summary>
    private static (float, float) R(float x, float y)
    {
        var r = Mathf.Sqrt(x * x + y * y);
        var theta = Mathf.Atan2(y, x);
        return (r, theta);
    }
}