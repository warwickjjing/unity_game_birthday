using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Light switch controller
/// </summary>

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]

public class UBS_LightSwitch : MonoBehaviour
{
    // UBS - Simple Light Switch Function

    // Switches one or more lights on and off

    // Script is attached to light switch object
    // Light objects are added to the lights list array in the editor
    // Sounds are attached to light switch function in the editor
    // Action text is configured to indicate player action when switch object is in focus
    // During play, Player toggles light switch to toggle the array of lights

    public enum EnumSwitchType
    {
        Toggle, Main
    }
    [ContextMenuItem("Set Data for Object Type", "SetMovementValues")]
    public EnumSwitchType switchType;

    [Space(10)]
    public Transform toggleObject;
    public Vector3 toggleRotation = new Vector3(0f, 60f, 0f);

    [Space(10)]
    public string actionText = "Press Mouse 0 to Toggle Light";
    Material highlightMaterial;
    public AudioClip sound1;
    public AudioClip sound2;

    [Space(10)]
    public bool binaryState;
    [Space(10)]
    public List<GameObject> LightBulbList = new List<GameObject>();

    AudioSource audioSource;
    Vector3 startAngle;
    Vector3 toggleAngle;

    Material originalMaterial;

    Text messageText;
    Color tempColor;


    Image crossHair;

    private void Start()
    {
        // TitleScene에서는 UBS 상호작용 UI를 사용하지 않으므로 초기화하지 않음
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;

        if (UBS_Global.instance != null && UBS_Global.instance.defaultObjects != null)
            highlightMaterial = UBS_Global.instance.defaultObjects.highlightMaterial;

        var msgGo = GameObject.Find("MessageText1");
        if (msgGo != null) messageText = msgGo.GetComponent<Text>();

        var crossGo = GameObject.Find("CrossHair2");
        if (crossGo != null) crossHair = crossGo.GetComponent<Image>();
        if (crossHair != null) crossHair.enabled = false;

        if (toggleObject != null)
        {
            startAngle = toggleObject.localRotation.eulerAngles;
            toggleAngle = startAngle + toggleRotation;
        }
        else
        {
            // 필수 참조가 없으면 동작 비활성화 (NRE 방지)
            enabled = false;
            return;
        }

        foreach (var linkedLightBulb in LightBulbList) // turn all lights off at startup
        {
            if (linkedLightBulb == null) continue;
            var bulb = linkedLightBulb.GetComponent<UBS_LightBulb>();
            if (bulb != null) bulb.Off();
        }
    }


    void OnMouseDown()
    {
        audioSource.PlayOneShot(sound1, 1.0F);
        binaryState = !binaryState;
        if (binaryState)
        {
            toggleObject.localRotation = Quaternion.Euler(toggleAngle);
        }
        else
        {
            toggleObject.localRotation = Quaternion.Euler(startAngle);
        }
        foreach (var linkedLightBulb in LightBulbList)
        {
            if (switchType == EnumSwitchType.Toggle)
            {
                linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState = !linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState;
            }
            if (switchType == EnumSwitchType.Main)
            {
                linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState = binaryState;
            }
        }
    }

    void OnMouseEnter()
    {
        originalMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = highlightMaterial;
        messageText.fontSize = 24;
        messageText.text = actionText;
        crossHair.enabled = true;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material = originalMaterial;
        messageText.text = "";
        crossHair.enabled = false;
    }

 
}