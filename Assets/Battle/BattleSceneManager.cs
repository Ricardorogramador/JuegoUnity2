using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BattleSceneManager : MonoBehaviour
{
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
        foreach( var goblin in gm.colony)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.SetParent(goblinSide);
            g.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0, 0);
            g.name = goblin.nombre;
        }
        foreach(var human in gm.raidActual.enemigos)
        {
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            h.transform.SetParent(humansSide);
            h.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0, 0);
            h.name = human.nombre;
        }

        combatLog.text = $"Comienza la batalla: {gm.colony.Count} goblins vs {gm.raidActual.enemigos.Count} humanos";
        BattleSystem bs = FindObjectOfType<BattleSystem>();
        StartCoroutine(bs.SimulateBattle(new List<Goblin>(gm.colony), new List<Human>(gm.raidActual.enemigos), gm, combatLog));

        var battleSystem = FindObjectOfType<BattleSystem>();
        if (battleSystem != null)
        {
            StartCoroutine(battleSystem.SimulateBattle(new List<Goblin>(gm.colony), new List<Human>(gm.raidActual.enemigos), gm, combatLog));
        }

    }
}
