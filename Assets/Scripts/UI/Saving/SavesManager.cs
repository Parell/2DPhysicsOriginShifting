using UnityEngine;
using UnityEngine.UI;

namespace Decel
{
    public class SavesManager : MonoBehaviour
    {
        public static SavesManager Instance;
        [SerializeField] private Text titleText;
        [SerializeField] private SaveSlot[] saveSlots;
        [SerializeField] public SaveData saveData;
        private float timeAccumulator;

        private void Start()
        {
            Instance = this;
            saveSlots = GetComponentsInChildren<SaveSlot>(true);
            foreach (var slot in saveSlots)
            {
                slot.button = slot.GetComponent<Button>();
            }
            InitializeSlots();
        }

        private void Update()
        {
            if (FadeManager.CurrentScene != 0 && FadeManager.CurrentScene != 1)
            {
                timeAccumulator += Time.unscaledDeltaTime;
                if (timeAccumulator >= 1)
                {
                    saveData.playTime++;
                    timeAccumulator = 0;
                }
            }
        }

        public static string Path(int i)
        {
            return $"{Application.dataPath}/Data/SaveData{i}.json";
        }

        private void InitializeSlots()
        {
            for (int i = 0; i < saveSlots.Length; i++)
            {
                saveSlots[i].index = i;
                if (FileManager.Exists(Path(i)))
                {
                    var saveData = FileManager.Load<SaveData>(Path(i));
                    saveSlots[i].isEmpty = false;

                    var playTime = (saveData.playTime / 360).ToString("00") + "hr";

                    saveSlots[i].UpdateInfo(i, saveData.saveName, playTime);
                }
                else
                {
                    saveSlots[i].isEmpty = true;
                    saveSlots[i].UpdateInfo(i, default, default);
                }
            }
        }

        public void LoadGame()
        {
            titleText.text = "LOAD";
            MenuManager.Instance.SaveLoadScreen(true);
            InitializeSlots();
            foreach (var slot in saveSlots)
            {
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(slot.Load);
            }
        }

        public void SaveGame()
        {
            titleText.text = "SAVE";
            MenuManager.Instance.SaveLoadScreen(true);
            InitializeSlots();
            foreach (var slot in saveSlots)
            {
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(slot.Save);
            }
        }
    }
}
