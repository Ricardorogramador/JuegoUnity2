using UnityEngine;
using TMPro;

// Pon este script en un GameObject de la escena de la colonia (por ejemplo, "RaidUIManager")
public class ColonyUIBinder : MonoBehaviour
{
    [Header("Referencias de UI en la escena de Colonia")]
    public TMP_Text timeText;      // arrastra "Time text"
    public TMP_Text dayText;       // arrastra "Day text"
    public TMP_Text soulsText;     // arrastra "Souls"
    public Transform goblinPanel;  // arrastra el contenedor donde van los ítems (ej. "content")
    public GameObject goblinPrefab; // arrastra el prefab del item de goblin

    void Awake()
    {
        // Al cargar la escena, entrega las referencias al GameManager persistente
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AttachColonyUI(timeText, dayText, soulsText, goblinPanel, goblinPrefab);
        }
    }
}