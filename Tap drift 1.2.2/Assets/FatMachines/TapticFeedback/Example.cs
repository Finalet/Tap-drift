using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour {

    [SerializeField] Text text;
    [SerializeField] Image tapticImage;
    [SerializeField] Color on;
    [SerializeField] Color off;

    void Start() {
        text.text = Taptic.tapticOn ? "TURN OFF" : "TURN ON";
    }

    public void TriggerTaptic(string type) {
        if (type == "warning") {
            Taptic.Warning();
        } else if (type == "failure") {
            Taptic.Failure();
        } else if (type == "success") {
            Taptic.Success();
        } else if (type == "light") {
            Taptic.Light();
        } else if (type == "medium") {
            Taptic.Medium();
        } else if (type == "heavy") {
            Taptic.Heavy();
        }else if(type == "default"){
            Taptic.Default();
        }else if(type == "vibrate"){
            Taptic.Vibrate();
        }else if(type == "selection"){
            Taptic.Selection();
        }
    }

    public void Toggle() {
        Taptic.tapticOn = !Taptic.tapticOn;
        text.text = Taptic.tapticOn ? "TURN OFF" : "TURN ON";
        tapticImage.color = Taptic.tapticOn ? on : off;
        Taptic.Selection();
    }

}