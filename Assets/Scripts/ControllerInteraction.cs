using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BowAndArrowLocomotion;

public class ControllerInteraction : MonoBehaviour
{
    [SerializeField] private ControllerEnum side;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bow")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().SetGrabbableBow(other.GetComponent<Bow>(), this.side);
        }
        else if (other.tag == "BowString")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().SetGrabbableString(other.GetComponentInParent<Bow>(), this.side);
        }
        else if (other.tag == "BowHolster")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().SetAccessibleBowHolster(other.GetComponent<BowHolster>(), this.side);
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            BowAndArrowLocomotion.Instance.StartInteractionTaskMeasure(other);
        }
        else if (other.CompareTag("banner"))
        {
            BowAndArrowLocomotion.Instance.HandleBannerPassed(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bow")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().RemoveGrabbableBow(other.GetComponent<Bow>(), this.side);
        }
        else if (other.tag == "BowString")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().RemoveGrabbableString(other.GetComponentInParent<Bow>(), this.side);
        }
        else if (other.tag == "BowHolster")
        {
            BowAndArrowLocomotion.Instance.GetGrabManager().RemoveAccessibleBowHolster(other.GetComponent<BowHolster>(), this.side);
        }
    }
}
