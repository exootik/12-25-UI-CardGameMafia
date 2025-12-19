using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
    //, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Stats Display")]
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text damageText;

    [Header("State Display - Icons")]
    [SerializeField] private Image stateIcon;
    [SerializeField] private Sprite movementIcon;
    [SerializeField] private Sprite attackIcon;

    [Header("Enemy Visual")]
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform bodyRect;

    public event Action OnEnemyClicked;

    public void UpdateView(EnemyRuntime enemyRuntime)
    {
        lifeText.text = enemyRuntime.Life.ToString();
        damageText.text = enemyRuntime.Damage.ToString();
    }

    /// <summary>
    /// Met à jour l'icône de state en fonction de la prochaine action
    /// </summary>
    public void SetStateIcon(EnemyRuntime.TurnDecision newTurnDecision)
    {
        if (stateIcon == null)
        {
            Debug.LogWarning("[EnemyView] stateIcon is null!");
            return;
        }

        // Déterminer quelle icône afficher
        switch (newTurnDecision.action)
        {
            case EnemyState.Moving:
                // Afficher l'icône de déplacement
                if (movementIcon != null)
                {
                    stateIcon.sprite = movementIcon;
                    stateIcon.enabled = true;
                    Debug.Log("[EnemyView] Displaying MOVEMENT icon");
                }
                else
                {
                    Debug.LogWarning("[EnemyView] movementIcon sprite is not assigned!");
                    stateIcon.enabled = false;
                }
                break;

            case EnemyState.Attacking:
                // Afficher l'icône d'attaque
                if (attackIcon != null)
                {
                    stateIcon.sprite = attackIcon;
                    stateIcon.enabled = true;
                    Debug.Log("[EnemyView] Displaying ATTACK icon");
                }
                else
                {
                    Debug.LogWarning("[EnemyView] attackIcon sprite is not assigned!");
                    stateIcon.enabled = false;
                }
                break;

            case EnemyState.Stunned:
                //stateIcon.enabled = false;
                Debug.Log("[EnemyView] Enemy stunned - no icon");
                break;

            case EnemyState.Idle:
                //stateIcon.enabled = false;
                break;

            case EnemyState.Dead:
                stateIcon.enabled = false;
                break;

            default:
                //stateIcon.enabled = false;
                break;
        }
    }

    public void OnStateChanged(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Moving:
                PlayAnimation_StartMoving();
                break;
            case EnemyState.Idle:
                PlayAnimation_StopMoving();
                break;
            case EnemyState.Attacking:
                PlayAnimation_Attack();
                break;
            case EnemyState.Stunned:
                break;
            case EnemyState.Dead:
                PlayAnimation_Death();
                break;
        }
    }

    public void OnHurt()
    {
        Debug.Log("[EnemyView] Hurt Enemy");
        PlayAnimation_Hurt();
    }

    public void OnDeath()
    {
        Debug.Log("[EnemyView] Mort de l'enemy");
        PlayAnimation_Death();
    }

    public void SetFacing(bool faceRight)
    {
        if (bodyRect == null) return;
        Vector3 s = bodyRect.localScale;
        s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
        bodyRect.localScale = s;
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    Debug.Log("[EnemyView] OnPointerClick");

    //    OnEnemyClicked?.Invoke();
    //}

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    Debug.Log("[EnemyView] OnPointerEnter");
    //    //if (animator != null) animator.SetBool("IsHover", true);
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    Debug.Log("[EnemyView] OnPointerExit");
    //    //if (animator != null) animator.SetBool("IsHover", false);
    //}

    private void PlayAnimation_StartMoving() { if (animator != null) animator.SetBool("IsWalking", true); }
    private void PlayAnimation_StopMoving() { if (animator != null) animator.SetBool("IsWalking", false); }
    private void PlayAnimation_Attack() { if (animator != null) animator.SetTrigger("Attack"); }
    private void PlayAnimation_Death() { if (animator != null) animator.SetTrigger("Death"); }
    private void PlayAnimation_Hurt() { if (animator != null) animator.SetTrigger("Hurt"); }

    public Vector3 GetBodyWorldPosition()
    {
        if (bodyRect != null)
        {
            return bodyRect.transform.position;
        }
        return transform.position;
    }
}
