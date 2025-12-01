using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetalController : MonoBehaviour
{
    public int value;
    public bool isSelected = false;

    // Quitamos [SerializeField] porque ya no hace falta arrastrarlo manual
    private TextMeshProUGUI numberText;
    private Image petalImage;
    private AudioSource audioSource;
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;

    private GameManager gameManager;

    // AÑADE ESTA FUNCION AWAKE
    void Awake()
    {
        // Esto busca los componentes automáticamente dentro del mismo objeto
        petalImage = GetComponent<Image>();
        numberText = GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        // Verificación de seguridad
        if (petalImage == null) Debug.LogError("¡Falta el componente Image en el pétalo!");
        if (numberText == null) Debug.LogError("¡Falta el componente TextMeshPro en el pétalo!");
        if (audioSource == null) Debug.LogError("¡Falta AudioSource en el pétalo!");
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // Asegúrate de que el botón también existe
        Button btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(ToggleSelection);

        UpdateVisuals();
    }

    public void Setup(int number)
    {
        value = number;
        // Verificamos que numberText no sea null antes de usarlo
        if (numberText != null) numberText.text = value.ToString();

        isSelected = false;
        UpdateVisuals();
    }

    void ToggleSelection()
    {
        if (audioSource != null)
            audioSource.Play();
        isSelected = !isSelected;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Solo cambiamos el color si petalImage fue encontrado
        if (petalImage != null)
        {
            petalImage.color = isSelected ? selectedColor : defaultColor;
        }
    }
}
