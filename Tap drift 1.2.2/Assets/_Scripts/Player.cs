using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum Model
    {
        Elegant,
        Mustang,
        Sports,
        HotRod,
        SUV,
        Jeep
    };

    public float currentSpeed;
    public float baseSpeed;
    public float boostedMultiplier;
    public float driftMultiplier;
    public bool boosted;

    [Header("Drift")]
    [SerializeField] float driftDuration;
    public float driftFuel;
    public float maxDriftFuel;

    [Header("Bulldozer")]
    public bool bulldozerDeployed;
    public GameObject bulldozer;

    [Space]
    public GameObject roadGenerator;
    
    [Space]
    public Model currentModel;
    public GameObject carModel;
    public ParticleSystem explosion;

    [Header("Segments")]
    public GameObject currentSegment;
    public GameObject nextSegment;
    public GameObject previousSegment;
    public GameObject previousSegment2;


    SplineFollower follower;

    [System.NonSerialized] public bool driftLeft;
    [System.NonSerialized] public bool driftRight;

    float driftDirectoinChangeTimer;

    [Header("Car Models")]
    public GameObject ElegantCar;
    public GameObject BigJeep;
    public GameObject HotRod;
    public GameObject Mustang;
    public GameObject SportCar;
    public GameObject SUV;

    [Space]
    public bool gameStarted;
    public GameObject directionalLight;


    void Awake()
    {
        if (ES3.KeyExists("currentModel"))
            currentModel = ES3.Load<Model>("currentModel");
        else
            currentModel = Model.Elegant;

        if (currentModel == Model.Elegant)
            carModel = Instantiate(ElegantCar, transform);
        else if (currentModel == Model.Jeep)
            carModel = Instantiate(BigJeep, transform);
        else if (currentModel == Model.HotRod)
            carModel = Instantiate(HotRod, transform);
        else if (currentModel == Model.Mustang)
            carModel = Instantiate(Mustang, transform);
        else if (currentModel == Model.Sports)
            carModel = Instantiate(SportCar, transform);
        else if (currentModel == Model.SUV)
            carModel = Instantiate(SUV, transform);

        carModel.tag = "Player";
        carModel.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void SpawnNewModel ()
    {
        Destroy(carModel.gameObject);

        if (currentModel == Model.Elegant)
            carModel = Instantiate(ElegantCar, transform);
        else if (currentModel == Model.Jeep)
            carModel = Instantiate(BigJeep, transform);
        else if (currentModel == Model.HotRod)
            carModel = Instantiate(HotRod, transform);
        else if (currentModel == Model.Mustang)
            carModel = Instantiate(Mustang, transform);
        else if (currentModel == Model.Sports)
            carModel = Instantiate(SportCar, transform);
        else if (currentModel == Model.SUV)
            carModel = Instantiate(SUV, transform);

        carModel.tag = "Player";
        carModel.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void SaveModelIndex ()
    {
        ES3.Save<Model>("currentModel", currentModel);
    }

    void Start()
    {
        follower = GetComponent<SplineFollower>();
        follower.onEndReached += OnEndReached;

        StartCoroutine(GenerateSecondSegment());
    }

    bool one = false;
    public void StartGame ()
    {
        gameStarted = true;
        Invoke("EnableFollower", 2);
        Invoke("ShowDriftTip", 8);
        Invoke("ShowTutorial", 2);
        driftTimer = 20;
        StartCoroutine(GameManager.instance.Canvas.GetComponent<CanvasScript>().CountDown());
        one = true;
        Camera.main.rect = new Rect(0, 0, 1, 1);
        Camera.main.GetComponent<BackgroundColor>().gameStarted = true;
        Camera.main.GetComponent<BackgroundColor>().time = 0;
        GameManager.instance.Canvas.GetComponent<CanvasScript>().StartGame();

        Image[] fuel = GameManager.instance.Canvas.GetComponent<CanvasScript>().fullFuelBar.GetComponentsInChildren<Image>();
        foreach (Image img in fuel)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }

        Image[] levelBar = GameManager.instance.Canvas.GetComponent<CanvasScript>().levelBar.GetComponentsInChildren<Image>();
        foreach (Image img in levelBar)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }
    }

    void EnableFollower ()
    {
        one = false;
        GetComponent<SplineFollower>().enabled = true;
        GetComponent<SplineFollower>().autoFollow = true;
        GameManager.instance.stopScore = 1;
    }


    void Update()
    {
        explosion.transform.position = carModel.transform.position + 0.6f * Vector3.up;

        BulldozerUpdate();

        //Rotate car before start of the game
        if (!gameStarted)
            carModel.transform.Rotate(Vector3.up, 20 * Time.deltaTime);

        if (one)
        {
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, new Vector3 (0, 4.35f, -9.5f), 0.1f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 0.1f);

            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 0.1f);
            transform.position = Vector3.Lerp(transform.position, Vector3.zero, 0.1f);

            directionalLight.transform.rotation = Quaternion.Lerp(directionalLight.transform.rotation, Quaternion.Euler(50, -30, 0), 0.1f);
        }


        driftFuel = Mathf.Clamp(driftFuel, 0, maxDriftFuel);

        currentSpeed = Mathf.Round(baseSpeed * boostedMultiplier * driftMultiplier);
        follower.followSpeed = currentSpeed;


        if (Input.GetKeyDown(KeyCode.Mouse0) && !GameManager.instance.lost && gameStarted && GetComponent<SplineFollower>().isActiveAndEnabled) DoubleClick();

        driftDuration = driftFuel / 10;

        if (driftLeft == true)
        {
            Quaternion driftLeftVector = Quaternion.Euler(0, 35, 0);
            carModel.transform.localRotation = Quaternion.Lerp(carModel.transform.localRotation, driftLeftVector, 0.1f);
            driftFuel -= Time.deltaTime*10;
            
            if (transform.rotation.y >= 0)
            {
                if(driftDirectoinChangeTimer <= 0.5f)
                {
                    driftDirectoinChangeTimer += Time.deltaTime;
                } else
                {
                    driftLeft = false;
                    driftRight = true;
                    driftDirectoinChangeTimer = 0;
                }
            }

            if (driftFuel <= 0) ExitDrift();

        } else if (driftRight == true)
        {
            Quaternion driftRightVector = Quaternion.Euler(0, -35, 0);
            carModel.transform.localRotation = Quaternion.Lerp(carModel.transform.localRotation, driftRightVector, 0.1f);
            driftFuel -= Time.deltaTime * 10;
            
            if (transform.rotation.y <= 0)
            {
                if (driftDirectoinChangeTimer <= 0.5f)
                {
                    driftDirectoinChangeTimer += Time.deltaTime;
                }
                else
                {
                    driftLeft = true;
                    driftRight = false;
                    driftDirectoinChangeTimer = 0;
                }
            }

            if (driftFuel <= 0) ExitDrift();

        } else if (gameStarted)
        {
            carModel.transform.localRotation = Quaternion.Lerp(carModel.transform.localRotation, Quaternion.Euler(0,0,0), 0.1f);
        }

        //Reset doubleClick
        ResetDoubleClick();
        DriftTipTimer();
    }


    void OnEndReached()
    {
        follower.computer = nextSegment.GetComponent<SplineComputer>();
        follower.SetDistance(0);
        previousSegment2 = previousSegment;
        previousSegment = currentSegment;
        currentSegment = nextSegment;

        roadGenerator.GetComponent<RoadGenerator>().GenerateSegment();
        StartCoroutine(DeletePreviousSegment2());
    }

    public IEnumerator GenerateSecondSegment()
    {
        yield return new WaitForSeconds(1);
        roadGenerator.GetComponent<RoadGenerator>().GenerateSegment();
    }
    
    IEnumerator DeletePreviousSegment2 ()
    {
        yield return new WaitForSeconds(1);
        Destroy(previousSegment2.gameObject);
    }

    public float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    public void DoubleClick ()
    {
        clicked++;
        if (clicked == 1) clicktime = Time.time;

        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            if (driftLeft == false && driftRight == false) Drift();
            else ExitDrift();

        }
        else if (clicked > 2 || Time.time - clicktime > 0.5f) clicked = 0;
    }

    void ResetDoubleClick ()
    {
        if (Time.time - clicktime > 0.5f) clicked = 0; //Reset doubleClick
    }

    void Drift()
    {
        if (GameManager.instance.isPaused)
            return;

        if (driftFuel > 0)
        {
            if (transform.rotation.y <= 0) driftLeft = true;
            else driftRight = true;
            driftMultiplier = 1.5f;

            HideDriftTip();
            driftTimer = 0;

            if (GameManager.instance.soundOn) Camera.main.GetComponent<AudioSource>().Play();

            Taptic.Warning();
        }
    }

    public void ExitDrift()
    {
        if (GameManager.instance.isPaused)
            return;

        if (driftLeft == true) driftLeft = false;
        else if (driftRight == true) driftRight = false;

        driftFuel = Mathf.Round(driftFuel);
        driftMultiplier = 1;

        driftTimer = 20;
        timer = false;
        StartCoroutine(GameManager.instance.Canvas.GetComponent<CanvasScript>().AddDriftScore());

        if (GameManager.instance.soundOn) Camera.main.GetComponent<AudioSource>().Stop();
    }


    public IEnumerator Boost ()
    {
        boosted = true;
        boostedMultiplier = 2f;
        yield return new WaitForSeconds(0.7f);
        bool end = false;
        while (end == false)
        {
            boostedMultiplier -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
            if (boostedMultiplier - 1 <= 0.01f)
            {
                end = true;
                boostedMultiplier = Mathf.Round(boostedMultiplier);
            }
        }
        boosted = false;
    }

    void MoveCar ()
    {
#if UNITY_EDITOR

        if (Input.GetKey(KeyCode.Mouse0))
        {
            float moveCar = carModel.transform.localPosition.x + Input.GetAxis("Mouse X") * 0.2f;
            float carsX = Mathf.Clamp(moveCar, -2.1f, 2.1f);
            carModel.transform.localPosition = new Vector3(carsX, carModel.transform.localPosition.y, carModel.transform.localPosition.z);
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            float moveCarPercentage = carModel.transform.localPosition.x + 4.2f * (Input.GetTouch(0).deltaPosition.x / Screen.width);
            float carsX = Mathf.Clamp(moveCarPercentage, -2.1f, 2.1f);
            carModel.transform.localPosition = new Vector3(carsX, carModel.transform.localPosition.y, carModel.transform.localPosition.z);
        }
#endif
    }

    void FixedUpdate()
    {
        if (gameStarted) MoveCar();

        float desiredFOV = 60 + (currentSpeed - baseSpeed) * 2f;
        float clampedFOV = Mathf.Clamp(desiredFOV, 60, 110);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, clampedFOV, 0.1f);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            DeployBulldozer();
