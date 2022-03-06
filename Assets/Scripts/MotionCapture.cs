using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Motions { Deselect, None, Tap, Up, Down, Left, Right, Swipe }
public class MotionCapture : MonoBehaviour
{
    [SerializeField]
    private float minSwipeLength = 200f;

    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    private Motions currentMotion = Motions.None;

    public Motions CurrentMotion { get { Motions tmp = currentMotion; currentMotion = Motions.None; return tmp; } }

    public Vector3 CurrentClick { get { return firstPressPos; } }
    public Vector3 CurrentEndClick { get { return secondPressPos; } }

    // Start is called before the first frame update


    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    void Update()
    {

        catchMotions();
    }
    private void catchMotions() 
    {

        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);

            if (checkForGUI(t.fingerId))
                return;


            if (t.phase == TouchPhase.Began)
            {
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }

            if (t.phase == TouchPhase.Ended)
            {
                secondPressPos = new Vector2(t.position.x, t.position.y);
                currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

                // Make sure it was a legit swipe, not a tap
                if (currentSwipe.magnitude < minSwipeLength)
                {
                    currentMotion = Motions.Tap;
                    return;
                }

                currentSwipe.Normalize();

                currentMotion = Motions.Swipe;

                /*

                // Swipe up
                if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
                    currentMotion = Motions.Up;
                } else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
                    // Swipe down
                    currentMotion = Motions.Down;
                } else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
                    // Swipe left
                    currentMotion = Motions.Left;
                } else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
                    // Swipe right
                    currentMotion = Motions.Right;
                }
                */
            }
        }
        else
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_WP_8_1
            currentMotion = Motions.None;
#endif
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                if (checkForGUI())
                    return;
                firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (checkForGUI())
                    return;

                secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

                if (currentSwipe.magnitude < minSwipeLength)
                {
                    // Click
                    currentMotion = Motions.Tap;
                    return;
                }
                currentSwipe.Normalize();

                currentMotion = Motions.Swipe;
                /*
                if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    // Swipe up
                    currentMotion = Motions.Up;
                }else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    // Swipe down
                    currentMotion = Motions.Down;
                }else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    // Swipe left
                    currentMotion = Motions.Left;
                }else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    // Swipe right
                    currentMotion = Motions.Right;
                }
                */
            }
#endif
        }
        
    }
    private bool checkForGUI(int clickID = -1) 
    {
        bool isOnGui = true;
        bool isOverEventSystem = false;

        if (clickID >= 0)
            isOverEventSystem = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(clickID);
        else
            isOverEventSystem = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (isOverEventSystem)
        {
            //We are on GUI
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
            {
                // GUI Element Interactable
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.CompareTag("TileUI"))
                {
                    //Its a game UI so Deny it
                    isOnGui = false;
                }
            }
            else
            {
                //Not interactable GUI Element 
            }
        }
        else
        {
            //Not over GUI
            isOnGui = false;
        }
        if(isOnGui)
        {
            firstPressPos = Vector2.zero;
            secondPressPos = Vector2.zero;
            currentMotion = Motions.Deselect;
        }

        return isOnGui;
    }
    public void resetMotion() 
    {
        currentMotion = Motions.None; 
    }

}
