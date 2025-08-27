using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // <- añadido

/// <summary>
/// BreedingManager: mantiene tareas de cría y estados ocupados/rest de prisioneras.
/// Muestra métodos públicos que GameManager delega.
/// Importante: usa GameManager.Instance para añadir el nuevo goblin a la lista pública.
/// </summary>
public class BreedingManager : MonoBehaviour
{
    private GameManager gm;

    // Estado de cría (mantenido aquí)
    private readonly List<BreedingTask> activeBreedings = new List<BreedingTask>();
    private readonly HashSet<Human> captivesBusy = new HashSet<Human>();
    private readonly HashSet<Goblin> goblinsBusy = new HashSet<Goblin>();
    private readonly Dictionary<Human, int> prisonerRestUntil = new Dictionary<Human, int>();

    public void Setup(GameManager gameManager) { gm = gameManager; }
    public void Initialize() { /* placeholder */ }

    public bool StartBreeding(Goblin father, Human captive)
    {
        if (gm == null) return false;
        if (father == null || captive == null)
        {
            Debug.LogWarning("[Breeding] Parámetros inválidos.");
            return false;
        }
        if (captive.sexo != HumanSex.Femenino)
        {
            Debug.LogWarning("[Breeding] Solo se puede criar con prisioneras femeninas.");
            return false;
        }
        if (!gm.prisioneras.Contains(captive))
        {
            Debug.LogWarning("[Breeding] La prisionera no está en la lista de prisioneras.");
            return false;
        }
        if (goblinsBusy.Contains(father))
        {
            Debug.LogWarning("[Breeding] El goblin seleccionado ya está ocupado en otra cría.");
            return false;
        }
        if (captivesBusy.Contains(captive))
        {
            Debug.LogWarning("[Breeding] La prisionera ya está ocupada en otra cría.");
            return false;
        }
        int now = gm.GetCurrentMinutes();
        if (IsPrisonerResting(captive, now))
        {
            int remain = GetMinutesUntilPrisonerAvailable(captive, now);
            Debug.LogWarning($"[Breeding] La prisionera está descansando. Disponible en {remain} minutos (~{remain / 60f:0.0}h).");
            return false;
        }

        int duration = Mathf.Max(1, gm.breedingDurationHours * 60);
        var task = new BreedingTask(father, captive, now, duration);
        activeBreedings.Add(task);
        goblinsBusy.Add(father);
        captivesBusy.Add(captive);

        Debug.Log($"[Breeding] Iniciada cría: {father.nombre} + {captive.nombre}. Termina a los {task.endMinute} min.");
        return true;
    }

    public void ProcessBreedingTasks()
    {
        if (activeBreedings.Count == 0 || gm == null) return;

        int now = gm.GetCurrentMinutes();
        List<BreedingTask> finished = new List<BreedingTask>();
        foreach (var task in activeBreedings)
            if (now >= task.endMinute)
                finished.Add(task);
        if (finished.Count == 0) return;

        foreach (var task in finished)
        {
            Goblin child = ComputeOffspring(task.father, task.captive);
            gm.colony.Add(child);
            goblinsBusy.Remove(task.father);
            int restMinutes = Mathf.Max(0, gm.restAfterBreedingHours * 60);
            int restUntil = now + restMinutes;
            prisonerRestUntil[task.captive] = restUntil;
            captivesBusy.Remove(task.captive);

            Debug.Log($"[Breeding] Nueva cría creada: {child.nombre} (F:{child.fuerza} M:{child.magia} D:{child.divino}). " +
                      $"Prisionera '{task.captive.nombre}' en descanso hasta minuto {restUntil} (~{gm.restAfterBreedingHours}h).");
        }

        foreach (var task in finished)
            activeBreedings.Remove(task);

        // Aquí se corrige la comprobación de escena usando UnityEngine.SceneManagement (importado arriba)
        if (SceneManager.GetActiveScene().name == gm.colonySceneName)
        {
            gm.RebuildGoblinUI();
            gm.uiManager.UpdateUI();
        }
    }

    private Goblin ComputeOffspring(Goblin father, Human captive)
    {
        int f = Mathf.Max(1, Mathf.RoundToInt((father.fuerza + captive.fuerza) / 1.5f));
        int m = Mathf.Max(1, Mathf.RoundToInt((father.magia + captive.magia) / 1.5f));
        int d = Mathf.Max(1, Mathf.RoundToInt((father.divino + captive.divino) / 1.5f));
        if (Random.value <= gm.mutationChance)
        {
            int bonus = Random.Range(gm.mutationBonusRange.x, gm.mutationBonusRange.y + 1);
            int stat = Random.Range(0, 3);
            switch (stat)
            {
                case 0: f += bonus; break;
                case 1: m += bonus; break;
                default: d += bonus; break;
            }
        }
        string childName = NameManager.Instance?.GetGoblinName() ?? $"Goblin_{gm.colony.Count + 1}";
        return new Goblin(childName, f, m, d);
    }

    public bool IsPrisonerAvailable(Human captive)
    {
        int now = gm.GetCurrentMinutes();
        return !captivesBusy.Contains(captive) && !IsPrisonerResting(captive, now);
    }

    private bool IsPrisonerResting(Human captive, int now)
    {
        if (prisonerRestUntil.TryGetValue(captive, out int until)) return now < until;
        return false;
    }

    public int GetMinutesUntilPrisonerAvailable(Human captive, int now)
    {
        if (captivesBusy.Contains(captive)) return int.MaxValue;
        if (prisonerRestUntil.TryGetValue(captive, out int until)) return Mathf.Max(0, until - now);
        return 0;
    }
}