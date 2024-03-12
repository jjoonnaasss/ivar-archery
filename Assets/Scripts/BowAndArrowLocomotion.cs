using Oculus.Interaction.Input.Visuals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAndArrowLocomotion : MonoBehaviour
{
    public static BowAndArrowLocomotion Instance { get; private set; }
    public enum ControllerEnum { none, left, right };

    [Header("References")]
    [SerializeField] private Transform teleportAnchor;
    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private CustomSelectionTaskMeasure customSelectionTaskMeasure;
    [SerializeField] private CustomParkourCounter customParkourCounter;
    [SerializeField] private AudioSource coinAudioSource;
    [SerializeField] private AudioSource teleportAudioSource;
    [SerializeField] private GrabManager grabManager;
    [SerializeField] private DataLogging dataLogging;

    [Header("AudioClips")]
    [SerializeField] private AudioClip[] teleportSounds;

    [Header("Settings")]
    [SerializeField] private float buttonInputDelay = 0.5f;
    [SerializeField] private float grabThreshold = 0.8f;
    [SerializeField] private float baseCoinPitch = 1f;
    [SerializeField] private float pitchPerCoinCombo = 0.1f;

    [Header("Debugging")]
    [SerializeField] private bool pausePlayWithOptionButton = false;
    [SerializeField] private bool drawGizmos;

    private float lastButtonInput;
    private string currentStage;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        this.UpdateLastButtonInput();
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (this.pausePlayWithOptionButton && OVRInput.GetDown(OVRInput.Button.Start)) UnityEditor.EditorApplication.isPaused = true;
        #endif

        this.HandleControllerInputs();
        this.grabManager.HandleDrawing();

        if (this.customSelectionTaskMeasure.HasControllableObjectT() && this.grabManager.GetInteractionBowController() != null) this.customSelectionTaskMeasure.SetObjectTRotation(this.grabManager.GetInteractionBowController().rotation);
    }

    public void UpdateLastButtonInput()
    {
        this.lastButtonInput = Time.time;
    }

    private void HandleControllerInputs()
    {
        // left grab for grabbing/releasing bows and bowstrings
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch) >= this.grabThreshold)
        {
            if (Time.time - this.lastButtonInput >= this.buttonInputDelay) this.grabManager.HandleLeftGrab();
        }
        else this.grabManager.HandleLeftGrabRelease();

        // right grab for grabbing/releasing bows and bowstrings
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch) >= this.grabThreshold)
        {
            if (Time.time - this.lastButtonInput >= this.buttonInputDelay) this.grabManager.HandleRightGrab();
        }
        else this.grabManager.HandleRightGrabRelease();

        // respawing/resetting your position
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (this.customParkourCounter.GetParkourStarted() && !this.customSelectionTaskMeasure.GetTaskRunning()) this.TeleportPlayer(this.customParkourCounter.GetCurrentRespawnPos());
        }

        // confirming the current t-shape rotation
        if (OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three))
        {
            if (this.customSelectionTaskMeasure.HasControllableObjectT()) this.customSelectionTaskMeasure.ConfirmObjectTRotation();
        }

        // option button for writing a data log with the current stats
        if (OVRInput.Get(OVRInput.Button.Start) && Time.time - this.lastButtonInput >= this.buttonInputDelay)
        {
            this.UpdateLastButtonInput();
            this.customParkourCounter.WriteDataLog();
        }
    }

    public void TeleportPlayer(Vector3? pos)
    {
        if (!pos.HasValue) return;

        // subtract current offset of the player's head to make sure the player is placed where the arrow landed
        Vector3 offsetXZ = this.centerEyeAnchor.transform.localPosition;
        offsetXZ.y = 0;

        this.teleportAnchor.position = pos.Value - offsetXZ;
    }

    public void PlayTeleportAudio()
    {
        this.teleportAudioSource.PlayOneShot(this.teleportSounds[Random.Range(0, this.teleportSounds.Length)]);
    }

    public void StartInteractionTaskMeasure(Collider interactionTaskCollider)
    {
        // deactivate collider, so that each interaction task can only be started once
        interactionTaskCollider.enabled = false;

        // start object interaction task
        this.customSelectionTaskMeasure.StartMeasure(interactionTaskCollider, this.centerEyeAnchor);
    }

    public CustomSelectionTaskMeasure GetCustomSelectionTaskMeasure()
    {
        return this.customSelectionTaskMeasure;
    }

    public void CollectCoin(GameObject coin, int coinCombo)
    {
        this.customParkourCounter.IncrementCoinCounter();
        this.dataLogging.LogCoinCollectionDistance(Vector3.Distance(coin.transform.position, this.teleportAnchor.position));
        this.coinAudioSource.pitch = this.baseCoinPitch + this.pitchPerCoinCombo * (coinCombo - 1);
        this.coinAudioSource.Play();
        coin.SetActive(false);
    }

    public void HandleBannerPassed(Collider bannerCollider)
    {
        this.currentStage = bannerCollider.gameObject.name;
        bannerCollider.enabled = false;
        this.customParkourCounter.HandleBannerPassed();
    }

    public string GetCurrentStage()
    {
        return this.currentStage;
    }

    public GrabManager GetGrabManager()
    {
        return this.grabManager;
    }

    public DataLogging GetDataLogging()
    {
        return this.dataLogging;
    }

    private void OnDrawGizmos()
    {
        if (!this.drawGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(this.teleportAnchor.position, 0.02f);
    }
}
