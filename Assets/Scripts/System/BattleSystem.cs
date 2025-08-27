using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleSystem : MonoBehaviour
{
    private bool battleEnd = false;

    [Header("Vista/Spawns")]
    public BattleSpawner spawner;

    [Header("Reglas de la batalla")]
    [Tooltip("Máximo de unidades por lado que entran al campo.")]
    public int maxUnitsPerSide = 5;

    [Tooltip("Si hay más de 'maxUnitsPerSide', elegir aleatoriamente a los que entran.")]
    public bool randomizeSelection = true;

    private Dictionary<Goblin, UnitToken> goblinTokens = new Dictionary<Goblin, UnitToken>();
    private Dictionary<Human, UnitToken> humanTokens  = new Dictionary<Human, UnitToken>();

    public IEnumerator SimulateBattle(List<Goblin> goblins, List<Human> humans, GameManager gm, TMP_Text combatLog)
    {
        if (gm == null) yield break;

        var goblinsBattle = SelectUpTo(goblins, maxUnitsPerSide, randomizeSelection);
        var humansBattle  = SelectUpTo(humans,  maxUnitsPerSide, randomizeSelection);

        Debug.Log($"[Battle] Participantes: Goblins {goblinsBattle.Count}/{goblins?.Count ?? 0}, Humanos {humansBattle.Count}/{humans?.Count ?? 0} (máximo {maxUnitsPerSide} por lado)");

        if (spawner != null)
        {
            goblinTokens = spawner.SpawnGoblins(goblinsBattle);
            humanTokens  = spawner.SpawnHumans(humansBattle);
        }

        int raidLevelAtStart = (gm.raidActual != null) ? gm.raidActual.nivel : Mathf.Max(1, gm.raidLevel - 1);

        Debug.Log("La batalla ha iniciado");

        while (goblinsBattle.Count > 0 && humansBattle.Count > 0)
        {
            if (goblinsBattle.Count > 0 && humansBattle.Count > 0)
            {
                Goblin attackerG = goblinsBattle[Random.Range(0, goblinsBattle.Count)];
                Human targetH = humansBattle[Random.Range(0, humansBattle.Count)];

                int damageG = Random.Range(5, 15) + attackerG.fuerza;
                targetH.vida -= damageG;

                string msgG = $"{attackerG.nombre} ataca a {targetH.nombre} e inflige {damageG} de daño. vida restante:  {targetH.vida}\n";
                if (combatLog != null) combatLog.text = msgG;
                Debug.Log(msgG);

                if (targetH.vida <= 0)
                {
                    string deathMsgH = $"{targetH.nombre} murió!\n";
                    if (combatLog != null) combatLog.text = deathMsgH;
                    Debug.Log(deathMsgH);

                    if (humanTokens.TryGetValue(targetH, out var tokenH) && tokenH != null)
                        Destroy(tokenH.gameObject);
                    humanTokens.Remove(targetH);

                    humansBattle.Remove(targetH);
                }
            }
            yield return new WaitForSeconds(1f);

            if (humansBattle.Count > 0 && goblinsBattle.Count > 0)
            {
                Human attackerH = humansBattle[Random.Range(0, humansBattle.Count)];
                Goblin targetG = goblinsBattle[Random.Range(0, goblinsBattle.Count)];

                int damageH = Random.Range(5, 15) + attackerH.fuerza;
                targetG.vida -= damageH;

                string msgH = $"{attackerH.nombre} ataca a {targetG.nombre} e inflige {damageH} de daño!. vida restante: {targetG.vida}\n";
                if (combatLog != null) combatLog.text = msgH;
                Debug.Log(msgH);

                if (targetG.vida <= 0)
                {
                    string deathMsgG = $"{targetG.nombre} murió!\n";
                    if (combatLog != null) combatLog.text = deathMsgG;
                    Debug.Log(deathMsgG);

                    if (goblinTokens.TryGetValue(targetG, out var tokenG) && tokenG != null)
                        Destroy(tokenG.gameObject);
                    goblinTokens.Remove(targetG);

                    goblinsBattle.Remove(targetG);
                }
            }
            yield return new WaitForSeconds(1f);
        }

        if (!battleEnd)
        {
            battleEnd = true;

            if (goblinsBattle.Count > 0)
            {
                gm.raidVictory(raidLevelAtStart);
                gm.HealAllGoblins();
                Human capturada = gm.TryCaptureFromRaid(gm.raidActual);
                if (combatLog != null)
                {
                    if (capturada != null)
                        combatLog.text = "Los goblins ganaron. Has capturado a " + capturada.nombre + ".";
                    else
                        combatLog.text = "Los goblins ganaron.";
                }
            }
            else
            {
                gm.raidDefeat();
                if (combatLog != null) combatLog.text = "Los humanos ganaron";
            }

            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene("ColonyScene");
        }
    }

    private List<T> SelectUpTo<T>(List<T> source, int max, bool randomize)
    {
        var result = new List<T>();
        if (source == null || source.Count == 0 || max <= 0) return result;
        if (source.Count <= max) { result.AddRange(source); return result; }

        if (!randomize)
        {
            for (int i = 0; i < max; i++) result.Add(source[i]);
            return result;
        }

        int n = source.Count;
        var indices = new List<int>(n);
        for (int i = 0; i < n; i++) indices.Add(i);

        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (int i = 0; i < max; i++) result.Add(source[indices[i]]);
        return result;
    }
}