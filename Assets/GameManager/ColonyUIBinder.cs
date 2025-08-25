using UnityEngine;
using TMPro;

// Pon este script en un GameObject de la escena de la colonia (por ejemplo, "RaidUIManager")
public class ColonyUIBinder : MonoBehaviour
{
    [Header("Referencias de UI en la escena de Colonia")]
    public TMP_Text timeText;       // arrastra "Time text"
    public TMP_Text dayText;        // arrastra "Day text"
    public TMP_Text soulsText;      // arrastra "Souls"
    public Transform goblinPanel;   // arrastra el contenedor donde van los ítems (ej. "content")
    public GameObject goblinPrefab; // arrastra el prefab del item de goblin

    // Evita que la UI se ate dos veces si hay más de un binder o se recarga la escena
    private static bool boundOnce = false;

    void Awake()
    {
        if (boundOnce) return;

        // Al cargar la escena, entrega las referencias al GameManager persistente
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AttachColonyUI(timeText, dayText, soulsText, goblinPanel, goblinPrefab);
            boundOnce = true;
            // Debug.Log("[ColonyUIBinder] UI de Colonia enlazada.");
        }
        // else: si el GameManager aún no existe, puedes mover esta llamada a Start o
        // implementar un retry, pero normalmente GameManager es persistente y ya está.
    }

    void OnDestroy()
    {
        // Permite re-atado en la próxima carga de la escena
        boundOnce = false;
    }
}