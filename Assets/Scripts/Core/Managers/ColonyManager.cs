using UnityEngine;

/// <summary>
/// ColonyManager: inicialización y operaciones sobre la colonia (añadir/kill/heal).
/// Usa GameManager.Instance.colony para preservar los datos serializados.
/// </summary>
public class ColonyManager : MonoBehaviour
{
    private GameManager gm;

    public void Setup(GameManager gameManager) { gm = gameManager; }
    public void Initialize()
    {
        if (gm == null) gm = GameManager.Instance;
        // Si ya hay goblins (por salvado/Inspector), no re-inicializamos por defecto.
        if (gm.colony != null && gm.colony.Count > 0) return;

        for (int i = 0; i < 3; i++)
        {
            int f = Random.Range(0, 5);
            int m = Random.Range(0, 5);
            int d = Random.Range(0, 5);
            string nombre = NameManager.Instance != null ? NameManager.Instance.GetGoblinName() : $"Goblin_{i}";
            gm.colony.Add(new Goblin(nombre, f, m, d));
        }
        gm.RebuildGoblinUI();
    }

    public void EndDay()
    {
        int requiredSouls = gm.colony.Count;
        if (gm.heroSouls >= requiredSouls)
        {
            gm.heroSouls -= requiredSouls;
        }
        else
        {
            float chance = Random.Range(0f, 1f);
            if (chance < 0.66f) ApplyRandomDebuff();
            else KillRandomGoblin();
            gm.heroSouls = 0;
        }
    }

    void ApplyRandomDebuff()
    {
        if (gm.colony.Count == 0) return;
        Goblin target = gm.colony[Random.Range(0, gm.colony.Count)];
        int stat = Random.Range(0, 3);
        switch (stat)
        {
            case 0: target.fuerza = Mathf.Max(1, (int)(target.fuerza * 0.8f)); break;
            case 1: target.magia = Mathf.Max(1, (int)(target.magia * 0.8f)); break;
            case 2: target.divino = Mathf.Max(1, (int)(target.divino * 0.8f)); break;
        }
    }

    void KillRandomGoblin()
    {
        if (gm.colony.Count == 0) return;
        int index = Random.Range(0, gm.colony.Count);
        gm.colony.RemoveAt(index);
        if (gm.goblinPanel != null && index < gm.goblinPanel.childCount)
            Destroy(gm.goblinPanel.GetChild(index).gameObject);
    }

    public void HealAllGoblins()
    {
        foreach (Goblin g in gm.colony)
        {
            g.vida = g.maxVida;
            g.mana = g.maxMana;
        }
    }
}