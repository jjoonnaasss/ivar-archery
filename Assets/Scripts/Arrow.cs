using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public enum ArrowType { teleportation, coinCollection, objectInteraction };

    [Header("Settings")]
    [SerializeField] private ArrowType arrowType;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private float waterReturnDelay;
    [SerializeField] private Vector3 tShapeRotation;
    [SerializeField] private float maxTeleportAngle;

    [Header("References")]
    [SerializeField] private Transform tip;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip impactAudioSolid;
    [SerializeField] private AudioClip[] impactAudioWater;

    [Header("Prefabs")]
    [SerializeField] private GameObject comboTextPrefab;

    [Header("Debugging")]
    [SerializeField] private bool drawGizmos;

    private AudioSource audioSource;
    private Rigidbody rb;
    private ArrowPool arrowPool;
    private Vector3? collisionPos;
    private float length;
    private System.Random random;
    private Transform tShape;
    private bool isMuted;
    private int coinCount;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
        this.audioSource = this.GetComponent<AudioSource>();
        this.length = (this.tip.position - this.transform.position).magnitude;
        this.random = new System.Random();

        if (this.arrowType == ArrowType.objectInteraction) this.tShape = this.transform.Find("ObjectT");
    }

    private void FixedUpdate()
    {
        this.RotateWithTrajectory();
        if (this.tShape != null) this.RotateTShape();
    }

    public void Init(ArrowPool pool)
    {
        this.arrowPool = pool;
        this.SetTrailVisibility(false);
    }

    private void RotateWithTrajectory()
    {
        if (this.rb.isKinematic || this.rb.velocity == Vector3.zero) return;

        this.transform.rotation = Quaternion.LookRotation(this.rb.velocity, this.transform.up);
    }

    private void RotateTShape()
    {
        this.tShape.Rotate(this.tShapeRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // prevent triggering multiple times
        if (this.rb.isKinematic) return;

        //Debug.LogWarning("Collided with " + collision.gameObject.name);

        // all arrow types just stick into obstacles without triggering anything
        if (collision.gameObject.tag == "Obstacle")
        {
            this.HandleSolidHit(false);
            return;
        }

        // teleportation arrow ignore coins, but trigger teleportation when hitting the floor
        if (this.arrowType == ArrowType.teleportation)
        {
            if (collision.gameObject.tag == "Floor")
            {
                // prevent teleportation to the side of e.g. the bridge
                for (int i = 0; i < collision.contactCount; i++)
                {
                    if (Vector3.Angle(Vector3.up, collision.GetContact(i).normal) < this.maxTeleportAngle)
                    {
                        this.HandleSolidHit(true);
                        return;
                    }
                }

                this.HandleSolidHit(false);
            }
        }
        // coin collection arrows collect coins, but do nothing when hitting the floor
        else if (this.arrowType == ArrowType.coinCollection)
        {
            if (collision.gameObject.tag == "Floor")
            {
                this.HandleSolidHit(false);
            }
        }
        // object interaction arrows trigger UI and t-shape targets, but do nothing when hitting the floor
        else if (this.arrowType == ArrowType.objectInteraction)
        {
            if (collision.gameObject.tag == "Floor")
            {
                this.HandleSolidHit(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogWarning("Triggered " + other.gameObject.name);

        // both arrows types just disappear into the water
        if (other.tag == "Water")
        {
            this.HandleWaterHit();
        }
        // only the coin collection arrows interact with coins
        else if (this.arrowType == ArrowType.coinCollection)
        {
            if (other.tag == "coin")
            {
                this.coinCount++;
                BowAndArrowLocomotion.Instance.CollectCoin(other.gameObject, this.coinCount);
                this.ShowComboText(other.transform);
            }
        }
        // only the interaction arrows can interact with the task UI and object interaction targets
        else if (this.arrowType == ArrowType.objectInteraction)
        {
            if (other.tag == "selectionTaskStart" && this.tShape == null)
            {
                BowAndArrowLocomotion.Instance.GetCustomSelectionTaskMeasure().StartNewIteration();
            }
            else if (other.tag == "done" && this.tShape == null)
            {
                BowAndArrowLocomotion.Instance.GetCustomSelectionTaskMeasure().EndCurrentIteration();
            }
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        this.audioSource.PlayOneShot(clip);
    }

    private Vector3? DetermineEntryPosition(LayerMask layerMask)
    {
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, this.length, layerMask)) return hit.point;

        return null;
    }

    public void HandleArrowShot()
    {
        this.SetTrailVisibility(true);
        BowAndArrowLocomotion.Instance.GetDataLogging().IncreaseArrowShotCounter(this.arrowType);
        this.coinCount = 0;
    }

    private void HandleSolidHit(bool teleport)
    {
        this.rb.isKinematic = true;
        if (!this.isMuted) this.PlayAudio(this.impactAudioSolid);
        else this.isMuted = false;

        if (teleport)
        {
            BowAndArrowLocomotion.Instance.PlayTeleportAudio();
            this.collisionPos = this.DetermineEntryPosition(this.floorLayer);
        }

        Invoke(teleport ? "TeleportAndReturnArrow" : "ReturnArrow", this.impactAudioSolid.length);
    }

    private void HandleWaterHit()
    {
        this.isMuted = true;
        this.PlayAudio(this.GetWaterImpactAudioClip());
    }

    public void ReturnArrow()
    {
        if (this.arrowType == ArrowType.coinCollection && this.coinCount > 0) BowAndArrowLocomotion.Instance.GetDataLogging().LogCoinCombo(this.coinCount);
        this.SetTrailVisibility(false);
        this.arrowPool.ReturnArrow(this.gameObject);
    }

    private void TeleportAndReturnArrow()
    {
        BowAndArrowLocomotion.Instance.TeleportPlayer(this.collisionPos);
        this.ReturnArrow();
    }

    public void HandleTargetHit()
    {
        this.HandleSolidHit(false);
    }

    private AudioClip GetWaterImpactAudioClip()
    {
        return this.impactAudioWater[this.random.Next(this.impactAudioWater.Length)];
    }

    private void SetTrailVisibility(bool visible)
    {
        if (this.trailRenderer != null) this.trailRenderer.enabled = visible;
    }

    private void ShowComboText(Transform coinTransform)
    {
        if (this.coinCount < 2) return;

        ComboText comboText = Instantiate(this.comboTextPrefab, coinTransform.position, Quaternion.identity).GetComponent<ComboText>();
        if (comboText != null) comboText.SetComboText("x" + this.coinCount.ToString());
    }

    private void OnDrawGizmos()
    {
        if (!this.drawGizmos) return; 

        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward.normalized * (this.tip.position - this.transform.position).magnitude);

        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, (this.tip.position - this.transform.position).magnitude, this.floorLayer)) Gizmos.DrawSphere(hit.point, 0.01f);
    }
}
