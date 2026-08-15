using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Action
{
    public enum EasingTypes
    {
        None = -1,
        EaseIn,
        EaseOut,
        EaseInOut
    };

    //given, the object this action acts upon
    public GameObject _ActionObject;
    //given, how long this action should last
    public float _duration;
    //given, how much time should pass before the action starts
    public float _delay;
    //given, should this action stop all other actions from running?
    public bool _blocking;
    //given, if not 0 then we should add some organic randomness
    public float _rand = 0;
    //given, the type of easing
    public EasingTypes _myEasing = EasingTypes.None;

    public System.Action _callback = null;

    //calculated, how much time has passed since the action was created
    public float _timepassed;
    //calculated, a number from 0-1 that represents how far this action has to go
    public float _percent;
    //if this action should update its start position
    public bool isFirstUpdate = true;

    //if update returns false, the action is finished and should be removed from the list
    public abstract bool Update();

    public abstract string DebugTextPrint();

    public void SetCallback(System.Action callback)
    {
        _callback = callback;
    }

    //returns false if the action is still in a delay state
    public bool IncrementTime(float dt)
    {
        //in delay state
        if (_delay > 0)
        {
            _delay -= dt;
            return false;
        }

        _timepassed += dt;

        _percent = _timepassed / _duration;

        switch (_myEasing)
        {
            case EasingTypes.None:
                //linear
                break;

            case EasingTypes.EaseIn:
                //square root
                _percent = Mathf.Sqrt(_percent);
                break;

            case EasingTypes.EaseOut:
                //square
                _percent = _percent * _percent;
                break;

            case EasingTypes.EaseInOut:
                //idk
                if (_percent < 0.5f)
                    _percent = (2 * _percent * _percent) * 0.5f;
                else
                    _percent = Mathf.Sqrt((_percent - 0.5f) * 2) * 0.5f + 0.5f;

                break;
            default:
                break;
        }



        Mathf.Clamp(_percent, 0, 1);

        //Debug.Log(_percent);

        return true;

    }

    public float TimeLeft() { return _duration - _timepassed; }

}



public class MoveAction : Action
{
    public Vector3 _startPos;
    public Vector3 _endPos;

    public float _yRand = 0;

    public MoveAction(GameObject gameObject, float duration, float delay, bool blocking, Vector2 startPos, Vector2 endPos)
    {
        _ActionObject = gameObject;
        _startPos = new Vector3 (startPos.x, startPos.y, gameObject.transform.position.z);
        _endPos = new Vector3(endPos.x, endPos.y, gameObject.transform.position.z);
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
    }

    public MoveAction(GameObject gameObject, float duration, float delay, bool blocking, Vector2 endPos)
    {
        _ActionObject = gameObject;
        _startPos = gameObject.transform.position;
        _endPos = new Vector3(endPos.x, endPos.y, gameObject.transform.position.z);
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
    }

    public MoveAction(GameObject gameObject, float duration, float delay, bool blocking, Vector2 endPos, float rand)
    {
        _ActionObject = gameObject;
        _startPos = gameObject.transform.position;
        _endPos = new Vector3(endPos.x, endPos.y, gameObject.transform.position.z);
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _rand = UnityEngine.Random.Range(-rand, rand);
        _yRand = UnityEngine.Random.Range(-rand, rand);



    }

    public MoveAction(GameObject gameObject, float duration, float delay, bool blocking, EasingTypes myEasing, Vector2 endPos)
    {
        _ActionObject = gameObject;
        _startPos = gameObject.transform.position;
        _endPos = new Vector3(endPos.x, endPos.y, gameObject.transform.position.z);
        _duration = duration;
        _delay = delay;
        _blocking = blocking;

        _myEasing = myEasing;
    }

