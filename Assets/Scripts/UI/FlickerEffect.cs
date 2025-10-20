using UnityEngine;
using UnityEngine.UI;

namespace Decel
{
    public class FlickerEffect : MonoBehaviour
    {
        [SerializeField] private Color final;
        [SerializeField] private float speed;
        private Text text;
        private Color inital;

        private void Start()
        {
            text = GetComponent<Text>();
            inital = text.color;
        }

        private void Update()
        {
            text.color = Color.Lerp(inital, final, Mathf.PingPong(Time.unscaledTime * speed, 1));
        }
    }
}
