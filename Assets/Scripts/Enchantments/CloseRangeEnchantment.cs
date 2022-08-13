using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enchantments/Close Range")]
public class CloseRangeEnchantment : Enchantment
{
    [SerializeField] private float startScale = 1.5f;
    [SerializeField] private float endScale = 0f;

    public override void intialize(GameObject gameObject)
    {
        base.intialize(gameObject);

        GameEvents.instance.onFire += addCloseRangeEffect;
    }

    // Basic deinitalize just nullifies current attatched gameobject
    public override void unintialize()
    {
        GameEvents.instance.onFire -= addCloseRangeEffect;
        base.unintialize();
    }

    private void addCloseRangeEffect(Weapon weapon, Projectile projectile) {
        if (this.weapon == weapon) {
            projectile.StartCoroutine(decreaseSize(1f, projectile));
        }
    }

    private IEnumerator decreaseSize(float duration, Projectile projectile) {
        float elapsedTime = 0f;

        // Start swing
        while (elapsedTime < duration)
        {
            // Slerp position
            projectile.transform.localScale = Vector3.Lerp(Vector3.one * startScale,
                                                Vector3.one * endScale, 
                                                elapsedTime / duration);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
