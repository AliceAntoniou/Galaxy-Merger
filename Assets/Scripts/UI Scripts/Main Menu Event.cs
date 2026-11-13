using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvent : MonoBehaviour
{
    #region Variables
    private UIDocument document;
    private VisualElement buttonWrapper;
    private VisualElement settingsButtons;
    private Button playButton;
    private Button settingsButton;
    private Button exitButton;
    private List<Button> menuButtons = new List<Button>();
    private AudioSource audioSource;

    [SerializeField] private VisualTreeAsset settingsButtonTemplate;
    //for later use like clicks
    #endregion

    #region Awake
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        document = GetComponent<UIDocument>();

        buttonWrapper = document.rootVisualElement.Q<VisualElement>("Buttons");
        settingsButton = document.rootVisualElement.Q<Button>("SettingsButton");
        playButton = document.rootVisualElement.Q<Button>("PlayButton");
        exitButton = document.rootVisualElement.Q<Button>("ExitButton");

        settingsButton.clicked += SettingsButtonOnClicked;
        playButton.RegisterCallback<ClickEvent>(OnPlayGameClick);

        settingsButtons = settingsButtonTemplate.CloneTree();

        var BackButton = settingsButtons.Q<Button>("BackButton");
        BackButton.clicked += BackButtonOnClicked;

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

    private void SettingsButtonOnClicked()
    {
        buttonWrapper.Clear();
        buttonWrapper.Add(settingsButtons);
    }

    private void BackButtonOnClicked()
    {
        buttonWrapper.Clear();
        buttonWrapper.Add(playButton);
        buttonWrapper.Add(settingsButton);
        buttonWrapper.Add(exitButton);
        
    }
    #endregion
}
