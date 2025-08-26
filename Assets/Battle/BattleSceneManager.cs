using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BattleSceneManager : MonoBehaviour
{
    // Estas referencias pueden quedar aunque no se usen para spawns; el spawner usa sus propias zonas
    public Transform goblinSide;
    public Transform humansSide;
    public TMP_Text combatLog;

    private GameManager gm;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null || gm.raidActual == null)
        {
            Debug.Log("No hay GameManager o raid activa");
            return;
        }

        if (combatLog != null)
            combatLog.text = $"Comienza la batalla: {gm.colony.Count} goblins vs {gm.raidActual.enemigos.Count} humanos";

        var bs = FindObjectOfType<BattleSystem>();
        if (bs != null)
        {
            StartCoroutine(bs.SimulateBattle(
                new List<Goblin>(gm.colony),
                new List<Human>(gm.raidActual.enemigos),
                gm, combatLog));
        }
        else
        {
            Debug.LogWarning("BattleSystem no encontrado en la escena.");
        }
    }
}