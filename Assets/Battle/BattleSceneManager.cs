using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BattleSceneManager : MonoBehaviour
{
    public Transform goblinSide;
    public Transform humansSide;
    public TMP_Text combatLog;

    private GameManager gm;
    private bool _started; // Guard para evitar arranque múltiple

    private void Start()
    {
        if (_started) return;
        _started = true;

        gm = FindObjectOfType<GameManager>();
        if (gm == null || gm.raidActual == null)
        {
            Debug.Log("No hay GameManager o raid activa");
            return;
        }

        foreach (var goblin in gm.colony)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.SetParent(goblinSide);
            g.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0, 0);
            g.name = goblin.nombre;
        }

        foreach (var human in gm.raidActual.enemigos)
        {
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            h.transform.SetParent(humansSide);
            h.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0, 0);
            h.name = human.nombre;
        }

        combatLog.text = $"Comienza la batalla: {gm.colony.Count} goblins vs {gm.raidActual.enemigos.Count} humanos";

        // Una sola búsqueda y una sola simulación
        var battleSystem = FindObjectOfType<BattleSystem>();
        if (battleSystem == null)
        {
            Debug.LogWarning("No se encontró BattleSystem en la escena.");
            return;
        }

        // Mantiene la lógica actual (usar copias) pero evita la doble llamada
        StartCoroutine(battleSystem.SimulateBattle(
            new List<Goblin>(gm.colony),
            new List<Human>(gm.raidActual.enemigos),
            gm,
            combatLog
        ));
    }
}