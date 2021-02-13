using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class Views
{
    public string location;
    public Sprite[] images;
    public string[] dates;
}

public class ImageHandler : MonoBehaviour
{
    private const string filepath = "/../../web-version/views.json";
    private Dictionary<string, Texture2D> textures;
    private Views[] views;

    // Start is called before the first frame update
    void Start()
    {
        string text = System.IO.File.ReadAllText(Application.dataPath + filepath);
        JSONNode data = JSON.Parse(text)["views"];
        textures = new Dictionary<string, Texture2D>();
        views = new Views[data.Count];
        for (int i = 0; i != data.Count; ++i) {
            views[i] = new Views();
            views[i].images = new Sprite[data[i].Count];
            views[i].dates = new string[data[i].Count];
            for (int j = 0; j != data[i].Count; ++j) {
                string path = data[i][j]["path"].Value;
                Texture2D t = new Texture2D(1,1);
                t.LoadImage(System.IO.File.ReadAllBytes(path));
                textures[path] = t;
                views[i].images[j] = Sprite.Create(textures[path], new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                views[i].dates[j] = data[i][j]["data"].Value;
            }
        }
        print(views);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
