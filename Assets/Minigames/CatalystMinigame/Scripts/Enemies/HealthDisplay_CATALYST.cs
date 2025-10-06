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
        player = FindFirstObjectByType<Player_CATALYST>();
        CreateSimpleHealthDisplay();
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
        int pounceCharges = player.GetPounceCharges();
        
        if (healthText != null)
        {
            if (player.IsPoweredUp())
            {
                healthText.text = $"Health: {currentHealth}/{maxHealth} | Pounce: {pounceCharges}";
                healthText.color = Color.cyan; // Glow when powered up
            }
            else
            {
                healthText.text = $"Health: {currentHealth}/{maxHealth}";
                
                if (currentHealth <= 1)
                    healthText.color = Color.red;
                else if (currentHealth <= maxHealth / 2)
                    healthText.color = Color.yellow;
                else
                    healthText.color = Color.green;
            }
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
        canvas.sortingOrder = 1000;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.color = Color.red;
        int currentHealth = player.GetCurrentHealth();
        int maxHealth = player.GetMaxHealth();
        text.text = $"Health: {currentHealth}/{maxHealth}";
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, 200);
        rectTransform.sizeDelta = new Vector2(400, 60);
        
        healthText = text;
    }
}