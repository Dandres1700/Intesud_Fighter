using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController2 : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public float danio = 10f;
    public float rangoGolpe = 2.5f;
    public float limiteIzquierdo = -8f;
    public float limiteDerecho   = 8f;
    public Transform jugador1;

    [Header("Salto")]
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float fuerzaDobleSalto = 7f;

    [Header("Particulas doble salto")]
    [FormerlySerializedAs("efectoDobleSaltoPrefab")]
    public GameObject particulasDobleSalto;
    [SerializeField] private Vector3 offsetEfectoDobleSalto = new Vector3(0f, -0.4f, 0f);
    [SerializeField] private float duracionEfectoDobleSalto = 1f;
    [SerializeField] private float escalaEfectoDobleSalto = 0.3f;

    [Header("Mando PS4 - Jugador 2")]
    public int numeroJoystick = 2;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private bool estaEnSuelo  = false;
    private bool estaAtacando = false;
    private bool stickArribaPresionado = false;
    private bool saltoBloqueado        = false;
    private bool inputBloqueado        = false;
    private bool dobleSaltoDisponible  = false;

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        sr.flipX = true;
        IgnorarColisionEntreJugadores();
    }

    void OnEnable()  => PrepararNuevaRonda();
    void OnDisable() => CancelInvoke();

    public void BloquearInput(bool bloquear)
    {
        inputBloqueado = bloquear;
        if (bloquear && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool("isRunning", false);
        }
    }

    public void PrepararNuevaRonda()
    {
        estaAtacando          = false;
        estaEnSuelo           = false;
        inputBloqueado        = false;
        dobleSaltoDisponible  = false;
        stickArribaPresionado = false;
        saltoBloqueado        = false;
        CancelInvoke();

        if (rb != null)
        {
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale    = 1f;
            rb.WakeUp();
        }

        if (anim != null) anim.Rebind();
        IgnorarColisionEntreJugadores();
    }

    void IgnorarColisionEntreJugadores()
    {
        if (jugador1 == null) return;
        Collider2D colPropio = GetComponent<Collider2D>();
        Collider2D colJ1     = jugador1.GetComponent<Collider2D>();
        if (colPropio != null && colJ1 != null)
            Physics2D.IgnoreCollision(colPropio, colJ1, true);
    }

    void Update()
    {
        if (inputBloqueado) return;

        estaEnSuelo = EstaTocandoSuelo();

        float movimiento = 0f;
        if (Input.GetKey(GetPlayer2LeftKey()))  movimiento = -1f;
        if (Input.GetKey(GetPlayer2RightKey())) movimiento =  1f;

        if (PadInput.HasGamepad(numeroJoystick))
        {
            float ejePS4 = PadInput.ReadHorizontal(numeroJoystick);
            if (Mathf.Abs(ejePS4) > 0.25f)
                movimiento = ejePS4;
        }

        if (!estaAtacando)
        {
            rb.linearVelocity = new Vector2(movimiento * velocidad, rb.linearVelocity.y);
            anim.SetBool("isRunning", Mathf.Abs(movimiento) > 0.1f);

            if (movimiento > 0) sr.flipX = false;
            if (movimiento < 0) sr.flipX = true;
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, limiteIzquierdo, limiteDerecho);
        transform.position = pos;

        bool saltoTeclado = Input.GetKeyDown(GetPlayer2JumpKey());
        bool saltoPS4 = false;

        if (PadInput.HasGamepad(numeroJoystick))
        {
            float ejeV = PadInput.ReadVertical(numeroJoystick);
            bool cruzPS4 = PadInput.SouthPressedThisFrame(numeroJoystick);
            bool stickArribaPS4 = ejeV > 0.8f && !stickArribaPresionado;

            if (ejeV <= 0.5f && !cruzPS4)
            {
                stickArribaPresionado = false;
                saltoBloqueado        = false;
            }

            if (!saltoBloqueado && (cruzPS4 || stickArribaPS4))
            {
                saltoPS4              = true;
                saltoBloqueado        = true;
                stickArribaPresionado = true;
            }
        }

        bool puedeSaltarDesdeSuelo = estaEnSuelo && rb.linearVelocity.y <= 0.01f;
        bool quiereSaltar = saltoTeclado || saltoPS4;

        if (quiereSaltar)
        {
            if (puedeSaltarDesdeSuelo)
            {
                Saltar();
                dobleSaltoDisponible = true;
            }
            else if (dobleSaltoDisponible)
            {
                Saltar(true);
                dobleSaltoDisponible = false;
            }
        }
        else if (puedeSaltarDesdeSuelo)
        {
            dobleSaltoDisponible = false;
        }

        rb.gravityScale = (rb.linearVelocity.y < 0 && !estaEnSuelo) ? multiplicadorCaida : 1f;

        bool ataqueTeclado = Input.GetKeyDown(GetPlayer2AttackKey());
        bool ataquePS4     = PadInput.WestPressedThisFrame(numeroJoystick);

        if ((ataqueTeclado || ataquePS4) && !estaAtacando)
            Golpear();
    }

    void Golpear()
    {
        estaAtacando = true;
        anim.SetTrigger("Attack");

        if (jugador1 != null && jugador1.gameObject.activeSelf)
        {
            float distancia = Vector2.Distance(transform.position, jugador1.position);
            if (distancia <= rangoGolpe)
            {
                jugador1.GetComponent<PlayerHealth>().RecibirDanio(danio);
                Debug.Log("Jugador2 golpeó a Jugador1!");
            }
        }

        Invoke("ResetAtaque", 0.5f);
    }

    void ResetAtaque() => estaAtacando = false;
    public void RecibirDanio() => anim.SetTrigger("Hurt");

    private void Saltar(bool esDobleSalto = false)
    {
        rb.gravityScale   = 1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        float fuerza = esDobleSalto ? fuerzaDobleSalto : fuerzaSalto;
        rb.AddForce(new Vector2(0, fuerza), ForceMode2D.Impulse);

        if (esDobleSalto)
            ReproducirEfectoDobleSalto();
    }

    private void ReproducirEfectoDobleSalto()
    {
        if (particulasDobleSalto == null) return;

        Vector3 posicion = transform.position + offsetEfectoDobleSalto;
        GameObject efecto = Instantiate(particulasDobleSalto, posicion, Quaternion.identity);
        efecto.transform.localScale = Vector3.one * escalaEfectoDobleSalto;
        if (duracionEfectoDobleSalto > 0f)
            Destroy(efecto, duracionEfectoDobleSalto);
    }

    void OnCollisionEnter2D(Collision2D col) { if (col.gameObject.name == "Suelo") estaEnSuelo = true; }
    void OnCollisionExit2D(Collision2D col)  { if (col.gameObject.name == "Suelo") estaEnSuelo = false; }

    bool EstaTocandoSuelo()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;
        Bounds bounds = col.bounds;
        Vector2 origen = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, 0.12f);
        return hit.collider != null && hit.collider.gameObject.name == "Suelo";
    }

    KeyCode GetPlayer2LeftKey()   => (KeyCode)PlayerPrefs.GetInt("Player2LeftKey",   (int)KeyCode.LeftArrow);
    KeyCode GetPlayer2RightKey()  => (KeyCode)PlayerPrefs.GetInt("Player2RightKey",  (int)KeyCode.RightArrow);
    KeyCode GetPlayer2JumpKey()   => (KeyCode)PlayerPrefs.GetInt("Player2JumpKey",   (int)KeyCode.UpArrow);
    KeyCode GetPlayer2AttackKey() => (KeyCode)PlayerPrefs.GetInt("Player2AttackKey", (int)KeyCode.M);

    static KeyCode JoystickButton(int joystick, int boton)
    {
        int baseIndex = (int)KeyCode.Joystick1Button0 + (joystick - 1) * 20;
        return (KeyCode)(baseIndex + boton);
    }
}