    public MoveAction(GameObject gameObject, float duration, float delay, bool blocking, EasingTypes myEasing, Vector2 endPos, float rand)
    {
        _ActionObject = gameObject;
        _startPos = gameObject.transform.position;
        _endPos = new Vector3(endPos.x, endPos.y, gameObject.transform.position.z);
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _rand = UnityEngine.Random.Range(-rand, rand);
        _yRand = UnityEngine.Random.Range(-rand, rand);

        _myEasing = myEasing;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //calculate start pos on first update
        //add organic movement, if it exists
        if(isFirstUpdate)
        {
            _startPos = _ActionObject.transform.position;
            _endPos += new Vector3(_rand, _yRand, 0);
            isFirstUpdate = false;
        }

        //if the action is done
        if (_percent >= 1) { _percent = 1; }

        Vector3 newPosition = _startPos + (_endPos - _startPos) * _percent;

        _ActionObject.transform.position = new Vector3(newPosition.x, newPosition.y, _ActionObject.transform.position.z);

        //if the action is done
        if (_percent >= 1)
            return false;

        return true;
        
    }

    public override string DebugTextPrint()
    {
        return "MOVE TO: " + _endPos + " (" + _percent * 100 + "%)\n" ;
    }

}

public class RotateZAction : Action
{
    public float _startRot;
    public float _endRot;

    //public RotateZAction(GameObject gameObject, float duration, float delay, bool blocking, float startRot, float endRot)
    //{
    //	_ActionObject = gameObject;
    //	_startRot = startRot;
    //	_endRot = endRot;
    //	_duration = duration;
    //	_delay = delay;
    //	_blocking = blocking;
    //}

    public RotateZAction(GameObject gameObject, float duration, float delay, bool blocking, float endRot)
    {
        _ActionObject = gameObject;
        _startRot = gameObject.transform.rotation.z;
        _endRot = endRot;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
    }

    public RotateZAction(GameObject gameObject, float duration, float delay, bool blocking, float endRot, float rand)
    {
        _ActionObject = gameObject;
        _startRot = gameObject.transform.rotation.z;
        _endRot = endRot;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _rand = _rand = UnityEngine.Random.Range(-rand, rand);
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //calculate start rot on first update
        if (isFirstUpdate)
        {
            _startRot = _ActionObject.transform.rotation.z;
            _endRot += _rand;
            isFirstUpdate = false;
        }

        if (_percent >= 1) { _percent = 1; }

        float newZ = _startRot + (_endRot - _startRot) * _percent;
        _ActionObject.transform.rotation = Quaternion.Euler(new Vector3(_ActionObject.transform.rotation.eulerAngles.x
                                                                    , _ActionObject.transform.rotation.eulerAngles.y
                                                                    , newZ));
        //if the action is done
        if (_percent >= 1)
            return false;

        return true;

    }

    public override string DebugTextPrint()
    {
        return "SORT ORDER: " + _endRot + " (" + _percent * 100 + "%)\n";
    }

}

public class ZPosAction : Action
{
    float _startZ;
    float _endZ;

    public ZPosAction(GameObject gameObject, float duration, float delay, bool blocking, float startZ, float endZ)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _startZ = startZ;
        _endZ = endZ;
    }

    public ZPosAction(GameObject gameObject, float duration, float delay, bool blocking, float endZ)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _startZ = gameObject.transform.position.z;
        _endZ = endZ;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //calculate start pos on first update
        if (isFirstUpdate)
        {
            _startZ = _ActionObject.transform.position.z;
            isFirstUpdate = false;
        }

        //if we go beyond one, set percent back to one
        if (_percent >= 1)
        {
            _percent = 1;
        }

        _ActionObject.transform.position = new Vector3(_ActionObject.transform.position.x, _ActionObject.transform.position.y, (_startZ + (_endZ - _startZ) * _percent));

        //if the action is done
        if (_percent >= 1)
            return false;

        return true;
    }

    public override string DebugTextPrint()
    {
        return "Z POS: " + _endZ + " (" + _percent * 100 + "%)\n";
    }
}

public class ReorderAction : Action
{
    int _startZ;
    int _endZ;

