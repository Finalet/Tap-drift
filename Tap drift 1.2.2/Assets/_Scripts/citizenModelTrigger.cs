using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class citizenModelTrigger : MonoBehaviour
{

    bool triggered;

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "CitizenCar")
        {
            triggered = true;
            StartCoroutine(Recheck());
        } else if (other.gameObject.tag == "Player")
        {
            GameManager.instance.Loose();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "CitizenCar")
        {
            triggered = false;
        }
    }

    IEnumerator Recheck()
    {
        yield return new WaitForSeconds(0.2f);
        if (triggered == true)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
