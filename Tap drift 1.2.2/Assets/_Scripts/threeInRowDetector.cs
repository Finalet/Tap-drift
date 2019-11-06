using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class threeInRowDetector : MonoBehaviour
{
    public List<GameObject> carsInRow;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CitizenCar")
        {
            carsInRow.Add(other.gameObject);
        }
    }

    void Update()
    {
        if (carsInRow.Count >= 3)
        {
            int x = Random.Range(0, 3);
            if (x == 0)
            {
                Destroy(transform.parent.gameObject);
            }
            else
            {
                if (carsInRow[x] != null) {
                    Destroy(carsInRow[x].transform.parent.gameObject);
                    carsInRow.RemoveAt(x);
                } 
            }
           
        }
    }
}
