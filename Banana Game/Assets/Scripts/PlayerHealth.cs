using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxCan = 3;
    private int _guncelCan;

    [Header("Referanslar")]
    public Transform spawnPoint;
    private ModularPlayerController _playerController;
    private CharacterController _charController;

    private Renderer[] _renderers;
    private List<Color> _orijinalRenkler = new List<Color>();

    private void Start()
    {
        _guncelCan = maxCan;
        _playerController = GetComponent<ModularPlayerController>();
        _charController = GetComponent<CharacterController>();

        // Renkleri kaydet
        _renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in _renderers)
        {
            _orijinalRenkler.Add(r.material.color);
        }

        // OYUN BAŞLAR BAŞLAMAZ SPAWN NOKTASINA GİT
        Işınlan();
    }

    public void HasarAl(Vector3 darbeYonu)
    {
        _guncelCan--; // Canı azalt

        // --- DEĞİŞİKLİK BURADA ---

        // 1. Gelen darbeYonu sadece yatay (X, Z) olabilir. Biz buna dikey (Y) ekliyoruz.
        // Vector3.up * 1.5f -> Havaya kaldırma miktarı. (Daha yükseğe uçsun istersen 2f veya 3f yap)
        Vector3 yeniFirlatmaYonu = (darbeYonu.normalized + Vector3.up * 1.8f).normalized;

        // 2. Şimdi bu yeni kavisli yönü PlayerController'a veriyoruz.
        // 60f -> İtme şiddeti. Uzağa gitmiyorsa bunu artır (80f, 100f gibi).
        _playerController.AddImpact(yeniFirlatmaYonu, 60f);

        // -------------------------

        StartCoroutine(KirmiziYanSone());

        if (_guncelCan <= 0)
        {
            OlveYenidenDog();
        }
    }

    private void OlveYenidenDog()
    {
        Debug.Log("ÖLDÜN! Yeniden doğuluyor...");
        _guncelCan = maxCan;
        Işınlan();
    }

    // Işınlanma işlemini ayrı bir fonksiyon yaptık ki hem Start'ta hem Ölünce kullanabilelim
    private void Işınlan()
    {
        if (spawnPoint == null) return;

        // 1. Karakter Kontrolcüsünü Kapat (Çatışmayı önler)
        _charController.enabled = false;

        // 2. Pozisyonu Değiştir
        transform.position = spawnPoint.position;

        // 3. FİZİĞİ SIFIRLA (İşte düşmeyi engelleyen sihirli kod!)
        _playerController.FizigiSifirla();

        // 4. Unity Fiziğini Zorla Güncelle
        Physics.SyncTransforms();

        // 5. Kontrolcüyü Geri Aç
        _charController.enabled = true;
    }

    IEnumerator KirmiziYanSone()
    {
        foreach (var r in _renderers) r.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].material.color = _orijinalRenkler[i];
        }
    }


    // Bu fonksiyonu PlayerHealth.cs içine ekle
    public void AnindaOldur()
    {
        _guncelCan = 0; // Canı direkt sıfırla
        OlveYenidenDog(); // Ölüm fonksiyonunu çalıştır
    }
}