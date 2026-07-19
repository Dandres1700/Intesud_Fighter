using UnityEngine;
using UnityEngine.Serialization;

public class CPUController : MonoBehaviour
{
    [Header("Referencia al oponente")]
    public Transform objetivo;

    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;

    [Header("Combate")]
    public float danio = 10f;
    public float rangoGolpe = 1.5f;
    public float rangoDeteccion = 8f;

    [Header("Salto")]
    [SerializeField] private float multiplicadorCaida = 2.5f;

    [Header("Particulas doble salto")]
    [FormerlySerializedAs("efectoDobleSaltoPrefab")]
    public GameObject particulasDobleSalto;
    [SerializeField] private Vector3 offsetEfectoDobleSalto = new Vector3(0f, -0.4f, 0f);
    [SerializeField] private float duracionEfectoDobleSalto = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private bool estaEnSuelo = false;
    private bool inputBloqueado = false;

    private float timerAtaque = 0f;
    private float timerSalto = 0f;
    private float timerMovimiento = 0f;

    private float intervaloAtaque;
    private float intervaloSalto;
    private float distanciaIdeal;
    private float chanceEsquivar;
    private float chanceDobleSalto;
    private float delayDobleSalto;
    private float chanceAtaque;
    private float rangoVerticalGolpe;
    private float factorMovimientoLateral;
    private float chancePresionarEnRango;
    private float tiempoDecisionMin;
    private float tiempoDecisionMax;
    private float tiempoPrediccion;

