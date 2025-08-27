using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NameBankCreator
{
    [MenuItem("Assets/Create/Game/Name Bank (Default Sets)")]
    public static void CreateDefault()
    {
        var asset = ScriptableObject.CreateInstance<NameBankSO>();
        asset.goblinNames = new List<string>(GoblinDefault());
        asset.humanMaleNames = new List<string>(HumanMaleDefault());
        asset.humanFemaleNames = new List<string>(HumanFemaleDefault());

        string path = EditorUtility.SaveFilePanelInProject("Create NameBank", "NameBank", "asset", "Elige ubicación para NameBank.asset");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log("NameBank creado en: " + path);
        }
    }

    private static IEnumerable<string> GoblinDefault() => new[]
    {
        "Grubnak","Snagga","Ruk","Skrit","Gritch","Mogluk","Zug","Snort","Krunk","Boggle",
        "Skab","Nurg","Uglox","Vrig","Gnasher","Snikkit","Glim","Bogrot","Grash","Splug",
        "Mugrub","Drak","Skug","Zib","Gizzik","Brak","Grob","Snik","Urk","Grik",
        "Drokk","Morg","Scrag","Pug","Vrek","Zag","Krizz","Blarg","Gorf","Snub",
        "Trug","Skabnak","Nog","Grukk","Zogg","Skrag","Rikkit","Grimz","Skag","Zruk"
    };

    private static IEnumerable<string> HumanMaleDefault() => new[]
    {
        "Alejandro","Andrés","Antonio","Bruno","Carlos","Cristian","Daniel","David","Diego","Eduardo",
        "Emilio","Esteban","Fernando","Francisco","Gabriel","Guillermo","Gonzalo","Hugo","Iván","Javier",
        "Joaquín","Jorge","José","Juan","Julián","Leonardo","Luis","Manuel","Marcos","Mario",
        "Martín","Mateo","Miguel","Nicolás","Óscar","Pablo","Pedro","Rafael","Raúl","Ricardo",
        "Roberto","Rodrigo","Rubén","Salvador","Samuel","Santiago","Sergio","Tomás","Valentín","Vicente"
    };

    private static IEnumerable<string> HumanFemaleDefault() => new[]
    {
        "Alejandra","Alicia","Ana","Andrea","Ángela","Beatriz","Camila","Carla","Carolina","Cecilia",
        "Clara","Daniela","Diana","Elena","Elisa","Elvira","Emilia","Esther","Fernanda","Gabriela",
        "Inés","Isabel","Jimena","Josefina","Julia","Laura","Lorena","Lucía","Luna","Magdalena",
        "Manuela","Marina","Marta","Mercedes","Micaela","Natalia","Noelia","Nora","Olivia","Paula",
        "Patricia","Pilar","Renata","Rosa","Sara","Silvia","Sofía","Teresa","Valentina","Victoria"
    };
}