    public ReorderAction(GameObject gameObject, float duration, float delay, bool blocking, int startZ, int endZ) 
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _startZ = startZ;
        _endZ = endZ;
    }

    public ReorderAction(GameObject gameObject, float duration, float delay, bool blocking, int endZ)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _startZ = gameObject.GetComponent<SpriteRenderer>().sortingOrder;
        _endZ = endZ;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //calculate start pos on first update
        if (isFirstUpdate)
        {
            _startZ = _ActionObject.GetComponent<SpriteRenderer>().sortingOrder;
            isFirstUpdate = false;
        }

        //if we go beyond one, set percent back to one
        if (_percent >= 1)
        {
            _percent = 1;
        }

        //_ActionObject.transform.position = new Vector3(_ActionObject.transform.position.x, _ActionObject.transform.position.y, (_startZ + (_endZ - _startZ) * _percent));
        _ActionObject.GetComponent<SpriteRenderer> ().sortingOrder = (int)(_startZ + (_endZ - _startZ) * _percent);

        //if the action is done
        if (_percent >= 1)
            return false;

        return true;
    }

    public override string DebugTextPrint()
    {
        return "Z ORDER: " + _endZ + " (" + _percent * 100 + "%)\n";
    }
}

public class FadeAction : Action
{
    float _startAlpha;
    float _endAlpha;

    SpriteRenderer[] _sprites;
    TextMeshPro[] _textmeshes;
    TextMeshProUGUI[] _textUImeshes;

    public FadeAction(GameObject gameObject, float duration, float delay, bool blocking, float startAlpha, float endAlpha)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;

        _startAlpha = startAlpha;
        _endAlpha = endAlpha;
    }

    public FadeAction(GameObject gameObject, float duration, float delay, bool blocking, float endAlpha)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;

        _startAlpha = 0.0f;
        _endAlpha = endAlpha;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //calculate initial alpha, list of sprite renderers & text
        if (isFirstUpdate)
        {
            _sprites = _ActionObject.GetComponentsInChildren<SpriteRenderer>();
            _textmeshes = _ActionObject.GetComponentsInChildren<TextMeshPro>();
            _textUImeshes = _ActionObject.GetComponentsInChildren<TextMeshProUGUI>();

            //if the object doesn't have any sprites, the initial alpha is text
            if (_sprites.Length != 0) 
                 _startAlpha = _sprites[0].color.a; 
            else if (_textmeshes.Length != 0)
                _startAlpha = _textmeshes[0].color.a;
            else
                 _startAlpha = _textUImeshes[0].color.a;

            isFirstUpdate = false;
        }

        //if we go beyond one, set percent back to one
        if (_percent >= 1)
        {
            _percent = 1;
        }

        float newalpha = (_startAlpha + (_endAlpha - _startAlpha) * _percent);

        //set all sprite opacity
        foreach (SpriteRenderer sprite in _sprites) 
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newalpha);
        }

        //set all text opacity
        foreach (TextMeshPro text in _textmeshes)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, newalpha);
        }

        //set all ui opacity
        foreach (TextMeshProUGUI text in _textUImeshes)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, newalpha);
        }

        //if the action is done
        if (_percent >= 1)
            return false;

        return true;

    }

    public override string DebugTextPrint()
    {
        return "FADE " + _endAlpha + " (" + _percent * 100 + "%)\n";
    }

}

public class SetTurn : Action
{
    Seat _PreviousPlayer;
    public SetTurn(GameObject gameObject, float duration, float delay, bool blocking, Seat previousPlayer)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        _PreviousPlayer = previousPlayer;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        Seat SeatComp = _ActionObject.GetComponent<Seat>();

        //if the object is not a seat
        if (SeatComp == null) { return false; }

        //activate this player
        SeatComp.MyTurn = true;
        Transform activebg = SeatComp.gameObject.transform.Find("ActiveBG");
        if (activebg != null) { activebg.gameObject.SetActive(true); }

        if (SeatComp.myName == Seat.PlayerNames.You)
        {
            GameManager.Instance.PlayerTurn();
            //GameManager.Instance.SetButtonsActive(true);
        }
        else
        {
            GameManager.Instance.NonPlayerTurn(SeatComp.myName);
        }


        //deactivate previous player
        _PreviousPlayer.MyTurn = false;
        activebg = _PreviousPlayer.gameObject.transform.Find("ActiveBG");
        if (activebg != null) { activebg.gameObject.SetActive(false); }

        //deactivating player seat
        if (_PreviousPlayer.myName == Seat.PlayerNames.You)
        {
            GameManager.Instance.SetButtonsActive(false);
        }

