using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomParkourCounter : MonoBehaviour
{
    [Header("Banners")]
    [SerializeField] private GameObject startBanner;
    [SerializeField] private GameObject firstBanner;
    [SerializeField] private GameObject secondBanner;
    [SerializeField] private GameObject finalBanner;
    
    [Header("Coins")]
    [SerializeField] private GameObject firstCoins;
    [SerializeField] private GameObject secondCoins;
    [SerializeField] private GameObject finalCoins;
    
    [Header("Object Interaction Tasks")]
    [SerializeField] private GameObject objIX1;
    [SerializeField] private GameObject objIX2;
    [SerializeField] private GameObject objIX3;
    
    [Header("Respawn Points")]
    [SerializeField] private Transform start2FirstRespawn;
    [SerializeField] private Transform first2SecondRespawn;
    [SerializeField] private Transform second2FinalRespawn;

    [Header("Texts")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject timeTextGO;
    [SerializeField] private GameObject coinTextGO;
    [SerializeField] private GameObject recordTextGO;
    [SerializeField] private GameObject endTextGO;

    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource endSoundEffect;

    [Header("Settings")]
    [SerializeField] private bool respawnCoinsOnNewRound;
    
    private CustomSelectionTaskMeasure selectionTaskMeasure;

    private Vector3 currentRespawnPos;
    private float timeCounter;
    private float part1Time;
    private float part2Time;
    private float part3Time;
    private int coinCount;

    private int part1Count; // 17
    private int part2Count; // 33
    private int part3Count; // 24
    private bool parkourStarted;
    private bool isStageChange;

    void Start()
    {
        this.coinCount = 0;
        this.timeCounter = 0.0f;
        this.firstBanner.SetActive(false);
        this.secondBanner.SetActive(false);
        this.finalBanner.SetActive(false);
        this.firstCoins.SetActive(false);
        this.secondCoins.SetActive(false);
        this.finalCoins.SetActive(false);
        this.objIX2.SetActive(false);
        this.objIX3.SetActive(false);
        this.objIX1.SetActive(false);
        this.parkourStarted = false;
        this.endTextGO.SetActive(false);
        this.selectionTaskMeasure = this.GetComponent<CustomSelectionTaskMeasure>();
    }

    void Update()
    {
        if (this.isStageChange)
        {
            this.isStageChange = false;
            if (BowAndArrowLocomotion.Instance.GetCurrentStage() == startBanner.name)
            {
                this.parkourStarted = true;
                this.timeTextGO.SetActive(true);
                this.coinTextGO.SetActive(true);
                this.recordTextGO.SetActive(true);
                this.ActivateSectionByIndex(1);
            }
            else if (BowAndArrowLocomotion.Instance.GetCurrentStage() == firstBanner.name)
            {
                this.ActivateSectionByIndex(2);
                this.CalculateAndDisplayStats(1);
            }
            else if (BowAndArrowLocomotion.Instance.GetCurrentStage() == secondBanner.name)
            {
                this.ActivateSectionByIndex(3);
                this.CalculateAndDisplayStats(2);
            }
            else if (BowAndArrowLocomotion.Instance.GetCurrentStage() == finalBanner.name)
            {
                this.HandleParkourFinished();
            }
        }

        if (this.parkourStarted)
        {
            this.UpdateCounters();
        }
    }

    private void UpdateCounters()
    {
        this.timeCounter += Time.deltaTime;
        this.timeText.text = "time: " + this.timeCounter.ToString("F1");
        this.coinText.text = "coins: " + this.coinCount.ToString();
    }

    private void UpdateRecordText(int part, float time, int coinsCount, int coinsInPart)
    {
        string newRecords = "loco" + part.ToString() + ": " + time.ToString("F1") + ", " + coinsCount + "/" + coinsInPart + "\n" +
                            "obj" + part.ToString() + ": " + (selectionTaskMeasure.GetPartSumTime() / 5f).ToString("F1") + "," + (selectionTaskMeasure.GetPartSumErr() / 5).ToString("F2");
        recordText.text = recordText.text + "\n" + newRecords;
    }

    private void ActivateSectionByIndex(int index)
    {
        this.startBanner.SetActive(index == 0);

        this.firstBanner.SetActive(index == 1);
        this.firstCoins.SetActive(index == 1);
        this.objIX1.SetActive(index == 1);

        this.secondBanner.SetActive(index == 2);
        this.secondCoins.SetActive(index == 2);
        this.objIX2.SetActive(index == 2);

        this.finalBanner.SetActive(index == 3);
        this.finalCoins.SetActive(index == 3);
        this.objIX3.SetActive(index == 3);

        this.selectionTaskMeasure.SetTaskUIPosition((index == 3 ? this.objIX3 : (index == 2 ? this.objIX2 : this.objIX1)).transform.position);
        this.currentRespawnPos = (index == 3 ? this.second2FinalRespawn : (index == 2 ? this.first2SecondRespawn : this.start2FirstRespawn)).position;
    }

    private void CalculateAndDisplayStats(int section)
    {
        if (section == 1)
        {
            this.part1Time = this.timeCounter;
            this.part1Count = this.coinCount;
            UpdateRecordText(1, this.part1Time, this.part1Count, 16);
        }
        else if (section == 2)
        {
            this.part2Time = this.timeCounter - this.part1Time;
            this.part2Count = this.coinCount - this.part1Count;
            UpdateRecordText(2, this.part2Time, this.part2Count, 30);
        } 
        else if (section == 3)
        {
            this.part3Time = this.timeCounter - (this.part1Time + this.part2Time);
            this.part3Count = this.coinCount - (this.part1Count + this.part2Count);
            UpdateRecordText(3, this.part3Time, this.part3Count, 23);
        }
    }

    private void HandleParkourFinished()
    {
        this.parkourStarted = false;
        this.finalCoins.SetActive(false);
        this.objIX3.SetActive(false);

        this.CalculateAndDisplayStats(3);

        this.timeTextGO.SetActive(false);
        this.coinTextGO.SetActive(false);
        this.recordTextGO.SetActive(false);

        this.endTextGO.SetActive(true);
        this.endTextGO.GetComponent<TMP_Text>().text = "Round Finished!\n" + this.recordText.text + "\ntotal: " + this.timeCounter.ToString("F1") + ", " + this.coinCount.ToString() + "/69";
        Debug.Log(endTextGO.GetComponent<TMP_Text>().text);
        this.endSoundEffect.Play();

        this.WriteDataLog();

        // reset the parkour so the player can start a new round
        this.ResetForNewRound();
    }

    public bool GetParkourStarted()
    {
        return this.parkourStarted;
    }

    public Vector3 GetCurrentRespawnPos()
    {
        return this.currentRespawnPos;
    }

    public void IncrementCoinCounter()
    {
        this.coinCount++;
    }

    public void HandleBannerPassed()
    {
        this.isStageChange = true;
    }

    public void WriteDataLog()
    {
        BowAndArrowLocomotion.Instance.GetDataLogging().WriteDataLog(this.timeCounter, this.coinCount);
    }

    public void ResetForNewRound()
    {
        // reenable the start banner
        this.ActivateSectionByIndex(0);

        // reenable the object interaction task initiators
        this.objIX1.GetComponent<CapsuleCollider>().enabled = true;
        this.objIX2.GetComponent<CapsuleCollider>().enabled = true;
        this.objIX3.GetComponent<CapsuleCollider>().enabled = true;

        // reenable the banner colliders
        this.startBanner.GetComponent<BoxCollider>().enabled = true;
        this.firstBanner.GetComponent<BoxCollider>().enabled = true;
        this.secondBanner.GetComponent<BoxCollider>().enabled = true;
        this.finalBanner.GetComponent<BoxCollider>().enabled = true;

        // reenable the coins
        if (this.respawnCoinsOnNewRound)
        {
            this.SetActiveOnChildren(this.firstCoins.transform, true);
            this.SetActiveOnChildren(this.secondCoins.transform, true);
            this.SetActiveOnChildren(this.finalCoins.transform, true);
        }

        // hide the end text
        this.endTextGO.SetActive(false);

        // reset part counter to start over with the first target speed
        this.selectionTaskMeasure.ResetPartCounter();

        // increase round counter of the data logging
        BowAndArrowLocomotion.Instance.GetDataLogging().IncrementRoundCounter();
    }

    private void SetActiveOnChildren(Transform parent, bool active)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(active);
        }
    }
}
