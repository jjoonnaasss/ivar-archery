using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CustomSelectionTaskMeasure : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float taskUIEyeDistance;
    [SerializeField] private float targetSpawnMaxOffsetX;
    [SerializeField] private float targetSpawnMaxOffsetY;
    [SerializeField] private float targetSpawnOffsetZ;
    [SerializeField] private float targetMovementDistance;
    [SerializeField] private float[] targetSpeeds;
    [SerializeField] private float iterationCooldown;

    [Header("References")]
    [SerializeField] private Bow interactionBow;
    [SerializeField] private BowHolster interactionBowHolster;
    [SerializeField] private ArrowPoolSwitcher arrowPoolSwitcher;

    [Header("Prefabs")]
    [SerializeField] private GameObject targetTPrefab;
    [SerializeField] private GameObject objectTPrefab;
    [SerializeField] private GameObject[] targetPrefabs;

    [Header("UI")]
    [SerializeField] private GameObject taskStartPanel;
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TMP_Text startPanelText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject taskUI;

    // t-shapes and targets
    private GameObject targetT;
    private GameObject objectT;
    private Target currentTarget;

    private int iterationCount;
    private bool isTaskStart;
    private bool isTaskEnd;
    private bool objectTRotationConfirmed;
    private bool isCountdown;
    private float taskTime;
    private int part;
    private float partSumTime;
    private float partSumErr;
    private Vector3 eyePosition;
    private bool taskRunning;
    private float currentTargetSpeed;

    void Start()
    {
        this.ResetPartCounter();
        this.donePanel.SetActive(false);
        this.taskStartPanel.SetActive(false);
        this.scoreText.text = "Part" + part.ToString();

        if (this.targetSpeeds.Length < 3) Debug.LogError("CustomSelectionTaskMeasure: Not enough target speeds configured!");
        if (this.targetPrefabs.Length < 3) Debug.LogError("CustomSelectionTaskMeasure: Not enough target prefabs assigned!");
    }

    void Update()
    {
        if (isTaskStart)
        {
            // recording time
            taskTime += Time.deltaTime;
        }

        if (isCountdown)
        {
            taskTime += Time.deltaTime;
            startPanelText.text = (3.0 - taskTime).ToString("F1");
        }
    }

    public void ResetPartCounter()
    {
        this.part = 1;
    }

    public void StartMeasure(Collider interactionTaskCollider, Transform centerEyeAnchor)
    {
        this.isTaskStart = true;
        this.scoreText.text = "";
        this.partSumErr = 0f;
        this.partSumTime = 0f;
        this.eyePosition = centerEyeAnchor.position;

        this.PositionTaskUI(centerEyeAnchor);

        // force player to put down the current bow, let him grab the interaction bow
        BowAndArrowLocomotion.Instance.GetGrabManager().ForceBowGrab(this.interactionBow);
        this.taskRunning = true;
    }

    private void PositionTaskUI(Transform centerEyeAnchor)
    {
        // place task UI in front of the user (eye forward-direction, but only x and z)
        Vector3 eyeForward = centerEyeAnchor.forward;
        eyeForward.y = 0;
        this.taskUI.transform.position = centerEyeAnchor.transform.position + eyeForward.normalized * this.taskUIEyeDistance;

        // rotate task UI to face the user
        this.taskUI.transform.LookAt(centerEyeAnchor.position);
        this.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
        this.taskStartPanel.SetActive(true);
    }

    public void StartNewIteration()
    {
        if (this.isCountdown) return;

        // update variables
        this.isTaskStart = true;
        this.taskTime = 0f;

        // show and hide UI
        this.taskStartPanel.SetActive(false);
        this.donePanel.SetActive(true);

        // switch to arrows with t-shapes
        this.arrowPoolSwitcher.SwitchArrowPool(1);

        // spawn randomized target indicator to show where the t-shape has to go
        this.targetT = Instantiate(targetTPrefab, this.GetTargetTStartingPos(), this.GetTargetTStartingRot());

        // spawn target allowing the user to shoot at it in order to place a t-shape
        this.SpawnTarget();
    }

    private Vector3 GetTargetTStartingPos()
    {
        // set target position on the z-axis using the taskUI-position and a configured fixed value
        Vector3 pos = this.taskUI.transform.position + this.taskUI.transform.forward * this.targetSpawnOffsetZ;

        // randomly offset the target position on the x- and y-axes
        pos += this.taskUI.transform.right * Random.Range(-1f, 1f) * this.targetSpawnMaxOffsetX;
        pos += this.taskUI.transform.up * Random.Range(-1f, 1f) * this.targetSpawnMaxOffsetY;

        return pos;
    }

    private Quaternion GetTargetTStartingRot()
    {
        return new Quaternion(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    public void EndCurrentIteration()
    {
        this.donePanel.SetActive(false);

        // release
        this.isTaskEnd = true;
        this.isTaskStart = false;
        
        // distance error
        Vector3 manipulationError = Vector3.zero;
        for (int i = 0; i < targetT.transform.childCount; i++)
        {
            manipulationError += targetT.transform.GetChild(i).transform.position - objectT.transform.GetChild(i).transform.position;
        }

        // update UI and data logging
        this.scoreText.text = scoreText.text + "Time: " + taskTime.ToString("F1") + ", offset: " + manipulationError.magnitude.ToString("F2") + "\n";
        this.partSumErr += manipulationError.magnitude;
        this.partSumTime += taskTime;
        BowAndArrowLocomotion.Instance.GetDataLogging().AddInteractionTaskData(this.currentTargetSpeed, BowAndArrowLocomotion.Instance.GetCurrentStage(), iterationCount, taskTime, manipulationError);

        // Debug.Log("Time: " + taskTime.ToString("F1") + "\nPrecision: " + manipulationError.magnitude.ToString("F1"));
        Destroy(objectT);
        Destroy(targetT);
        StartCoroutine(Countdown(this.iterationCooldown));
    }

    private void FinishMeasure()
    {
        taskStartPanel.SetActive(false);
        scoreText.text = "Done Part" + part.ToString();
        part += 1;
        iterationCount = 0;

        // take the interaction bow out of the user's hands
        BowAndArrowLocomotion.Instance.GetGrabManager().ForceBowRelease(this.interactionBowHolster.GetBow(), this.interactionBowHolster, true);
        this.taskRunning = false;
    }

    IEnumerator Countdown(float t)
    {
        taskTime = 0f;
        taskStartPanel.SetActive(true);
        isCountdown = true;
        iterationCount += 1;

        if (iterationCount > 4)
        {
            this.FinishMeasure();
        }
        else
        {
            yield return new WaitForSeconds(t);
            isCountdown = false;
            startPanelText.text = "start";
        }
        isCountdown = false;
        yield return 0;
    }

    public float GetPartSumTime()
    {
        return this.partSumTime;
    }

    public float GetPartSumErr()
    {
        return this.partSumErr;
    }

    public void SetTaskUIPosition(Vector3 pos)
    {
        this.taskUI.transform.position = pos;
    }

    private void SpawnTarget()
    {
        // calculate target positions and determine its speed
        Vector3 tTargetPos = this.targetT.transform.position;
        Vector3 movementVector = (tTargetPos - this.eyePosition).normalized;
        Vector3 posA = tTargetPos - movementVector * (this.targetMovementDistance / 2);
        Vector3 posB = tTargetPos + movementVector * (this.targetMovementDistance / 2);
        this.currentTargetSpeed = this.targetSpeeds[this.part - 1];

        // instantiate and init the target
        this.currentTarget = Instantiate(this.targetPrefabs[this.part - 1]).GetComponent<Target>();
        this.currentTarget.Init(posA, posB, this.currentTargetSpeed, this);
    }

    public void SpawnObjectT(Vector3 position, Quaternion rotation)
    {
        this.objectT = Instantiate(this.objectTPrefab, position, rotation);
        this.objectTRotationConfirmed = false;

        // switch back to arrows without t-shapes
        this.arrowPoolSwitcher.SwitchArrowPool(0);
    }

    public bool HasControllableObjectT()
    {
        return this.isTaskStart && this.objectT != null && !this.objectTRotationConfirmed;
    }

    public void SetObjectTRotation(Quaternion rotation)
    {
        this.objectT.transform.rotation = rotation;
    }

    public void ConfirmObjectTRotation()
    {
        this.objectTRotationConfirmed = true;
    }

    public bool GetTaskRunning()
    {
        return this.taskRunning;
    }
}
