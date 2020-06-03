using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public Color IdleColour;
    public Color HoverColour;
    public Color ActiveColour;
    public List<TabButton> tabButtons;
    public TabButton selectedTab;
    public List<GameObject> objectsToSwap;
    
    public void Subscribe(TabButton button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    void Start()
    {
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if(!selectedTab || button != selectedTab)
        {
            button.background.color = HoverColour;
        }
        
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.color = ActiveColour;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if(i == index)
            {
                objectsToSwap[i].SetActive(true);
            }
            else
            {
                objectsToSwap[i].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach(TabButton button in tabButtons)
        {
            if(!selectedTab || button != selectedTab)
            {
                button.background.color = IdleColour;
            }
        }
    }
}
