using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RaidDropdownItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button button;

    public void Init(string text, UnityAction onClick)
    {
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (button == null) button = GetComponent<Button>();

        label.text = text;
        button.onClick.RemoveAllListeners();
        if (onClick != null) button.onClick.AddListener(onClick);
    }
}