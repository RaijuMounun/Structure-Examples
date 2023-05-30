using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] float outOfBoundsThreshold = 40f;
    [SerializeField] float itemSpacing = 30f;
    float childWidth = 125f;
    float childHeight = 125f;
    ScrollRect scrollRect;
    Transform _transform;


    Vector2 _lastDragPosition;
    bool _positiveDrag;
    int _childCount = 0;
    float _height = 0f;

    private void Awake()
    {
        var child = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().rect;
        childWidth = child.width;
        childHeight = child.height;
        scrollRect = GetComponent<ScrollRect>();
        _transform = GetComponent<RectTransform>();
    }

    IEnumerator Start()
    {
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        _childCount = scrollRect.content.childCount;
        _height = Screen.height;
        scrollRect.content.localPosition = Vector3.up * _height * 2f;
        
        yield return new WaitForSeconds(1f);
        var counter = 0;
        while (counter < 300)
        {
            _positiveDrag = true;
            HandleScrollRectValueChanged(Vector2.zero);
            counter++;
            scrollRect.content.transform.Translate(Time.deltaTime * 3f * Vector2.up);
            yield return null;
        }
        
        
    }

    void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(HandleScrollRectValueChanged);
    }
    void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(HandleScrollRectValueChanged);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var newPosition = eventData.position;
        _positiveDrag = newPosition.y > _lastDragPosition.y;
        _lastDragPosition = newPosition;
    }

    bool ReachedThreshold(Transform item)
    {
        var positiveYThreshold = _transform.position.y + _height * .5f + outOfBoundsThreshold;
        var negativeYThreshold = _transform.position.y - _height * .5f - outOfBoundsThreshold;
        return _positiveDrag
            ? item.position.y - childWidth * .5f > positiveYThreshold
            : item.position.y + childWidth * .5f < negativeYThreshold;
    }
    void HandleScrollRectValueChanged(Vector2 value)
    {
        var currentItemIndex = _positiveDrag ? _childCount - 1 : 0;
        var currentItem = scrollRect.content.GetChild(currentItemIndex);
        
        if (!ReachedThreshold(currentItem)) return;
        
        var endItemIndex= _positiveDrag ? 0 : _childCount - 1;
        var endItem = scrollRect.content.GetChild(endItemIndex);
        Vector2 newPosition = endItem.position;

        if (_positiveDrag)
        {
            newPosition.y = endItem.position.y - childHeight*1.5f + itemSpacing;
        }else
        {
            newPosition.y = endItem.position.y + childHeight*1.5f - itemSpacing;
        }

        currentItem.position = newPosition;
        currentItem.SetSiblingIndex(endItemIndex);
    }
}
