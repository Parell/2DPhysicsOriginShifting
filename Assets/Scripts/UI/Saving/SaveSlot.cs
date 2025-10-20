using UnityEngine;
using UnityEngine.UI;

namespace Decel
{
    public class SaveSlot : MonoBehaviour
    {
        public int index;
        public bool isEmpty;
        [HideInInspector] public Button button;
        [SerializeField] private Text numberText;
        [SerializeField] private Text nameText;
        [SerializeField] private Text playTimeText;
        [SerializeField] private Text noDataText;

        public void UpdateInfo(int index, string playerName, string playTime)
        {
            if (isEmpty)
            {
                numberText.text = (index + 1).ToString();

                nameText.gameObject.SetActive(false);
                playTimeText.gameObject.SetActive(false);
                noDataText.gameObject.SetActive(true);
            }
            else
            {
                nameText.gameObject.SetActive(true);
                playTimeText.gameObject.SetActive(true);
                noDataText.gameObject.SetActive(false);

                numberText.text = (index + 1).ToString();
                nameText.text = playerName;
                playTimeText.text = playTime;
            }
        }

        public void Save()
        {
            var path = SavesManager.Path(index);

            if (isEmpty)
            {
                // if (MissionManager.Instance != null)
                // {
                //     MissionManager.Instance.SaveMissions();
                // }
                FileManager.Save(path, SavesManager.Instance.saveData);
                MenuManager.Instance.SaveLoadScreen(false);
            }
            else
            {
                // if (MissionManager.Instance != null)
                // {
                //     MissionManager.Instance.SaveMissions();
                // }
                FileManager.Save(path, SavesManager.Instance.saveData);
                MenuManager.Instance.SaveLoadScreen(false);
            }
        }

        public void Load()
        {
            var path = SavesManager.Path(index);

            if (FileManager.Exists(path))
            {
                SavesManager.Instance.saveData = FileManager.Load<SaveData>(path);
            }

            if (isEmpty)
            {
                MenuManager.Instance.saveLoadScreen.SetActive(false);
                MenuManager.Instance.Play(2);
            }
            else
            {
                MenuManager.Instance.saveLoadScreen.SetActive(false);
                MenuManager.Instance.Play(2);
            }
        }
    }
}
