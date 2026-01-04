using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public float acilmaAcisi = 90f; // Kapı kaç derece açılacak?
    public float acilmaHizi = 2f;

    private bool _acikMi = false;
    private Quaternion _kapaliRotasyon;
    private Quaternion _acikRotasyon;

    private void Start()
    {
        _kapaliRotasyon = transform.rotation;
        // Mevcut rotasyonun üzerine 90 derece ekliyoruz (Y ekseninde)
        _acikRotasyon = _kapaliRotasyon * Quaternion.Euler(0, acilmaAcisi, 0);
    }

    public void kapiyiTetikle() // Bu fonksiyonu dışarıdan çağıracağız
    {
        _acikMi = !_acikMi; // Açıksa kapat, kapalıysa aç
        StopAllCoroutines(); // Önceki hareket bitmediyse durdur
        StartCoroutine(HareketEt(_acikMi ? _acikRotasyon : _kapaliRotasyon));
    }

    IEnumerator HareketEt(Quaternion hedef)
    {
        while (Quaternion.Angle(transform.rotation, hedef) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, hedef, Time.deltaTime * acilmaHizi);
            yield return null;
        }
        transform.rotation = hedef;
    }
}