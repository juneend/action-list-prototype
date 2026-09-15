using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Action;

public class ActionList : MonoBehaviour
{
    public List<Action> actionList = new List<Action>();

    public float speedMultiplier = 1.0f;

    public bool ispaused = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetSpeedMultiplier(float speed)
    {
        speedMultiplier = speed;
    }

    // Update is called once per frame
    void Update()
    {
        //print (actionList.Count);

        //get debug mode
        bool debug = GameManager.Instance.DEBUG_MODE;

        float dt = Time.deltaTime * speedMultiplier;

        if (ispaused )
        {
            dt *= 0;
        }

        //clear all debug text
        if (debug)
        {
            for (int i = 0; i < actionList.Count; i++)
            {
                Transform debugbg = actionList[i]?._ActionObject?.transform.Find("DebugBG");

                if (debugbg != null)
                {
                    //concat debug text
                    debugbg.GetComponentInChildren<TextMeshPro>().text = "";
                }
            }
        }

        for (int i = 0; i < actionList.Count; i++)
        {

            //debug text
            if (debug)
            {
                Transform debugbg = actionList[i]?._ActionObject?.gameObject?.transform.Find("DebugBG");

                if (debugbg != null)
                {
                    //concat debug text
                    debugbg.GetComponentInChildren<TextMeshPro>().text += actionList[i].DebugTextPrint();
                }

            }


            //if we're still in delay state
            if (actionList[i].IncrementTime(dt) == false)
                continue;

            //if there's a blocking action
            if (actionList[i]._blocking == true)
            {
                if (actionList[i].Update() == false)
                {
                    //call the callback, if it has one
                    actionList[i]._callback?.Invoke();

                    actionList.RemoveAt(i);
                    i--;
                }

                break;
            }

            //if action is done
            if (actionList[i].Update() == false)
            {
                //call the callback, if it has one
                actionList[i]._callback?.Invoke();

                actionList.RemoveAt(i);
                i--;
            }
        }

        //print(actionList.Count);

    }

    public void AddAction(Action action)
    {
        if (action != null)
        {
            actionList.Add(action);
        }
    }


}


