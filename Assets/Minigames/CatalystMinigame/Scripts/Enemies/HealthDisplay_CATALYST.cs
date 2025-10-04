using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay_CATALYST : MonoBehaviour
{
    public Text healthText;
    public Image[] healthHearts;
    public Color fullHeartColor = Color.red;
    public Color emptyHeartColor = Color.gray;
    
    Player_CATALYST player;
    
    void Start()
    {
        player = FindObjectOfType<Player_CATALYST>();
        
        if (healthText == null && healthHearts == null)
        {
            CreateSimpleHealthDisplay();
        }
    }
    
    void Update()
    {
        if (player != null)
        {
            UpdateHealthDisplay();
        }
    }
    
    void UpdateHealthDisplay()
    {
        int currentHealth = player.GetCurrentHealth();
        int maxHealth = player.GetMaxHealth();
        
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
            
            if (currentHealth <= 1)
                healthText.color = Color.red;
            else if (currentHealth <= maxHealth / 2)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.white;
        }
        
        if (healthHearts != null)
        {
            for (int i = 0; i < healthHearts.Length; i++)
            {
                if (i < maxHealth && healthHearts[i] != null)
                {
                    healthHearts[i].gameObject.SetActive(true);
                    healthHearts[i].color = (i < currentHealth) ? fullHeartColor : emptyHeartColor;
                }
                else if (healthHearts[i] != null)
                {
                    healthHearts[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    void CreateSimpleHealthDisplay()
    {
        GameObject canvasObj = new GameObject("HealthCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.text = "Health: 3/3";
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        healthText = text;
    }
}