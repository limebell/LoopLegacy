using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    /// <summary>
    /// 가상화된 스크롤 리스트 컴포넌트
    /// 고정된 수의 GameObject를 재사용하여 대량의 데이터를 효율적으로 표시
    /// </summary>
    public class VirtualizedScrollRect : MonoBehaviour
    {
        [Header("Scroll Rect Settings")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _content;
        [SerializeField] private GameObject _itemPrefab;
        
        [Header("Virtualization Settings")]
        [SerializeField] private int _maxElements = 6;
        [SerializeField] private float _itemHeight = 96f;
        
        // 가상화 관련 변수들
        private List<GameObject> _itemElements = new List<GameObject>();
        private List<object> _dataList = new List<object>();
        private int _firstVisibleIndex = 0;
        private int _lastVisibleIndex = 0;
        
        // 이벤트
        public event Action<GameObject, object, int> OnItemUpdate; // (element, data, index)
        
        private void Start()
        {
            InitializeElements();
            SetupScrollRect();
            UpdateContentSize();
            UpdateVisibleElements();
        }

        /// <summary>
        /// 가상화 요소들을 초기화
        /// </summary>
        private void InitializeElements()
        {
            for (int i = 0; i < _maxElements; i++)
            {
                var itemElement = Instantiate(_itemPrefab, _content.transform);
                
                // RectTransform 설정 - Content의 전체 width를 사용하도록 설정
                var rectTransform = itemElement.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);
                
                itemElement.SetActive(false);
                _itemElements.Add(itemElement);
            }
        }
        
        /// <summary>
        /// 스크롤 이벤트 설정
        /// </summary>
        private void SetupScrollRect()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        
        /// <summary>
        /// 데이터 리스트 설정
        /// </summary>
        /// <param name="dataList">표시할 데이터 리스트</param>
        public void SetData(List<object> dataList)
        {
            _dataList = dataList ?? new List<object>();
            UpdateContentSize();
            UpdateVisibleElements();
        }
        
        /// <summary>
        /// Content 크기 업데이트
        /// </summary>
        private void UpdateContentSize()
        {
            var contentRect = _content.GetComponent<RectTransform>();
            
            // Content 크기 설정
            float totalHeight = _dataList.Count * _itemHeight;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
            
            // 스크롤 위치를 맨 위로 리셋
            _scrollRect.verticalNormalizedPosition = 1f;
        }
        
        /// <summary>
        /// 보이는 요소 범위 계산
        /// </summary>
        private void CalculateVisibleRange()
        {
            float scrollPosition = _scrollRect.verticalNormalizedPosition;
            float contentHeight = _dataList.Count * _itemHeight;
            float viewportHeight = _scrollRect.viewport.rect.height;
            
            float scrollOffset = (1f - scrollPosition) * (contentHeight - viewportHeight);
            
            _firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(scrollOffset / _itemHeight));
            _lastVisibleIndex = Mathf.Min(_dataList.Count - 1, 
                _firstVisibleIndex + _maxElements - 1);
        }
        
        /// <summary>
        /// 스크롤 값 변경 이벤트
        /// </summary>
        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            UpdateVisibleElements();
        }
        
        /// <summary>
        /// 보이는 요소들 업데이트
        /// </summary>
        private void UpdateVisibleElements()
        {
            if (_dataList.Count == 0) return;
            
            CalculateVisibleRange();
            
            // 요소들을 순환하면서 위치와 내용 업데이트
            for (int i = 0; i < _maxElements && i < _itemElements.Count; i++)
            {
                int dataIndex = _firstVisibleIndex + i;
                
                if (dataIndex <= _lastVisibleIndex && dataIndex < _dataList.Count)
                {
                    // 데이터가 있으면 요소 활성화하고 위치/내용 업데이트
                    var element = _itemElements[i];
                    element.SetActive(true);
                    
                    // 위치 설정
                    var rectTransform = element.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(0, -dataIndex * _itemHeight);
                    
                    // width가 0이면 다시 설정
                    if (rectTransform.rect.width <= 0)
                    {
                        rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);
                    }
                    
                    // 내용 업데이트 이벤트 발생
                    OnItemUpdate?.Invoke(element, _dataList[dataIndex], dataIndex);
                }
                else
                {
                    // 데이터가 없으면 요소 비활성화
                    _itemElements[i].SetActive(false);
                }
            }
        }

        public void ForceUpdate()
        {
            UpdateVisibleElements();
        }
        
        /// <summary>
        /// 특정 인덱스로 스크롤
        /// </summary>
        /// <param name="index">스크롤할 인덱스</param>
        public void ScrollToIndex(int index)
        {
            if (index < 0 || index >= _dataList.Count) return;
            
            float normalizedPosition = 1f - (index * _itemHeight) / (_dataList.Count * _itemHeight - _scrollRect.viewport.rect.height);
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            
            _scrollRect.verticalNormalizedPosition = normalizedPosition;
        }
        
        /// <summary>
        /// 데이터 개수 반환
        /// </summary>
        public int GetDataCount()
        {
            return _dataList.Count;
        }
        
        /// <summary>
        /// 특정 인덱스의 데이터 반환
        /// </summary>
        public object GetDataAt(int index)
        {
            if (index < 0 || index >= _dataList.Count) return null;
            return _dataList[index];
        }
        
        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }
    }
}
