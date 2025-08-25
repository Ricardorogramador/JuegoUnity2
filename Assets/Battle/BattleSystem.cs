using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleSystem : MonoBehaviour
{
    private bool battleEnd = false;

    public IEnumerator SimulateBattle(List<Goblin> goblins, List<Human> humans, GameManager gm, TMP_Text combatLog)
    {
        if (gm == null) yield break;

        // Captura el nivel de la raid al inicio para calcular bien la recompensa
        int raidLevelAtStart = (gm.raidActual != null) ? gm.raidActual.nivel : Mathf.Max(1, gm.raidLevel - 1);

        Debug.Log("La batalla ha iniciado");

        while (goblins.Count > 0 && humans.Count > 0)
        {
            if (goblins.Count > 0 && humans.Count > 0)
            {
                Goblin attackerG = goblins[Random.Range(0, goblins.Count)];
                Human targetH = humans[Random.Range(0, humans.Count)];

                int damageG = Random.Range(5, 15) + attackerG.fuerza;
                targetH.vida -= damageG;

                string msgG = $"{attackerG.nombre} ataca a {targetH.nombre} e inflige {damageG} de daño\n";
                if (combatLog != null) combatLog.text = msgG;
                Debug.Log(msgG);

                if (targetH.vida <= 0)
                {
                    string deathMsgH = $"{targetH.nombre} murió!\n";
                    if (combatLog != null) combatLog.text = deathMsgH;
                    Debug.Log(deathMsgH);
                    humans.Remove(targetH);
                }
            }
            yield return new WaitForSeconds(1f);

            if (humans.Count > 0 && goblins.Count > 0)
            {
                Human attackerH = humans[Random.Range(0, humans.Count)];
                Goblin targetG = goblins[Random.Range(0, goblins.Count)];

                int damageH = Random.Range(5, 15) + attackerH.fuerza;
                targetG.vida -= damageH;

                string msgH = $"{attackerH.nombre} ataca a {targetG.nombre} e inflige {damageH} de daño!\n";
                if (combatLog != null) combatLog.text = msgH;
                Debug.Log(msgH);

                if (targetG.vida <= 0)
                {
                    string deathMsgG = $"{targetG.nombre} murió!\n";
                    if (combatLog != null) combatLog.text = deathMsgG;
                    Debug.Log(deathMsgG);
                    goblins.Remove(targetG);
                }
            }
            yield return new WaitForSeconds(1f);
        }

        if (!battleEnd)
        {
            battleEnd = true;

            if (goblins.Count > 0)
            {
                gm.raidVictory(raidLevelAtStart);
                if (combatLog != null) combatLog.text = "Los goblins ganaron";
            }
            else
            {
                gm.raidDefeat();
                if (combatLog != null) combatLog.text = "Los humanos ganaron";
            }

            // Espera y vuelve a la colonia (solo una vez)
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene("ColonyScene");
        }
    }
}