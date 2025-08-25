using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Raid
{
    public List<Human> enemigos;
    public int nivel;
    public bool activa;

    public Raid(int _nivel)
    {
        // Asegura nivel mínimo 1 por seguridad
        nivel = Mathf.Max(1, _nivel);
        activa = true;

        // Determina cantidad de enemigos según nivel
        int cantidad = 1;
        if (nivel <= 5)
        {
            cantidad = Random.Range(1, 3); // 1-2 enemigos
        }
        else if (nivel <= 10)
        {
            cantidad = Random.Range(3, 5); // 3-4 enemigos
        }
        else
        {
            cantidad = Random.Range(4, 6); // 4-5 enemigos
        }

        // Pre-asigna capacidad para evitar realocaciones
        enemigos = new List<Human>(cantidad);

        // Crear enemigos con stats basados en el nivel
        for (int i = 0; i < cantidad; i++)
        {
            int f = Random.Range(1, 5 + nivel);
            int m = Random.Range(1, 5 + nivel);
            int d = Random.Range(1, 5 + nivel);
            HumanSex sex = (Random.Range(0, 2) == 0) ? HumanSex.Masculino : HumanSex.Femenino;

            enemigos.Add(new Human("Humano_" + i, f, m, d, sex));
        }
    }
}