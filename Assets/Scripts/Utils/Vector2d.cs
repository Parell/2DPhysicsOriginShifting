// Unity C# reference source
// Copyright (c) Unity Technologies. for terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using uei = UnityEngine.Internal;

namespace UnityEngine
{
    // Representation of 3D vectors and points.
    [StructLayout(LayoutKind.Sequential), Serializable]
    public partial struct Vector2d
    {
        // *Undocumented*
        public const double kEpsilon = 0.00001f;
        // *Undocumented*
        public const double kEpsilonNormalSqrt = 1e-15f;

        // X component of the vector.
        public double x;
        // Y component of the vector.
        public double y;

        // Linearly interpolates between two vectors.
        public static Vector2d Lerp(Vector2d a, Vector2d b, double t)
        {
            t = Mathd.Clamp01(t);
            return new Vector2d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Linearly interpolates between two vectors without clamping the interpolant
        public static Vector2d LerpUnclamped(Vector2d a, Vector2d b, double t)
        {
            return new Vector2d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Moves a point /current/ in a straight line towards a /target/ point.
        public static Vector2d MoveTowards(Vector2d current, Vector2d target, double maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            double toVector_x = target.x - current.x;
            double toVector_y = target.y - current.y;

            double sqdist = toVector_x * toVector_x + toVector_y * toVector_y;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = (double)Math.Sqrt(sqdist);

            return new Vector2d(current.x + toVector_x / dist * maxDistanceDelta, current.y + toVector_y / dist * maxDistanceDelta);
        }

        public static Vector2d SmoothDamp(Vector2d current, Vector2d target, ref Vector2d currentVelocity, double smoothTime, double maxSpeed)
        {
            double deltaTime = Time.deltaTime;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static Vector2d SmoothDamp(Vector2d current, Vector2d target, ref Vector2d currentVelocity, double smoothTime)
        {
            double deltaTime = Time.deltaTime;
            double maxSpeed = Mathd.Infinity;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        // Gradually changes a vector towards a desired goal over time.
        public static Vector2d SmoothDamp(Vector2d current, Vector2d target, ref Vector2d currentVelocity, double smoothTime, [uei.DefaultValue("Mathd.Infinity")] double maxSpeed, [uei.DefaultValue("Time.deltaTime")] double deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Mathd.Max(0.0001f, smoothTime);
            double omega = 2f / smoothTime;

            double x = omega * deltaTime;
            double exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            double change_x = current.x - target.x;
            double change_y = current.y - target.y;
            Vector2d originalTo = target;

            // Clamp maximum speed
            double maxChange = maxSpeed * smoothTime;

            double maxChangeSq = maxChange * maxChange;
            double sqrmag = change_x * change_x + change_y * change_y;
            if (sqrmag > maxChangeSq)
            {
                var mag = (double)Math.Sqrt(sqrmag);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;

            double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;

            double output_x = target.x + (change_x + temp_x) * exp;
            double output_y = target.y + (change_y + temp_y) * exp;

            // Prevent overshooting
            double origMinusCurrent_x = originalTo.x - current.x;
            double origMinusCurrent_y = originalTo.y - current.y;
            double outMinusOrig_x = output_x - originalTo.x;
            double outMinusOrig_y = output_y - originalTo.y;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y > 0)
            {
                output_x = originalTo.x;
                output_y = originalTo.y;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
            }

            return new Vector2d(output_x, output_y);
        }

        // Access the x, y, z components using [0], [1], [2] respectively.
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }
        }

        // Creates a new vector with given x, y, z components.
        public Vector2d(double x, double y) { this.x = x; this.y = y; }

        // Set x, y and z components of an existing Vector2d.
        public void Set(double newX, double newY) { x = newX; y = newY; }

        // Multiplies two vectors component-wise.
        public static Vector2d Scale(Vector2d a, Vector2d b) { return new Vector2d(a.x * b.x, a.y * b.y); }

        // Multiplies every component of this vector by the same component of /scale/.
        public void Scale(Vector2d scale) { x *= scale.x; y *= scale.y; }

        // // Cross Product of two vectors.
        // public static Vector2d Cross(Vector2d lhs, Vector2d rhs)
        // {
        //     return new Vector2d(
        //         lhs.y * rhs.z - lhs.z * rhs.y,
        //         lhs.z * rhs.x - lhs.x * rhs.z,
        //         lhs.x * rhs.y - lhs.y * rhs.x);
        // }

        // used to allow Vector3s to be used as keys in hash tables
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        // also required for being able to use Vector3s as keys in hash tables
        public override bool Equals(object other)
        {
            if (!(other is Vector2d)) return false;

            return Equals((Vector2d)other);
        }

        public bool Equals(Vector2d other)
        {
            return x == other.x && y == other.y;
        }

        // Reflects a vector off the plane defined by a normal.
        public static Vector2d Reflect(Vector2d inDirection, Vector2d inNormal)
        {
            double factor = -2f * Dot(inNormal, inDirection);
            return new Vector2d(factor * inNormal.x + inDirection.x, factor * inNormal.y + inDirection.y);
        }

        // *undoc* --- we have normalized property now
        public static Vector2d Normalize(Vector2d value)
        {
            double mag = Magnitude(value);
            if (mag > kEpsilon)
                return value / mag;
            else
                return zero;
        }

        // Makes this vector have a ::ref::magnitude of 1.
        public void Normalize()
        {
            double mag = Magnitude(this);
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        // Returns this vector with a ::ref::magnitude of 1 (RO).
        public Vector2d normalized { get { return Normalize(this); } }

        // Dot Product of two vectors.
        public static double Dot(Vector2d lhs, Vector2d rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }

        // Projects a vector onto another vector.
        // public static Vector2d Project(Vector2d vector, Vector2d onNormal)
        // {
        //     double sqrMag = Dot(onNormal, onNormal);
        //     if (sqrMag < Mathd.Epsilon)
        //         return zero;
        //     else
        //     {
        //         var dot = Dot(vector, onNormal);
        //         return new Vector2d(onNormal.x * dot / sqrMag,
        //             onNormal.y * dot / sqrMag,
        //             onNormal.z * dot / sqrMag);
        //     }
        // }

        public static Vector2d Perpendicular(Vector2d inDirection)
        {
            return new Vector2d(0f - inDirection.y, inDirection.x);
        }

        public static double Cross(Vector2d a, Vector2d b)
        {
            return a.x * b.y - a.y * b.x;
        }

        // // Projects a vector onto a plane defined by a normal orthogonal to the plane.
        // public static Vector2d ProjectOnPlane(Vector2d vector, Vector2d planeNormal)
        // {
        //     double sqrMag = Dot(planeNormal, planeNormal);
        //     if (sqrMag < Mathd.Epsilon)
        //         return vector;
        //     else
        //     {
        //         var dot = Dot(vector, planeNormal);
        //         return new Vector2d(vector.x - planeNormal.x * dot / sqrMag,
        //             vector.y - planeNormal.y * dot / sqrMag,
        //             vector.z - planeNormal.z * dot / sqrMag);
        //     }
        // }

        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        public static double Angle(Vector2d from, Vector2d to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = (double)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0f;

            double dot = Mathd.Clamp(Dot(from, to) / denominator, -1f, 1f);
            return ((double)Math.Acos(dot)) * Mathd.Rad2Deg;
        }

        // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        public static double SignedAngle(Vector2d from, Vector2d to, Vector2d axis)
        {
            double unsignedAngle = Angle(from, to);

            double sign = Mathd.Sign(from.x * to.y - from.y * to.x);
            return unsignedAngle * sign;
        }

        // Returns the distance between /a/ and /b/.
        public static double Distance(Vector2d a, Vector2d b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            return (double)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        // Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        public static Vector2d ClampMagnitude(Vector2d vector, double maxLength)
        {
            double sqrmag = vector.sqrMagnitude;
            if (sqrmag > maxLength * maxLength)
            {
                double mag = (double)Math.Sqrt(sqrmag);
                //these intermediate variables force the intermediate result to be
                //of double precision. without this, the intermediate result can be of higher
                //precision, which changes behavior.
                double normalized_x = vector.x / mag;
                double normalized_y = vector.y / mag;
                return new Vector2d(normalized_x * maxLength,
                    normalized_y * maxLength);
            }
            return vector;
        }

        // *undoc* --- there's a property now
        public static double Magnitude(Vector2d vector) { return (double)Math.Sqrt(vector.x * vector.x + vector.y * vector.y); }

        // Returns the length of this vector (RO).
        public double magnitude
        {
            get { return (double)Math.Sqrt(x * x + y * y); }
        }

        // *undoc* --- there's a property now
        public static double SqrMagnitude(Vector2d vector) { return vector.x * vector.x + vector.y * vector.y; }

        // Returns the squared length of this vector (RO).
        public double sqrMagnitude { get { return x * x + y * y; } }

        // Returns a vector that is made from the smallest components of two vectors.
        public static Vector2d Min(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y));
        }

        // Returns a vector that is made from the largest components of two vectors.
        public static Vector2d Max(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y));
        }

