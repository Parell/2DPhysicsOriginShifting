using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Decel
{
    public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string originalText;
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
            originalText = text.text;
        }

        private void OnDisable()
        {
            text.text = originalText;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StopAllCoroutines();
            StartCoroutine(Off());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            StartCoroutine(On());
        }

        private IEnumerator On()
        {
            text.text = "  " + originalText;
            yield return new WaitForSecondsRealtime(0.05f);
            text.text = " " + originalText;
            yield return new WaitForSecondsRealtime(0.05f);
            text.text = originalText;
        }

        private IEnumerator Off()
        {
            text.text = " " + originalText;
            yield return new WaitForSecondsRealtime(0.05f);
            text.text = "  " + originalText;
            yield return new WaitForSecondsRealtime(0.05f);
            text.text = "   " + originalText;
        }
    }
}
