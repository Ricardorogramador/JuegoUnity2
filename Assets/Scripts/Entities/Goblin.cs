
using UnityEngine;

public enum GoblinClass { Guerrero, Mago, Divino}

[System.Serializable]
public class Goblin
{

    public string nombre;
    public int fuerza;
    public int magia;
    public int divino;
    public int vida;
    public int mana;
    public int maxVida;
    public int maxMana;
    public GoblinClass goblinClass;

    public Goblin(string _nombre, int _fuerza, int _magia, int _divino)
    {
        nombre = _nombre;
        fuerza = _fuerza;
        magia = _magia;
        divino = _divino;
        vida = (int)((100 + _fuerza) * 1.2);
        maxVida = vida;
        mana = (int)((50 + _magia) * 1.2);
        maxMana = mana;

        goblinClass = AsignarClase();
    }

    private GoblinClass AsignarClase()
    {
        if (fuerza >= magia && fuerza >= divino)
            return GoblinClass.Guerrero;
        else if (magia >= fuerza && magia >= divino)
            return GoblinClass.Mago;
        else
            return GoblinClass.Divino;
    }
}
