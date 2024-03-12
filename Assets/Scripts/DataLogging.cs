using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static Arrow;

public class DataLogging : MonoBehaviour
{
    [SerializeField] private string logFileName = "ivarParkourLog";
    [SerializeField] private string uiInfoTextContent = "Save file created: ";
    [SerializeField] private GameObject uiInfo;
    [SerializeField] private TMP_Text uiInfoText;
    [SerializeField] private float uiInfoDuration = 3;

    [Header("Debugging")]
    [SerializeField] private bool debugMode = false;

    private List<InteractionTaskData> interactionTaskData = new List<InteractionTaskData>();
    private List<CoinComboData> coinCombos = new List<CoinComboData>();
    private int teleportationArrowsShot = 0;
    private int coinCollectionArrowsShot = 0;
    private int interactionArrowsShot = 0;
    private float maxCoinCollectionDistance = 0;
    private int currentRound;

    [System.Serializable]
    private struct InteractionTaskData
    {
        public int round;
        public float targetSpeed;
        public string banner;
        public int number;
        public float taskTime;
        public Vector3 error; // error is calculated by the distance between three cubes (e.g., red to red, green to green, and blue to blue)

        public InteractionTaskData(int round, float targetSpeed, string banner, int number, float taskTime, Vector3 error)
        {
            this.round = round;
            this.targetSpeed = targetSpeed;
            this.banner = banner;
            this.number = number;
            this.taskTime = taskTime;
            this.error = error;
        }
    }

    [System.Serializable]
    private struct CoinComboData
    {
        public int combo;
        public int count;

        public CoinComboData(int combo)
        {
            this.combo = combo;
            this.count = 0;
        }

        public CoinComboData Increment()
        {
            this.count++;
            return this;
        }
    }

    [System.Serializable]
    private struct ParkourRunData
    {
        public List<InteractionTaskData> interactionTaskData;
        public List<CoinComboData> coinCombos;
        public int teleportationArrowsShot;
        public int coinCollectionArrowsShot;
        public int interactionArrowsShot;
        public float totalTime;
        public int coinsCollected;
        public float coinCollectionAccuracy;
        public float maxCoinCollectionDistance;
        public int completedRounds;

        public ParkourRunData(
            List<InteractionTaskData> interactionTaskData,
            List<CoinComboData> coinCombos,
            int teleportationArrowsShot, int coinCollectionArrowsShot, 
            int interactionArrowsShot, float totalTime, int coinsCollected,
            float maxCoinCollectionDistance, int completedRounds)
        {
            this.interactionTaskData = interactionTaskData;
            this.coinCombos = coinCombos;
            this.teleportationArrowsShot = teleportationArrowsShot;
            this.coinCollectionArrowsShot = coinCollectionArrowsShot;
            this.interactionArrowsShot = interactionArrowsShot;
            this.totalTime = totalTime;
            this.coinsCollected = coinsCollected;
            this.coinCollectionAccuracy = (float)coinsCollected / (float)coinCollectionArrowsShot;
            this.maxCoinCollectionDistance = maxCoinCollectionDistance;
            this.completedRounds = completedRounds;
        }
    }

    private void Start()
    {
        this.currentRound = 0;

        if (this.debugMode) this.WriteDemoData();
    }

    public void IncrementRoundCounter()
    {
        this.currentRound++;
    }

    public void AddInteractionTaskData(float targetSpeed, string banner, int number, float taskTime, Vector3 error)
    {
        this.interactionTaskData.Add(new InteractionTaskData(this.currentRound, targetSpeed, banner, number, taskTime, error));
    }

    public void IncreaseArrowShotCounter(ArrowType arrowType)
    {
        if (arrowType == ArrowType.teleportation) this.teleportationArrowsShot++;
        else if (arrowType == ArrowType.coinCollection) this.coinCollectionArrowsShot++;
        else if (arrowType == ArrowType.objectInteraction) this.interactionArrowsShot++;
    }

    public void LogCoinCollectionDistance(float distance)
    {
        if (distance > this.maxCoinCollectionDistance) this.maxCoinCollectionDistance = distance;
    }

    public void LogCoinCombo(int combo)
    {
        if (this.coinCombos.Count < combo) for (int i = this.coinCombos.Count; i < combo; i++) this.coinCombos.Add(new CoinComboData(i + 1));
        this.coinCombos[combo - 1] = this.coinCombos[combo - 1].Increment();
    }
    
    public void WriteDataLog(float totalTime, int coinsCollected)
    {
        ParkourRunData data = new ParkourRunData(this.interactionTaskData, this.coinCombos, this.teleportationArrowsShot, this.coinCollectionArrowsShot, this.interactionArrowsShot, totalTime, coinsCollected, this.maxCoinCollectionDistance, this.currentRound);
        string path = this.GetNextFileName();
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);

        if (this.debugMode) Debug.LogWarning(json);
        Debug.LogWarning("Wrote data log to the following path: " + path);
        this.ShowInfoUI(path);
    }

    private void WriteDemoData()
    {
        this.CreateDemoData(3, 5, 8);
        this.WriteDataLog(12.34f, 42);
    }

    private void CreateDemoData(int rounds, int iterationsPerRound, int comboCount)
    {
        for (int round = 0; round < rounds; round++)
        {
            for (int iteration = 0; iteration < iterationsPerRound; iteration++)
            {
                this.AddInteractionTaskData(1, (round + 1).ToString(), iteration, Random.Range(0.5f, 5f), new Vector3(Random.Range(0.1f, 2f), Random.Range(0.1f, 2f), Random.Range(0.1f, 2f)));
            }
        }

        for (int i = 0; i < comboCount; i++)
        {
            this.LogCoinCombo(Random.Range(1, 6));
        }

        this.teleportationArrowsShot = Random.Range(20, 100);
        this.coinCollectionArrowsShot = Random.Range(20, 100);
        this.interactionArrowsShot = Random.Range(20, 100);
        this.maxCoinCollectionDistance = Random.Range(1f, 20f);
    }

    private string GetNextFileName()
    {
        FileInfo[] files = new DirectoryInfo(Application.persistentDataPath).GetFiles();
        int fileCount = 0;
        foreach (FileInfo file in files) if (file.Name.StartsWith(this.logFileName)) fileCount++;

        return Application.persistentDataPath + "/" + this.logFileName + fileCount.ToString() + ".json";
    }

    private void ShowInfoUI(string path)
    {
        string[] split = path.Split('/');
        if (split.Length <= 1) split = path.Split('\\');
        string filename = split.Length > 1 ? split[split.Length - 1] : path;

        this.uiInfoText.text = this.uiInfoTextContent + filename;
        this.uiInfo.SetActive(true);

        Invoke("HideInfoUI", this.uiInfoDuration);
    }

    private void HideInfoUI()
    {
        this.uiInfo.SetActive(false);
    }
}
