using UnityEngine;
namespace Decel
{
    public static class ThrusterAllocator2D
    {
        /// <summary>
        /// Projected Gauss-Newton allocator for 2D thrusters.
        /// thrusterWrench: n x 3 matrix (row i: [Fx_i, Fy_i, Tau_i] per unit thrust)
        /// desired: target wrench [Fx, Fy, Tau]
        /// uMin/uMax: optional bounds (length n). If null, defaults to [0, +inf).
        /// Returns thrust vector u (length n).
        /// </summary>
        public static float[] Allocate(
            float[,] thrusterWrench,   // n x 3
            Vector3 desired,
            float[] uInitial = null,   // optional initial guess (length n)
            float[] uMin = null,
            float[] uMax = null,
            int maxIters = 10,
            float lambda = 1e-3f,      // Tikhonov damping
            float tol = 1e-4f)
        {
            int n = thrusterWrench.GetLength(0);
            // A = J^T is 3 x n; but we work with J (n x 3) to build AtA (n x n) and Atb (n)
            float[] u = new float[n];

            // Bounds defaults
            if (uMin == null) { uMin = new float[n]; for (int i = 0; i < n; i++) uMin[i] = 0f; }
            if (uMax == null) { uMax = new float[n]; for (int i = 0; i < n; i++) uMax[i] = float.PositiveInfinity; }

            // Initial guess
            if (uInitial != null && uInitial.Length == n)
            {
                for (int i = 0; i < n; i++) u[i] = Mathf.Clamp(uInitial[i], uMin[i], uMax[i]);
            }
            else
            {
                for (int i = 0; i < n; i++) u[i] = Mathf.Clamp(0f, uMin[i], uMax[i]);
            }

            // Precompute A^T A (n x n) and A^T b (n)
            // A = [Fx; Fy; Tau] rows over n columns; equivalently use J (n x 3)
            float[,] AtA = new float[n, n];
            float[] Atb = new float[n];
            {
                // Build AtA = sum over k in {0..2} (col_k * col_k^T) using J (n x 3)
                for (int k = 0; k < 3; k++)
                {
                    // Collect column k from J
                    // For each (i,j): AtA[i,j] += J[i,k]*J[j,k]
                    for (int i = 0; i < n; i++)
                    {
                        float Ji = thrusterWrench[i, k];
                        for (int j = i; j < n; j++)
                        {
                            float v = Ji * thrusterWrench[j, k];
                            AtA[i, j] += v;
                            if (j != i) AtA[j, i] += v;
                        }
                    }
                }

                // Atb = A^T * desired = sum over axes k of desired_k * J[:,k]
                float[] b = new float[3] { desired.x, desired.y, desired.z };
                for (int i = 0; i < n; i++)
                {
                    float s = 0f;
                    for (int k = 0; k < 3; k++) s += thrusterWrench[i, k] * b[k];
                    Atb[i] = s;
                }
            }

            // Iterative projected Gauss-Newton (for bounds handling)
            float prevCost = float.PositiveInfinity;
            for (int it = 0; it < maxIters; it++)
            {
                // Normal equations: (AtA + lambda I) * delta = Atb - AtA * u
                float[] rhs = new float[n];
                MultiplySymmetric(AtA, u, rhs);
                for (int i = 0; i < n; i++) rhs[i] = Atb[i] - rhs[i];

                // Solve (AtA + λI) * delta = rhs
                float[,] M = new float[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++) M[i, j] = AtA[i, j];
                    M[i, i] += lambda;
                }
                float[] delta = SolveSPD_Cholesky(M, rhs);

                // Update + project to bounds
                for (int i = 0; i < n; i++)
                {
                    u[i] = Mathf.Clamp(u[i] + delta[i], uMin[i], uMax[i]);
                }

                // Evaluate cost: 0.5 * ||A u - b||^2
                Vector3 Au = MultiplyA_u(thrusterWrench, u); // 3x1
                Vector3 r = Au - desired;
                float cost = 0.5f * Vector3.Dot(r, r);

                if (Mathf.Abs(prevCost - cost) < tol) break;
                prevCost = cost;
            }

            return u;
        }

        // Compute A u where A = J^T, J = thrusterWrench (n x 3). Returns 3x1.
        private static Vector3 MultiplyA_u(float[,] J, float[] u)
        {
            int n = J.GetLength(0);
            float fx = 0f, fy = 0f, tz = 0f;
            for (int i = 0; i < n; i++)
            {
                float ui = u[i];
                fx += ui * J[i, 0];
                fy += ui * J[i, 1];
                tz += ui * J[i, 2];
            }
            return new Vector3(fx, fy, tz);
        }

        // y = (AtA) * x for symmetric AtA (n x n)
        private static void MultiplySymmetric(float[,] S, float[] x, float[] y)
        {
            int n = x.Length;
            for (int i = 0; i < n; i++) y[i] = 0f;
            for (int i = 0; i < n; i++)
            {
                float sum = 0f;
                for (int j = 0; j < n; j++) sum += S[i, j] * x[j];
                y[i] = sum;
            }
        }

        // Solve (SPD) * x = b via Cholesky. Returns zero vector if factorization fails.
        private static float[] SolveSPD_Cholesky(float[,] A, float[] b)
        {
            int n = b.Length;
            float[,] L = new float[n, n];

            // Cholesky factorization A = L L^T
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float sum = A[i, j];
                    for (int k = 0; k < j; k++) sum -= L[i, k] * L[j, k];
                    if (i == j)
                    {
                        if (sum <= 0f) return new float[n]; // not SPD; return zeros
                        L[i, j] = Mathf.Sqrt(sum);
                    }
                    else
                    {
                        L[i, j] = sum / L[j, j];
                    }
                }
            }

            // Forward solve L y = b
            float[] y = new float[n];
            for (int i = 0; i < n; i++)
            {
                float sum = b[i];
                for (int k = 0; k < i; k++) sum -= L[i, k] * y[k];
                y[i] = sum / L[i, i];
            }

            // Backward solve L^T x = y
            float[] x = new float[n];
            for (int i = n - 1; i >= 0; i--)
            {
                float sum = y[i];
                for (int k = i + 1; k < n; k++) sum -= L[k, i] * x[k];
                x[i] = sum / L[i, i];
            }
            return x;
        }
    }
}