using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject fracturedPrefab;

    [Header("Debugging")]
    [SerializeField] private bool drawGizmos;

    // position closer to the user
    private Vector3 posA;
    // position further away from the user
    private Vector3 posB;

    private float speed;
    private CustomSelectionTaskMeasure customSelectionTaskMeasure;

    private float movementDistance;
    private Vector3 movementVector;
    private bool direction;

    public void Init(Vector3 posA, Vector3 posB, float speed, CustomSelectionTaskMeasure customSelectionTaskMeasure)
    {
        this.posA = posA;
        this.posB = posB;
        this.speed = speed;
        this.customSelectionTaskMeasure = customSelectionTaskMeasure;

        this.movementDistance = (posA - posB).magnitude;
        this.movementVector = (posA - posB).normalized;
        this.direction = false;

        // let target look towards posA
        this.transform.forward = (posA - posB).normalized;

        // randomly place the target along its movement path
        this.transform.position = posA + (posB - posA) * Random.Range(0.0f, 1.0f);
    }

    private void Update()
    {
        if (this.EndPointReached()) this.direction = !this.direction;

        this.Move();
    }

    private bool EndPointReached()
    {
        if (this.direction)
        {
            // did the target reach posB yet?
            return (this.transform.position - this.posA).magnitude >= this.movementDistance;
        }
        else
        {
            // did the target reach posA yet?
            return (this.transform.position - this.posB).magnitude >= this.movementDistance;
        }
    }

    private void Move()
    {
        this.transform.position += this.movementVector * this.speed * Time.deltaTime * (this.direction ? -1 : 1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "objectT")
        {
            // spawn manipulable t-shape at the position where the arrow's t-shape hit the target
            Transform tShape = collision.collider.transform.parent;
            this.customSelectionTaskMeasure.SpawnObjectT(tShape.position, tShape.rotation);

            // stop the arrow and destroy this target
            tShape.parent.GetComponent<Arrow>().HandleTargetHit();
            this.Selfdestruct();
        }
    }

    private void Selfdestruct()
    {
        Instantiate(this.fracturedPrefab, this.transform.position, this.transform.rotation);

        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!this.drawGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.posA, 0.02f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.posB, 0.02f);
    }
}
