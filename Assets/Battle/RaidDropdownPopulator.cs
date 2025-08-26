using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaidDropdownPopulator : MonoBehaviour
{
    [Tooltip("Componente RaidDropdown que controla abrir/cerrar y el panel.")]
    public RaidDropdown dropdown;

    [Tooltip("Botón que abre/cierra el dropdown (normalmente el mismo que está en RaidDropdown).")]
    public Button toggleButton;

    private GameManager gm;

    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        if (dropdown == null) dropdown = GetComponent<RaidDropdown>();
        if (toggleButton == null && dropdown != null) toggleButton = dropdown.toggleButton;
    }

    void OnEnable()
    {
        // Refresca al habilitar (útil al entrar en escena)
        RefreshList();
        if (toggleButton != null)
        {
            // Vuelve a refrescar cada vez que pulsas el botón (antes o después de abrir)
            toggleButton.onClick.AddListener(RefreshList);
        }
    }

    void OnDisable()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(RefreshList);
    }

    public void RefreshList()
    {
        if (dropdown == null || gm == null || gm.activeRaids == null) return;

        dropdown.ClearItems();

        foreach (var raid in gm.activeRaids)
        {
            string title = $"Raid nv {raid.nivel} ({raid.enemigos.Count} enemigos)";
            dropdown.AddItem(title, () =>
            {
                // Al hacer click en una opción:
                gm.EnterRaid(raid);
            });
        }
    }
}