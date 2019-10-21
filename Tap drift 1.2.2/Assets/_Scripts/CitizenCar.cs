using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
public class CitizenCar : MonoBehaviour
{
    GameObject model;
    public string line;
    [System.NonSerialized]
    public GameObject nextSpline;
    public GameObject threeInRowDetector;

    SplineFollower follower;

    public GameObject ElegantCar;
    public GameObject BigJeep;
    public GameObject HotRod;
    public GameObject Mustang;
    public GameObject SportCar;
    public GameObject SUV;

    public Material ElegantCarTexture;
    public Material BigJeepTexture;
    public Material HotRodTexture;
    public Material MustangTexture;
    public Material SportCarTexture;
    public Material SUVTexture;


    void Awake()
    {
        int x = Random.Range(0, 6);
        if (x == 0)
        {
            model = Instantiate(ElegantCar, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = ElegantCarTexture;
            }
        }
        else if (x == 1)
        {
            model = Instantiate(BigJeep, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = BigJeepTexture;
            }
        }
        else if (x == 2)
        { 
            model = Instantiate(HotRod, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = HotRodTexture;
            }
        }
        else if (x == 3)
        {
            model = Instantiate(Mustang, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = MustangTexture;
            }
        }
        else if (x == 4)
        {
            model = Instantiate(SportCar, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = SportCarTexture;
            }
        }
        else if (x == 5)
        {
            model = Instantiate(SUV, transform);
            MeshRenderer[] mats = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mats)
            {
                if (m.gameObject.name != "breakLights")
                    m.material = SUVTexture;
            }
        }


        model.AddComponent<citizenModelTrigger>();
        model.AddComponent<Rigidbody>();
        model.GetComponent<Rigidbody>().isKinematic = true;
        model.GetComponent<BoxCollider>().isTrigger = true;
        model.tag = "CitizenCar";
        follower = GetComponent<SplineFollower>();
        threeInRowDetector.SetActive(false);
        Invoke("EnableThreeInRow", 1);
    }

    void Start()
    {
        follower.computer = gameObject.GetComponentInParent<SplineComputer>();
        follower.followSpeed = GameManager.instance.speedOfCitizenCars;
        follower.SetPercent(Random.Range(0f, 1f));
        model.transform.localPosition = new Vector3(randomLine(), model.transform.localPosition.y, model.transform.localPosition.z); //Random line of the road
        follower.onEndReached += OnEndReached;
        transform.parent = GameManager.instance.CitizenCarPool.transform;
    }

    float randomLine()
    {
        int x = Random.Range(0, 3);
        if (x == 0)
        {   
            float X = 0;
            line = "center";
            return X;
        } else if (x == 1)
        {
            float X = 2.12f;
            line = "right";
            Destroy(threeInRowDetector);
            return X;
        } else if (x == 2)
        {
            float X = -2.12f;
            line = "left";
            Destroy(threeInRowDetector);
            return X;
        } else
        {
            return 0;
        }
    }

    void OnEndReached()
    {
        if (nextSpline != null)
        {
            follower.computer = nextSpline.GetComponent<SplineComputer>();
            follower.SetDistance(0);
            nextSpline = null;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Road")
            nextSpline = other.gameObject;
    }

    void Update()
    {

        if (GameManager.instance.lost || !GameManager.instance.Player.GetComponent<Player>().gameStarted)
            follower.followSpeed = 0;
        else
            follower.followSpeed = GameManager.instance.speedOfCitizenCars;

        if (follower.computer == null)
            Destroy(gameObject);
    }

    void EnableThreeInRow ()
    {
        if (threeInRowDetector != null)
            threeInRowDetector.SetActive(true);
    }
}
