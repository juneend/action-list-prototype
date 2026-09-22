using Godot;
using System;
using System.Numerics;

public partial class GameManager : Node2D
{
	ActionList list;
	Move2DAction moveup, movedown, moveleft, moveright;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//unpack the actionlist scene and inst it
		list = GD.Load<PackedScene>("res://Scenes/action_list.tscn").Instantiate<ActionList>(); 
		AddChild(list);
		//list.AddAction(action);

		/*moveup = GD.Load<Move2DAction>("res://Objects/move up.tres");
		moveup._ActionObj = GetNode("Icon");

		movedown = GD.Load<Move2DAction>("res://Objects/move down.tres");
		movedown._ActionObj = GetNode("Icon");

		moveright = GD.Load<Move2DAction>("res://Objects/move right.tres");
		moveright._ActionObj = GetNode("Icon");

		moveleft = GD.Load<Move2DAction>("res://Objects/move left.tres");
		moveleft._ActionObj = GetNode("Icon");*/



		/*list.AddAction(new Move2DAction
		{
			_ActionObj = null,
			_duration = 3,
			_delay = 0,
			_blocking = false,
			_rand = 0,
			_easing =
		});

		Godot.Vector2 testVec = new Godot.Vector2(0f, 100f);

		GD.Print(Variant.From(testVec));*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey input && input.Pressed)
		{
			if(input.Keycode == Key.W)
			{
				list.AddAction("res://Objects/move up.tres", GetNode("Icon"));
			}

			if(input.Keycode == Key.S)
			{
				list.AddAction("res://Objects/move down.tres", GetNode("Icon"));
			}

			if(input.Keycode == Key.D)
			{
				list.AddAction("res://Objects/move right.tres", GetNode("Icon"));
			}

			if(input.Keycode == Key.A)
			{
				list.AddAction("res://Objects/move left.tres", GetNode("Icon"));
			}
		}
    }

}
