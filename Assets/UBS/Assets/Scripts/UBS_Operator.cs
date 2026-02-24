using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Object Operation Highlight and Mouse Trigger Controller
/// for actuation of objects with attached Actuator script
/// </summary>


[RequireComponent(typeof(BoxCollider))]

public class UBS_Operator : MonoBehaviour
{

    // UBS - Operation Trigger

    // object selection and mouse triggered operation

    public enum EnumOperatorType
    {
        Toggle, Open, Close
    }
    public EnumOperatorType operatorType;

    private bool init = false;
    private Text messageText;
    private Transform player;

    public float operateRange = 3.5f;
    public string actionText = "Press Mouse 0 to Operate";

    Image crossHair;

    Material highlightMaterial;
    Material originalMaterial;
    GameObject lastHighlightedObject;

    [Header("Status")]
    public bool inRange;         //"in range to operate" flag
    [HideInInspector] public bool triggered;         //one-shot trigger to initiate operation locally or by external objects

    [Header("Controlled Objects")]
    public List<Transform> objectList = new List<Transform>();



    void Start()
    {
        // TitleScene에서는 UBS 상호작용 UI를 사용하지 않으므로 초기화하지 않음
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            init = false;
            return;
        }

        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) player = playerGo.transform;

        var msgGo = GameObject.Find("MessageText1");
        if (msgGo != null) messageText = msgGo.GetComponent<Text>();

        var crossGo = GameObject.Find("CrossHair2");
        if (crossGo != null) crossHair = crossGo.GetComponent<Image>();

        if (UBS_Global.instance != null && UBS_Global.instance.defaultObjects != null)
            highlightMaterial = UBS_Global.instance.defaultObjects.highlightMaterial;

        if (crossHair != null) crossHair.enabled = false;

        // 필수 참조가 없으면 조용히 비활성화 (NRE 방지)
        if (player == null || messageText == null || crossHair == null || highlightMaterial == null)
        {
            init = false;
            return;
        }

        init = true;

    } // End Start


    private void Update()
    {
        if (!init) return;

        HighlightObjectInCenterOfCam();
        UserInput();

    }// End Update


    /// <summary>
	/// Highlight operator object that is wiithin range
	/// </summary>
    void HighlightObjectInCenterOfCam()
    {
        Ray ray;
        RaycastHit rayHit;
        bool isHit;
        GameObject hitObject;
        bool isHittingMe;
        float dist;

        if (!init) return;

        try
        { 
            // Ray from the center of the viewport.
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            // Check if we hit something.
            isHit = Physics.Raycast(ray, out rayHit, operateRange);
            hitObject = rayHit.collider.gameObject;
            isHittingMe = isHit && (hitObject == gameObject); // The object hit is this object
            dist = Vector3.Distance(player.position, hitObject.transform.position);

            if (isHittingMe && dist <= operateRange)
            {
                HighlightObject();
                inRange = true;
            }
            else
            {
                ClearHighlighted();
                inRange = false;
            }
        }
        catch
        { }

    }// End of HighlightObjectInCenterOfCam


    /// <summary>
	/// Highlight operator object
	/// </summary>

    void HighlightObject()
    {
        if (!init) return;
        if (lastHighlightedObject != gameObject)
        {
            ClearHighlighted();
            originalMaterial = transform.GetComponent<MeshRenderer>().sharedMaterial;
            transform.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;
            lastHighlightedObject = gameObject;
            messageText.fontSize = 24;
            messageText.text = actionText;
            crossHair.enabled = true;
        }
    }// End of HighlightObject


    /// <summary>
	/// Clear highlight of operator object
	/// </summary>
    void ClearHighlighted()
    {
        if (!init) return;
        if (lastHighlightedObject != null)
        {
            lastHighlightedObject.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
            lastHighlightedObject = null;
            messageText.text = "";
            crossHair.enabled = false;
        }
    }// End of ClearHighlighted


    void UserInput()
    {
        if (!init) return;
        if (inRange)
        {
            if (Input.GetMouseButtonDown(0))
            {
                triggered = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                triggered = false;
            }
        }
        if (triggered)
        {
            triggered = false; // One shot trigger latch prevents chatter
            foreach (var subObject in objectList)
            {
                switch (operatorType)
                {
                    case EnumOperatorType.Toggle:
                        subObject.GetComponent<UBS_Actuator>().ActivateToggle(); // activate toggle for controlled objects
                        break;
                    case EnumOperatorType.Open:
                        subObject.GetComponent<UBS_Actuator>().ActivateOpen(); // activate toggle for controlled objects
                        break;
                    case EnumOperatorType.Close:
                        subObject.GetComponent<UBS_Actuator>().ActivateClose(); // activate toggle for controlled objects
                        break;
                    default:
                        ;
                        break;
                }
            }
        }
    } // End TriggerOperation

} //End of Class