using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class WeaponFire : MonoBehaviour
{
    public Transform firePoint;
    public float weaponRange = 100f;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public ParticleSystem muzzleFlash;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;
    private LineRenderer lineRenderer;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(1f, 0.8f, 0f, 0.8f);
            lineRenderer.endColor = new Color(1f, 0.5f, 0f, 0.8f);
            lineRenderer.enabled = false;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void OnEnable()
    {
        interactable.activated.AddListener(Fire);
    }

    void OnDisable()
    {
        interactable.activated.RemoveListener(Fire);
    }

    private void Fire(ActivateEventArgs args)
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSound != null && audioSource != null) audioSource.PlayOneShot(fireSound);
        
        RaycastHit hit;
        Vector3 forward = firePoint.forward;
        
        if (Physics.Raycast(firePoint.position, forward, out hit, weaponRange))
        {
            StartCoroutine(RenderTracer(firePoint.position, hit.point));
            
            // Comprueba si el objeto con el que ha chocado el rayo tiene (o es hijo de) el script HumanoidReactions
            HumanoidReactions reaction = hit.collider.GetComponentInParent<HumanoidReactions>();
            if (reaction != null)
            {
                reaction.OnShotOrTouched();
            }
        }
        else
        {
            StartCoroutine(RenderTracer(firePoint.position, firePoint.position + forward * weaponRange));
        }
    }

    private IEnumerator RenderTracer(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(0.05f);
        lineRenderer.enabled = false;
    }
}