    private bool estaAtacando = false;
    private bool dobleSaltoDisponible = false;
    private int direccionActual = 0;
    private float timerDobleSalto = 0f;
    private Transform objetivoCacheado;
    private Rigidbody2D rbObjetivo;

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        AplicarDificultad();
    }

    private void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        PrepararNuevaRonda();
    }

    private void OnDisable()
    {
        CancelInvoke();
        estaAtacando = false;
        dobleSaltoDisponible = false;
    }

    private void AplicarDificultad()
    {
        switch (GameModeManager.DificultadActual)
        {
            case GameModeManager.Dificultad.Facil:
                intervaloAtaque = 2.5f;
                intervaloSalto  = 4f;
                distanciaIdeal  = 3f;
                chanceEsquivar  = 0.1f;
                chanceDobleSalto = 0.15f;
                delayDobleSalto  = 0.45f;
                chanceAtaque = 0.55f;
                rangoVerticalGolpe = 1.0f;
                factorMovimientoLateral = 0.35f;
                chancePresionarEnRango = 0.25f;
                tiempoDecisionMin = 0.6f;
                tiempoDecisionMax = 1.1f;
                tiempoPrediccion = 0.05f;
                velocidad       = 3f;
                break;

            case GameModeManager.Dificultad.Medio:
                intervaloAtaque = 1.5f;
                intervaloSalto  = 2.5f;
                distanciaIdeal  = 2f;
                chanceEsquivar  = 0.3f;
                chanceDobleSalto = 0.45f;
                delayDobleSalto  = 0.3f;
                chanceAtaque = 0.75f;
                rangoVerticalGolpe = 1.25f;
                factorMovimientoLateral = 0.45f;
                chancePresionarEnRango = 0.45f;
                tiempoDecisionMin = 0.35f;
                tiempoDecisionMax = 0.75f;
                tiempoPrediccion = 0.12f;
                velocidad       = 4f;
                break;

            case GameModeManager.Dificultad.Dificil:
                intervaloAtaque = 0.8f;
                intervaloSalto  = 1.5f;
                distanciaIdeal  = 1.5f;
                chanceEsquivar  = 0.6f;
                chanceDobleSalto = 0.75f;
                delayDobleSalto  = 0.18f;
                chanceAtaque = 0.93f;
                rangoVerticalGolpe = 1.6f;
                factorMovimientoLateral = 0.6f;
                chancePresionarEnRango = 0.7f;
                tiempoDecisionMin = 0.18f;
                tiempoDecisionMax = 0.45f;
                tiempoPrediccion = 0.2f;
                velocidad       = 5.5f;
                break;
        }
    }

    public void BloquearInput(bool bloquear)
    {
        inputBloqueado = bloquear;
        if (bloquear && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool("isRunning", false);
            dobleSaltoDisponible = false;
        }
    }

    public void PrepararNuevaRonda()
    {
        AplicarDificultad();
        CancelInvoke();

        estaAtacando    = false;
        estaEnSuelo     = false;
        inputBloqueado  = false;
        dobleSaltoDisponible = false;
        timerAtaque     = 1f;    // delay antes del primer ataque
        timerSalto      = 1f;
        timerMovimiento = 0f;
        timerDobleSalto = 0f;
        direccionActual = 0;

        if (rb != null)
        {
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale    = 1f;
            rb.WakeUp();
        }

        IgnorarColisionConObjetivo();

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

    private void Update()
    {
        if (inputBloqueado) return;
        if (objetivo == null) return;
        if (!objetivo.gameObject.activeSelf) return;

        ActualizarCacheObjetivo();
        estaEnSuelo = EstaTocandoSuelo();

        Vector2 posicionObjetivo = ObtenerPosicionObjetivoPredicha();
        Vector2 delta = posicionObjetivo - (Vector2)transform.position;
        float distancia = delta.magnitude;
        float distanciaHorizontal = Mathf.Abs(delta.x);
        float diferenciaAltura = Mathf.Abs(objetivo.position.y - transform.position.y);
        float direccion = delta.x >= 0f ? 1f : -1f;

        timerAtaque     -= Time.deltaTime;
        timerSalto      -= Time.deltaTime;
        timerMovimiento -= Time.deltaTime;
        timerDobleSalto -= Time.deltaTime;

        ManejarOrientacion(direccion);
        ManejarAtaque(distanciaHorizontal, diferenciaAltura);
        ManejarMovimiento(distanciaHorizontal, diferenciaAltura, direccion);
        ManejarSalto(distancia, distanciaHorizontal, diferenciaAltura);

        rb.gravityScale = (rb.linearVelocity.y < 0 && !estaEnSuelo) ? multiplicadorCaida : 1f;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        transform.position = pos;
    }

    private void ManejarOrientacion(float direccion)
    {
        if (sr == null) return;
        sr.flipX = direccion < 0;
    }

    private void ManejarMovimiento(float distanciaHorizontal, float diferenciaAltura, float direccion)
    {
        if (estaAtacando) return;

        float movimiento = 0f;
        float distanciaMuyCorta = Mathf.Min(distanciaIdeal - 0.5f, rangoGolpe * 0.55f);

        if (distanciaHorizontal > distanciaIdeal)
        {
            movimiento = direccion;
        }
        else if (distanciaHorizontal < distanciaMuyCorta && diferenciaAltura <= rangoVerticalGolpe)
        {
            movimiento = -direccion;
        }
        else
        {
            if (timerMovimiento <= 0f)
            {
                direccionActual = ElegirMovimientoCorto(distanciaHorizontal, direccion);
                timerMovimiento = Random.Range(tiempoDecisionMin, tiempoDecisionMax);
            }
            movimiento = direccionActual * factorMovimientoLateral;
        }

        rb.linearVelocity = new Vector2(movimiento * velocidad, rb.linearVelocity.y);
        anim?.SetBool("isRunning", Mathf.Abs(movimiento) > 0.1f);
    }

    private int ElegirMovimientoCorto(float distanciaHorizontal, float direccion)
    {
        int direccionHaciaObjetivo = direccion >= 0f ? 1 : -1;

        if (distanciaHorizontal > rangoGolpe * 0.9f)
            return direccionHaciaObjetivo;

        if (Random.value < chancePresionarEnRango)
            return direccionHaciaObjetivo;

        if (Random.value < chanceEsquivar)
            return -direccionHaciaObjetivo;

        return Random.Range(-1, 2);
    }

    private void ManejarSalto(float distancia, float distanciaHorizontal, float diferenciaAltura)
    {
        bool puedeSaltarDesdeSuelo = estaEnSuelo && rb.linearVelocity.y <= 0.01f;

        if (!puedeSaltarDesdeSuelo)
        {
            ManejarDobleSalto(distancia, distanciaHorizontal, diferenciaAltura);
            return;
        }

        dobleSaltoDisponible = false;
        if (timerSalto > 0f) return;

        bool objetivoAlto = objetivo.position.y > transform.position.y + 0.65f;
        bool objetivoCerca = distanciaHorizontal < distanciaIdeal + 1f;
        bool deberiaEsquivar = objetivoCerca && Random.value < chanceEsquivar;
        bool deberiaPerseguirAlto = objetivoAlto
            && distanciaHorizontal <= distanciaIdeal + 1.5f
            && Random.value < Mathf.Max(chanceDobleSalto, 0.25f);

        if (deberiaEsquivar || deberiaPerseguirAlto)
        {
            Saltar(fuerzaSalto);
            dobleSaltoDisponible = true;
            timerDobleSalto = delayDobleSalto;
            timerSalto = intervaloSalto;
        }
    }

    private void ManejarDobleSalto(float distancia, float distanciaHorizontal, float diferenciaAltura)
    {
        if (!dobleSaltoDisponible) return;
        if (timerDobleSalto > 0f) return;
        if (!ConvieneDobleSalto(distancia, distanciaHorizontal, diferenciaAltura))
        {
            timerDobleSalto = 0.25f;
            return;
        }

        dobleSaltoDisponible = false;
        Saltar(fuerzaSalto, true);
    }

    private bool ConvieneDobleSalto(float distancia, float distanciaHorizontal, float diferenciaAltura)
    {
        if (rb == null || objetivo == null) return false;

        bool cayendo = rb.linearVelocity.y <= 0.1f;
        bool objetivoCerca = distanciaHorizontal <= distanciaIdeal + 1.5f;
        bool objetivoMasAlto = objetivo.position.y > transform.position.y + 0.75f;
        bool puedeAlcanzar = distancia <= rangoDeteccion;
        bool alturaUtil = diferenciaAltura <= rangoVerticalGolpe * 2f;

        switch (GameModeManager.DificultadActual)
        {
            case GameModeManager.Dificultad.Facil:
                if (!(cayendo && objetivoCerca && alturaUtil)) return false;
                break;

            case GameModeManager.Dificultad.Medio:
                if (!(objetivoCerca && (cayendo || objetivoMasAlto))) return false;
                break;

            case GameModeManager.Dificultad.Dificil:
                if (!((objetivoCerca && cayendo) || objetivoMasAlto || (puedeAlcanzar && cayendo && alturaUtil))) return false;
                break;
        }

        return Random.value < chanceDobleSalto;
    }

    private void Saltar(float fuerza, bool esDobleSalto = false)
    {
        if (rb == null) return;

        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0, fuerza), ForceMode2D.Impulse);

        if (esDobleSalto)
            ReproducirEfectoDobleSalto();
    }

    private void ReproducirEfectoDobleSalto()
    {
        if (particulasDobleSalto == null) return;

        Vector3 posicion = transform.position + offsetEfectoDobleSalto;
        GameObject efecto = Instantiate(particulasDobleSalto, posicion, Quaternion.identity);
        if (duracionEfectoDobleSalto > 0f)
        {
            Destroy(efecto, duracionEfectoDobleSalto);
        }
    }

    private void ManejarAtaque(float distanciaHorizontal, float diferenciaAltura)
    {
        if (estaAtacando) return;
        if (timerAtaque > 0f) return;
        if (!EstaEnRangoDeAtaque(distanciaHorizontal, diferenciaAltura)) return;
        if (Random.value > chanceAtaque)
        {
            timerAtaque = Mathf.Min(0.35f, intervaloAtaque * 0.35f);
            return;
        }

        estaAtacando = true;
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        anim?.SetBool("isRunning", false);
        anim?.SetTrigger("Attack");
        Invoke(nameof(Golpear), 0.2f);
        timerAtaque = intervaloAtaque;
    }

    private bool EstaEnRangoDeAtaque(float distanciaHorizontal, float diferenciaAltura)
    {
        return distanciaHorizontal <= rangoGolpe && diferenciaAltura <= Mathf.Max(rangoVerticalGolpe, rangoGolpe);
    }

    private void ActualizarCacheObjetivo()
    {
        if (objetivoCacheado == objetivo) return;

        objetivoCacheado = objetivo;
        rbObjetivo = objetivo != null ? objetivo.GetComponent<Rigidbody2D>() : null;
    }

    private Vector2 ObtenerPosicionObjetivoPredicha()
    {
        Vector2 posicion = objetivo.position;
        if (rbObjetivo != null)
            posicion += rbObjetivo.linearVelocity * tiempoPrediccion;

        return posicion;
    }

    private void IgnorarColisionConObjetivo()
    {
        if (objetivo == null) return;

        Collider2D colPropio = GetComponent<Collider2D>();
        Collider2D colObjetivo = objetivo.GetComponent<Collider2D>();

        if (colPropio != null && colObjetivo != null)
            Physics2D.IgnoreCollision(colPropio, colObjetivo, true);
    }

    private void Golpear()
    {
        if (objetivo == null || !objetivo.gameObject.activeSelf)
        {
            estaAtacando = false;
            return;
        }

        float distanciaHorizontal = Mathf.Abs(objetivo.position.x - transform.position.x);
        float diferenciaAltura = Mathf.Abs(objetivo.position.y - transform.position.y);
        if (EstaEnRangoDeAtaque(distanciaHorizontal, diferenciaAltura))
        {
            objetivo.GetComponent<PlayerHealth>()?.RecibirDanio(danio);
            Debug.Log($"[CPU] Golpe conectado a {objetivo.name}");
        }

        estaAtacando = false;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Suelo") estaEnSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.name == "Suelo") estaEnSuelo = false;
    }

    private bool EstaTocandoSuelo()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;
        Bounds bounds = col.bounds;
        Vector2 origen = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, 0.12f);
        return hit.collider != null && hit.collider.gameObject.name == "Suelo";
    }
}
