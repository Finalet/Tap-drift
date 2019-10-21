using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockedCars : MonoBehaviour
{
    public bool mustang;
    public bool sports;
    public bool hotrod;
    public bool suv;
    public bool jeep;

    public Sprite mst;
    public Sprite sprt;
    public Sprite htr;
    public Sprite SUV;
    public Sprite jp;

    Image img;
    void Start()
    {
        img = GetComponent<Image>();
    }
    void Update()
    {
        if (mustang)
            img.sprite = mst;
        else if (sports)
            img.sprite = sprt;
        else if (hotrod)
            img.sprite = htr;
        else if (suv)
            img.sprite = SUV;
        else if (jeep)
            img.sprite = jp;
    }

    void OnDisable()
    {
        mustang = false;
        sports = false;
        hotrod = false;
        suv = false;
        jeep = false;
    }
}
