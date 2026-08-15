using Godot;
using System;
using System.Runtime.InteropServices.Marshalling;

[GlobalClass]
public abstract partial class Action : Resource
{
    //the object this action acts upon
    //once the action is added to a list, if this param is null
    //than the action will effect the parent of the actionlist node
    public Node _ActionObj = null;

    //how long this action should last
    [Export] public float _duration = 0;

    //how much time should pass before the action starts
    [Export] public float _delay = 0;

    //TODO: integrate blocking groups with bitmaps
    //does this action stop all other actions in the list?
    [Export] public bool _blocking = false;

    //should there be some randomness in the ending state?
    [Export] public float _rand = 0;

    //TODO: seperate this into easein/easeout probably
    //type of easing in/out curve
    [Export] public Curve _easing;

    //deltatime elapsed since action began
    public float _timeElapsed;

    //TODO: how to do actions with easing that go past 1?
    //zero to one percentage representing how far the action is to completion
    public float _percent;

    /// <summary>
    /// this function updates the action parameter based on _percent
    /// </summary>
    /// <returns></returns>
    public abstract bool Update();

    /// <summary>
    /// this function is called everytime the action list updates
    /// it increments _timeElapsed & _percent according to easingtypes
    /// </summary>
    /// <param name="dt">delta time</param>
    /// <returns>false = action is still being delayed
    /// true = action has changed percentage
    /// </returns>
    public bool IncrementTime(float dt)
    {
        //if in delay state, increment that instead
        if (_delay > 0)
        {
            _delay -= dt;
            return false;
        }

        _timeElapsed += dt;

        //evaluate the point on the easing curve that matches the percent this action is to completion
        _percent = _easing.SampleBaked(_timeElapsed / _duration);

        return true;

    }

    public float TimeLeft() {return _duration - _timeElapsed;}

    
}
