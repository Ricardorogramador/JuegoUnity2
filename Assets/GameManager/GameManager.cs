using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Datos
    public List<Goblin> colony = new List<Goblin>();
    public List<Human> enemies = new List<Human>();

    // UI (estas referencias se rellenan al entrar a la escena de colonia)
    [Header("UI (se reatacha al cargar escena)")]
    public Transform goblinPanel;
    public GameObject goblinPrefab;
    public TMP_Text timeText;
    public TMP_Text dayText;
    public TMP_Text soulstext;

    [Header("Config")]
    public string colonySceneName = "ColonyScene";

    // Tiempo
    public int day = 1;
    public int hour = 0;
    public int minute = 0;
    public bool paused = false;

    private float timer = 0f;
    public float timeSpeed = 1f;

    // Raids
    public int heroSouls = 20;
    private float raidTimer = 0f;
    public float raidIntervalHours = 12f;

    public List<Raid> activeRaids = new List<Raid>();
    public int raidLevel = 1;
    public BattleSystem battleSystem;
    public Raid raidActual;

    // Capturas
    [Header("Capturas")]
    [Tooltip("Probabilidad base de capturar a una mujer de la raid al ganar.")]
    public float baseCaptureChance = 0.60f;     // 60%
    [Tooltip("Bonificación de probabilidad por cada mujer presente en la raid.")]
    public float bonusPerFemale = 0.15f;        // +15% por mujer
    [Tooltip("Probabilidad máxima de captura.")]
    public float maxCaptureChance = 0.95f;      // Tope 95%
    [Tooltip("Lista de prisioneras capturadas en raids.")]
    public List<Human> prisioneras = new List<Human>();

    private bool initialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {

        // Si arrancas directamente en ColonyScene, intenta auto-bindeo
        TryAutoBindColonyUI();

        // Binding de UI: ahora lo hace exclusivamente ColonyUIBinder.Awake en la escena de colonia.


        if (!initialized)
        {
            InicializarColonia();
            InicializarEnemigos();
            GenerateRaid();
            UpdateUI();
            initialized = true;
        }
    }

    void Update()
    {
        if (!paused)
        {
            timer += Time.deltaTime * timeSpeed;

            if (timer >= 1f)
            {
                timer = 0f;
                minute += 5;

                if (minute >= 60)
                {
                    minute = 0;
                    hour++;
                }

                if (hour >= 24)
                {
                    hour = 0;
                    day++;
                    endDay();
                }

                raidTimer += 5;
                if (raidTimer >= raidIntervalHours * 60f)
                {
                    raidTimer = 0f;
                    GenerateRaid();
                }

                UpdateUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si la escena cargada es la colonia, reanuda y auto-bindea
        if (scene.name == colonySceneName)
        {
            paused = false;

            TryAutoBindColonyUI();

            raidActual = null; // evita residuo de la raid previa

            UpdateUI();
        }
    }

    // Llamado por el ColonyUIBinder de la escena
    public void AttachColonyUI(TMP_Text time, TMP_Text day, TMP_Text souls, Transform panel, GameObject prefab)
    {
        timeText = time;
        dayText = day;
        soulstext = souls;
        goblinPanel = panel;
        goblinPrefab = prefab;

        // Reconstruye la lista y refresca cabeceras
        RebuildGoblinUI();
        UpdateUI();

        Debug.Log("[GameManager] Colony UI attached correctamente.");
    }


    // Intenta encontrar automáticamente un ColonyUIBinder en la escena (si el desarrollador se olvidó)

    // Utilidad manual por si olvidas colocar el binder (no se llama automáticamente).

    bool TryAutoBindColonyUI()
    {
        var binder = FindObjectOfType<ColonyUIBinder>();
        if (binder != null)
        {
            AttachColonyUI(binder.timeText, binder.dayText, binder.soulsText, binder.goblinPanel, binder.goblinPrefab);
            return true;
        }
        else
        {
            Debug.LogWarning("[GameManager] No se encontró ColonyUIBinder en la escena. Arrastra el script a un objeto (p.ej. RaidUIManager) y asigna las referencias.");
            return false;
        }
    }

    void RebuildGoblinUI()
    {
        if (goblinPanel == null) return;

        // Limpia hijos actuales
        var toDestroy = new List<GameObject>();
        foreach (Transform child in goblinPanel) toDestroy.Add(child.gameObject);
        foreach (var go in toDestroy) Destroy(go);

        if (goblinPrefab == null)
        {
            Debug.LogWarning("[GameManager] goblinPrefab no asignado. La lista no se podrá dibujar.");
            return;
        }

        foreach (Goblin g in colony)
        {
            var item = Instantiate(goblinPrefab, goblinPanel);
            var ui = item.GetComponent<GoblinUi>();
            if (ui != null) ui.SetData(g);
        }
    }

    void InicializarColonia()
    {
        for (int i = 0; i < 3; i++)
        {
            int f = UnityEngine.Random.Range(0, 5);
            int m = UnityEngine.Random.Range(0, 5);
            int d = UnityEngine.Random.Range(0, 5);
            colony.Add(new Goblin("Goblin_" + i, f, m, d));
        }
        RebuildGoblinUI();
    }

    void InicializarEnemigos()
    {
        for (int i = 0; i < 2; i++)
        {
            int f = UnityEngine.Random.Range(0, 5);
            int m = UnityEngine.Random.Range(0, 5);
            int d = UnityEngine.Random.Range(0, 5);
            HumanSex sex = (UnityEngine.Random.Range(0, 2) == 0) ? HumanSex.Masculino : HumanSex.Femenino;
            enemies.Add(new Human("Humano_" + i, f, m, d, sex));
        }
    }

    public void GenerateRaid()
    {
        Raid nuevaRaid = new Raid(raidLevel);
        activeRaids.Add(nuevaRaid);
        raidLevel++;
        paused = true;
        Debug.Log("Nueva raid generada. El juego se pausa.");
    }

    public void EnterRaid(Raid raid)
    {
        if (raid == null) return;
        raidActual = raid;
        activeRaids.Remove(raid);
        SceneManager.LoadScene("BattleScene");
    }

    void endDay()
    {
        int requiredSouls = colony.Count;
        if (heroSouls >= requiredSouls)
        {
            heroSouls -= requiredSouls;
        }
        else
        {
            float chance = UnityEngine.Random.Range(0f, 1f);
            if (chance < 0.66f) applyRandomDebuff();
            else killRandomGoblin();
            heroSouls = 0;
        }
    }

    void applyRandomDebuff()
    {
        if (colony.Count == 0) return;
        Goblin target = colony[UnityEngine.Random.Range(0, colony.Count)];
        int stat = UnityEngine.Random.Range(0, 3);
        switch (stat)
        {
            case 0: target.fuerza = Math.Max(1, (int)(target.fuerza * 0.8f)); break;
            case 1: target.magia = Math.Max(1, (int)(target.magia * 0.8f)); break;
            case 2: target.divino = Math.Max(1, (int)(target.divino * 0.8f)); break;
        }
    }

    void killRandomGoblin()
    {
        if (colony.Count == 0) return;
        int index = UnityEngine.Random.Range(0, colony.Count);
        colony.RemoveAt(index);
        if (goblinPanel != null && index < goblinPanel.childCount)
            Destroy(goblinPanel.GetChild(index).gameObject);
    }



    // NUEVO: sobrecarga correcta que usa el nivel de la raid completada
    public void raidVictory(int completedRaidLevel)
    {
        int rewardSouls = completedRaidLevel * 2;
        heroSouls += rewardSouls;
        UpdateUI();
    }

    [Obsolete("Usa raidVictory(int completedRaidLevel) pasando el nivel real de la raid completada.")]
    public void raidVictory()
    {
        int rewardSouls = raidLevel * 2;
        heroSouls += rewardSouls;
    }

    public void raidDefeat()
    {
        if (colony.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, colony.Count);
            colony.RemoveAt(idx);
        }
    }

    public void HealAllGoblins()
    {
        foreach (Goblin g in colony)
        {
            g.vida = g.maxVida;
            g.mana = g.maxMana;
        }
    }

    // CAPTURA: intenta capturar UNA mujer de la raid ganada
    // Devuelve la prisionera capturada o null si no hubo captura
    public Human TryCaptureFromRaid(Raid raid)
    {
        if (raid == null || raid.enemigos == null || raid.enemigos.Count == 0)
            return null;

        // Reúne candidatas femeninas
        List<Human> candidatas = new List<Human>();
        foreach (var h in raid.enemigos)
        {
            // Asumiendo que Human tiene un campo o propiedad 'sexo' de tipo HumanSex
            if (h.sexo == HumanSex.Femenino)
            {
                candidatas.Add(h);
            }
        }

        if (candidatas.Count == 0) return null;

        float chance = Mathf.Min(maxCaptureChance, baseCaptureChance + bonusPerFemale * candidatas.Count);
        float roll = UnityEngine.Random.value;

        if (roll <= chance)
        {
            Human capturada = candidatas[UnityEngine.Random.Range(0, candidatas.Count)];
            AddPrisionera(capturada);

            // Limpieza opcional: remover de la lista de enemigos de la raid
            raid.enemigos.Remove(capturada);
            // Puedes marcar la raid como no activa si ya no la usarás
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
        if (h == null) return;
        if (prisioneras == null) prisioneras = new List<Human>();
        prisioneras.Add(h);
    }

    void UpdateUI()
    {
        if (timeText != null) timeText.text = string.Format("{0:00}:{1:00}", hour, minute);
        if (dayText != null) dayText.text = "Day " + day;
        if (soulstext != null) soulstext.text = "Almas: " + heroSouls;
    }
}