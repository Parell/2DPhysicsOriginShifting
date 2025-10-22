using System.Collections.Generic;
using UnityEngine;

namespace Decel
{
    [DefaultExecutionOrder(10)]
    public class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager Instance;
        [SerializeField] private Body _mainBody;
        [SerializeField] private float _deltaTime;
        [SerializeField] private double _currentTime;
        [SerializeField] private float _timeScale;
        [SerializeField] private float _physicsRange;
        [SerializeField] private List<BodyData> _bodyData = new List<BodyData>();
        private List<Body> _bodies = new List<Body>();
        private int tightTimeScale;
        private float looseTimeScale;

        public static Body mainBody
        {
            get { return Instance._mainBody; }
            set { Instance._mainBody = value; }
        }

        public static List<Body> bodies
        {
            get { return Instance._bodies; }
        }

        public static List<BodyData> bodyData
        {
            get { return Instance._bodyData; }
        }

        public static float deltaTime
        {
            get { return Instance._deltaTime; }
        }

        public static double currentTime
        {
            get { return Instance._currentTime; }
        }

        public static float physicsRange
        {
            get { return Instance._physicsRange; }
        }

        public static float timeScale
        {
            get { return Instance._timeScale; }
            set { Instance._timeScale = value; }
        }

        private void Awake()
        {
            Instance = this;
            Time.fixedDeltaTime = _deltaTime;
            FindAllBodies();
            Instance.ResetPosition();
            Instance.ResetVelocity();
        }

        private void FixedUpdate()
        {
            _timeScale = Mathf.Clamp(_timeScale, 0, 100);
            tightTimeScale = _timeScale < 1 ? 1 : (int)_timeScale;
            looseTimeScale = _timeScale < 1 ? _timeScale : 1 + _timeScale - tightTimeScale;
            Time.timeScale = looseTimeScale;
            _deltaTime = Time.fixedDeltaTime * _timeScale;
            UpdateStates();
            Simulate(Time.fixedDeltaTime);
        }

