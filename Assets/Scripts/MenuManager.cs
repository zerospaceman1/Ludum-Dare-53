using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public MenuPage.Page currentPage = MenuPage.Page.main;
    public List<GameObject> pages;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void openPage(MenuPage.Page page)
    {
        pages.ForEach(p => {
            // this logic keeps the in game UI on screen if game is paused
            if (!(p.GetComponent<MenuPage>().page == MenuPage.Page.game && page == MenuPage.Page.gameMenu))
            {
                p.SetActive(false);
            }
        });

        List<GameObject> pagesToOpen = pages.FindAll(p => p.GetComponent<MenuPage>().page == page);
        pagesToOpen.ForEach(p => p.SetActive(true));

        currentPage = page;
    }
}
