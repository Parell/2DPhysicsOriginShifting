using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Decel
{
    [ExecuteAlways]
    public class PredictionManager : MonoBehaviour
    {
        public static PredictionManager Instance;
        public Body referenceFrameBody;
        [SerializeField] private float plotTime;
        [SerializeField] private float stepSize;
        [SerializeField] private int steps;
        [SerializeField] private SimpleBodyData[] copiedBodyData;
        private Vector3[][] plotData;
        //         private Task currentTask;
        //         private CancellationTokenSource cancellationTokenSource;

        //         private LineRenderer[] lineRenderers;
        private bool inEditor => Application.isEditor && !Application.isPlaying;
        private bool predictionKeyReleased;

        private void Start()
        {
            Instance = this;
        }

        private void Update()
        {
            if (inEditor == false)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    Debug.Log("Prediction in progress");
                    plotTime += 100 * Time.deltaTime;
                }
                else if (Input.GetKeyUp(KeyCode.Tab))
                {
                    Debug.Log("Prediction stopped");
                    predictionKeyReleased = true;
                }

                if (predictionKeyReleased && plotTime > 0)
                {
                    plotTime -= 10 * Time.deltaTime;
                    UpdatePlot();
                }
                else if (predictionKeyReleased && plotTime < 0)
                {
                    predictionKeyReleased = false;
                    plotTime = 0;
                }
            }

            if (plotTime <= 0 || stepSize == 0)
            {
                steps = 0;
            }
            else
            {
                steps = Mathf.RoundToInt(Mathf.Abs(plotTime / stepSize));
            }
        }

        private void CopyBodyData()
        {
            var bodyData = PhysicsManager.bodyData;
            copiedBodyData = new SimpleBodyData[bodyData.Count];
            plotData = new Vector3[bodyData.Count][];

            // for (int i = 0; i < bodyData.Count; i++)
            // {
            //     if (PhysicsManager.bodies[i].type == BodyType.Celestial)
            //     {
            //         copiedBodyData[i] = new SimpleBodyData(bodyData[i].index, bodyData[i].position, bodyData[i].velocity);
            //         plotData[i] = new Vector3[steps];
            //     }
            // }
        }

        private void UpdatePlot()
        {
            CopyBodyData();

            for (int i = 0; i < copiedBodyData.Length; i++)
            {
                int index = copiedBodyData[i].index;

                for (int step = 0; step < steps; step++)
                {
                    copiedBodyData[index].velocity += PhysicsManager.Instance.Acceleration(index, copiedBodyData[index].position) * stepSize;
                    copiedBodyData[index].position += copiedBodyData[index].velocity * stepSize;

                    plotData[index][step] = (Vector3)copiedBodyData[index].position;
                }
            }
        }

        // private void OnDestroy()
        // {
        //     cancellationTokenSource?.Cancel();
        // }

        // private async void Update()
        // {
        //     // #if UNITY_EDITOR
        //     if (inEditor == false && currentTask == null)
        //     {
        //         if (plotTime <= 0 || stepSize == 0)
        //         {
        //             steps = 0;
        //         }
        //         else
        //         {
        //             steps = Mathf.RoundToInt(Mathf.Abs(plotTime / stepSize));
        //         }

        //         await UpdatePlotAsync();
        //     }
        //     // #endif
        // }

        // private async Task UpdatePlotAsync()
        // {
        //     var bodyData = PhysicsManager.bodyData;
        //     copiedBodyData = new BodyData[bodyData.Count];
        //     plotData = new Vector3[bodyData.Count][];

        //     for (int i = 0; i < bodyData.Count; i++)
        //     {
        //         if (PhysicsManager.bodies[i].lineRenderer != null || bodyData[i].type == BodyType.Celestial)
        //         {
        //             copiedBodyData[i] = new BodyData(bodyData[i].index, bodyData[i].type, bodyData[i].mass, bodyData[i].position, bodyData[i].velocity, bodyData[i].angularVelocity, bodyData[i].forceKinematic);
        //             plotData[i] = new Vector3[steps];
        //         }
        //     }

        //     cancellationTokenSource = new CancellationTokenSource();
        //     currentTask = Task.Run(() => SimulatePlot(), cancellationTokenSource.Token);
        //     await currentTask;

        //     for (int i = 0; i < copiedBodyData.Length; i++)
        //     {
        //         int index = copiedBodyData[i].index;

        //         var lineRenderer = PhysicsManager.bodies[index].lineRenderer;

        //         //if (lineRenderer == null) { continue; }

        //         lineRenderer.positionCount = steps;
        //         lineRenderer.SetPositions(plotData[index]);
        //     }

        //     currentTask = null;
        // }

        // private void SimulatePlot()
        // {
        //     for (int i = 0; i < copiedBodyData.Length; i++)
        //     {
        //         int index = copiedBodyData[i].index;

        //         for (int step = 0; step < steps; step++)
        //         {
        //             copiedBodyData[index].acceleration = PhysicsManager.Instance.Acceleration(index, copiedBodyData[index].position);

        //             copiedBodyData[index].velocity += copiedBodyData[index].acceleration * stepSize;
        //             copiedBodyData[index].position += copiedBodyData[index].velocity * stepSize;

        //             Debug.Log(copiedBodyData[index].position);

        //             plotData[index][step] = (Vector3)copiedBodyData[index].position;
        //         }
        //     }
        // }
    }

    [System.Serializable]
    public class SimpleBodyData
    {
        public int index;
        public Vector2d position;
        public Vector2d velocity;

        public SimpleBodyData(int index, Vector2d position, Vector2d velocity)
        {
            this.index = index;
            this.position = position;
            this.velocity = velocity;
        }
    }
}