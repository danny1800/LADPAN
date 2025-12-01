using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Velocidades")]
    public float normalSpeed = 5f;
    public float boostSpeed = 10f;
    public float laneSwitchSpeed = 10f;
    private float currentSpeed;

    [Header("Configuración de Carriles")]
    // Las posiciones X de nuestros 3 carriles: Izq (-3), Centro (0), Der (3)
    private float[] laneXPositions = { -3f, 0f, 3f };
    private int currentLaneIndex = 1; // Empezamos en el carril central (índice 1)
    private float targetXPosition;

    [Header("Estado del Turbo")]
    public float boostDuration = 2f;
    private float boostTimer = 0f;
    private bool isBoosting = false;

    // Variables para detectar el Swipe táctil
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float minSwipeDistance = 50f; // Mínima distancia para considerar que hubo swipe

    void Start()
    {
        // Posición inicial
        targetXPosition = laneXPositions[currentLaneIndex];
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        // 1. Manejar el movimiento lateral (Swipe)
        HandleSwipeInput();

        // Suavizar el movimiento hacia el carril objetivo
        Vector3 newPosition = transform.position;
        // MoveTowards nos ayuda a ir de la posición X actual a la X objetivo suavemente
        newPosition.x = Mathf.MoveTowards(newPosition.x, targetXPosition, laneSwitchSpeed * Time.deltaTime);
        transform.position = newPosition;

        // 2. Manejar el movimiento hacia adelante constante
        HandleForwardMovement();
    }

    void HandleSwipeInput()
    {
        // Detectar si hay toques en la pantalla
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                DetectSwipeDirection();
            }
        }

        // --- PARA PROBAR EN PC (BORRAR LUEGO SI SOLO ES PARA MOVIL) ---
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ChangeLane(1);
        // -------------------------------------------------------------
    }

    void DetectSwipeDirection()
    {
        // Calcular la diferencia entre donde empezó y terminó el toque
        float xDifference = touchEndPos.x - touchStartPos.x;
        float yDifference = touchEndPos.y - touchStartPos.y;

        // Verificar que el swipe fue horizontal y lo suficientemente largo
        if (Mathf.Abs(xDifference) > minSwipeDistance && Mathf.Abs(xDifference) > Mathf.Abs(yDifference))
        {
            if (xDifference > 0)
            {
                // Swipe a la derecha
                ChangeLane(1);
            }
            else
            {
                // Swipe a la izquierda
                ChangeLane(-1);
            }
        }
    }

    // Función para cambiar el índice del carril
    void ChangeLane(int direction)
    {
        currentLaneIndex += direction;
        // Clamp asegura que el índice nunca baje de 0 ni suba de 2
        currentLaneIndex = Mathf.Clamp(currentLaneIndex, 0, 2);
        targetXPosition = laneXPositions[currentLaneIndex];
    }

    void HandleForwardMovement()
    {
        // Lógica del Turbo Temporizado
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                isBoosting = false;
                currentSpeed = normalSpeed;
                GameManagerS.instance.UpdateBoostUI(false);
            }
        }

        // Mover el objeto hacia arriba (eje Y positivo es "adelante" en 2D top-down)
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
    }

    // Función pública para activar el turbo desde fuera
    public void ActivateBoost()
    {
        isBoosting = true;
        currentSpeed = boostSpeed;
        boostTimer = boostDuration;
        GameManagerS.instance.UpdateBoostUI(true);
    }
}
