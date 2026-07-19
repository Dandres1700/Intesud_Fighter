using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float vida = 100f;
    public float vidaMaxima = 100f;
    public Slider barraVida;

    [Header("Efecto de golpe")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color colorFlash = Color.white;
    [SerializeField] private float duracionFlash = 0.1f;

    [Header("Efecto de partículas")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Vector3 offsetEfecto = new Vector3(0, 1f, 0);

    private bool estaMuerto = false;
    private Color colorOriginal;
    private Coroutine flashCoroutine;
    private Animator anim;

    void Start()
    {
        barraVida.maxValue = vidaMaxima;
        barraVida.value = vida;

        anim = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            colorOriginal = spriteRenderer.color;
    }

    public void RecibirDanio(float danio)
    {
        if (estaMuerto) return;

        vida -= danio;
        vida = Mathf.Clamp(vida, 0, vidaMaxima);
        barraVida.value = vida;

        // Animación Hurt en el jugador que recibe el daño
        anim?.SetTrigger("Hurt");

        ReproducirEfectoGolpe();

        if (vida <= 0)
            Morir();
    }

    private void ReproducirEfectoGolpe()
    {
        // Flash blanco
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Partículas en el punto de impacto
        if (hitEffectPrefab != null)
        {
            Vector3 posicion = transform.position + offsetEfecto;
            GameObject efecto = Instantiate(hitEffectPrefab, posicion, Quaternion.identity);
            Destroy(efecto, 1f);
        }
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = colorFlash;
        yield return new WaitForSeconds(duracionFlash);
        spriteRenderer.color = colorOriginal;
    }

    public void ResetVida()
    {
        estaMuerto = false;
        vida = vidaMaxima;
        barraVida.value = vida;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = colorOriginal;
    }

    void Morir()
    {
        estaMuerto = true;
        Debug.Log(gameObject.name + " ha perdido!");
        gameObject.SetActive(false);
    }
}