        return false;


    }

    public override string DebugTextPrint()
    {
        return "MY TURN, PREVIOUS PLAYER:" + _PreviousPlayer.myName;
    }


}

public class CardFlip : Action
{
    bool _faceup;
    bool reverse = false;

    float startScale = 1, endScale = 0;
    public CardFlip(GameObject gameObject, float duration, float delay, bool blocking, bool faceup) 
    {
        _ActionObject = gameObject;
        _duration = duration / 2;
        _delay = delay;
        _blocking = blocking;
        _faceup = faceup;
    }

    public CardFlip(GameObject gameObject, float duration, float delay, bool blocking)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
        
        Card cardComp = _ActionObject.GetComponent<Card>();

        if (cardComp  != null)
        {
            _faceup = !cardComp._faceUp;
        }
        else
            Assert.IsNotNull(cardComp, "card not found!");


    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        if (isFirstUpdate)
        {
            //if the card is already face up/ face down, stop the action
            Card cardComp = _ActionObject.GetComponent<Card>();

            if (cardComp._faceUp == _faceup)
                _ActionObject = null;

            isFirstUpdate = false;
        }

        //if the object has despawned
        if (_ActionObject == null) { return false; }

        //if we go beyond two, set percent back to two
        if (_percent >= 2)
        {
            _percent = 2;
        }

        if (reverse == false)
        {
            _ActionObject.transform.localScale = new Vector3(startScale + (endScale - startScale) * _percent,
                                                            _ActionObject.transform.localScale.y,
                                                            _ActionObject.transform.localScale.z);
        }
        else
        {
            _ActionObject.transform.localScale = new Vector3(startScale + (endScale - startScale) * (_percent - 1),
                                                _ActionObject.transform.localScale.y,
                                                _ActionObject.transform.localScale.z);
        }


        //if the action is halfway over, flip the card
        if (_percent >= 1 && reverse == false)
        {
            startScale = 0;
            endScale = 1;

            _ActionObject.GetComponent<Card>().SetCardSideUp(_faceup);

            reverse = true;
        }

        //if the action is done
        if (_percent >= 2)
            return false;


        return true;

    }

    public override string DebugTextPrint()
    {
        return "FLIP TO: " + (_faceup ? "face up" : "face down");
    }

}

public class SetText : Action
{
    string _newText;
    public SetText(GameObject gameObject, float duration, float delay, bool blocking, string newText)
    {
        _ActionObject = gameObject;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;

        _newText = newText;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        TextMeshPro textcomp = _ActionObject.GetComponent<TextMeshPro>();

        textcomp.text = _newText;

        return false;


    }

    public override string DebugTextPrint()
    {
        return "SET TEXT TO: " + _newText;
    }


}

public class ScaleAction : Action
{
    float _startScale, _endScale;
    Vector3 originalScaleVect;

    public ScaleAction(GameObject gameObject, float duration, float delay, bool blocking, float startScale, float endScale)
    {
        _ActionObject = gameObject;
        _startScale = startScale;
        _endScale = endScale;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
    }

    public ScaleAction(GameObject gameObject, float duration, float delay, bool blocking, float endScale)
    {
        _ActionObject = gameObject;
        _startScale = 1;
        _endScale = endScale;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;
    }

    public ScaleAction(GameObject gameObject, float duration, float delay, bool blocking, EasingTypes easing ,float endScale)
    {
        _ActionObject = gameObject;
        _startScale = 1;
        _endScale = endScale;
        _duration = duration;
        _delay = delay;
        _blocking = blocking;

        _myEasing = easing;
    }

    public override bool Update()
    {
        //if the object has despawned
        if (_ActionObject == null) { return false; }

        if (isFirstUpdate)
        {
            originalScaleVect = _ActionObject.transform.localScale;
            isFirstUpdate = false;
        }

        //if the action is done
        if (_percent >= 1) { _percent = 1; }

        float scaleMultip = _startScale + (_endScale - _startScale) * _percent;

        _ActionObject.transform.localScale = originalScaleVect * scaleMultip;

        //if the action is done
        if (_percent >= 1)
            return false;

        return true;

    }

    public override string DebugTextPrint()
    {
        return "MULTIPLY SCALE BY: " + _startScale + (_endScale - _startScale) * _percent;
    }

}