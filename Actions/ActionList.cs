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
	//OPTM: on high level memory management, if i ever want to make this cache friendly
	//this array of resources should be contiguous when loaded
	//maybe godot does this automatically? idk, should research l8r
	[Export] public Array<Action> actionList = new Array<Action>();

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
			if(action._ActionObj == null )
			{
				//actions affect either the node they're given, or the actionlist's parent in an ECS-ey way
				action._ActionObj = GetParent<Node>();

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

	public bool AddAction(Action action)
	{
		if (action != null)
		{
			//TODO: make sure all actions that come through here should be instances of what's on disk
			//Action uniqueAction = (Action)action.Duplicate();

			//if action has no object to act on
			if(action._ActionObj == null)
			{
				action._ActionObj = GetParent<Node>();
			}

			actionList.Add(action);

			//connect signals to the dbg container
			if (debug) 
			{
				action.ActionReadied += DBG_AddEntry;
				action.ActionUpdate += DBG_UpdateEntry;
				action.ActionRemoved += DBG_RemoveEntry;				
			}
			return true;
		}

		return false;
	}

	public bool AddAction(string path, Node obj)
	{
		//try loading action resource
		Action action = GD.Load<Action>(path);

		if (action != null)
		{
			//set the new action's onject if specified
			if (obj != null) {action._ActionObj = obj;}

			return AddAction(action);

		}
		return false;
	}


	public void RemoveAction(ref int index)
	{
		//TODONE: emit a signal when an action completes
		//derived class handles ^ now!
		actionList[index].Remove();
		actionList.RemoveAt(index);
		index--;

	}

	//TODO: maybe delete name param? i don't need it anymore really
	public void DBG_AddEntry(string name, Action action)
	{
		//FIXME: actions present in the actionlist on runtime should autocreate entries before they are readied 
		if (action is Move2DAction moveAction)
		{
			GD.Print("readied action with name: ", action.GetType().Name);
		}

		//for this action, create a matching dbg entry
		FoldableContainer DBG_Entry = GD.Load<PackedScene>(
				"res://Scenes/action_entry_dbg.tscn").Instantiate<FoldableContainer>();

		UpdateEntryLabels(ref DBG_Entry, action, Variant.From(0));

		//add entry as child of the container
		DBGContainerRef.AddChild(DBG_Entry);


	}

	public void DBG_UpdateEntry(string name, Action action, Variant changedVal)
	{
		//FEAT: cool feature: actions that are readied and updating should be auto unfolded

		if (action is Move2DAction moveAction)
		{
			GD.Print("updated action with name: ", action.GetType().Name, " to value ", changedVal);
		}

		//OPTM: this operation might be kinda expensive with a big list
		//GOOD THING IT'S A DEBUG FUNC LOOOOOLL
		int index = System.Array.IndexOf<Action>(actionList.ToArray(), action);

		//find the entry with this index inside the DBG container
		//+1 for the hbox
		FoldableContainer entry = DBGContainerRef.GetChild<FoldableContainer>(index + 1);
		if (entry == null)
			{GD.PrintErr("could not find child at index ", index, " for action ", action.GetType().Name);}

		UpdateEntryLabels(ref entry, action, changedVal);

	}

	public void DBG_RemoveEntry(string name, Action action, Variant finalVal)
	{
		if (action is Move2DAction moveAction)
		{
			GD.Print("removed action with name: ", action.GetType().Name, " and final value ", finalVal);
		}
	}

	public void UpdateEntryLabels(ref FoldableContainer entryRef, Action action, Variant changedVal)
	{
		//concat title together eg. Move2DAction | (0.352/2.000) Sec
		entryRef.Title = action.GetType().Name + " | (" + action.TimeLeft() + "/" + action._duration + ") Sec";

		//add ActionObj, Value, Delay, Blocking
		//HACK: this is kinda rigid there's probably a better way here
		entryRef.GetChild(0).GetChild<Label>(0).Text = "ActionObj = " + action._ActionObj.Name;
		entryRef.GetChild(0).GetChild<Label>(1).Text = "Value = " + changedVal;
		entryRef.GetChild(0).GetChild<Label>(2).Text = "Delay = " + action._delay;
		entryRef.GetChild(0).GetChild<Label>(3).Text = "Blocking = " + action._blocking;
	}
}


