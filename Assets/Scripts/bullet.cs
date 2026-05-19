using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 2f;
    private int damage = 1;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetDamage(int playerDamage)
    {
        damage = Mathf.Max(1, Mathf.RoundToInt(playerDamage * 0.5f));
    }

    void OnTriggerEnter(Collider other)
    {

        Enemy enemigo = other.GetComponent<Enemy>();
        if (enemigo != null)
        {
            enemigo.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        EnemyTarget enemigoTarget = other.GetComponent<EnemyTarget>();
        if (enemigoTarget != null)
        {
            enemigoTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.CompareTag("Escenario") || other.gameObject.layer == 0)
        {
            Destroy(gameObject);
        }
    }
}