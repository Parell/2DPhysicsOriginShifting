using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Decel
{
    public class HudManager : MonoBehaviour
    {
        // [SerializeField] private int refreshRate = 2;
        [SerializeField] private Text velocityText;
        [SerializeField] private Text timeScaleText;
        [SerializeField] private Text referenceFrameText;
        [SerializeField] private Text referenceNameText;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider healthSlider;
        //private Damageable damageable;
        // private Body mainBody;
        // private string textData;
        // private int index;
        // private float selectionTimer;
        // private float selectionRefreshTime = 0.25f;
        // private List<(Body body, string name)> referenceData;

        // private void Update()
        // {
        //     if (!PhysicsManager.Instance || !PredictionManager.Instance) { return; }

        //     mainBody = PhysicsManager.mainBody;
        //     // if (damageable == null)
        //     // {
        //     //     damageable = mainBody.GetComponent<Damageable>();
        //     // }

        //     if (Time.frameCount % refreshRate == 0)
        //     {
        //         // healthText.text = ((int)((float)damageable.Health / damageable.MaxHealth * 100)).ToString();
        //         // healthSlider.maxValue = damageable.MaxHealth;
        //         // healthSlider.value = damageable.Health;

        //         float velocity = MathExtentions.FastMagnitude((Vector3)(PredictionManager.Instance.referenceFrameBody.bodyData.velocity - mainBody.bodyData.velocity));

        //         timeScaleText.text = "TIME " + PhysicsManager.timeScale.ToString("###0.0");

        //         if (velocity >= 1000) { velocityText.text = (velocity / 1000).ToString("###0.00 km/s"); }
        //         else { velocityText.text = velocity.ToString("###0.0 m/s"); }
        //     }

        //     UpdateSelectionScreen();
        // }

        // private void UpdateSelectionScreen()
        // {
        //     void FindReferenceBodies(List<Body> bodies)
        //     {
        //         referenceData = new List<(Body body, string name)>();

        //         for (int i = 0; i < bodies.Count; i++)
        //         {
        //             if (bodies[i] != mainBody && (bodies[i].type == BodyType.Celestial || bodies[i].type == BodyType.Vessel || bodies[i].type == BodyType.Station))
        //             {
        //                 referenceData.Add((bodies[i], bodies[i].name.ToUpper()));
        //             }
        //         }
        //     }


        //     if (Input.GetKey(KeyCode.LeftAlt))
        //     {
        //         if (Input.GetKeyDown(KeyCode.LeftAlt))
        //         {
        //             textData = "...";
        //             selectionTimer = selectionRefreshTime;
        //         }

        //         int delta = -(int)Input.mouseScrollDelta.y;
        //         index = Mathf.Clamp(index += delta, 0, referenceData.Count - 1);

        //         if (selectionTimer > 0)
        //         {
        //             selectionTimer -= Time.deltaTime;
        //             referenceFrameText.text = textData;
        //         }
        //         else
        //         {
        //             textData = "";

        //             var bodies = PhysicsManager.bodies;
        //             FindReferenceBodies(bodies);

        //             for (int i = 0; i < referenceData.Count; i++)
        //             {
        //                 if (referenceData[index].body == referenceData[i].body)
        //                 {
        //                     textData += "<b>" + referenceData[i].name + "</b>\n";
        //                 }
        //                 else
        //                 {
        //                     textData += referenceData[i].name + "\n";
        //                 }
        //             }

        //             referenceFrameText.text = textData;
        //         }
        //     }
        //     else
        //     {
        //         var bodies = PhysicsManager.bodies;
        //         FindReferenceBodies(bodies);

        //         referenceFrameText.text = "";
        //         referenceNameText.text = referenceData[index].name;
        //         PredictionManager.Instance.referenceFrameBody = bodies[bodies.IndexOf(referenceData[index].body)];

        //         textData = referenceData[index].name;
        //         if (referenceData[index].body != PredictionManager.Instance.referenceFrameBody)
        //         {
        //             PredictionManager.Instance.referenceFrameBody = bodies[bodies.IndexOf(referenceData[index].body)];
        //         }
        //     }
        // }
    }
}
