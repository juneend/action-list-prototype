using Godot;
using System;

[GlobalClass]
public partial class Move2DAction : Action
{
    [Export] public Vector2 _startPos;
    [Export] public Vector2 _endPos;

    private Node2D transform;

     public override bool Update()
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

        //if (_percent >= 1) { _percent = 1; }

        node2D.GlobalPosition = _startPos + (_endPos - _startPos) * _percent;

        //if the action is done
        if (_timeElapsed >= _duration)
            return false;

        return true;
        
    }
}
