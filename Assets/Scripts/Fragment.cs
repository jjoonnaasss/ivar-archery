using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fragment : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1f;

    private bool onFloor = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (this.onFloor) return;

        if (collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Obstacle") Destroy(this.gameObject, this.destroyDelay);
    }
}
