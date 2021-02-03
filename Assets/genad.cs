using System.IO;
using UnityEngine;

public class genad : MonoBehaviour
{
    public Texture2D line, waterMark;
    private Color32[] colors, markedColors;
    private Color32[] allpixels;
    private Texture2D outTexture, paletteTexture;
    private bool isWaterMarked;
    private int leftAdopts;
    //Установка маркерных цветов
    private Color32[] SetMarkedColors()
    {
        return new Color32[] {
            new Color32(0,0,255,255),
            new Color32(0,255,0,255),
            new Color32(0,255,255,255),
            new Color32(255,0,0,255),
            new Color32(255,0,255,255),
            new Color32(255,255,0,255),
        };
    }

    //Генерация палитр
    private Color32[] GeneratePalette() {
        Color32[] colors = new Color32[markedColors.Length];
        //текстура для вывода палитры на экран
        paletteTexture = new Texture2D(1, markedColors.Length);

        //создание основного цвета
        float mainH = Random.Range(0f, 1f);

        //генерация остальных цветов
        for (int i = 0; i < colors.Length/2; ++i) {
            colors[i*2] = Color.HSVToRGB(mainH + Random.Range(-0.1f, 0.1f), Random.Range(0, 0.5f), Random.Range(0.5f, 1f));
            paletteTexture.SetPixel(0, i, colors[i*2]);
            colors[i*2+1] = Color.HSVToRGB(mainH + Random.Range(-0.1f, 0.1f), Random.Range(0, 0.5f), Random.Range(0.5f, 1f));
            paletteTexture.SetPixel(0, i, colors[i*2+1]);
        }

        paletteTexture.Apply();

        //возврат готовой палитры
        return colors;
    }

    //Разукраска - замена маркерных цветов созданными
    private Texture2D Fill()
    {
        //получение всех пикселей изображения
        allpixels = line.GetPixels32();
        
        //замена маркерных цветов фактическими
        for (int i = 0; i < allpixels.Length; ++i) {
            for (int c = 0; c < markedColors.Length; ++c) {
                if (allpixels[i].Equals(markedColors[c])) {
                    allpixels[i] = colors[c];
                    break;
                }
            }
        }

        //создание новой текстуры и её возврат
        Texture2D texture = new Texture2D(line.width, line.height);
        texture.SetPixels32(allpixels);
        texture.Apply();
        return texture;
    }

    //Установка ватермарки
    private Texture2D ApplyMark(Texture2D texture, Texture2D mark) {
        Texture2D bufferTexture=new Texture2D(texture.width,texture.height);
        bufferTexture.SetPixels32(texture.GetPixels32());
        bufferTexture.SetPixels32(texture.width / 2 - mark.width / 2, texture.height / 2 - mark.height / 2, mark.width, mark.height, mark.GetPixels32());
        bufferTexture.Apply();
        return bufferTexture;
    }
    //Сохранение
    private void SaveFile(Texture2D texture) {
        //Файл получает имя, соответсвующее имени заготовки+дата и время
        //Если нужна версия с ватермаркой - сохраняются обе
        File.WriteAllBytes(Application.dataPath + "/out/"+line.name+"_"+ System.DateTime.Now.ToString("dd_hh_mm_ss_ms")+leftAdopts + ".png",texture.EncodeToPNG());
        if (isWaterMarked) { 
        File.WriteAllBytes(Application.dataPath + "/out/"+line.name+"_"+ System.DateTime.Now.ToString("dd_hh_mm_ss_ms") + leftAdopts + "(марка).png",ApplyMark(texture,waterMark).EncodeToPNG());
        }
    }
    //Подготовительные процедуры
    private void Start()
    {
        //Назначить маркеры
        markedColors=SetMarkedColors();
        //Создать директории
        Directory.CreateDirectory(Application.dataPath + "/out");
        Directory.CreateDirectory(Application.dataPath + "/in");
        //Вылететь, если чего-то для работы не хватает
        try
        {
            line.LoadImage(File.ReadAllBytes(Application.dataPath + "/in/line.png"));
            waterMark.LoadImage(File.ReadAllBytes(Application.dataPath + "/in/waterMark.png"));
        }
        catch {
            Application.Quit();
        }
    }
    private void Update()
    {
        if (leftAdopts > 0) {
            colors = GeneratePalette();
            outTexture = Fill();
            SaveFile(outTexture);
            --leftAdopts;
        }
    }
    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.5f - 400, Screen.height - 60, 200, 20), "Сгенерировать цвета")) {
           colors=GeneratePalette();
        }
        if (GUI.Button(new Rect(Screen.width * 0.5f - 400, Screen.height - 40, 200, 20), "Сгенерировать адопта"))
        {
            outTexture = Fill();
        }
        if (GUI.Button(new Rect(Screen.width * 0.5f - 400, Screen.height - 20, 200, 20), "Сохранить адопта"))
        {
            SaveFile(outTexture);
        }
        if (GUI.Button(new Rect(Screen.width * 0.5f-200, Screen.height - 20, 200, 20), "Сгенерировать партию")) {
            leftAdopts = 50;
        }
        if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height - 20, 200, 20), isWaterMarked ? "Маркируется" : "Не маркируется"))
        {
            isWaterMarked = !isWaterMarked;
        }
        GUI.Box(new Rect(Screen.width * 0.5f - 400, 0, 800, 800), outTexture);
        GUI.Box(new Rect(Screen.width -100, Screen.height-100, 100, 100), waterMark);
        GUI.DrawTexture(new Rect(Screen.width - 104, Screen.height - 100, 4, 100), paletteTexture);
        if (leftAdopts > 0) { GUI.Box(new Rect(0, 0, 90, 20), "Осталось: " + leftAdopts); }
    }
}
