using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
public class RoadGenerator : MonoBehaviour
{
    public GameObject RoadSegment;


    public void GenerateSegment()
    {
        GameObject nextSegment = Instantiate(RoadSegment);
        GameManager.instance.Player.GetComponent<Player>().nextSegment = nextSegment;
    }
}
