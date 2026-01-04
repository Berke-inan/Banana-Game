using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ModularPlayerController : MonoBehaviour
{
    [Header("Input Referansları (Sürükle-Bırak)")]
    // Buraya resimdeki o mavi şimşekleri sürükleyeceğiz
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference lookAction; // Mouse hareketi için
    public InputActionReference switchCameraAction; // Yeni oluşturduğun V tuşu
    public InputActionReference interactAction;
    public Animator characterAnimator;

    [Header("Hareket Ayarları")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -15.0f;
    public float rotationSpeed = 15f;
    [Header("Zemin Kontrolü (Gelişmiş)")]
    public LayerMask zeminKatmani; // Hangi objeler 'Zemin' sayılır? (Default, Ground vb.)
    public float zeminYaricapi = 0.4f; // Karakterin genişliğinden (Radius) biraz az olsun
    public float zeminOfseti = 0.1f;

    [Header("Kamera Ayarları")]
    public CinemachineCamera tpsCamera;
    public CinemachineCamera fpsCamera;
    public Transform cameraTarget; // Kafa hizasındaki obje

    [Header("Etkileşim Ayarları")]
    public float etkilesimMesafesi = 3f; // Ne kadar yakından basabilsin?
    public LayerMask etkilesimKatmani;   // Hangi objelere basabilir?

    // Private Değişkenler
    private CharacterController _controller;
    private Transform _mainCameraTransform;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isFpsMode = false;
    private Vector3 _impact = Vector3.zero;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (Camera.main != null) _mainCameraTransform = Camera.main.transform;

        // V tuşuna basılma olayını dinle
        switchCameraAction.action.performed += ctx => ToggleCameraMode();

        // Zıplama olayını dinle
        jumpAction.action.performed += ctx => Jump();

        interactAction.action.performed += ctx => TryInteract();
    }

    // Inputları Aktif/Pasif yapma (Zorunlu)
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
        switchCameraAction.action.Enable();
        interactAction.action.Enable(); // YENİ
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        lookAction.action.Disable();
        switchCameraAction.action.Disable();
        interactAction.action.Disable();
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
        HandleImpact();
    }


    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // Yere doğru vurmasın, hafif yukarı sektirsin
        _impact += dir.normalized * force / 3.0f; // Force miktarını karaktere uygula
    }

    private void HandleImpact()
    {
        // Darbe kuvveti varsa uygula ve zamanla azalt (Sürtünme gibi)
        if (_impact.magnitude > 0.2f)
        {
            _controller.Move(_impact * Time.deltaTime);
            _impact = Vector3.Lerp(_impact, Vector3.zero, 5 * Time.deltaTime);
        }
    }
    private void HandleMovement()
    {

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Hareket vektörü (input) 0 değilse (yani tuşlara basılıyorsa) yürüyordur
        bool yuruyorMu = input != Vector2.zero;

        // Animatöre haber veriyoruz
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("isRunning", yuruyorMu);
        }
        // 1. Inputtan veriyi oku (Vector2 olarak gelir: x=sağ/sol, y=ileri/geri)
        input = moveAction.action.ReadValue<Vector2>();

        if (input == Vector2.zero) return;

        // 2. Kameranın baktığı yöne göre yön belirle
        Vector3 forward = _mainCameraTransform.forward;
        Vector3 right = _mainCameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * input.y + right * input.x).normalized;

        // 3. Karakteri hareket ettir
        _controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // 4. Dönüş Mantığı (TPS ise dön, FPS ise zaten kafası dönüyor)
        if (!_isFpsMode && moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // FPS modundaysa gövdeyi kameranın baktığı yere çevir
        if (_isFpsMode)
        {
            transform.rotation = Quaternion.Euler(0, _mainCameraTransform.eulerAngles.y, 0);
        }
    }

    private void TryInteract()
    {
        Debug.Log("1. [INPUT] E tuşu algılandı. Işın yollanıyor...");

        // Işını oluştur
        Ray ray = new Ray(_mainCameraTransform.position, _mainCameraTransform.forward);
        RaycastHit hit;

        // GÖRSEL HATA AYIKLAMA: 
        // Oyundayken Scene penceresine geçersen karakterden çıkan kırmızı bir çizgi göreceksin.
        // Bu çizgi tam butona değiyor mu kontrol et.
        Debug.DrawRay(ray.origin, ray.direction * etkilesimMesafesi, Color.red, 2f);

        // Raycast atıyoruz
        if (Physics.Raycast(ray, out hit, etkilesimMesafesi, etkilesimKatmani))
        {
            Debug.Log("2. [FİZİK] Işın bir şeye çarptı: " + hit.collider.gameObject.name);

            // Çarptığı objede script var mı?
            ButtonInteract buton = hit.collider.GetComponent<ButtonInteract>();

            if (buton != null)
            {
                Debug.Log("3. [BAŞARI] ButtonInteract scripti bulundu! Tetikleniyor...");
                buton.ButonaBasildi();
            }
            else
            {
                // Belki scripti yanlış yere (parent/child) koymuşuzdur
                Debug.LogWarning("4. [HATA] Işın '" + hit.collider.name + "' objesine çarptı ama üzerinde 'ButtonInteract' scripti bulamadı!");
            }
        }
        else
        {
            Debug.LogWarning("2. [BOŞLUK] Işın hiçbir şeye çarpmadı. Mesafe yetmiyor olabilir veya Layer ayarı yanlış.");
        }
    }

    private void HandleGravity()
    {
        _isGrounded = ZemindeMiyim();

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Yerdeyken hızı sabitle (hafif baskı)
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private bool ZemindeMiyim()
    {
        // 1. Başlangıç noktası: Ayakların çok az yukarısı (içinden başlamasın diye)
        Vector3 baslangic = transform.position + Vector3.up * zeminOfseti;

        // 2. SphereCast atıyoruz (Küre fırlatma)
        // Mantık: (Nereden, Yarıçapı Ne, Hangi Yöne, Çarpan Bilgisi, Ne Kadar Uzağa, Hangi Katmanlar)
        bool yereCarpti = Physics.SphereCast(baslangic, zeminYaricapi, Vector3.down, out RaycastHit hit, 0.2f, zeminKatmani);

        return yereCarpti;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 baslangic = transform.position + Vector3.up * zeminOfseti;
        Gizmos.DrawWireSphere(baslangic + Vector3.down * 0.2f, zeminYaricapi);
    }

    private void ToggleCameraMode()
    {
        _isFpsMode = !_isFpsMode;

        if (_isFpsMode)
        {
            fpsCamera.Priority = 20; // FPS öne geçer
            tpsCamera.Priority = 10;
        }
        else
        {
            tpsCamera.Priority = 20; // TPS öne geçer
            fpsCamera.Priority = 10;
        }
    }

    // Bu fonksiyonu ModularPlayerController.cs içinde en alta (son parantezden önce) ekle
    public void FizigiSifirla()
    {
        _velocity = Vector3.zero; // Düşüş hızını sıfırla
        _impact = Vector3.zero;   // Geri tepme etkisini sıfırla
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Çarptığımız şeyin Rigidbody'si var mı?
        Rigidbody body = hit.collider.attachedRigidbody;

        // Rigidbody yoksa veya Kinematic ise (hareket etmiyorsa) itmeye çalışma
        if (body == null || body.isKinematic) return;

        // Eğer objenin tepesine çıktıysak (ezmeyelim diye) itme
        if (hit.moveDirection.y < -0.3f) return;

        // İtme yönünü belirle (Biz nereye gidiyorsak oraya)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Güç uygula (5.0f itme gücüdür, artırabilirsin)
        // Unity 6 kullanıyorsan 'linearVelocity', eskiyse 'velocity' yaz.
        body.linearVelocity = pushDir * 5.0f;
    }
}