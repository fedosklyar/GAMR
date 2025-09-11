using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public int numEnemyKilled = 0;
    public bool miniBoss1 = false;

    public GameObject boss1;
    public GameObject minienemyL1;

    public Vector3 keyPos;
    public GameObject key;
    public GameObject keyUI;
    public int keyLevel = 0;

    EnemyStats bossStat;

    public int levelObjective;

    public GameObject objective1;
    public GameObject objective2;

    public int counter1 = 0;
    public int enemy1Objective;
    public int counter2 = 0;
    public int enemy2Objective;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;

    public UIManager ui;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        bossStat = boss1.GetComponent<EnemyStats>();
        key.SetActive(false);
        minienemyL1.SetActive(false);

        objective1.SetActive(false);
        objective2.SetActive(false);
        if (enemy1Objective != 0)
        {
            objective1.SetActive(true);
            text1.text = "0 of " + enemy1Objective.ToString();
        }
        if (enemy2Objective != 0)
        {
            objective2.SetActive(true);
            text2.text = "0 of " + enemy2Objective.ToString();
        }

    }

    // Update is called once per frame
    void Update() {
        if(numEnemyKilled == levelObjective - 1)
        {
            minienemyL1.SetActive(true);
        }

        if(numEnemyKilled >= levelObjective)
        {
            miniBoss1 = true;
        }

        if(bossStat && bossStat.bossDead == true){
            key.transform.position = keyPos;
            key.SetActive(true);
        }

        if(keyLevel == 1)
        {
            keyUI.SetActive(true);            
        }         
    }

    public void ObjectiveStatus(string enemyName)
    {
        numEnemyKilled += 1;
        if(enemyName.Contains("Enemy_"))
        {
            counter1++;
            text1.text = counter1.ToString() + " of " +enemy1Objective.ToString();
        }
        else if(enemyName.Contains("Enemy2_"))   
        {
            counter2++;
            text2.text = counter2.ToString() + " of " +enemy2Objective.ToString();
        }  
    }

    public void GameOver()
    {
        if(ui != null)
        {
            ui.ToggleDeathPanel();
        }
    }
}
