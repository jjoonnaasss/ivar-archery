using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BowAndArrowLocomotion;

public class GrabManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private BowHolster leftHolster;
    [SerializeField] private BowHolster rightHolster;

    [Header("Settings")]
    [SerializeField] private Vector3 leftBowOffset;
    [SerializeField] private Vector3 rightBowOffset;

    [Header("Debugging")]
    [SerializeField] private bool logDebugMessages = false;

    // variables for grabbing bows
    private Bow grabbableBow;
    private ControllerEnum grabbableBowController;
    private Bow grabbedBowLeft;
    private Bow grabbedBowRight;
    private Transform interactionBowController;

    // variables for grabbing bowstrings
    private Bow grabbableStringBow;
    private ControllerEnum grabbableStringController;
    private Bow grabbedStringBowLeft;
    private Bow grabbedStringBowRight;

    // variables for picking up bows from holsters
    private BowHolster accessibleHolster;
    private ControllerEnum accessibleHolsterController;

    public void HandleDrawing()
    {
        if (this.grabbedStringBowLeft != null) this.grabbedStringBowLeft.DrawString(this.leftController.position);
        else if (this.grabbedStringBowRight != null) this.grabbedStringBowRight.DrawString(this.rightController.position);
    }

    public void HandleLeftGrab()
    {
        if (this.logDebugMessages) Debug.LogWarning("HandleLeftGrab Start");

        // grab a bow or bowstring if the controller is free
        if (this.GetControllerFreeToGrab(ControllerEnum.left))
        {
            if (this.grabbableBowController == ControllerEnum.left) this.GrabBow(this.grabbableBow, ControllerEnum.left);
            else if (this.grabbableStringController == ControllerEnum.left) this.GrabString(this.grabbableStringBow, ControllerEnum.left);
            else if (this.accessibleHolsterController == ControllerEnum.left && this.accessibleHolster.HasBow()) this.GrabBow(this.accessibleHolster.GetBow(), ControllerEnum.left);
        }
        // release the bow if the controller is holding one
        else
        {
            if (this.accessibleHolsterController == ControllerEnum.left && !this.accessibleHolster.HasBow() && grabbedStringBowRight != this.grabbedBowLeft) this.ReleaseBow(this.accessibleHolster, ref this.grabbedBowLeft, ControllerEnum.left);
        }

        if (this.logDebugMessages) Debug.LogWarning("HandleLeftGrab End");
    }

    public void HandleRightGrab()
    {
        if (this.logDebugMessages) Debug.LogWarning("HandleRightGrab Start");

        // grab a bow or bowstring if the controller is free
        if (this.GetControllerFreeToGrab(ControllerEnum.right))
        {
            if (this.grabbableBowController == ControllerEnum.right) this.GrabBow(this.grabbableBow, ControllerEnum.right);
            else if (this.grabbableStringController == ControllerEnum.right) this.GrabString(this.grabbableStringBow, ControllerEnum.right);
            else if (this.accessibleHolsterController == ControllerEnum.right && this.accessibleHolster.HasBow()) this.GrabBow(this.accessibleHolster.GetBow(), ControllerEnum.right);
        }
        // release the bow if the controller is holding one
        else
        {
            if (this.accessibleHolsterController == ControllerEnum.right && !this.accessibleHolster.HasBow() && grabbedStringBowLeft != this.grabbedBowRight) this.ReleaseBow(this.accessibleHolster, ref this.grabbedBowRight, ControllerEnum.right);
        }

        if (this.logDebugMessages) Debug.LogWarning("HandleRightGrab End");
    }

    public void HandleLeftGrabRelease()
    {
        if (this.grabbedStringBowLeft != null && this.grabbedStringBowLeft == this.grabbedBowRight) this.ReleaseString(ref this.grabbedStringBowLeft);
        else this.CancelDrawnBow(ref this.grabbedStringBowLeft);
    }

    public void HandleRightGrabRelease()
    {
        if (this.grabbedStringBowRight != null && this.grabbedStringBowRight == this.grabbedBowLeft) this.ReleaseString(ref this.grabbedStringBowRight);
        else this.CancelDrawnBow(ref this.grabbedStringBowRight);
    }

    private void GrabBow(Bow bow, ControllerEnum controller)
    {
        if (this.logDebugMessages) Debug.LogWarning("Trying to grab bow " + bow.gameObject.name);
        if (controller == ControllerEnum.none) return;

        BowAndArrowLocomotion.Instance.UpdateLastButtonInput();

        // choose controller transform and bow slots according to the received controller enum
        Transform controllerTransform = controller == ControllerEnum.left ? this.leftController : this.rightController;
        ref Bow slotToAssign = ref (controller == ControllerEnum.left ? ref this.grabbedBowLeft : ref this.grabbedBowRight);
        ref Bow otherSlot = ref (controller == ControllerEnum.left ? ref this.grabbedBowRight : ref this.grabbedBowLeft);

        // set transform parent and apply the correct offset and orientation
        bow.gameObject.SetActive(true);
        bow.transform.parent = controllerTransform;
        bow.transform.localPosition = controller == ControllerEnum.left ? this.leftBowOffset : this.rightBowOffset;
        bow.transform.localRotation = bow.GetHandle().localRotation;

        // remove bow from the other hand/controller/slot, if it is currently assigned to it
        if (bow == otherSlot) otherSlot = null;

        // assign bow to the new slot
        slotToAssign = bow;

        // hide controller visuals
        controllerTransform.GetComponentInChildren<OVRControllerHelper>().m_showState = OVRInput.InputDeviceShowState.ControllerNotInHand;

        if (this.logDebugMessages) Debug.LogWarning("Grabbed bow " + bow.gameObject.name);
    }

    private void ReleaseBow(BowHolster holster, ref Bow bowSlot, ControllerEnum controller, bool ignoreAllowed = false)
    {
        if (this.logDebugMessages) Debug.LogWarning("Trying to release bow " + bowSlot.gameObject.name);
        if (!ignoreAllowed && !bowSlot.GetAllowReleasingToHolster()) return;

        BowAndArrowLocomotion.Instance.UpdateLastButtonInput();

        // hide bow and store it in the holster
        bowSlot.gameObject.SetActive(false);
        bowSlot.transform.parent = holster.transform;
        holster.StoreBow(bowSlot);

        // make sure the stored bow isn't blocking the grabbable slot
        this.RemoveGrabbableBow(bowSlot, controller);

        // show controller visuals
        if (controller == ControllerEnum.left) this.leftController.GetComponentInChildren<OVRControllerHelper>().m_showState = OVRInput.InputDeviceShowState.ControllerInHandOrNoHand;
        else if (controller == ControllerEnum.right) this.rightController.GetComponentInChildren<OVRControllerHelper>().m_showState = OVRInput.InputDeviceShowState.ControllerInHandOrNoHand;

        if (this.logDebugMessages) Debug.LogWarning("Released bow " + bowSlot.gameObject.name);

        bowSlot = null;
    }

    private void GrabString(Bow stringBow, ControllerEnum controller)
    {
        BowAndArrowLocomotion.Instance.UpdateLastButtonInput();

        // only allow grabbing the bowstring, if the bow it is attached is currently being grabbed by the other controller
        if (controller == ControllerEnum.none || (controller == ControllerEnum.left && this.grabbedBowRight != stringBow) || (controller == ControllerEnum.right && this.grabbedBowLeft != stringBow)) return;

        if (controller == ControllerEnum.right) this.grabbedStringBowRight = stringBow;
        else this.grabbedStringBowLeft = stringBow;

        stringBow.HandleStringGrabbed();

        if (this.logDebugMessages) Debug.LogWarning("Grabbed bowstring of " + stringBow.gameObject.name);
    }

    private void ReleaseString(ref Bow grabbedStringBowSlot)
    {
        if (this.logDebugMessages) Debug.LogWarning("Releasing bowstring of " + grabbedStringBowSlot.gameObject.name);

        grabbedStringBowSlot.HandleStringReleased();
        grabbedStringBowSlot = null;
    }

    private void CancelDrawnBow(ref Bow drawnBowSlot)
    {
        if (drawnBowSlot == null) return;

        drawnBowSlot.CancelDraw();
        drawnBowSlot = null;
    }

    public void SetGrabbableBow(Bow bow, ControllerEnum controller)
    {
        this.grabbableBow = bow;
        this.grabbableBowController = controller;

        if (this.logDebugMessages) Debug.LogWarning("Bow " + bow.gameObject.name + " grabbable by " + controller.ToString() + " controller");
    }

    public void RemoveGrabbableBow(Bow bow, ControllerEnum controller)
    {
        if (bow == this.grabbableBow && controller == this.grabbableBowController)
        {
            this.grabbableBow = null;
            this.grabbableBowController = ControllerEnum.none;
        }
    }

    public void SetGrabbableString(Bow bow, ControllerEnum controller)
    {
        this.grabbableStringBow = bow;
        this.grabbableStringController = controller;

        if (this.logDebugMessages) Debug.LogWarning("String of " + bow.gameObject.name + " grabbable by " + controller.ToString() + " controller");
    }

    public void RemoveGrabbableString(Bow bow, ControllerEnum controller)
    {
        if (bow == this.grabbableStringBow && controller == this.grabbableStringController)
        {
            this.grabbableStringBow = null;
            this.grabbableStringController = ControllerEnum.none;
        }
    }

    public void SetAccessibleBowHolster(BowHolster holster, ControllerEnum controller)
    {
        this.accessibleHolster = holster;
        this.accessibleHolsterController = controller;

        if (this.logDebugMessages) Debug.LogWarning("Bow holster " + holster.gameObject.name + " accessible by " + controller.ToString() + " controller");
    }

    public void RemoveAccessibleBowHolster(BowHolster holster, ControllerEnum controller)
    {
        if (holster == this.accessibleHolster && controller == this.accessibleHolsterController)
        {
            this.accessibleHolster = null;
            this.accessibleHolsterController = ControllerEnum.none;
        }
    }

    private bool GetControllerFreeToGrab(ControllerEnum controller)
    {
        if (controller == ControllerEnum.left) return this.grabbedBowLeft == null && this.grabbedStringBowLeft == null;
        else if (controller == ControllerEnum.right) return this.grabbedBowRight == null && this.grabbedStringBowRight == null;

        return false;
    }
    public void ForceBowGrab(Bow bow)
    {
        ControllerEnum releasedController = this.ForceBowReleaseAll();
        if (releasedController == ControllerEnum.none) Debug.LogError("BowAndArrowLocomotion: Couldn't force a bow release!");

        this.GrabBow(bow, releasedController);
        this.interactionBowController = releasedController == ControllerEnum.left ? this.leftController : this.rightController;
    }

    private ControllerEnum ForceBowReleaseAll()
    {
        ControllerEnum controller = ControllerEnum.none;
        BowHolster freeHolster = this.GetFreeHolster();

        if (this.grabbedBowLeft != null)
        {
            if (freeHolster == null) return controller;

            this.ReleaseBow(freeHolster, ref this.grabbedBowLeft, ControllerEnum.left);
            controller = ControllerEnum.left;

            freeHolster = this.GetFreeHolster();
        }

        if (this.grabbedBowRight != null)
        {
            if (freeHolster == null) return controller;

            this.ReleaseBow(freeHolster, ref this.grabbedBowRight, ControllerEnum.right);
            controller = ControllerEnum.right;
        }

        return controller;
    }

    public void ForceBowRelease(Bow bow, BowHolster holster, bool ignoreAllowed = false)
    {
        if (this.grabbedBowLeft == bow) this.ReleaseBow(holster, ref this.grabbedBowLeft, ControllerEnum.left, ignoreAllowed);
        else if (this.grabbedBowRight == bow) this.ReleaseBow(holster, ref this.grabbedBowRight, ControllerEnum.right, ignoreAllowed);
    }

    private BowHolster GetFreeHolster()
    {
        if (!this.leftHolster.HasBow()) return this.leftHolster;
        if (!this.rightHolster.HasBow()) return this.rightHolster;

        return null;
    }

    public Transform GetInteractionBowController()
    {
        return this.interactionBowController;
    }
}
