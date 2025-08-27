using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class NameManager : MonoBehaviour
{
    public static NameManager Instance { get; private set; }

    [SerializeField] private NameBankSO bank;

    [Tooltip("Si se agotan los nombres de goblin, ¿se vuelve a mezclar para permitir reutilización?")]
    [SerializeField] private bool allowReuseGoblinWhenExhausted = true;

    [Tooltip("Marcar si quieres que sobreviva entre escenas.")]
    [SerializeField] private bool persistAcrossScenes = true;

    private readonly List<string> goblinPool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        ResetGoblinPool();
    }

    public void AssignBank(NameBankSO newBank)
    {
        bank = newBank;
        ResetGoblinPool();
    }

    // Re-haz el pool (llámalo al iniciar nueva partida o nueva batalla si prefieres unicidad por batalla)
    public void ResetGoblinPool()
    {
        goblinPool.Clear();
        if (bank != null && bank.goblinNames != null && bank.goblinNames.Count > 0)
        {
            goblinPool.AddRange(bank.goblinNames);
            Shuffle(goblinPool);
        }
    }

    public string GetGoblinName()
    {
        if (bank == null)
            return "Goblin_" + Random.Range(1000, 9999);

        if (goblinPool.Count == 0)
        {
            if (!allowReuseGoblinWhenExhausted)
                return "Goblin_" + Random.Range(1000, 9999);

            ResetGoblinPool();
            if (goblinPool.Count == 0)
                return "Goblin_" + Random.Range(1000, 9999);
        }

        int last = goblinPool.Count - 1;
        string name = goblinPool[last];
        goblinPool.RemoveAt(last);
        return name;
    }

    public string GetHumanName(HumanSex sex)
    {
        if (bank == null) return sex == HumanSex.Femenino ? "Humana_" + Random.Range(1000, 9999) : "Humano_" + Random.Range(1000, 9999);

        if (sex == HumanSex.Femenino && bank.humanFemaleNames.Count > 0)
            return bank.humanFemaleNames[Random.Range(0, bank.humanFemaleNames.Count)];

        if (sex == HumanSex.Masculino && bank.humanMaleNames.Count > 0)
            return bank.humanMaleNames[Random.Range(0, bank.humanMaleNames.Count)];

        // Fallbacks
        if (bank.humanMaleNames.Count > 0 || bank.humanFemaleNames.Count > 0)
        {
            var pool = new List<string>();
            pool.AddRange(bank.humanMaleNames);
            pool.AddRange(bank.humanFemaleNames);
            return pool[Random.Range(0, pool.Count)];
        }

        return "Humano_" + Random.Range(1000, 9999);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}