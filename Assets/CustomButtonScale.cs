using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CustomButtonScale : CustomButtonBase
{
    private const float OriginalScale = 1.0f;
    private Vector3 _originalTextPos;

    [SerializeField] private float toScale;
    [SerializeField] private float duration;
    [SerializeField] private float moveDistance = 1f;

    private GameObject _text;
    private GameObject _ball;
    private Animator _ballAnim;

    private void Start()
    {
        _text = transform.GetChild(0).gameObject;
        _ball = transform.GetChild(1).gameObject;
        _ballAnim = _ball.GetComponent<Animator>();

        _originalTextPos = _text.transform.localPosition;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        _ballAnim.SetTrigger("Show");

        _text.transform.DOScale(toScale, duration).SetEase(Ease.InOutSine);
        _text.transform.DOLocalMoveX(_originalTextPos.x + moveDistance, duration).SetEase(Ease.InOutSine);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _ballAnim.SetTrigger("Mask");

        _text.transform.DOScale(OriginalScale, duration).SetEase(Ease.InOutSine);
        _text.transform.DOLocalMoveX(_originalTextPos.x, duration).SetEase(Ease.InOutSine);
    }
}
