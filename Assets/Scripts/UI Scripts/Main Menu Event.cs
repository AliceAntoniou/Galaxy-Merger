using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvent : MonoBehaviour
{
    private UIDocument document;
    private Button button;
    private List<Button> menuButtons = new List<Button>();
    private AudioSource audioSource;
    //for later use like clicks

    #region Awake
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        document = GetComponent<UIDocument>();

        button = document.rootVisualElement.Q("StartGame") as Button;
        button.RegisterCallback<ClickEvent>(OnPlayGameClick);

        menuButtons = document.rootVisualElement.Query<Button>().ToList();
        for (int i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }
    #endregion

    #region OnDisable
    private void OnDisable()
    {
        button.UnregisterCallback<ClickEvent>(OnPlayGameClick);

        for (int i = 0;i < menuButtons.Count;i++)
        {
            menuButtons[i].UnregisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }
    #endregion

    #region Functions
    private void OnPlayGameClick(ClickEvent evt)
    {
        Debug.Log("button pressed (Play)");
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        audioSource.Play();
    }
    #endregion
}
