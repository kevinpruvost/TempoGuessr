using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmostEngine;
using SimpleJSON;

public class Views
{
    public string location;
    public Sprite[] images;
    public string[] dates;
}

public class ImageHandler : Singleton<ImageHandler>
{
    private const string filepath = "views";
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private Views[] views;
    
    private System.Random rand = new System.Random();
    private Dictionary<int, bool> gotViews = new Dictionary<int, bool>();

    //public UnityEngine.UI.Image img;
    //public UnityEngine.UI.Image img2;
    //public UnityEngine.UI.Image img3;

    // Start is called before the first frame update
    void Start()
    {
        print(Resources.Load<TextAsset>(filepath));
        string text = Resources.Load<TextAsset>(filepath).text;// System.IO.File.ReadAllText(Application.dataPath + filepath);
        JSONNode data = JSON.Parse(text)["views"];
        
        views = new Views[data.Count];
        for (int i = 0; i != data.Count; ++i) {
            int size = data[i]["paths"].Count;
            views[i] = new Views();
            views[i].images = new Sprite[size];
            views[i].dates = new string[size];
            views[i].location = data[i]["location"].Value;
            for (int j = 0; j != size; ++j) {
                string path = data[i]["paths"][j].Value;
                Texture2D t = (Texture2D)Resources.Load(path.Replace(".png", ""));
                textures[path] = t;
                views[i].images[j] = Sprite.Create(textures[path], new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                views[i].dates[j] = data[i]["dates"][j].Value;
            }
        }
    }

    public Views GetView()
    {
        if (gotViews.Count == views.Length)
            gotViews.Clear();
        int n = rand.Next(views.Length);
        while (gotViews.ContainsKey(n))
            if (++n >= views.Length) n = 0;
        gotViews[n] = true;
        return views[n];
    }
/*
    public void SetImage()
    {
        Views v = GetView();
        img.sprite = v.images[0];
        img2.sprite = v.images[1];
        img3.sprite = v.images[2];
    }
*/
    // Update is called once per frame
    void Update()
    {
        
    }
}
