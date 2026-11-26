using System;
using Multi_1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal build menu: pick what to build, then click a tile (Tile.OnClicked reads BuildMenuUI.Selected).
/// Celestial Objects go on a free tile, planets on a Star System tile. Clicking the same button again deselects.
/// </summary>
public class BuildMenuUI : MonoBehaviour
{
    [Serializable]
    public class Option
    {
        public Button button;
        public EntityType type;
    }

    [SerializeField] private Option[] options;
    [SerializeField] private TMP_Text selectedText;

    public static EntityType Selected { get; private set; } = EntityType.None;

    private void Start()
    {
        Select(EntityType.None);
        foreach (var option in options)
        {
            var type = option.type;
            option.button.onClick.AddListener(() => Select(Selected == type ? EntityType.None : type));
        }
    }

    private void OnDestroy()
    {
        Selected = EntityType.None;
    }

    private void Select(EntityType type)
    {
        Selected = type;
        if (selectedText != null)
            selectedText.text = type == EntityType.None ? "Build: nothing selected" : "Build: " + type;
    }
}
