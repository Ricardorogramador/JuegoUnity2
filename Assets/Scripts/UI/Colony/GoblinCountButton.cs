using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra la cantidad de goblins en un pequeño botón/icono y permite togglear el panel de colonia.
/// Llama a Refresh() cuando quieras actualizar el contador (UIManager.UpdateUI lo hará).
/// </summary>
public class GoblinCountButton : MonoBehaviour
{
    [Header("UI")]
    public Image icon;               // imagen del botón (opcional)
    public TMP_Text countText;       // texto que muestra la cantidad
    public Button button;            // referencia al botón (puede ser el mismo GameObject)
    [Tooltip("Opcional: panel de colonia (GameObject) que se abrirá/cerrará al pulsar.")]
    public GameObject colonyPanel;   // si lo asignas, se alternará éste. Si no, usa GameManager.goblinPanel.

    void Reset()
    {
        // intentar autoconfigurar referencias básicas
        button = GetComponent<Button>();
        if (button == null)
        {
            var b = gameObject.AddComponent<Button>();
            button = b;
        }
    }

    void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(TogglePanel);
        Refresh();
    }

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(TogglePanel);
    }

    /// <summary>
    /// Actualiza el texto con la cantidad actual de goblins.
    /// Llamar desde UIManager.UpdateUI() o después de añadir/quitar goblins.
    /// </summary>
    public void Refresh()
    {
        int count = 0;
        if (GameManager.Instance != null && GameManager.Instance.colony != null)
            count = GameManager.Instance.colony.Count;

        if (countText != null) countText.text = count.ToString();
    }

    /// <summary>
    /// Al pulsar: alterna el panel de colonia. Primero usa colonyPanel si está asignado,
    /// sino intenta usar GameManager.goblinPanel (Transform -> GameObject).
    /// </summary>
    public void TogglePanel()
    {
        if (colonyPanel != null)
        {
            colonyPanel.SetActive(!colonyPanel.activeSelf);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.goblinPanel != null)
        {
            var go = GameManager.Instance.goblinPanel.gameObject;
            go.SetActive(!go.activeSelf);
        }
    }
}