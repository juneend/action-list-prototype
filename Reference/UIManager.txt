using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public ActionList actionList;

    public GameObject activeSubmenu = null;

    public GameObject MainMenu;

    public GameObject VolumeMenu, GraphicsMenu, DifficultyMenu;

    public Button ResumeButton;

    private System.Random RNG;

    public List<string> MenuButtonsChosen = new List<string>();
    public List<string> SubmenuButtonsChosen = new List<string>();

    private void Awake()
    {
        Instance = this;
        RNG = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pause()
    {
        print("pause");
        GameManager.Instance.actionList.ispaused = true;
        GameManager.Instance.isPaused = true;

        MoveAction moveDownMainMenu = new MoveAction(MainMenu, 0.5f, 0.0f, true, Action.EasingTypes.EaseOut,
            new Vector2(0, 0));

        actionList.AddAction(moveDownMainMenu);

        //if auto mode is on, then set a callback after the menu has moved to choose a random option
        if (GameManager.Instance.AutoMode)
        {
            moveDownMainMenu.SetCallback(ChooseRandomSubmenu);
        }


        ResumeButton.Select();
    }

    public void Unpause()
    {
        print("unpause");
        GameManager.Instance.actionList.ispaused = false;
        GameManager.Instance.isPaused = false;

        MoveAction moveUpMainMenu = new MoveAction(MainMenu, 0.5f, 0.0f, true,
            new Vector2(0, 10));

        if (activeSubmenu != null) { DeactivateSubmenu(activeSubmenu); }

        actionList.AddAction(moveUpMainMenu);

        EventSystem.current.SetSelectedGameObject(null);



    }

    public void SlideLeft()
    {
        actionList.AddAction(new MoveAction(MainMenu, 0.3f, 0.0f, false, Action.EasingTypes.EaseOut,
            new Vector2(-5, 0)));
    }

    public void SlideRight()
    {
        actionList.AddAction(new MoveAction(MainMenu, 0.3f, 0.0f, false, Action.EasingTypes.EaseOut,
            new Vector2(0, 0)));
    }

    public void ActivateSubmenu(GameObject submenu)
    {
        activeSubmenu = submenu;

        SlideLeft();
        actionList.AddAction(new MoveAction(submenu, 0.3f, 0.0f, true,
            new Vector2(5, 0)));

        Button[] submenuButtons = submenu.GetComponentsInChildren<Button>();

        //deselect all buttons
        EventSystem.current.SetSelectedGameObject(null);

        //select the first submenu button
        submenuButtons[0].Select();

    }

    public void DeactivateSubmenu(GameObject submenu)
    {
        activeSubmenu = null;

        actionList.AddAction(new MoveAction(submenu, 0.3f, 0.0f, true,
            new Vector2(15, 0)));

        EventSystem.current.SetSelectedGameObject(null);
        ResumeButton.Select();

        SlideRight();
    }

    public void ShowFeedbackText(GameObject self)
    {
        Transform t = self.transform.Find("Feedback Text");

        if (t == null)
        {
            print("COULD NOT FIND FEEDBACK TEXT");
            return;
        }

        actionList.AddAction(new ScaleAction(self, 0.3f, 0.0f, false, Action.EasingTypes.EaseOut , 1.1f));
        actionList.AddAction(new ScaleAction(self, 0.3f, 0.3f, false, Action.EasingTypes.EaseIn , (1/1.1f)));

        actionList.AddAction(new RotateZAction(t.gameObject, 0.2f, 0.3f, false, 0.0f, 10.0f));
        actionList.AddAction(new FadeAction(t.gameObject, 0.7f, 0.0f, true, 1.0f));

        FadeAction fadeTextOut = new FadeAction(t.gameObject, 0.7f, 0.0f, true, 0.0f);
        actionList.AddAction(fadeTextOut);

        //if we're in auto mode, after the feedback text is done showing, unpause and resume play
        if (GameManager.Instance.AutoMode)
        {
            Unpause();
        }

    }


    public void QuitGame()
    {
        print("QUIT");

        //clear line between card data and menu data
        GameManager.Instance.DataStream.WriteLine("");
        //write headers
        GameManager.Instance.DataStream.WriteLine("Submenu chosen, Option Chosen");

        for(int i = 0; i < MenuButtonsChosen.Count; i++)
        {
            GameManager.Instance.DataStream.WriteLine(MenuButtonsChosen[i] + "," + SubmenuButtonsChosen[i]);
        }

        GameManager.Instance.DataStream.Close();
        Application.Quit();
    }

    public void ChooseRandomSubmenu()
    {
        //pick an index 1, 2, or 3 for each of the buttons
        int randIndex = RNG.Next(1, 4);

        Transform submenuButton = MainMenu.transform.GetChild(randIndex);

        MenuButtonsChosen.Add(submenuButton.name);

        //click the submenu button
        submenuButton.gameObject.GetComponent<Button>().onClick.Invoke();

        GameObject submenu = null;

        switch (randIndex)
        {
            case 1:
                //volume menu
                submenu = VolumeMenu;
            break;
            case 2:
                submenu = GraphicsMenu;
            break;
            case 3:
                submenu = DifficultyMenu;
            break;
        }

        if (submenu == null ) { print("AUTO MODE MENU SELECTION FAILED"); }
        else
        {
            //pick a random button on the submenu
            randIndex = RNG.Next(1, 4);

            submenuButton = submenu.transform.GetChild(randIndex);

            SubmenuButtonsChosen.Add(submenuButton.name);
            
            //click the submenu button
            submenuButton.gameObject.GetComponent<Button>().onClick.Invoke();
        }

    }


}