        static readonly Vector2d zeroVector = new Vector2d(0f, 0f);
        static readonly Vector2d oneVector = new Vector2d(1f, 1f);
        static readonly Vector2d upVector = new Vector2d(0f, 1f);
        static readonly Vector2d downVector = new Vector2d(0f, -1f);
        static readonly Vector2d leftVector = new Vector2d(-1f, 0f);
        static readonly Vector2d rightVector = new Vector2d(1f, 0f);
        static readonly Vector2d positiveInfinityVector = new Vector2d(double.PositiveInfinity, double.PositiveInfinity);
        static readonly Vector2d negativeInfinityVector = new Vector2d(double.NegativeInfinity, double.NegativeInfinity);

        // Shorthand for writing @@Vector2d(0, 0, 0)@@
        public static Vector2d zero { get { return zeroVector; } }
        // Shorthand for writing @@Vector2d(1, 1, 1)@@
        public static Vector2d one { get { return oneVector; } }
        // Shorthand for writing @@Vector2d(0, 0, 1)@@
        // Shorthand for writing @@Vector2d(0, 1, 0)@@
        public static Vector2d up { get { return upVector; } }
        public static Vector2d down { get { return downVector; } }
        public static Vector2d left { get { return leftVector; } }
        // Shorthand for writing @@Vector2d(1, 0, 0)@@
        public static Vector2d right { get { return rightVector; } }
        // Shorthand for writing @@Vector2d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)@@
        public static Vector2d positiveInfinity { get { return positiveInfinityVector; } }
        // Shorthand for writing @@Vector2d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)@@
        public static Vector2d negativeInfinity { get { return negativeInfinityVector; } }

