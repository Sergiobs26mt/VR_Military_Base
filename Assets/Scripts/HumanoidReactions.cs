using UnityEngine;

public class HumanoidReactions : MonoBehaviour
{
    public float reactionDistance = 5f;
    public float rotationSpeed = 5f;
    public AudioClip hitSound;
    private AudioSource audioSource;
    private Transform playerTransform;
    private bool hasGreeted = false;
    private TextMesh dialogueText;

    void Start()
    {
        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Crea un objeto de texto flotante para mostrar el diálogo del humanoide
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2.2f, 0); // Coloca el texto un poco por encima de la cabeza
        
        dialogueText = textObj.AddComponent<TextMesh>();
        dialogueText.characterSize = 0.05f;
        dialogueText.fontSize = 60;
        dialogueText.anchor = TextAnchor.MiddleCenter;
        dialogueText.alignment = TextAlignment.Center;
        dialogueText.text = "";
        dialogueText.color = Color.yellow;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Hace que el texto mire siempre hacia la cámara del jugador para que sea legible desde cualquier ángulo
        if (dialogueText != null)
        {
            dialogueText.transform.rotation = Quaternion.LookRotation(dialogueText.transform.position - playerTransform.position);
        }

        // Ignora el eje Y (la altura) para calcular la distancia en un plano 2D y evitar que el humanoide mire hacia arriba o abajo
        Vector3 myPos = transform.position;
        myPos.y = 0;
        Vector3 playerPos = playerTransform.position;
        playerPos.y = 0;

        float distance = Vector3.Distance(myPos, playerPos);

        if (distance <= reactionDistance)
        {
            // Lógica del diálogo: se activa solo una vez cuando el jugador entra en el rango de distancia
            if (!hasGreeted)
            {
                hasGreeted = true;
                Speak("¡Alto ahi, soldado!\nEstas en una zona restringida\nde la base militar.");
            }

            // Hace que el humanoide rote de forma suave (interpolar) para mirar en la dirección del jugador
            Vector3 direction = playerPos - myPos;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else 
        {
            // Reinicia el saludo si el jugador se aleja lo suficiente (distancia de reacción + 2 metros de margen para evitar parpadeos)
            if (distance > reactionDistance + 2f && hasGreeted)
            {
                hasGreeted = false;
                dialogueText.text = "";
            }
        }
    }

    private void Speak(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
            CancelInvoke("ClearDialogue");
            Invoke("ClearDialogue", 4f); // Programa el borrado del texto para que desaparezca automáticamente después de 4 segundos
        }
    }

    private void ClearDialogue()
    {
        if (dialogueText != null) dialogueText.text = "";
    }

    // Esta función debe ser llamada externamente (por ejemplo, desde el script de un arma o al tocarlo) para desencadenar la reacción de daño
    public void OnShotOrTouched()
    {
        Speak("¡Me han dado! ¡Cuidado!");
        
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Reacción visual: busca todos los renderizadores (mallas) del modelo y los tiñe de color rojo momentáneamente
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                r.material.color = Color.red;
            }
            else if (r.material.HasProperty("_BaseColor")) // Compatibilidad por si el proyecto utiliza Universal Render Pipeline (URP)
            {
                r.material.SetColor("_BaseColor", Color.red);
            }
        }
        Invoke("ResetReaction", 0.5f);
    }

    void ResetReaction()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                r.material.color = Color.white;
            }
            else if (r.material.HasProperty("_BaseColor"))
            {
                r.material.SetColor("_BaseColor", Color.white);
            }
        }
    }
}