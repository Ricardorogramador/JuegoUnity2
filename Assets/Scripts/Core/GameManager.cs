using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager: mantiene TODAS las referencias públicas que estaban en tu script original
/// (para preservar lo que tienes en el Inspector). Internamente orquesta managers que
/// contienen la lógica. Sigue exponiendo las mismas funciones públicas para compatibilidad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Datos (igual que antes - preservado para el inspector)
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

    // Tiempo (se mantienen aquí para que el inspector siga controlando valores)
    public int day = 1;
    public int hour = 0;
    public int minute = 0;
    public bool paused = false;
    public float timeSpeed = 1f;

    // Raids (public para inspector)
    public int heroSouls = 20;
    public float raidIntervalHours = 12f;
    public List<Raid> activeRaids = new List<Raid>();
    public int raidLevel = 1;
    public BattleSystem battleSystem;
    public Raid raidActual;

    // Capturas (public)
    [Header("Capturas")]
    [UnityEngine.Tooltip("Probabilidad base de capturar a una mujer de la raid al ganar.")]
    public float baseCaptureChance = 0.60f;
    [UnityEngine.Tooltip("Bonificación de probabilidad por cada mujer presente en la raid.")]
    public float bonusPerFemale = 0.15f;
    [UnityEngine.Tooltip("Probabilidad máxima de captura.")]
    public float maxCaptureChance = 0.95f;
    [UnityEngine.Tooltip("Lista de prisioneras capturadas en raids.")]
    public List<Human> prisioneras = new List<Human>();

    // Cría/Apareamiento (preservado)
    [Header("Cría")]
    [UnityEngine.Tooltip("Duración en horas de juego para crear un nuevo goblin.")]
    public int breedingDurationHours = 8;
    [UnityEngine.Tooltip("Horas de descanso de la prisionera después de completar la cría.")]
    public int restAfterBreedingHours = 12;
    [Range(0f, 1f)] public float mutationChance = 0.30f;
    public Vector2Int mutationBonusRange = new Vector2Int(2, 4);

    // internal managers
    [HideInInspector] public TimeManager timeManager;
    [HideInInspector] public RaidManager raidManager;
    [HideInInspector] public CaptureManager captureManager;
    [HideInInspector] public BreedingManager breedingManager;
    [HideInInspector] public ColonyManager colonyManager;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public UIManager uiManager;

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

        // Obtener o añadir managers al mismo GameObject (esto permite que el inspector siga con el mismo GO)
        timeManager = GetComponent<TimeManager>() ?? gameObject.AddComponent<TimeManager>();
        raidManager = GetComponent<RaidManager>() ?? gameObject.AddComponent<RaidManager>();
        captureManager = GetComponent<CaptureManager>() ?? gameObject.AddComponent<CaptureManager>();
        breedingManager = GetComponent<BreedingManager>() ?? gameObject.AddComponent<BreedingManager>();
        colonyManager = GetComponent<ColonyManager>() ?? gameObject.AddComponent<ColonyManager>();
        enemyManager = GetComponent<EnemyManager>() ?? gameObject.AddComponent<EnemyManager>();
        uiManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();

        // Pass reference to managers if they need it
        timeManager.Setup(this);
        raidManager.Setup(this);
        captureManager.Setup(this);
        breedingManager.Setup(this);
        colonyManager.Setup(this);
        enemyManager.Setup(this);
        uiManager.Setup(this);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (!initialized)
        {
            colonyManager.Initialize();
            enemyManager.Initialize();
            raidManager.Initialize();
            uiManager.Initialize();
            // Genera la primera raid como antes
            raidManager.GenerateRaid();
            uiManager.UpdateUI();
            initialized = true;
        }
    }

    void Update()
    {
        // Delegamos Tick a TimeManager (contiene la lógica de timer, incremento de minutos, generación de raids, etc.)
        timeManager.Tick();

        // Breeding process tick - ya invocado desde TimeManager al pasar minutos,
        // pero dejo una llamada secundaria por seguridad (es seguro llamar ProcessBreedingTasks internamente).
        // breedingManager.Tick(); // ya llamado por TimeManager, no hace falta aquí

        // Toggle pausa por espacio (mantenemos la compatibilidad con tu flujo previo)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
            timeManager.paused = paused;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == colonySceneName)
        {
            paused = false;
            raidActual = null;
            uiManager.UpdateUI();
        }
    }

    // Exponemos las mismas funciones públicas que tenías, pero delegan a los managers.
    public void AttachColonyUI(TMP_Text time, TMP_Text day, TMP_Text souls, Transform panel, GameObject prefab)
    {
        timeText = time;
        dayText = day;
        soulstext = souls;
        goblinPanel = panel;
        goblinPrefab = prefab;

        uiManager.RebuildGoblinUI();
        uiManager.UpdateUI();
        Debug.Log("[GameManager] Colony UI attached correctamente.");
    }

    public bool TryAutoBindColonyUI()
    {
        var binder = FindObjectOfType<ColonyUIBinder>();
        if (binder != null)
        {
            AttachColonyUI(binder.timeText, binder.dayText, binder.soulsText, binder.goblinPanel, binder.goblinPrefab);
            return true;
        }
        else
        {
            Debug.LogWarning("[GameManager] No se encontró ColonyUIBinder en la escena.");
            return false;
        }
    }

    public void RebuildGoblinUI() => uiManager.RebuildGoblinUI();

    public void GenerateRaid() => raidManager.GenerateRaid();

    public void EnterRaid(Raid raid) => raidManager.EnterRaid(raid);

    public void raidVictory(int completedRaidLevel) => raidManager.RaidVictory(completedRaidLevel);
    public void raidDefeat() => raidManager.RaidDefeat();

    public void HealAllGoblins() => colonyManager.HealAllGoblins();

    public Human TryCaptureFromRaid(Raid raid) => captureManager.TryCaptureFromRaid(raid);

    public void AddPrisionera(Human h) => captureManager.AddPrisionera(h);

    public bool StartBreeding(Goblin father, Human captive) => breedingManager.StartBreeding(father, captive);

    public bool IsPrisonerAvailable(Human captive) => breedingManager.IsPrisonerAvailable(captive);

    public int GetMinutesUntilPrisonerAvailable(Human captive, int now) => breedingManager.GetMinutesUntilPrisonerAvailable(captive, now);

    public int GetCurrentMinutes()
    {
        // Día 1 = minuto 0..1439
        return ((day - 1) * 24 * 60) + (hour * 60) + minute;
    }
}