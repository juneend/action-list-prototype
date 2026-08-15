using Godot;
using System;

public partial class GameManager : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* //unpack the actionlist scene and inst it
		ActionList list = GD.Load<PackedScene>("res://Actions/action_list.tscn").Instantiate<ActionList>(); 
		AddChild(list);

		Move2DAction action = GD.Load<Move2DAction>("res://Objects/move right.tres");

		action._ActionObj = GetNode("Icon");
		list.AddAction(action);

		list.AddAction(new Move2DAction
		{
			_ActionObj = null,
			_duration = 3,
			_delay = 0,
			_blocking = false,
			_rand = 0,
			_easing =
		}); */
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
