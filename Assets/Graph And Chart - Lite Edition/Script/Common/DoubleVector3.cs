using System;
using UnityEngine;

namespace ChartAndGraph
{
    public struct DoubleVector3
    {
        public double x;

        public double y;

        public double z;

        public double this[int index]
        {
            get
            {
                double result;
                switch (index)
                {
                    case 0:
                        result = x;
                        break;
                    case 1:
                        result = y;
                        break;
                    case 2:
                        result = z;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid DoubleVector3 index!");
                }

                return result;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid DoubleVector3 index!");
                }
            }
        }

        public Vector2 ToVector2()
        {
            return new Vector2((float)x, (float)y);
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        public Vector3 ToVector4()
        {
            return new Vector4((float)x, (float)y, (float)z, 0f);
        }

        public DoubleVector4 ToDoubleVector4()
        {
            return new DoubleVector4(x, y, z, 0.0);
        }

        public DoubleVector2 ToDoubleVector2()
        {
            return new DoubleVector2(x, y);
        }

        public DoubleVector3 normalized => Normalize(this);

        public double magnitude => Math.Sqrt(x * x + y * y + z * z);

        public double sqrMagnitude => x * x + y * y + z * z;

        public static DoubleVector3 zero => new DoubleVector3(0f, 0f, 0f);

        public static DoubleVector3 one => new DoubleVector3(1f, 1f, 1f);

        public static DoubleVector3 forward => new DoubleVector3(0f, 0f, 1f);

        public static DoubleVector3 back => new DoubleVector3(0f, 0f, -1f);

        public static DoubleVector3 up => new DoubleVector3(0f, 1f, 0f);

        public static DoubleVector3 down => new DoubleVector3(0f, -1f, 0f);

        public static DoubleVector3 left => new DoubleVector3(-1f, 0f, 0f);

        public static DoubleVector3 right => new DoubleVector3(1f, 0f, 0f);

        [Obsolete("Use DoubleVector3.forward instead.")]
        public static DoubleVector3 fwd => new DoubleVector3(0f, 0f, 1f);

        public DoubleVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public DoubleVector3(double x, double y)
        {
            this.x = x;
            this.y = y;
            z = 0f;
        }

        public DoubleVector3(Vector3 v)
            : this(v.x, v.y, v.z)
        {
        }

        public static DoubleVector3 Lerp(DoubleVector3 a, DoubleVector3 b, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));
            return new DoubleVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static DoubleVector3 LerpUnclamped(DoubleVector3 a, DoubleVector3 b, double t)
        {
            return new DoubleVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static DoubleVector3 MoveTowards(DoubleVector3 current, DoubleVector3 target, double maxDistanceDelta)
        {
            var a = target - current;
            var magnitude = a.magnitude;
            DoubleVector3 result;
            if (magnitude <= maxDistanceDelta || magnitude == 0f)
                result = target;
            else
                result = current + a / magnitude * maxDistanceDelta;
            return result;
        }

        public void Set(double new_x, double new_y, double new_z)
        {
            x = new_x;
            y = new_y;
            z = new_z;
        }

        public static DoubleVector3 Scale(DoubleVector3 a, DoubleVector3 b)
        {
            return new DoubleVector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Scale(DoubleVector3 scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        public static DoubleVector3 Cross(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return new DoubleVector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            bool result;
            if (!(other is DoubleVector3))
            {
                result = false;
            }
            else
            {
                var vector = (DoubleVector3)other;
                result = x.Equals(vector.x) && y.Equals(vector.y) && z.Equals(vector.z);
            }

            return result;
        }

        public static DoubleVector3 Reflect(DoubleVector3 inDirection, DoubleVector3 inNormal)
        {
            return -2f * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static DoubleVector3 Normalize(DoubleVector3 value)
        {
            var num = Magnitude(value);
            DoubleVector3 result;
            if (num > 1E-05f)
                result = value / num;
            else
                result = zero;
            return result;
        }

        public void Normalize()
        {
            var num = Magnitude(this);
            if (num > 1E-05f)
                this /= num;
            else
                this = zero;
        }

        public static double Dot(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }


        public static double Distance(DoubleVector3 a, DoubleVector3 b)
        {
            var vector = new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static DoubleVector3 ClampMagnitude(DoubleVector3 vector, double maxLength)
        {
            DoubleVector3 result;
            if (vector.sqrMagnitude > maxLength * maxLength)
                result = vector.normalized * maxLength;
            else
                result = vector;
            return result;
        }

        public static double Magnitude(DoubleVector3 a)
        {
            return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        }

        public static double SqrMagnitude(DoubleVector3 a)
        {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }

        public static DoubleVector3 Min(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return new DoubleVector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        }

        public static DoubleVector3 Max(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return new DoubleVector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
        {
            return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b)
        {
            return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static DoubleVector3 operator -(DoubleVector3 a)
        {
            return new DoubleVector3(-a.x, -a.y, -a.z);
        }

        public static DoubleVector3 operator *(DoubleVector3 a, double d)
        {
            return new DoubleVector3(a.x * d, a.y * d, a.z * d);
        }

        public static DoubleVector3 operator *(double d, DoubleVector3 a)
        {
            return new DoubleVector3(a.x * d, a.y * d, a.z * d);
        }

        public static DoubleVector3 operator /(DoubleVector3 a, double d)
        {
            return new DoubleVector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
        }

        public static bool operator !=(DoubleVector3 lhs, DoubleVector3 rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", new object[]
            {
                x,
                y,
                z
            });
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", new object[]
            {
                x.ToString(format),
                y.ToString(format),
                z.ToString(format)
            });
        }
    }
}