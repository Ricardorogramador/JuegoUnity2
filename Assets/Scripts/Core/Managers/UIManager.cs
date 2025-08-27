using UnityEngine;
using TMPro;

/// <summary>
/// UIManager: actualiza los textos y reconstruye el panel de goblins,
/// usando las referencias públicas que están en GameManager (para no romper el Inspector).
/// </summary>
public class UIManager : MonoBehaviour
{
    private GameManager gm;
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
        if (gm == null) return;
        if (gm.timeText != null) gm.timeText.text = string.Format("{0:00}:{1:00}", gm.hour, gm.minute);
        if (gm.dayText != null) gm.dayText.text = "Day " + gm.day;
        if (gm.soulstext != null) gm.soulstext.text = "Almas: " + gm.heroSouls;
    }
}