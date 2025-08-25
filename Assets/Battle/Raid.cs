using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Raid
{
    public List<Human> enemigos;
    public int nivel;
    public Boolean activa;

    public Raid(int _nivel)
    {
        nivel = _nivel;
        activa = true;
        enemigos = new List<Human>();

        int cantidad = 1;

        // Configuración progresiva de enemigos
        if (nivel <= 5)
        {
            cantidad = UnityEngine.Random.Range(1, 3); // 1-2 enemigos
        }
        else if (nivel <= 10)
        {
            cantidad = UnityEngine.Random.Range(3, 5); // 3-4 enemigos
        }
        else
        {
            cantidad = UnityEngine.Random.Range(4, 6); // 4-5 enemigos
        }

        // Crear enemigos con stats basados en el nivel
        for (int i = 0; i < cantidad; i++)
        {
            int f = UnityEngine.Random.Range(1, 5 + nivel);
            int m = UnityEngine.Random.Range(1, 5 + nivel);
            int d = UnityEngine.Random.Range(1, 5 + nivel);
            HumanSex sex = (UnityEngine.Random.Range(0, 2) == 0) ? HumanSex.Masculino : HumanSex.Femenino;

            enemigos.Add(new Human("Humano_" + i, f, m, d, sex));
        }
    }
}
