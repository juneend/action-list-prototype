using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //A random number generator for this run
    public System.Random RNG;

    public StreamWriter DataStream;

    public GameObject cardPrefab;
    public GameObject playerPrefab;
    public GameObject chipPrefab;

    [HideInInspector]
    private List<GameObject> CardObjs;

    [HideInInspector]
    private List<GameObject> ChipObjs;

    [HideInInspector]
    public List<GameObject> Players;
    public Transform[] SeatPositions = new Transform[6];

    [HideInInspector]
    public ActionList actionList;

    //for giving the instantiated prefabs a parent
    public Transform CanvasTransform;

    public Deck deck, pot, discard;

    public Canvas canvas;

    public bool DEBUG_MODE = false;

    public int RoundNum = 0;
    public int HandNum = 1;

    public bool gameOver = false;

    public bool isPaused = false;

    public bool AutoMode = false;


    public enum CardVal
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    };

    string[] CardLetters = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" }; //15

    private void Awake()
    {
        Instance = this;

        Players = new List<GameObject>();
        CardObjs = new List<GameObject>();
        ChipObjs = new List<GameObject>();

        actionList = gameObject.GetComponent<ActionList>();
    }


    void Start()
    {
        RNG = new System.Random();

        DataStream = new StreamWriter("CardGameTelemetryData.csv", false);

        //write the column headers
        DataStream.WriteLine("HAND#,WINNER,You,Random_Ronald,Always_Bets_Andy,Always_Folds_Felicia,Smart_Sally,Smart_Sammy");

        //create players
        for (int i = 0; i < 6; i++)
        {
            GameObject newPlayer = Instantiate(playerPrefab, SeatPositions[i].position, Quaternion.identity);

            Players.Add(newPlayer);
            newPlayer.GetComponent<Seat>().InitSeat(i);

        }

        //instantiate prefabs
        CreateCards();
        CreateChips();


        //debug text
        if (DEBUG_MODE)
        {
            for (int i = 0; i < CardObjs.Count; i++)
            {
                Transform debugtrans = CardObjs[i].transform.Find("DebugBG");

                if (debugtrans != null)
                {
                    debugtrans.gameObject.SetActive(DEBUG_MODE);
                }

            }
        }

        //shuffle the deck
        deck.Shuffle();

        //set turn to player0 (you)
        actionList.AddAction(new SetTurn(Players[0], 0.1f, 0.0f, false, Players[5].GetComponent<Seat>()));

        //set the bet and fold buttons
        Button[] buttons = canvas.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(Players[0].GetComponent<Seat>().Bet);
        buttons[1].onClick.AddListener(Players[0].GetComponent<Seat>().Fold);

        //turn off the buttons
        SetButtonsActive(false);

    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyUp(KeyCode.P))
        //{
        //	DealCards();
        //}

        if (Input.GetKeyUp(KeyCode.A))
        {
            if (AutoMode)
            {
                actionList.SetSpeedMultiplier(1.0f);
                UIManager.Instance.actionList.SetSpeedMultiplier(1.0f);
                AutoMode = false;
            }
            else
            {
                actionList.SetSpeedMultiplier(5.0f);
                UIManager.Instance.actionList.SetSpeedMultiplier(5.0f);
                AutoMode = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        { 
            if (isPaused)
            {
                //if we're unpausing

                UIManager.Instance.Unpause();

            }
            else
            {
                //if we're pausing

                UIManager.Instance.Pause();

            }
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            DEBUG_MODE = !DEBUG_MODE;

            for (int i = 0; i < CardObjs.Count; i++)
            {
                Transform debugtrans = CardObjs[i].transform.Find("DebugBG");

                if (debugtrans != null)
                {
                    debugtrans.gameObject.SetActive(DEBUG_MODE);
                }

            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            for (int i = 1; i < Players.Count; i++)
            {
                AddPlayer(i);
            }


            //keep 0 and 1
            RemovePlayer(5);
            RemovePlayer(4);
            RemovePlayer(3);
            RemovePlayer(2);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            for (int i = 1; i < Players.Count; i++)
            {
                AddPlayer(i);
            }

            //keep 0, 1, 2
            RemovePlayer(5);
            RemovePlayer(4);
            RemovePlayer(3);

        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            for (int i = 1; i < Players.Count; i++)
            {
                AddPlayer(i);
            }

            //keep 0,1,2,3
            RemovePlayer(5);
            RemovePlayer(4);
        }

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            for (int i = 1; i < Players.Count; i++)
            {
                AddPlayer(i);
            }

            //keep 0,1,2,3,4
            RemovePlayer(5);
        }

        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            for (int i = 1; i < Players.Count; i++)
            {
                AddPlayer(i);
            }

        }


    }

    private void CreateCards()
    {
        bool cardColor = false;
        uint count = 0;


        //for each of 4 suits
        for (int i = 0; i < 4; ++i)
        {

            //for each of 13 cards
            for (int ii = 0; ii < 13; ++ii)
            {
                GameObject newcard = Instantiate(cardPrefab, new Vector3(deck.gameObject.transform.position.x, deck.gameObject.transform.position.y, count), Quaternion.identity);

                deck.deckStack.Add(newcard);

                newcard.GetComponent<Card>().InitCard(CardLetters[ii], cardColor, count, ii);
                CardObjs.Add(newcard);

                //actionList.AddAction(new CardFlip(newcard, 0.5f, 0.0f, false, false));

                count++;
            }

            if (i == 1)
            {
                cardColor = true;
            }


        }
    }

    private void DealCards()
    {
        List<int> drawnCards = new List<int>();

        //start with 5 cards
        for (int i = 0; i < 5; i++)
        {
            //for each player
            for (int ii = 0; ii < 6; ii++)
            {
                int random = RNG.Next(52);

                //if drawncards contains the random card, keep drawing
                while (drawnCards.Contains(random))
                {
                    random = RNG.Next(52);
                }
                drawnCards.Add(random);

                GameObject randomcard = CardObjs[random];

                Seat currentSeat = Players[ii].GetComponent<Seat>();

                //move to the player's card slot
                actionList.AddAction(new MoveAction(randomcard, 1.3f, 0.25f * ii + (0.25f * i), false, currentSeat.cardSlot));

                currentSeat.GiveCard(randomcard);

            }

        }
    }

    private GameObject DealCard(GameObject player)
    {
        Seat playerSeat = player.GetComponent<Seat>();

        //if the player is out, don't give a card
        if (playerSeat.IsFolded == true)
        {
            return null;
        }

        //take a card off the top of the deck
        GameObject card = deck.Take();

        //shuffle discard pile, move all to deck
        if (card == null)
        {
            discard.Shuffle();
            int n = discard.deckStack.Count;

            for (int i = 0; i < n; i++)
            {
                card = discard.Take();


                deck.deckStack.Add(card);
                //change card's z position
                actionList.AddAction(new ZPosAction(card, 0.5f, 0.0f, false, -(deck.deckStack.Count - 1)));
                //offset position
                Vector2 newPos = new Vector2(deck.gameObject.transform.position.x + (i * 0.01f), deck.gameObject.transform.position.y + (i * 0.01f));
                actionList.AddAction(new MoveAction(card, 0.5f, 0.0f, false, newPos));
            }

            card = deck.Take();
        }

        //move to the player's card slot, with some organics
        actionList.AddAction(new MoveAction(card, 0.3f, 0.0f, false, Action.EasingTypes.EaseOut, playerSeat.cardSlot, 0.05f));
        actionList.AddAction(new RotateZAction(card, 0.3f, 0.0f, true, 0, 10.0f));

        //give the player the card
        playerSeat.GiveCard(card);

        return card;
    }

    private void CreateChips()
    {
        int count = 0;

        //for each of 6 players
        for (int i = 0; i < 6; ++i)
        {

            //each player gets 10 tokens
            for (int ii = 0; ii < 10; ++ii)
            {
                GameObject newChip = Instantiate(chipPrefab, new Vector3(pot.gameObject.transform.position.x + (i* 0.05f),
                    pot.gameObject.transform.position.y + (ii * 0.05f), count), Quaternion.identity);

                Players[i].GetComponent<Seat>().GiveChip(newChip);

                count++;
            }
        }
    }

    public GameObject GetPlayerByIndex(int index)
    {
        return Players[index];
    }

    public Seat GetSeatByIndex(int index)
    {
        return Players[index].GetComponent<Seat>();
    }

    public void PlayerTurn()
    {
        GameObject roundtext = canvas.transform.Find("Round Text").gameObject;
        RectTransform rect = roundtext.GetComponent<RectTransform>();

        RoundNum++;

        if (RoundNum == 6)
        {
            RoundOver();
            return;
        }

        SetButtonsActive(true);

        GameObject card = DealCard(Players[0]);

        //flip the card to face up
        actionList.AddAction(new CardFlip(card, 0.3f, 0.0f, false, true));

        //move round text above screen
        actionList.AddAction(new MoveAction(roundtext, 0.5f, 0.0f, true,Action.EasingTypes.EaseOut,
            new Vector2(rect.position.x, rect.position.y + 2)));

        //set text
        actionList.AddAction(new SetText(roundtext, 0.5f, 0.0f, false, "Round: " + RoundNum));

        //move text back down
        actionList.AddAction(new MoveAction(roundtext, 0.5f, 0.0f, true, Action.EasingTypes.EaseIn,
            new Vector2(rect.position.x, rect.position.y)));
    }

    public void NonPlayerTurn(Seat.PlayerNames name)
    {
        DealCard(Players[(int)name]);
    }

    public void RoundOver()
    {
        gameOver = true;

        //if auto mode is on, open the menu and choose an option
        if (AutoMode)
        {
            UIManager.Instance.Pause();
            //return;
        }

        //remove the buttons
        SetButtonsActive(false);

        List<int> playerscores = new List<int>();
        for (int i = 0; i < Players.Count; ++i)
            playerscores.Add(0);


        //flip all the cards, calculate best hand
        for (int i = 0; i < Players.Count; i++)
        {
            Seat seat = Players[i].GetComponent<Seat>();

            foreach(GameObject card in seat.hand)
            {
                actionList.AddAction(new CardFlip(card, 0.1f, 0.0f, true, true));
                playerscores[i] += card.GetComponent<Card>()._worth;

            }

        }

        int highestScore = playerscores.IndexOf(playerscores.Max());

        GameObject wintext = canvas.transform.Find("Win Text").gameObject;

        //set text, fade in
        if (highestScore == 0)
            actionList.AddAction(new SetText(wintext, 0.5f, 0.0f, false, Enum.GetName(typeof(Seat.PlayerNames), highestScore) + " win!"));
        else
            actionList.AddAction(new SetText(wintext, 0.5f, 0.0f, false, Enum.GetName(typeof(Seat.PlayerNames), highestScore) + " wins!"));

        actionList.AddAction(new FadeAction(wintext, 0.3f, 0.0f, false, 1.0f));

        //write telemetry data for this hand
        //"HAND#,WINNER,You,Random_Ronald,Always_Bets_Andy,Always_Folds_Felicia,Smart_Sally,Smart_Sammy"
        string handData = HandNum.ToString() + "," + Enum.GetName(typeof(Seat.PlayerNames), highestScore) + ",";


        //turn all cards face down, write telemetry data
        for (int i = 0; i < Players.Count; i++)
        {
            Seat seat = Players[i].GetComponent<Seat>();

            if (seat.IsOut) //Out
                handData += "O";
            else if (seat.IsFolded) //Folded
                handData += "F";
            else //In
                handData += "I";

            handData += "(" + seat.chips.Count.ToString() + "),";


            foreach (GameObject card in seat.hand)
            {
                actionList.AddAction(new CardFlip(card, 0.1f, 0.0f, true, false));
                discard.Add(card, 0.0f);

            }

            //clear each player's hand
            seat.hand.Clear();
        }

        DataStream.WriteLine(handData);

        Seat winningPlayer = Players[highestScore].GetComponent<Seat>();

        int n = pot.deckStack.Count;
        //all chips in the pot go to the winner
        for (int i = 0; i < n; i++)
        {
            GameObject chip = pot.Take();
            winningPlayer.GiveChip(chip);
        }

        //fade win text out
        FadeAction fadeout = new FadeAction(wintext, 0.3f, 0.0f, false, 0.0f);

        actionList.AddAction(fadeout);
        fadeout.SetCallback(NewRound);

        //after this last action is finished, it should call new round

    }

    public void RemovePlayer(int index)
    {
        if (index >= Players.Count) { return; }

        Players[index].GetComponent<Seat>().Remove();
    }

    public void AddPlayer(int index)
    {
        if (index >= Players.Count) { return; }

        Players[index].GetComponent<Seat>().Add();
    }

    public void SetButtonsActive(bool active)
    {
        Button[] buttons = canvas.GetComponentsInChildren<Button>();

        buttons[0].interactable = active;
        buttons[1].interactable = active;

    }

    public void NewRound()
    {
        HandNum++;

        //all players who are still in should un-fold
        foreach (GameObject seat in Players)
        {

            Seat currentSeat = seat.GetComponent<Seat>();
            if (currentSeat.IsOut == false)
            {
                actionList.AddAction(new FadeAction(seat, 0.5f, 0.0f, true, 1.0f));
                currentSeat.IsFolded = false;
                currentSeat.ResetCardSlot();
            }

        }

        //if all but one players are out, the last player wins the game
        Seat[] activePlayers = GetNonFoldedPlayers();
        if (activePlayers.Length == 1)
        {
            GameObject wintext = canvas.transform.Find("Win Text").gameObject;

            actionList.AddAction(new SetText(wintext, 0.5f, 0.0f, false, "Last player standing: " + activePlayers[0].myName.ToString()));

            actionList.AddAction(new FadeAction(wintext, 0.3f, 0.0f, true, 1.0f));
            FadeAction fadeout = new FadeAction(wintext, 0.3f, 0.0f, false, 0.0f);

            //after text fades out, end the game
            actionList.AddAction(fadeout);
            fadeout.SetCallback(UIManager.Instance.QuitGame);

            return;
        }
        else if (activePlayers.Length == 0)
        {
            //if everyone's out, the player wins
            GameObject wintext = canvas.transform.Find("Win Text").gameObject;

            actionList.AddAction(new SetText(wintext, 0.5f, 0.0f, false, "Last player standing: " + Players[0].GetComponent<Seat>().myName.ToString()));

            actionList.AddAction(new FadeAction(wintext, 0.3f, 0.0f, true, 1.0f));
            FadeAction fadeout = new FadeAction(wintext, 0.3f, 0.0f, false, 0.0f);

            //after text fades out, end the game
            actionList.AddAction(fadeout);
            fadeout.SetCallback(UIManager.Instance.QuitGame);

            return;
        }

        //round = 0
        RoundNum = 0;

        GameObject roundtext = canvas.transform.Find("Round Text").gameObject;
        RectTransform rect = roundtext.GetComponent<RectTransform>();

        //move round text above screen
        actionList.AddAction(new MoveAction(roundtext, 0.5f, 0.0f, true, Action.EasingTypes.EaseOut,
            new Vector2(rect.position.x, rect.position.y + 2)));

        //set text
        actionList.AddAction(new SetText(roundtext, 0.5f, 0.0f, false, "Round: " + RoundNum));

        //move text back down
        actionList.AddAction(new MoveAction(roundtext, 0.5f, 0.0f, true, Action.EasingTypes.EaseIn,
            new Vector2(rect.position.x, rect.position.y)));

        //add player's turn
        //set turn to player0 (you)
        actionList.AddAction(new SetTurn(Players[0], 0.1f, 0.0f, false, Players[5].GetComponent<Seat>()));

        gameOver = false;

    }

    public Seat[] GetNonFoldedPlayers()
    {
        List<Seat> ActivePlayers = new List<Seat>();

        foreach(GameObject player in Players)
        {
            Seat PlayerSeat = player.GetComponent<Seat>();


            if (PlayerSeat.IsFolded == false) 
            {
                ActivePlayers.Add(PlayerSeat);
            }
        }

        return ActivePlayers.ToArray();
    }

}
