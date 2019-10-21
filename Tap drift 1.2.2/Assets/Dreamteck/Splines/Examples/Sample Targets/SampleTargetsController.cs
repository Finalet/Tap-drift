using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SampleTargetsController : MonoBehaviour {
    public Animator leftAnim;
    public Animator rightAnim;
    public Text framerate;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        framerate.text ="FPS: " +  Mathf.RoundToInt(1f/Time.smoothDeltaTime).ToString();
	}

    public void PlayLeftAnimation(string stateName)
    {
        leftAnim.Play(stateName, 0, 0f);
    }

    public void PlayRightAnimation(string stateName)
    {
        rightAnim.Play(stateName, 0, 0f);
    }
}
