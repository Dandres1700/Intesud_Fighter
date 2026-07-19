using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public float danio = 10f;
    public float rangoGolpe = 2.5f;
    public Transform jugador2;

    [Header("Salto")]
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float fuerzaDobleSalto = 7f;

    [Header("Particulas doble salto")]
    [FormerlySerializedAs("efectoDobleSaltoPrefab")]
    public GameObject particulasDobleSalto;
    [SerializeField] private Vector3 offsetEfectoDobleSalto = new Vector3(0f, -0.4f, 0f);
    [SerializeField] private float duracionEfectoDobleSalto = 1f;
    [SerializeField] private float escalaEfectoDobleSalto = 0.3f;

    [Header("Mando PS4 - Jugador 1")]
    public int numeroJoystick = 1;

    private Rigidbody2D rb;
    private bool estaEnSuelo = false;
    private Animator anim;
    private SpriteRenderer sr;
    private bool puedeAtacar = true;
    private bool stickArribaPresionado = false;
    private bool saltoBloqueado = false;
    private bool inputBloqueado = false;
    private bool dobleSaltoDisponible = false;

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
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
        CancelInvoke();
        puedeAtacar           = true;
        estaEnSuelo           = false;
        inputBloqueado        = false;
        dobleSaltoDisponible  = false;
        stickArribaPresionado = false;
        saltoBloqueado        = false;

        if (rb != null)
        {
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale    = 1f;
            rb.WakeUp();
        }

        if (anim != null) anim.Rebind();
    }

    void Update()
    {
        if (inputBloqueado) return;

        estaEnSuelo = EstaTocandoSuelo();

        float movimiento = 0f;
        if (Input.GetKey(GetPlayer1LeftKey()))  movimiento = -1f;
        if (Input.GetKey(GetPlayer1RightKey())) movimiento =  1f;

        if (PadInput.HasGamepad(numeroJoystick))
        {
            float ejePS4 = PadInput.ReadHorizontal(numeroJoystick);
            if (Mathf.Abs(ejePS4) > 0.25f)
                movimiento = ejePS4;
        }

        rb.linearVelocity = new Vector2(movimiento * velocidad, rb.linearVelocity.y);

        if (movimiento > 0) sr.flipX = false;
        else if (movimiento < 0) sr.flipX = true;

        anim.SetBool("isRunning", Mathf.Abs(movimiento) > 0.1f);

        bool saltoTeclado = Input.GetKeyDown(GetPlayer1JumpKey());
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

        bool ataqueTeclado = Input.GetKeyDown(GetPlayer1AttackKey());
        bool ataquePS4     = PadInput.WestPressedThisFrame(numeroJoystick);

        if ((ataqueTeclado || ataquePS4) && puedeAtacar)
        {
            puedeAtacar = false;
            anim.SetTrigger("Attack");
            Invoke("Golpear", 0.2f);
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        transform.position = pos;
    }

    void Golpear()
    {
        if (jugador2 == null || !jugador2.gameObject.activeSelf)
        {
            puedeAtacar = true;
            return;
        }

        float distancia = Vector2.Distance(transform.position, jugador2.position);
        if (distancia <= rangoGolpe)
        {
            jugador2.GetComponent<PlayerHealth>().RecibirDanio(danio);
            Debug.Log("Golpe conectado del Jugador 1!");
        }

        puedeAtacar = true;
    }

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

    KeyCode GetPlayer1LeftKey()   => (KeyCode)PlayerPrefs.GetInt("Player1LeftKey",   (int)KeyCode.A);
    KeyCode GetPlayer1RightKey()  => (KeyCode)PlayerPrefs.GetInt("Player1RightKey",  (int)KeyCode.D);
    KeyCode GetPlayer1JumpKey()   => (KeyCode)PlayerPrefs.GetInt("Player1JumpKey",   (int)KeyCode.W);
    KeyCode GetPlayer1AttackKey() => (KeyCode)PlayerPrefs.GetInt("Player1AttackKey", (int)KeyCode.F);

    static KeyCode JoystickButton(int joystick, int boton)
    {
        int baseIndex = (int)KeyCode.Joystick1Button0 + (joystick - 1) * 20;
        return (KeyCode)(baseIndex + boton);
    }
}