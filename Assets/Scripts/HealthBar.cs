using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float timeToDrain = 0.25f;
    [SerializeField] private Gradient healthBarGradient;
    [SerializeField] private Image image;
    [SerializeField] [Range(0,1)] private float target = 1f;
    private Color newHealthBarColor;

    private void Start()
    {
        image.color = healthBarGradient.Evaluate(target);
        CheckHealthBarGradientAmount();
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth, bool instant = false)
    {
        target = currentHealth / maxHealth;
        if (!image.isActiveAndEnabled) return;
        CheckHealthBarGradientAmount();
        if (!instant) 
            StartCoroutine(DrainHealthBar());
        else
        {
            image.fillAmount = target;
            image.color = newHealthBarColor;
        }
    }

    private IEnumerator DrainHealthBar()
    {;
        float fillAmount = image.fillAmount;
        Color currentColor = image.color;

        float elapsedTime = 0f;
        while(elapsedTime < timeToDrain) 
        {
            elapsedTime += Time.deltaTime;

            image.fillAmount = Mathf.Lerp(fillAmount, target, (elapsedTime / timeToDrain));

            image.color = Color.Lerp(currentColor, newHealthBarColor, (elapsedTime / timeToDrain));
            yield return null;
        }
    }

    private void CheckHealthBarGradientAmount()
    {
        newHealthBarColor = healthBarGradient.Evaluate(target);
    }

    private void OnDrawGizmos()
    {
        if (!image.IsActive()) return;
        image.fillAmount = target;
        image.color = healthBarGradient.Evaluate(image.fillAmount);
    }
}
