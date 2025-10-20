using System.Collections.Generic;
using UnityEngine;

namespace Decel
{
    [System.Serializable]
    public class BodyData
    {
        [HideInInspector] public int index;
        public double mass = 1;
        public int radius = 1;
        public Body attractor;
        public List<Body> children;
        public bool forceKinematic;
        public bool hasAcceleration;
        [HideInInspector] public bool onRails;
        public double a, e, M, w, sphereOfInfluence;
        public float rotation, angularVelocity;
        public Vector2d position, velocity, acceleration;

        public BodyData() { }

        public BodyData(int index, double mass, Vector2d position, Vector2d velocity, float angularVelocity)
        {
            this.index = index;
            this.mass = mass;
            this.position = position;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
        }

        private const double eccentricityTolerance = 1e-8;
        private const double eParabolicClamp = 1e-6;
        double WrapPi(double a) { a = System.Math.IEEERemainder(a, 2.0 * Mathd.PI); return (a <= -Mathd.PI) ? a + 2.0 * Mathd.PI : (a > Mathd.PI ? a - 2.0 * Mathd.PI : a); }
        double Sinh(double a) { return (Mathd.Exp(a) - Mathd.Exp(-a)) * 0.5; }
        double Cosh(double a) { return (Mathd.Exp(a) + Mathd.Exp(-a)) * 0.5; }

        public void CartesianToKeplerian()
        {
            Vector2d relitivePosition = attractor.bodyData.position - position;
            Vector2d relitiveVelocity = attractor.bodyData.velocity - velocity;

            double mu = attractor.bodyData.mass * Constant.G;
            double r = relitivePosition.magnitude;
            double v2 = relitiveVelocity.sqrMagnitude;
            double rv = Vector2d.Dot(relitivePosition, relitiveVelocity);
            // Specific angular momentum
            double h = Vector2d.Cross(relitivePosition, relitiveVelocity);
            if (Mathd.Abs(h) < 1e-12) h = Mathd.Sign(h) * 1e-12;

            // Eccentricity vector
            // e_vec = ((v^2 - μ/r) r - (r·v) v) / μ
            Vector2d evec = new Vector2d(
                ((v2 - mu / r) * relitivePosition.x - rv * relitiveVelocity.x) / mu,
                ((v2 - mu / r) * relitivePosition.y - rv * relitiveVelocity.y) / mu
            );
            e = evec.magnitude;
            // Clamp near-parabolic to avoid singularities
            if (Mathd.Abs(e - 1.0) < eParabolicClamp) e = e < 1.0 ? 1.0 - eParabolicClamp : 1.0 + eParabolicClamp;

            // Semi-major axis from vis-viva: 1/a = 2/r - v^2/μ
            double inva = 2.0 / r - v2 / mu;
            a = 1.0 / inva; // negative if hyperbolic

            // Argument of periapsis ω
            if (e > eccentricityTolerance)
            {
                w = WrapPi(Mathd.Atan2(evec.y, evec.x));
            }
            else
            {
                // circular: define ω = 0 and carry phase via M0
                w = 0.0;
            }

            // True anomaly f
            double cosf, sinf, f;
            if (e > eccentricityTolerance)
            {
                cosf = Vector2d.Dot(evec, relitivePosition) / (e * r);
                cosf = Mathd.Clamp(cosf, -1.0, 1.0);
                sinf = Vector2d.Cross(evec, relitivePosition) / (e * r);
                f = Mathd.Atan2(sinf, cosf);
            }
            else
            {
                // circular: f = argument of position vector
                f = Mathd.Atan2(relitivePosition.y, relitivePosition.x);
            }

            // Mean anomaly at epoch M0
            if (e < 1.0)
            {
                double cosE = (e + Mathd.Cos(f)) / (1.0 + e * Mathd.Cos(f));
                cosE = Mathd.Clamp(cosE, -1.0, 1.0);
                double sinE = Mathd.Sqrt(1.0 - e * e) * Mathd.Sin(f) / (1.0 + e * Mathd.Cos(f));
                double E = Mathd.Atan2(sinE, cosE);
                M = WrapPi(E - e * Mathd.Sin(E));
            }
            else
            {
                double denom = 1.0 + e * Mathd.Cos(f);
                // Avoid division by ~0 near asymptotes
                if (Mathd.Abs(denom) < 1e-12) { denom = Mathd.Sign(denom) * 1e-12; }
                double sinhH = Mathd.Sqrt(e * e - 1.0) * Mathd.Sin(f) / denom;
                double H = Mathd.Log(sinhH + Mathd.Sqrt(sinhH * sinhH + 1.0));
                M = e * sinhH - H; // hyperbolic mean anomaly
            }

            sphereOfInfluence = 0.9431 * a * Mathd.Pow(mass / attractor.bodyData.mass, 0.4);
        }

