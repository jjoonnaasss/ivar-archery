using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FracturedTarget : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] fragments;
    [SerializeField] private Material outerMaterial;
    [SerializeField] private Material innerMaterial;
    [SerializeField] private float explosionForce;
    [SerializeField] private Vector3 explosionBias;

    private void Start()
    {
        foreach (MeshRenderer fragment in this.fragments) this.InitFragment(fragment);
    }

    private void InitFragment(MeshRenderer fragment)
    {
        fragment.materials[0] = this.outerMaterial;
        fragment.materials[1] = this.innerMaterial;

        Vector3 direction = ((fragment.bounds.center - (this.transform.position)) + this.CalculateBiasDirection(fragment.transform)).normalized;

        fragment.GetComponent<Rigidbody>().AddForce(direction * this.explosionForce);
    }

    private Vector3 CalculateBiasDirection(Transform transform)
    {
        return (transform.right * this.explosionBias.x + transform.up * this.explosionBias.y + transform.forward * this.explosionBias.z).normalized;
    }
}
