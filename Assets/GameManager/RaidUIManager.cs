using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaidUIManager : MonoBehaviour
{

    public GameObject raidPanel;
    public Transform content;
    public GameObject raidEntryPrefab;

    private GameManager gm;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        raidPanel.SetActive(false);
    }

    public void ToggleRaidPanel()
    {
        if (raidPanel.activeSelf)
        {
            raidPanel.SetActive(false);
        }
        else
        {
            raidPanel.SetActive(true);
            RefreshRaidList();
        }
    }
    
    public void RefreshRaidList()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (Raid raid in gm.activeRaids)
        {
            GameObject entry = Instantiate(raidEntryPrefab, content);
            entry.GetComponentInChildren<TMP_Text>().text = $"Raid nv {raid.nivel} ({raid.enemigos.Count} enemigos)";

            entry.GetComponent<Button>().onClick.AddListener(() => gm.EnterRaid(raid));
        }
    }
}
