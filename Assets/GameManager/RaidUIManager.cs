using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaidUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject raidPanel;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject raidEntryPrefab;

    private GameManager gm;

    private void Start()
    {
        // Intenta usar el singleton primero para evitar búsquedas repetidas
        gm = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();

        if (raidPanel != null) raidPanel.SetActive(false);
        else Debug.LogWarning("[RaidUIManager] raidPanel no asignado.");

        if (gm == null)
        {
            Debug.LogWarning("[RaidUIManager] No se encontró GameManager.");
        }
    }

    // Opcional: si el panel ya está activo al habilitar el objeto, refresca la lista
    private void OnEnable()
    {
        if (raidPanel != null && raidPanel.activeSelf)
        {
            RefreshRaidList();
        }
    }

    public void ToggleRaidPanel()
    {
        if (raidPanel == null)
        {
            Debug.LogWarning("[RaidUIManager] raidPanel no asignado.");
            return;
        }

        bool show = !raidPanel.activeSelf;
        raidPanel.SetActive(show);

        if (show)
        {
            RefreshRaidList();
        }
    }

    public void RefreshRaidList()
    {
        if (gm == null)
        {
            Debug.LogWarning("[RaidUIManager] GameManager no disponible. No se puede refrescar la lista de raids.");
            return;
        }
        if (content == null)
        {
            Debug.LogWarning("[RaidUIManager] content no asignado.");
            return;
        }
        if (raidEntryPrefab == null)
        {
            Debug.LogWarning("[RaidUIManager] raidEntryPrefab no asignado.");
            return;
        }

        // Limpia entradas existentes
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        var raids = gm.activeRaids;
        if (raids == null || raids.Count == 0)
        {
            // Opcional: muestra un placeholder si no hay raids
            // Puedes comentar esto si prefieres que no se muestre nada.
            GameObject placeholder = new GameObject("NoRaidsPlaceholder", typeof(RectTransform));
            placeholder.transform.SetParent(content, false);
            var text = placeholder.AddComponent<TMP_Text>();
            text.text = "No hay raids activas";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            return;
        }

        // Crea una entrada por cada raid activa
        foreach (Raid raid in raids)
        {
            GameObject entry = Instantiate(raidEntryPrefab, content);
            // Obtiene el texto incluso si está en hijos inactivos dentro del prefab
            TMP_Text label = entry.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = $"Raid nv {raid.nivel} ({raid.enemigos.Count} enemigos)";
            }

            Button btn = entry.GetComponent<Button>();
            if (btn != null)
            {
                // Captura una variable local para evitar cualquier problema de cierre sobre 'raid'
                Raid currentRaid = raid;
                btn.onClick.AddListener(() =>
                {
                    gm.EnterRaid(currentRaid);
                });
            }
            else
            {
                Debug.LogWarning("[RaidUIManager] El prefab de entrada no tiene componente Button.");
            }
        }
    }
}