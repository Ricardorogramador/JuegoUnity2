using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TimeManager: se encarga del timer, incremento de minutos, días, y de disparar la generación de raids.
/// Trabaja sobre los campos públicos de GameManager.Instance para preservar el estado serializado.
/// </summary>
public class TimeManager : MonoBehaviour
{
    private GameManager gm;
    private float timer = 0f;
    [HideInInspector] public bool paused = false;

    public void Setup(GameManager gameManager)
    {
        gm = gameManager;
    }

    public void Initialize() { /* placeholder si hace falta */ }

    public void Tick()
    {
        if (gm == null) return;

        paused = gm.paused;
        if (paused) return;

        timer += Time.deltaTime * gm.timeSpeed;

        if (timer >= 1f)
        {
            timer = 0f;
            gm.minute += 5;

            if (gm.minute >= 60)
            {
                gm.minute = 0;
                gm.hour++;
            }

            if (gm.hour >= 24)
            {
                gm.hour = 0;
                gm.day++;
                // ejecutar end of day logic desde ColonyManager
                gm.colonyManager.EndDay();
            }

            // Mueve el contador de raids (en minutos)
            gm.raidIntervalHours = Mathf.Max(0.1f, gm.raidIntervalHours); // seguridad
            // gm.raidTimer era privado antes; usamos raidManager.raidTimer
            gm.raidManager.raidTimer += 5f;
            if (gm.raidManager.raidTimer >= gm.raidIntervalHours * 60f)
            {
                gm.raidManager.raidTimer = 0f;
                gm.raidManager.GenerateRaid();
            }

            // Procesar crías (se procesa por minuto paso)
            gm.breedingManager.ProcessBreedingTasks();

            // Actualizar UI
            gm.uiManager.UpdateUI();
        }
    }
}