        public void KeplerianToCartesian(out Vector2d relitivePosition, out Vector2d relitiveVelocity)
        {
            double mu = attractor.bodyData.mass * Constant.G;
            double dt = PhysicsManager.deltaTime;

            if (e < 1.0)
            {
                // Elliptic
                double n = Mathd.Sqrt(mu / (a * a * a));
                M = WrapPi(M + n * dt);

                // Solve Kepler for E (Newton)
                double E = M;
                for (int i = 0; i < 16; i++)
                {
                    double f = E - e * Mathd.Sin(E) - M;
                    double fp = 1.0 - e * Mathd.Cos(E);
                    E -= f / fp;
                }
                double cosE = Mathd.Cos(E), sinE = Mathd.Sin(E);
                double r = a * (1.0 - e * cosE);
                double x_pf = a * (cosE - e);
                double y_pf = a * Mathd.Sqrt(1.0 - e * e) * sinE;
                double p = a * (1.0 - e * e);
                double vscale = Mathd.Sqrt(mu / p);
                double vx_pf = -vscale * Mathd.Sin(Mathd.Atan2(y_pf, x_pf));
                double vy_pf = vscale * (e + Mathd.Cos(Mathd.Atan2(y_pf, x_pf)));

                // Rotate by ω
                double c = Mathd.Cos(w), s = Mathd.Sin(w);
                relitivePosition = attractor.bodyData.position - new Vector2d(c * x_pf - s * y_pf, s * x_pf + c * y_pf);
                relitiveVelocity = attractor.bodyData.velocity - new Vector2d(c * vx_pf - s * vy_pf, s * vx_pf + c * vy_pf);
            }
            else
            {
                // Hyperbolic
                double aAbs = Mathd.Abs(a);
                double n = Mathd.Sqrt(mu / (aAbs * aAbs * aAbs)); // "mean motion" magnitude
                M = M + n * dt;              // hyperbolic mean anomaly
                double f;

                // Solve hyperbolic Kepler: e*sinhH - H = M
                double H = Mathd.Log(2.0 * Mathd.Abs(M) / e + 1.8); // good starter
                for (int i = 0; i < 20; i++)
                {
                    double sH = Sinh(H), cH = Cosh(H);
                    f = e * sH - H - M;
                    double fp = e * cH - 1.0;
                    double fpp = e * sH;
                    // Halley for robustness
                    double dH = 2 * f * fp / (2 * fp * fp - f * fpp);
                    H -= dH;
                    if (Mathd.Abs(dH) < 1e-12) break;
                }
                double cH2 = Cosh(H), sH2 = Sinh(H);
                double r = a * (1.0 - e * cH2); // note: a<0, r>0
                double x_pf = a * (cH2 - e);
                double y_pf = a * Mathd.Sqrt(e * e - 1.0) * sH2;
                double p = a * (1.0 - e * e); // negative
                double vscale = Mathd.Sqrt(mu / Mathd.Abs(p));
                // True anomaly from H (for velocity direction)
                double cosf = (cH2 - e) / (1.0 - e * cH2);
                double sinf = Mathd.Sqrt(e * e - 1.0) * sH2 / (1.0 - e * cH2);
                f = Mathd.Atan2(sinf, cosf);
                double vx_pf = -vscale * Mathd.Sin(f);
                double vy_pf = vscale * (e + Mathd.Cos(f));

                double c = Mathd.Cos(w), s = Mathd.Sin(w);
                relitivePosition = attractor.bodyData.position - new Vector2d(c * x_pf - s * y_pf, s * x_pf + c * y_pf);
                relitiveVelocity = attractor.bodyData.velocity - new Vector2d(c * vx_pf - s * vy_pf, s * vx_pf + c * vy_pf);
            }
        }

        // // ---- Convenience: re-Keplerize at SOI swap ----
        // // vessel_bary: barycentric; newPrimary_bary: barycentric (both at t_s)
        // public static Orbit2D RecomputeKeplerAtSwap(
        //     dvec2 vessel_pos_km, dvec2 vessel_vel_kms,
        //     dvec2 newPrimary_pos_km, dvec2 newPrimary_vel_kms,
        //     double mu_newPrimary, double t_s)
        // {
        //     dvec2 rRel = new dvec2(vessel_pos_km.x - newPrimary_pos_km.x, vessel_pos_km.y - newPrimary_pos_km.y);
        //     dvec2 vRel = new dvec2(vessel_vel_kms.x - newPrimary_vel_kms.x, vessel_vel_kms.y - newPrimary_vel_kms.y);
        //     return CartesianToKepler2D(rRel, vRel, mu_newPrimary, t_s);
        // }
    }
}