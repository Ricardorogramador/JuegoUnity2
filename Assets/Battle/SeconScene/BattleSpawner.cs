using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    [Header("Zonas (BoxCollider2D)")]
    public BoxCollider2D goblinZone; // izquierda
    public BoxCollider2D humanZone;  // derecha

    [Header("Contenedores (opcional)")]
    public Transform goblinParent;
    public Transform humanParent;

    [Header("Tokens (mundo 2D)")]
    public float tokenRadius = 0.5f;   // tamaño de ficha y separación mínima (diámetro = 2*radius)
    public int maxAttemptsPerUnit = 120;
    public float tokensZ = 0f;
    public string sortingLayer = "Characters";
    public int sortingOrder = 0;

    [Header("Forma/color")]
    public ShapeType goblinShape = ShapeType.Square;
    public ShapeType humanShape  = ShapeType.Circle;

    public Color goblinColor = new Color(0.18f, 0.8f, 0.18f, 1f);
    public Color humanColor  = new Color(0.85f, 0.2f, 0.2f, 1f);

    public Dictionary<Goblin, UnitToken> SpawnGoblins(List<Goblin> goblins)
    {
        return SpawnForList(goblins, goblinZone, goblinParent, goblinShape, goblinColor);
    }

    public Dictionary<Human, UnitToken> SpawnHumans(List<Human> humans)
    {
        return SpawnForList(humans, humanZone, humanParent, humanShape, humanColor);
    }

    private Dictionary<T, UnitToken> SpawnForList<T>(
        List<T> list,
        BoxCollider2D zone,
        Transform parent,
        ShapeType shape,
        Color color)
    {
        var dict = new Dictionary<T, UnitToken>();
        if (list == null || list.Count == 0 || zone == null)
        {
            if (zone == null) Debug.LogWarning("[BattleSpawner] Zona no asignada.");
            return dict;
        }

        var placed = new List<Vector2>();

        Bounds b = zone.bounds;
        float minX = b.min.x + tokenRadius;
        float maxX = b.max.x - tokenRadius;
        float minY = b.min.y + tokenRadius;
        float maxY = b.max.y - tokenRadius;

        float minDist = tokenRadius * 2f;

        foreach (var unit in list)
        {
            bool placedOk = false;
            Vector2 chosen = Vector2.zero;

            for (int attempt = 0; attempt < maxAttemptsPerUnit; attempt++)
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);
                var candidate = new Vector2(x, y);

                bool farEnough = true;
                for (int i = 0; i < placed.Count; i++)
                {
                    if ((candidate - placed[i]).sqrMagnitude < (minDist * minDist))
                    {
                        farEnough = false;
                        break;
                    }
                }

                if (farEnough)
                {
                    chosen = candidate;
                    placed.Add(chosen);
                    placedOk = true;
                    break;
                }
            }

            if (!placedOk)
            {
                chosen = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
                placed.Add(chosen);
            }

            var go = new GameObject($"Token:{GetUnitName(unit)}", typeof(SpriteRenderer), typeof(UnitToken));
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.position = new Vector3(chosen.x, chosen.y, tokensZ);

            var token = go.GetComponent<UnitToken>();
            token.Init(shape, color, tokenRadius, sortingOrder, sortingLayer);

            dict[unit] = token;
        }

        return dict;
    }

    private string GetUnitName(object unit)
    {
        if (unit == null) return "Unit";
        var t = unit.GetType();
        var p = t.GetProperty("nombre", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (p != null)
        {
            var v = p.GetValue(unit) as string;
            if (!string.IsNullOrEmpty(v)) return v;
        }
        return t.Name;
    }

    private void OnDrawGizmos()
    {
        if (goblinZone != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawCube(goblinZone.bounds.center, goblinZone.bounds.size);
            Gizmos.color = new Color(0f, 0.6f, 0f, 0.9f);
            Gizmos.DrawWireCube(goblinZone.bounds.center, goblinZone.bounds.size);
        }
        if (humanZone != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawCube(humanZone.bounds.center, humanZone.bounds.size);
            Gizmos.color = new Color(0.6f, 0f, 0f, 0.9f);
            Gizmos.DrawWireCube(humanZone.bounds.center, humanZone.bounds.size);
        }
    }
}