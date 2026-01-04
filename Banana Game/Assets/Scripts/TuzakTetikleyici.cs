using UnityEngine;

public class TuzakTetikleyici : MonoBehaviour
{
    public Rigidbody dolapRb;

    // Sağdan sola gelmesi için X'e -1 veriyoruz (Sahneye göre değişebilir)
    public Vector3 itmeYonu = new Vector3(-1, 0, 0);

    // Sürtünmeyi yenip hızlı gitmesi için gücü çok artırıyoruz
    public float itmeGucu = 5000f;

    private bool _calistiMi = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_calistiMi && other.GetComponent<ModularPlayerController>() != null)
        {
            _calistiMi = true;

            dolapRb.isKinematic = false; // Havada durmayı bırak

            // Impulse moduyla ani ve sert bir itiş yapıyoruz
            dolapRb.AddForce(itmeYonu * itmeGucu, ForceMode.Impulse);

            Debug.Log("Tuzak Çalıştı! Dolap fırlatıldı.");
        }
    }
}