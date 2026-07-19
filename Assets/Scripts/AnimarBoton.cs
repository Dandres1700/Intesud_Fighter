using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AnimarBoton : MonoBehaviour
{
    [Header("Animación")]
    public Sprite[] frames;
    public float velocidad = 0.1f;

    [Tooltip("Si está activo, la animación reinicia y se reproduce en bucle.")]
    public bool repetir = true;

    [Tooltip("Si está activo, la animación empieza apenas se activa el objeto.")]
    public bool reproducirAlInicio = true;

    private Image imagen;
    private int frameActual = 0;
    private float timer = 0f;
    private bool reproduciendo = true;

    void Start()
    {
        imagen = GetComponent<Image>();

        if (imagen == null || frames == null || frames.Length == 0)
        {
            reproduciendo = false;
            return;
        }

        if (!reproducirAlInicio)
        {
            reproduciendo = false;
            return;
        }

        frameActual = 0;
        imagen.sprite = frames[frameActual];
    }

    void Update()
    {
        if (!reproduciendo || imagen == null || frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= velocidad)
        {
            timer = 0f;
            frameActual++;

            if (frameActual >= frames.Length)
            {
                if (repetir)
                {
                    frameActual = 0;
                }
                else
                {
                    frameActual = frames.Length - 1;
                    reproduciendo = false;
                }
            }

            imagen.sprite = frames[frameActual];
        }
    }

    public void IniciarAnimacion()
    {
        if (imagen == null)
            imagen = GetComponent<Image>();

        if (imagen == null || frames == null || frames.Length == 0)
            return;

        frameActual = 0;
        timer = 0f;
        reproduciendo = true;
        imagen.sprite = frames[frameActual];
    }

    public void DetenerAnimacion()
    {
        reproduciendo = false;
    }
}
