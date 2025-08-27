using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// RaidManager: gestiona activeRaids, raidActual, generación y entrada a batalla.
/// Opera sobre los campos públicos en GameManager.Instance.
/// </summary>
public class RaidManager : MonoBehaviour
{
    private GameManager gm;
    [HideInInspector] public float raidTimer = 0f;

    public void Setup(GameManager gameManager) { gm = gameManager; }

    public void Initialize() { /* placeholder */ }

    public void GenerateRaid()
    {
        if (gm == null) return;
        Raid nuevaRaid = new Raid(gm.raidLevel);
        gm.activeRaids.Add(nuevaRaid);
        gm.raidLevel++;
        gm.paused = true; // pausa el juego
        Debug.Log("Nueva raid generada. El juego se pausa.");
    }

    public void EnterRaid(Raid raid)
    {
        if (gm == null || raid == null) return;
        gm.raidActual = raid;
        gm.activeRaids.Remove(raid);
        SceneManager.LoadScene("BattleScene");
    }

    public void RaidVictory(int completedRaidLevel)
    {
        if (gm == null) return;
        int rewardSouls = completedRaidLevel * 2;
        gm.heroSouls += rewardSouls;
        gm.uiManager.UpdateUI();
    }

    public void RaidDefeat()
    {
        if (gm == null) return;
        if (gm.colony.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, gm.colony.Count);
            gm.colony.RemoveAt(idx);
        }
        gm.uiManager.UpdateUI();
    }
}