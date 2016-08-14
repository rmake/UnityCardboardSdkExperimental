using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScrollListView
{
    public delegate void Callback();
    public delegate void EachButtonCallback(GameObject button);

    public static GameObject Create(Dictionary<string, Callback> callbacks, EachButtonCallback eachButtonCallback, bool isVertical, GameObject canvas = null)
    {
        var buttonPrefab = Resources.Load<GameObject>("UtilityUi/ButtonForScrollView");

        var scrollList = new GameObject("ScrollList");
        


        var scrollRect = scrollList.AddComponent<ScrollRect>();
        var scrollListMask = scrollList.AddComponent<Mask>();
        var scrollListImage = scrollList.AddComponent<Image>();
        var scrollListRectTransform = scrollList.GetComponent<RectTransform>();
        scrollListImage.color = new Color(1, 1, 1, 0.25f);
        scrollListMask.showMaskGraphic = false;


        var viewport = new GameObject("Viewport");
        var viewportMask = viewport.AddComponent<Mask>();
        var viewportImage = viewport.AddComponent<Image>();
        var viewportRectTransform = viewport.GetComponent<RectTransform>();
        viewportImage.color = new Color(1, 1, 1, 0.25f);
        viewport.transform.SetParent(scrollList.transform, false);
        viewportRectTransform.anchorMin = new Vector2(0, 0);
        viewportRectTransform.anchorMax = new Vector2(1, 1);
        viewportRectTransform.sizeDelta = new Vector2(0, 0);

        var contentObject = new GameObject("Content");
        var content = contentObject.AddComponent<ContentSizeFitter>();
        content.transform.SetParent(viewport.transform);
        var contentTransform = content.GetComponent<RectTransform>();
        if (isVertical)
        {
            var layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
            contentTransform.pivot = new Vector2(0.5f, 1.0f);
            contentTransform.anchorMin = new Vector2(0.5f, 1.0f);
            contentTransform.anchorMax = new Vector2(0.5f, 1.0f);
            contentTransform.anchoredPosition = new Vector2(0, 0);
            content.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            layoutGroup.childForceExpandWidth = true;
        }
        else
        {
            var layoutGroup = contentObject.AddComponent<HorizontalLayoutGroup>();
            contentTransform.pivot = new Vector2(0.0f, 0.5f);
            contentTransform.anchorMin = new Vector2(0.0f, 0.5f);
            contentTransform.anchorMax = new Vector2(0.0f, 0.5f);
            contentTransform.anchoredPosition = new Vector2(0, 0);
            content.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.verticalFit = ContentSizeFitter.FitMode.MinSize;
            layoutGroup.childForceExpandHeight = true;
        }

        scrollRect.content = contentTransform;
        scrollRect.viewport = viewportRectTransform;


        foreach (var pair in callbacks)
        {
            var value = pair.Value;
            var button = GameObject.Instantiate(buttonPrefab);
            

            var buttonComponent = button.GetComponentInChildren<Button>();
            var text = button.GetComponentInChildren<Text>();
            text.text = pair.Key;

            buttonComponent.onClick.AddListener(() =>
            {
                value();
            });
            button.transform.SetParent(content.transform);
            if (eachButtonCallback != null)
            {
                eachButtonCallback(button);
            }
        }

        return scrollList;
    }

    public static GameObject Create(Vector2 position, Vector2 size, Dictionary<string, Callback> callbacks, EachButtonCallback eachButtonCallback, bool isVertical, GameObject canvas = null)
    {
        var go = Create(callbacks, eachButtonCallback, isVertical, canvas);

        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.localPosition = new Vector3(position.x, position.y);
        rectTransform.sizeDelta = size;
        return go;
    }

    public static GameObject Create(Dictionary<string, Callback> callbacks, GameObject canvas = null)
    {
        return Create(callbacks, null, true, canvas);
    }


    public static GameObject CreateVertical(Dictionary<string, Callback> callbacks, GameObject canvas = null)
    {
        return Create(callbacks, null, true, canvas);
    }


    public static GameObject CreateHorizontal(Dictionary<string, Callback> callbacks, GameObject canvas = null)
    {

        return Create(callbacks, null, false, canvas);
    }

    public static TabbedScrollListBehavior CreateTabedScroll(Rect rect, float tabHeight, Dictionary<string, Dictionary<string, Callback>> callbacks)
    {
        var root = new GameObject("TabedScrollView");
        GameObject conent = null;

        string currentTab = null;
        var tabCallbacks = new Dictionary<string, Callback>();

        foreach(var pair in callbacks)
        {
            var storedPair = pair;

            tabCallbacks[pair.Key] = () => {
                if (conent != null)
                {
                    GameObject.Destroy(conent);
                    var oldTab = currentTab;
                    currentTab = null;
                    if (oldTab == storedPair.Key)
                    {
                        return;
                    }
                }

                var dic = new Dictionary<string, Callback>();
                foreach (var callbackPair in storedPair.Value)
                {
                    var storedCallbackPair = callbackPair;
                    dic[storedCallbackPair.Key] = () =>
                    {
                        if (conent != null)
                        {
                            GameObject.Destroy(conent);
                            currentTab = null;
                        }
                        storedCallbackPair.Value();
                    };
                }

                conent = Create(dic, null, true);

                conent.transform.SetParent(root.transform, false);

                var margin = 4;
                var contentRectTransform = conent.GetComponent<RectTransform>();
                contentRectTransform.anchorMin = new Vector2(0, 1);
                contentRectTransform.anchorMax = new Vector2(0, 1);
                contentRectTransform.pivot = new Vector2(0, 1);
                contentRectTransform.localPosition = new Vector3(0, -tabHeight - margin);
                contentRectTransform.sizeDelta = new Vector2(rect.width, rect.height - tabHeight - margin);
                currentTab = storedPair.Key;

            };
        }


        var tabs = Create(tabCallbacks, (GameObject button) => {
            var layoutElement = button.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = rect.width / 2.5f;
        }, false);

        var rootRectTransform = root.AddComponent<RectTransform>();
        rootRectTransform.anchorMin = new Vector2(0, 1);
        rootRectTransform.anchorMax = new Vector2(0, 1);
        rootRectTransform.pivot = new Vector2(0, 1);
        rootRectTransform.localPosition = new Vector3(rect.x, rect.y);

        tabs.transform.SetParent(root.transform, false);

        var tabsRectTransform = tabs.GetComponent<RectTransform>();
        tabsRectTransform.anchorMin = new Vector2(0, 1);
        tabsRectTransform.anchorMax = new Vector2(0, 1);
        tabsRectTransform.pivot = new Vector2(0, 1);
        tabsRectTransform.localPosition = new Vector3(0, 0);
        tabsRectTransform.sizeDelta = new Vector2(rect.width, tabHeight);

        tabCallbacks[new List<string>(tabCallbacks.Keys)[0]]();

        var tabbedBehavior = root.AddComponent<TabbedScrollListBehavior>();
        tabbedBehavior.CloseContent = () => {
            if (conent != null)
            {
                GameObject.Destroy(conent);
                var oldTab = currentTab;
                currentTab = null;
            }
        };

        tabbedBehavior.SelectTab = (string name) => {
            tabCallbacks[name]();
        };

        return tabbedBehavior;
    }
}
