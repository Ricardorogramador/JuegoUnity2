using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CaptureManager: lógica de captura de prisioneras tras una victoria en raid.
/// Usa los parámetros públicos en GameManager.Instance.
/// </summary>
public class CaptureManager : MonoBehaviour
{
    private GameManager gm;

    public void Setup(GameManager gameManager) { gm = gameManager; }
    public void Initialize() { /* placeholder */ }

    public Human TryCaptureFromRaid(Raid raid)
    {
        if (gm == null || raid == null || raid.enemigos == null || raid.enemigos.Count == 0)
            return null;

        List<Human> candidatas = new List<Human>();
        foreach (var h in raid.enemigos)
        {
            if (h.sexo == HumanSex.Femenino) candidatas.Add(h);
        }
        if (candidatas.Count == 0) return null;

        float chance = Mathf.Min(gm.maxCaptureChance, gm.baseCaptureChance + gm.bonusPerFemale * candidatas.Count);
        float roll = Random.value;

        if (roll <= chance)
        {
            Human capturada = candidatas[Random.Range(0, candidatas.Count)];
            AddPrisionera(capturada);
            raid.enemigos.Remove(capturada);
            raid.activa = false;
            Debug.Log($"[Captura] Capturada: {capturada.nombre}. Roll {roll:F2} <= chance {chance:P0}");
            return capturada;
        }
        else
        {
            Debug.Log($"[Captura] Falló captura. Roll {roll:F2} > chance {chance:P0}");
            return null;
        }
    }

    public void AddPrisionera(Human h)
    {
        if (gm == null || h == null) return;
        if (gm.prisioneras == null) gm.prisioneras = new List<Human>();
        gm.prisioneras.Add(h);
    }
}