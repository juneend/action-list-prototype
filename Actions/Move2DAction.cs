using Godot;
using System;

[GlobalClass]
public partial class Move2DAction : Action
{
    public enum movement_type
    {
        Global,
        Local
    }
    
    public const string ACTION_NAME = "Move2DAction";
    [Export] public Vector2 _startPos;
    [Export] public Vector2 _endPos;

    //FEAT: implement global vs local movement
    //[Export] public movement_type _type = movement_type.Global;

    //RFTR: get this ref & use it instead of  node2d.GlobalPosition
    //private Node2D transform;

    public override bool Ready()
    {

        //if the object has despawned
        if (IsInstanceValid(_ActionObj) == false) { return false; }

        //if the node has no 2d  transformation
        if (_ActionObj is not Node2D node2D) {return false;}

        //if start pos isn't given, calculate on first update
        //TODO: add organic movement, if it exists
        if(_startPos == Vector2.Zero)
        {
            _startPos = node2D.GlobalPosition;
            //_endPos += new Vector3(_rand, _yRand, 0);
            
        }

        _active = true;

        //RFTR: maybe the actionlist should have control over these signals, instead of the action itself? 
        EmitSignal(SignalName.ActionReadied, ACTION_NAME, this);

        return true;
    } 

     public override bool Update()
    {
        //if the object has despawned
        if (IsInstanceValid(_ActionObj) == false) { return false; }

        //if the node has no 2d  transformation
        if (_ActionObj is not Node2D node2D) {return false;}
        
        node2D.GlobalPosition = _startPos + (_endPos - _startPos) * _percent;

        EmitSignal(SignalName.ActionUpdate, ACTION_NAME, this, Variant.From(node2D.GlobalPosition));

        //if the action is done
        if (_timeElapsed >= _duration)
            return false;

        return true;
        
    }

    public override bool Remove()
    {
         //if the node has no 2d  transformation
        if (_ActionObj is Node2D node2D) 
        {EmitSignal(SignalName.ActionRemoved, ACTION_NAME, this, node2D.GlobalPosition);}
        
        //since this action is going to be removed from the list, dispose of its resource
        Dispose();

        return true;
    }

}
