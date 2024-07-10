using UnityEngine;
using UnityEngine.UI;

public class RememberMeToggle : MonoBehaviour
{
    bool currentState;
    Toggle toogle;
    private void Awake()
    {
        toogle = GetComponent<Toggle>();
    }
    private void OnEnable()
    {
        toogle.isOn = GameManager.Instance.isRememberDataUser == "On" ? true : false;
    }
    void Start()
    {
        toogle.isOn = GameManager.Instance.isRememberDataUser == "On" ? true : false;
        currentState = toogle.isOn;
    }
    void Update()
    {
        if (currentState != GetComponent<Toggle>().isOn) { 
            currentState = !currentState;
            GameManager.Instance.isRememberDataUser = currentState == true ? "On" : "Off";
        }
    }
}
