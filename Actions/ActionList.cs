using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

public partial class ActionList : Node2D
{
	/// <summary>
	//	list that contains all relevant actions, is updated every frame
	/// </summary>
	//TODO: on high level memory management, if i ever want to make this cache friendly
	//this array of resources should be contiguous when loaded
	//maybe godot does this automatically? idk, should research l8r
	[Export] public Array<Action> actionList = new Array<Action>();

	[Export] public Node ActionObject = null;

	[Export] public float speedMultiplier = 1.0f;

	[Export] public bool ispaused = false;

	[Export] public bool debug = true;

	private VBoxContainer DBGContainerRef;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//inst the degug container
		if (debug)
		{
			DBGContainerRef = GD.Load<PackedScene>(
				"res://Scenes/action_container_dbg.tscn").Instantiate<VBoxContainer>();
			AddChild(DBGContainerRef);
			//TODO: probably more code to change like, the container position and stuff
		}

		//on start, loop through all actions set in editor and set their actionobj

		foreach (Action action in actionList)
		{
			 //if action has no object to act on
			if(action._ActionObj == null && ActionObject != null)
			{

				//TODO: this is kinda dumb, the actionlist probably shouldn't have a specified actionobj
				//ideally actions should affect either the node they're given, or this node's parent in an ECS-ey way
				action._ActionObj = ActionObject;

				//connect signals to the dbg container
				if (debug) 
				{
					action.ActionReadied += DBG_AddEntry;
					action.ActionUpdate += DBG_UpdateEntry;
					action.ActionRemoved += DBG_RemoveEntry;				
				}
			}
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = (float)delta * speedMultiplier;

		if (ispaused ){dt *= 0;}

		//if (debug){GD.Print("Actions left: ", actionList.Count);}

		for (int i = 0; i < actionList.Count; i++)
		{
			//if we're still in delay state, don't update this action but continue through the list
			bool delay = actionList[i].IncrementTime(dt);
			if (delay == false) {continue;}

			//if action is not ready, then call the ready function
			if (actionList[i]._active == false &&
				actionList[i].Ready() == false)
			{
				//if ready returns false (i.e. the obj does not exist or isn't correct type)
				//then remove it from list
				RemoveAction(ref i);
			}

			//if this action is blocking, update it and then stop looping
			if (actionList[i]._blocking == true)
			{

				if (actionList[i].Update() == false)
				{
					//call the callback, if it has one
					//actionList[i]._callback?.Invoke();

					RemoveAction(ref i);
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
	public void RemoveAction(ref int index)
	{
		//TODONE: emit a signal when an action completes
		//derived class handles ^ now!
		actionList[index].Remove();
		actionList.RemoveAt(index);
		index--;

	}

	public void DBG_AddEntry(string name, Action action)
	{
		if (action is Move2DAction moveAction)
		{
			GD.Print("readied action with name: ", action.GetType().Name);
		}

	}

	public void DBG_UpdateEntry(string name, Action action, Variant changedVal)
	{
		//TODO: cool feature: actions that are readied and updating should be auto unfolded

		if (action is Move2DAction moveAction)
		{
			GD.Print("updated action with name: ", action.GetType().Name, " to value ", changedVal);
		}
	}

	public void DBG_RemoveEntry(string name, Action action, Variant finalVal)
	{
		if (action is Move2DAction moveAction)
		{
			GD.Print("removed action with name: ", action.GetType().Name, " and final value ", finalVal);
		}
	}
}
