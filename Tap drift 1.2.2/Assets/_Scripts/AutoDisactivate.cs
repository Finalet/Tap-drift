using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisactivate : MonoBehaviour
{
    public float disactivationTime;

    void OnEnable()
    {
        Invoke("Disactivate", disactivationTime);
    }

    void Disactivate()
    {
        this.gameObject.SetActive(false);
    }
}
