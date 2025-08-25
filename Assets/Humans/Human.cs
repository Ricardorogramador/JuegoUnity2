using UnityEngine;

public enum HumanSex { Masculino, Femenino }

[System.Serializable]
public class Human
{
    public string nombre;
    public int fuerza;
    public int magia;
    public int divino;
    public int vida;
    public int mana;
    public GoblinClass humanClass;
    public HumanSex sexo;

    public Human(string _nombre, int _fuerza, int _magia, int _divino, HumanSex _sexo)
    {
        nombre = _nombre;
        fuerza = _fuerza;
        magia = _magia;
        divino = _divino;
        sexo = _sexo;

        vida = (int)((100 + _fuerza) * 1.5f);
        mana = (int)((50 + _magia) * 1.5f);

        humanClass = AsignarClase();
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
