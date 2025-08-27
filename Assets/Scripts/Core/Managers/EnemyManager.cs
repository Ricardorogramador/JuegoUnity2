using UnityEngine;

/// <summary>
/// EnemyManager: inicializa enemigos (humanos) si es necesario.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    private GameManager gm;

    public void Setup(GameManager gameManager) { gm = gameManager; }
    public void Initialize()
    {
        if (gm == null) gm = GameManager.Instance;
        if (gm.enemies != null && gm.enemies.Count > 0) return;

        for (int i = 0; i < 2; i++)
        {
            int f = Random.Range(0, 5);
            int m = Random.Range(0, 5);
            int d = Random.Range(0, 5);
            HumanSex sex = (Random.Range(0, 2) == 0) ? HumanSex.Masculino : HumanSex.Femenino;
            string nombre = NameManager.Instance != null ? NameManager.Instance.GetHumanName(sex) : $"Humano_{i}";
            gm.enemies.Add(new Human(nombre, f, m, d, sex));
        }
    }
}