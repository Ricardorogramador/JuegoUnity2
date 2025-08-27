using UnityEngine;
using TMPro;

/// <summary>
/// UIManager (actualizado): ahora contiene una referencia opcional al GoblinCountButton
/// y lo refresca cada vez que se llama UpdateUI().
/// Pega este archivo reemplazando el UIManager actual o añade la línea 'public GoblinCountButton goblinCountButton;'
/// a tu UIManager existente y la llamada a goblinCountButton.Refresh() en UpdateUI.
/// </summary>
public class UIManager : MonoBehaviour
{
    private GameManager gm;

    [Header("Referencias de UI (estas referencias siguen estando en GameManager y son las que realmente usa)")]
    public GoblinCountButton goblinCountButton; // referencia al botón contador (opcional)

    public void Setup(GameManager gameManager) { gm = gameManager; }
    public void Initialize() { /* placeholder */ }
    public void Tick() { /* placeholder */ }

    public void AttachColonyUI(TMP_Text time, TMP_Text day, TMP_Text souls, Transform panel, GameObject prefab)
    {
        gm.timeText = time;
        gm.dayText = day;
        gm.soulstext = souls;
        gm.goblinPanel = panel;
        gm.goblinPrefab = prefab;
        RebuildGoblinUI();
        UpdateUI();
    }

    public void RebuildGoblinUI()
    {
        if (gm.goblinPanel == null) return;
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in gm.goblinPanel) toDestroy.Add(child.gameObject);
        foreach (var go in toDestroy) Destroy(go);

        if (gm.goblinPrefab == null)
        {
            Debug.LogWarning("[UIManager] goblinPrefab no asignado. La lista no se podrá dibujar.");
            return;
        }
        foreach (Goblin g in gm.colony)
        {
            var item = Instantiate(gm.goblinPrefab, gm.goblinPanel);
            var ui = item.GetComponent<GoblinUi>();
            if (ui != null) ui.SetData(g);
        }
    }

    public void UpdateUI()
    {
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.timeText != null) gm.timeText.text = string.Format("{0:00}:{1:00}", gm.hour, gm.minute);
        if (gm.dayText != null) gm.dayText.text = "Day " + gm.day;
        if (gm.soulstext != null) gm.soulstext.text = "Almas: " + gm.heroSouls;

        // Actualiza el contador del botón si existe
        if (goblinCountButton != null) goblinCountButton.Refresh();
    }
}