#else 
        if (Input.touchCount > 0 && Input.GetTouch(0).deltaPosition.y >= Screen.height / 12 && gameStarted)
            DeployBulldozer();
#endif
    }


    void ShowDriftTip()
    {
        if (!driftLeft && !driftRight && gameStarted && !GameManager.instance.lost)
            GameManager.instance.Canvas.GetComponent<CanvasScript>().showDriftTip = true;
    }
    public void HideDriftTip()
    {
        GameManager.instance.Canvas.GetComponent<CanvasScript>().showDriftTip = false;
    }

    void ShowTutorial ()
    {
        GameManager.instance.Canvas.GetComponent<CanvasScript>().Tutorial.SetActive(true);
    }
    public void HideTutorial ()
    {
        GameManager.instance.Canvas.GetComponent<CanvasScript>().Tutorial.SetActive(false);
    }

    public float driftTimer = 0;
    bool timer = true;
    void DriftTipTimer()
    {
        if (driftTimer > 0)
        {
            driftTimer -= Time.deltaTime;
        }
        else if (!timer && gameStarted && !GameManager.instance.lost)
        {
            ShowDriftTip();
            timer = true;
        }

        if (GameManager.instance.lost)
            driftTimer = 0;
    }

    void BulldozerUpdate () {
        bulldozer.transform.localPosition = carModel.transform.localPosition + new Vector3(0, 0.15f - carModel.transform.localPosition.y, 1.7f);
        
        if (bulldozerDeployed) {
            bulldozer.SetActive(true);
        } else {
            bulldozer.SetActive(false);
        }
    }
    public void DeployBulldozer () {
        if (bulldozerDeployed)
            return;

        bulldozerDeployed = true;
        bulldozer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); 
    }
}
