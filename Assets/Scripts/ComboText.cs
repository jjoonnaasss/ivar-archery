using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float animationDuration;
    [SerializeField] private float risingSpeed;
    [SerializeField] private float targetScale;
    [SerializeField] private float destroyDelay;
    [SerializeField] private Vector3 offsetFromCoin;

    [Header("References")]
    [SerializeField] private TMP_Text text;

    private float startTime;
    private float scalePerSecond;
    private bool destroyScheduled = false;

    private void Start()
    {
        this.transform.LookAt(Camera.main.transform);
        this.transform.position = this.transform.TransformPoint(this.offsetFromCoin);

        this.startTime = Time.time;
        float startScale = this.transform.localScale.x;
        this.scalePerSecond = (this.targetScale - startScale) / this.animationDuration;
    }

    private void Update()
    {
        if (this.destroyScheduled) return;

        if (Time.time - this.startTime >= this.animationDuration)
        {
            Destroy(this.gameObject, this.destroyDelay);
            this.destroyScheduled = true;
        }
        else this.MoveAndScale();
    }

    private void MoveAndScale()
    {
        this.transform.position += this.transform.up * Time.deltaTime * this.risingSpeed;

        float scale = this.scalePerSecond * Time.deltaTime;
        this.transform.localScale += new Vector3(scale, scale, scale);
    }

    public void SetComboText(string text)
    {
        this.text.text = text;
    }
}
