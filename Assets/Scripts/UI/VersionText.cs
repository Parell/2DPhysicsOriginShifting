using UnityEngine;
using UnityEngine.UI;

namespace Decel
{
    public class VersionText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Text>().text = Application.version;
        }
    }
}