        private void UpdateStates()
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];

                if (body == _mainBody)
                {
                    body.Collisions(true);
                    continue;
                }

                if (body.bodyData.mass > 1e6)
                {
                    body.Collisions(false);
                    body.bodyData.onRails = !body.bodyData.forceKinematic ? true : false;
                    continue;
                }

                if (body.bodyData.forceKinematic)
                {
                    body.Collisions(false);
                    continue;
                }

                double sqrDistance = (_mainBody.bodyData.position - body.bodyData.position).sqrMagnitude
                - (_mainBody.bodyData.radius * _mainBody.bodyData.radius - body.bodyData.radius * body.bodyData.radius);

                if (sqrDistance <= physicsRange * physicsRange)
                {
                    body.Collisions(true);
                    continue;
                }

                body.Collisions(false);
                body.bodyData.onRails = !body.bodyData.hasAcceleration;
            }
        }

        private void PatchedConics(Body body)
        {
            body.bodyData.CartesianToKeplerian();

            var possibleSOIs = new List<Body>();

            var attractor = body.bodyData.attractor;

            if (attractor != null)
            {
                if (attractor.bodyData.attractor != null)
                {
                    possibleSOIs.Add(attractor.bodyData.attractor);
                }

                possibleSOIs.Add(attractor);

                foreach (var child in attractor.bodyData.children)
                {
                    if (child != body)
                    {
                        possibleSOIs.Add(child);
                    }
                }
            }

            int bestIndex = -1;
            double bestDistance = Mathf.Infinity;
            for (int g = 0; g < possibleSOIs.Count; g++)
            {
                double sqrDistance = (possibleSOIs[g].bodyData.position - body.bodyData.position).sqrMagnitude;
                if (sqrDistance < possibleSOIs[g].bodyData.sphereOfInfluence * possibleSOIs[g].bodyData.sphereOfInfluence)
                {
                    if (sqrDistance < bestDistance)
                    {
                        bestDistance = sqrDistance;
                        bestIndex = g;
                    }
                }
            }

            if (bestIndex >= 0)
            {
                body.bodyData.attractor = possibleSOIs[bestIndex];
            }
        }

        private void Simulate(float fixedDeltaTime)
        {
            var lastIndex = _bodies.Count - 1;
            if (mainBody.bodyData.index != lastIndex)
            {
                if (mainBody.bodyData.index == lastIndex) return;
                var tmp = _bodies[lastIndex];
                _bodies[lastIndex] = _bodies[mainBody.bodyData.index];
                _bodies[mainBody.bodyData.index] = tmp;
                _bodies[mainBody.bodyData.index].bodyData.index = mainBody.bodyData.index;
                mainBody.bodyData.index = lastIndex;
            }

            for (int i = 0; i < tightTimeScale; i++)
            {
                _currentTime += fixedDeltaTime;

                for (int j = 0; j < _bodies.Count; j++)
                {
                    if (_bodies[j].bodyData.attractor != null)
                    {
                        PatchedConics(_bodies[j]);
                    }
                    else
                    {
                        _bodyData[j].sphereOfInfluence = Mathf.Infinity;
                    }

                    _bodyData[j].acceleration += Acceleration(j, _bodyData[j].position);
                }

                for (int k = 0; k < _bodies.Count; k++)
                {
                    var body = _bodies[k];
                    var bodyData = body.bodyData;

                    bodyData.acceleration -= _mainBody.bodyData.acceleration;

                    if (body.rb.isKinematic)
                    {
                        bodyData.rotation += bodyData.angularVelocity * fixedDeltaTime * 0.5f;
                        bodyData.rotation = bodyData.rotation % 360f;
                        body.rb.rotation = bodyData.rotation;

                        Vector2d position, velocity;
                        if (bodyData.onRails)
                        {
                            (position, velocity) = Integrate(k, bodyData.acceleration, fixedDeltaTime);
                            bodyData.velocity += velocity;
                            bodyData.position += position;

                            body.rb.MovePosition((Vector2)bodyData.position);
                            continue;
                        }

                        (position, velocity) = Integrate(k, bodyData.acceleration, fixedDeltaTime);
                        bodyData.velocity += velocity;
                        bodyData.position += position;

                        body.rb.MovePosition((Vector2)bodyData.position);
                    }
                    else
                    {
                        body.rb.rotation = body.rb.rotation % 360f;
                        bodyData.angularVelocity = body.rb.angularVelocity;
                        bodyData.rotation = body.rb.rotation;

                        body.rb.AddForce((Vector2)(bodyData.acceleration * bodyData.mass), ForceMode2D.Force);
                        bodyData.position = (Vector2d)(body.rb.position + body.rb.velocity * fixedDeltaTime);
                        bodyData.velocity = (Vector2d)body.rb.velocity;
                    }
                }
            }

            Physics2D.Simulate(Time.fixedDeltaTime);
        }

        public Vector2d Acceleration(int index, Vector2d position)
        {
            var acceleration = Vector2d.zero;
            if (_bodyData[index].attractor == null) { return acceleration; }
            var r1 = _bodyData[index].attractor.bodyData.position - position;
            acceleration += r1.normalized * Constant.G * _bodyData[index].attractor.bodyData.mass / r1.sqrMagnitude;

            //acceleration += (0.5f * _bodyData[index].atmosphereicDesity * (_bodyData[index].airSpeed * _bodyData[index].airSpeed) 
            //* _bodyData[index].dragCoefficent * _bodyData[index].crossSectionalArea) / _bodyData[index].mass

            //acceleration += (0.5f * _bodyData[index].atmosphereicDesity * (_bodyData[index].airSpeed * _bodyData[index].airSpeed) * _bodyData[index].liftCoefficent 
            //* _bodyData[index].wingCrossSectionalArea) / _bodyData[index].mass

            return acceleration;
        }

        public (Vector2d position, Vector2d velocity) Integrate(int index, Vector2d acceleration, float deltaTime)
        {
            Vector2d Velocity(Vector2d position, float deltaTime)
            {
                return _bodyData[index].velocity + (Acceleration(index, position) * deltaTime);
            }

            Vector2d NextAcceleration(Vector2d acceleration, Vector2d velocity, float deltaTime)
            {
                return acceleration + (Acceleration(index, _bodyData[index].position + (velocity * deltaTime)) * deltaTime);
            }

            Vector2d k1, k2, k3, k4, position, velocity;
            {
                k1 = NextAcceleration(acceleration, _bodyData[index].velocity, 0);
                k2 = NextAcceleration(acceleration, _bodyData[index].velocity + (deltaTime * 0.5f * k1), deltaTime * 0.5f);
                k3 = NextAcceleration(acceleration, _bodyData[index].velocity + (deltaTime * 0.5f * k2), deltaTime * 0.5f);
                k4 = NextAcceleration(acceleration, _bodyData[index].velocity + (deltaTime * -k3), deltaTime);
                velocity = deltaTime * 0.16666666666f * (k1 + (2 * k2) + (2 * k3) + k4);

                k1 = Velocity(_bodyData[index].position, 0);
                k2 = Velocity(_bodyData[index].position + (deltaTime * 0.5f * k1), deltaTime * 0.5f);
                k3 = Velocity(_bodyData[index].position + (deltaTime * 0.5f * k2), deltaTime * 0.5f);
                k4 = Velocity(_bodyData[index].position + (deltaTime * -k3), deltaTime);
                position = deltaTime * 0.16666666666f * (k1 + (2 * k2) + (2 * k3) + k4);

                return (position, velocity);
            }
        }

        public void ResetVelocity()
        {
            var offset = _mainBody.bodyData.velocity;

            for (int i = 0; i < _bodies.Count; i++)
            {
                _bodyData[i].velocity -= offset;
                _bodies[i].rb.velocity -= (Vector2)offset;
            }
        }

        public void ResetPosition()
        {
            var offset = _mainBody.bodyData.position;

            for (int i = 0; i < _bodies.Count; i++)
            {
                _bodyData[i].position -= offset;
                _bodies[i].rb.position -= (Vector2)offset;
            }
        }

        public void FindAllBodies()
        {
            _bodies.Clear();
            _bodyData.Clear();
            _bodies = FindObjectsOfType<Body>(false).ToList();

            for (int i = 0; i < _bodies.Count; i++)
            {
                _bodies[i].bodyData.index = i;
                _bodyData.Add(_bodies[i].bodyData);
                _bodies[i].rb = _bodies[i].GetComponent<Rigidbody2D>();
                _bodies[i].rb.bodyType = RigidbodyType2D.Kinematic;
                _bodies[i].rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _bodies[i].rb.isKinematic = true;
            }
        }

        public void RemoveBody(Body body)
        {
            _bodyData.Remove(body.bodyData);
            _bodies.Remove(body);
        }

        public void AddBody(Body body)
        {
            body.rb = body.GetComponent<Rigidbody2D>();
            body.rb.bodyType = RigidbodyType2D.Kinematic;
            body.rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.rb.isKinematic = true;
            _bodies.Add(body);
            _bodyData.Add(body.bodyData);
            body.bodyData.index = _bodies.IndexOf(body);
        }

        // Remove reset to center and move to a editor controller that is removed on build
        [ContextMenu("Center")]
        public void ResetToCenter()
        {
            var tempBodies = FindObjectsOfType<Body>(false).ToList();
            var offset = _mainBody.transform.position;

            for (int i = 0; i < tempBodies.Count; i++)
            {
                tempBodies[i].transform.position -= offset;
            }
        }

        public void ResetToCenter(Vector3 offset)
        {
            var tempBodies = FindObjectsOfType<Body>(false).ToList();

            for (int i = 0; i < tempBodies.Count; i++)
            {
                tempBodies[i].transform.position -= offset;
            }
        }
    }
}