        // Adds two vectors.
        public static Vector2d operator +(Vector2d a, Vector2d b) { return new Vector2d(a.x + b.x, a.y + b.y); }
        // Subtracts one vector from another.
        public static Vector2d operator -(Vector2d a, Vector2d b) { return new Vector2d(a.x - b.x, a.y - b.y); }
        // Negates a vector.
        public static Vector2d operator -(Vector2d a) { return new Vector2d(-a.x, -a.y); }
        // Multiplies a vector by a number.
        public static Vector2d operator *(Vector2d a, double d) { return new Vector2d(a.x * d, a.y * d); }
        // Multiplies a vector by a number.
        public static Vector2d operator *(double d, Vector2d a) { return new Vector2d(a.x * d, a.y * d); }
        // Divides a vector by a number.
        public static Vector2d operator /(Vector2d a, double d) { return new Vector2d(a.x / d, a.y / d); }

        // Returns true if the vectors are equal.
        public static bool operator ==(Vector2d lhs, Vector2d rhs)
        {
            // Returns false in the presence of NaN values.
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
            double sqrmag = diff_x * diff_x + diff_y * diff_y;
            return sqrmag < kEpsilon * kEpsilon;
        }

        // Returns true if vectors are different.
        public static bool operator !=(Vector2d lhs, Vector2d rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        public static explicit operator Vector2(Vector2d vector)
        {
            return new Vector3((float)vector.x, (float)vector.y);
        }

        public static explicit operator Vector2d(Vector2 vector)
        {
            return new Vector2d(vector.x, vector.y);
        }

        public static explicit operator Vector2d(Vector3 vector)
        {
            return new Vector2d(vector.x, vector.y);
        }

        public static explicit operator Vector3(Vector2d vector)
        {
            return new Vector3((float)vector.x, (float)vector.y);
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F1";
            return string.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
        }
    }
}