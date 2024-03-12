using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPoolSwitcher : MonoBehaviour
{
    [SerializeField] private ArrowPool[] arrowPools;
    [SerializeField] private Transform[] arrowParents;

    private Bow bow;

    private void Start()
    {
        if (this.arrowPools.Length != this.arrowParents.Length) Debug.LogError("ArrowPoolSwitcher: lengths of pool- and parent-arrays don't match!");
        else this.bow = this.GetComponent<Bow>();
    }

    public void SwitchArrowPool(int index)
    {
        if (index < 0 || index >= this.arrowPools.Length) Debug.LogError("ArrowPoolSwitcher: Pool index out of bounds!");
        else if (this.bow == null) Debug.LogError("ArrowPoolSwitcher: Bow couldn't be found!");
        else this.bow.SwitchArrowPool(this.arrowPools[index], this.arrowParents[index]);
    }
}
