using TMPro;
using UnityEngine;

public class PrisionerasCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    void Awake()
    {
        if (counterText == null) counterText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        Refresh();
    }

    void Update()
    {
        // Si prefieres actualizar por evento, puedes quitar Update y llamar Refresh desde GameManager.UpdateUI
        Refresh();
    }

    void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm == null || counterText == null) return;

        int count = gm.prisioneras != null ? gm.prisioneras.Count : 0;
        counterText.text = "Prisioneras: " + count;
    }
}