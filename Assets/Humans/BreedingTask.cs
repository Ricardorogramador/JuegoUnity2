using UnityEngine;

[System.Serializable]
public class BreedingTask
{
    public Goblin father;
    public Human captive;
    public int startMinute; // tiempo absoluto en minutos (desde día 1)
    public int endMinute;   // start + duración

    public BreedingTask(Goblin father, Human captive, int startMinute, int durationMinutes)
    {
        this.father = father;
        this.captive = captive;
        this.startMinute = startMinute;
        this.endMinute = startMinute + durationMinutes;
    }
}