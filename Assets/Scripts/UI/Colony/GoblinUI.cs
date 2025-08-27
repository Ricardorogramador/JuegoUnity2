using TMPro;
using UnityEngine;

public class GoblinUi : MonoBehaviour
{
    public TMP_Text NombreText;
    public TMP_Text ClaseText;
    public TMP_Text StatsText;

    public void SetData(Goblin goblin)
    {
        NombreText.text = goblin.nombre;
        ClaseText.text = goblin.goblinClass.ToString();
        StatsText.text = $"F:{goblin.fuerza} M:{goblin.magia} D:{goblin.divino}";
    }
}
