using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ActionList : Node2D
{
	/// <summary>
	//	list that contains all relevant actions, is updated every frame
	/// </summary>
	//[Export] public List<Action> actionList = new List<Action>();
	[Export] public Array<Action> actionList = new Array<Action>();

    [Export] public Node ActionObject = null;

	[Export] public float speedMultiplier = 1.0f;

    [Export] public bool ispaused = false;

    [Export] public bool debug = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        //on start, loop through all actions set in editor and set their actionobj

        foreach (Action action in actionList)
        {
             //if action has no object to act on
            if(action._ActionObj == null && ActionObject != null)
            {
                action._ActionObj = ActionObject;
            }
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = (float)delta * speedMultiplier;

        if (ispaused ){dt *= 0;}

        if (debug){GD.Print("Actions left: ", actionList.Count);}

		for (int i = 0; i < actionList.Count; i++)
		{
			//if we're still in delay state, don't update this action but continue through the list
            bool delay = actionList[i].IncrementTime(dt);
			if (delay == false) {continue;}

            //if this action is blocking, update it and then stop looping
            if (actionList[i]._blocking == true)
            {
                if (actionList[i].Update() == false)
                {
                    //call the callback, if it has one
                    //actionList[i]._callback?.Invoke();

                    actionList.RemoveAt(i);
                    i--;
                }

                break;
            }

            //update action
            if (actionList[i].Update() == false)
            {
                //call the callback, if it has one
                //actionList[i]._callback?.Invoke();

                actionList.RemoveAt(i);
                i--;
            }
		}
	}

	public void AddAction(Action action)
    {
        if (action != null)
        {
            //if action has no object to act on
            if(action._ActionObj == null && ActionObject != null)
            {
                action._ActionObj = ActionObject;
            }

            actionList.Add(action);
        }
    }
}
