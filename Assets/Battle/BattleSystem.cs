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
        Debug.Log("La batalla ha iniciado");

        while(goblins.Count > 0 && humans.Count > 0)
        {
            if(goblins.Count > 0)
            {
                Goblin attacker = goblins[UnityEngine.Random.Range(0, goblins.Count)];
                Human target = humans[UnityEngine.Random.Range(0, humans.Count)];

                int damage = UnityEngine.Random.Range(5, 15) + attacker.fuerza;
                target.vida -= damage;

                string msg = $"{attacker.nombre} ataca a {target.nombre} e inflige {damage} de daño\n";
                combatLog.text = msg;
                Debug.Log(msg);

                Debug.Log($"{attacker.nombre} ataca a {target.nombre} e inflige {damage} de daño!");

                if (target.vida <= 0)
                {
                    string deathMsg = $"{target.nombre} murió!\n";
                    combatLog.text = deathMsg;
                    Debug.Log(deathMsg);
                    humans.Remove(target);
                }
            }
            yield return new WaitForSeconds(1f);

            if (humans.Count > 0)
            {
                Human attacker = humans[UnityEngine.Random.Range(0, humans.Count)];
                Goblin target = goblins[UnityEngine.Random.Range(0, goblins.Count)];

                int damage = UnityEngine.Random.Range(5, 15) + attacker.fuerza;
                target.vida -= damage;

                string msg = $"{attacker.nombre} ataca a {target.nombre} e inflige {damage} de daño!\n";
                combatLog.text = msg;
                Debug.Log(msg);


                Debug.Log($"{attacker.nombre} ataca a {target.nombre} e inflige {damage} de daño!");

                if (target.vida <= 0)
                {
                    string deathMsg = $"{target.nombre} murió!\n";
                    combatLog.text = deathMsg;
                    Debug.Log(deathMsg);
                    goblins.Remove(target);
                }
            }
            yield return new WaitForSeconds(1f);
        }
        if(!battleEnd) {
            battleEnd = true;

<<<<<<< Updated upstream
        if (goblins.Count > 0)
        {
            gm.raidVictory();
            combatLog.text = "Los goblins ganaron";
=======
            if (goblins.Count > 0)
            {
                gm.raidVictory(raidLevelAtStart);
                gm.HealAllGoblins();

                // Intento de captura (solo al ganar)
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

            // Espera y vuelve a la colonia (solo una vez)
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene("ColonyScene");
>>>>>>> Stashed changes
        }

        else
        {
            gm.raidDefeat();
            combatLog.text = "Los humanos ganaron";
        }
        }

        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene("ColonyScene");
    }
}
