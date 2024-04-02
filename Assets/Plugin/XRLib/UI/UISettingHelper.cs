using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using WI;


#if UNITY_EDITOR
public class UISettingHelper : Editor
{
    [MenuItem("Tools/UIClassGenerate")]
    public static void UIClassGenerate()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        string defalutUsing = "using UnityEngine;\nusing UnityEngine.UI;\nusing WI;\n\n";
        string defalutNameSpace = "namespace XED\n{\n";

        foreach (Canvas canvas in canvases)
        {
            string canvasPath;
            string canvasFileName = canvas.gameObject.name + ".cs";
            canvasPath = Path.Combine("Assets/Scripts/UI", canvasFileName);

            var childs = canvas.GetComponentsInChildren<Transform>();
            List<string> childPanels = new List<string>();
            Dictionary<string, int> dupleCount = new Dictionary<string, int>();
            foreach (var c in childs)
            {
                if (c.name.Split('_')[0] != "Panel")
                    continue;
                childPanels.Add(c.gameObject.name);

                if (c.TryGetComponent<PanelBase>(out var tb))
                    continue;

                string panelName = c.gameObject.name + ".cs";
                string panelPath = Path.Combine("Assets/Scripts/UI", panelName);

                var uiElements = c.FindAll<UIBehaviour>();

                using FileStream fs = File.Create(panelPath);
                using StreamWriter writer = new StreamWriter(fs);
                writer.Write(defalutUsing);
                writer.Write(defalutNameSpace);
                writer.WriteLine($"\tinternal class {c.gameObject.name} : PanelBase");
                writer.WriteLine("\t{");
                Dictionary<string, List<string>> nameTable = new Dictionary<string, List<string>>();
                foreach (var e in uiElements)
                {
                    var eType = e.GetType();
                    string eName;
                    if (eType == typeof(TextMeshProUGUI))
                        eName = "text";
                    else
                        eName = eType.Name;

                    if (e.name.Contains('_'))
                        eName += $"_{e.name.Split('_').Last()}";
                    else
                    {
                        eName += $"_{e.transform.parent.name.Split('_').Last()}";
                    }

                    eName = eName.Replace(" ", "");
                    //writer.WriteLine($"\t\tpublic {eType} {eName.ToLower()};");
                    if (!nameTable.ContainsKey(eType.FullName))
                        nameTable.Add(eType.FullName, new());

                    if (!dupleCount.ContainsKey(eName))
                        dupleCount.Add(eName, 0);
                    dupleCount[eName]++;
                    
                    if (nameTable[eType.FullName].Contains(eName))
                    {
                        eName += dupleCount[eName];
                    }
                    nameTable[eType.FullName].Add(eName);

                    
                }
                foreach (var nt in nameTable)
                {
                    var temp = nt.Value.OrderBy(a => a.Length);

                    foreach (var en in temp)
                    {
                        writer.WriteLine($"\t\tpublic {nt.Key} {en.ToLower()};");
                    }

                    writer.WriteLine();
                }

                writer.WriteLine("\t}");
                writer.Write("}");
            }

            if (canvas.TryGetComponent<CanvasBase>(out var cb))
                continue;

            using (FileStream fs = File.Open(canvasPath, FileMode.OpenOrCreate))
            {
                using StreamWriter writer = new StreamWriter(fs);
                writer.Write(defalutUsing);
                writer.Write(defalutNameSpace);
                writer.WriteLine($"\tpublic class {canvas.gameObject.name} : CanvasBase");
                writer.WriteLine("\t{");
                foreach (var cp in childPanels)
                {
                    writer.WriteLine($"\t\t{cp} {cp.ToLower()};");
                }
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
            //Debug.Log(canvas.gameObject);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/UIClassAttach")]
    public static void UIClassAttach()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();

        foreach (var canvas in canvases)
        {
            var canvasName = canvas.gameObject.name;

            var canvasClassType = assemblies.SelectMany(a => a.GetTypes())
                                            .FirstOrDefault(t => t.Name == canvasName);

            if (canvasClassType == null)
            {
                continue;
            }
            canvas.transform.GetOrAddComponent(canvasClassType);

            var childPanels = canvas.transform.FindAll<RectTransform>().Where(cr => cr.name.Split('_')[0] == "Panel");
            foreach (var panel in childPanels)
            {
                var panelName = panel.gameObject.name;
                var panelClassType = assemblies.SelectMany(a => a.GetTypes())
                                                .FirstOrDefault(t => t.Name == panelName);

                if (panelClassType == null)
                    continue;

                panel.transform.GetOrAddComponent(panelClassType);
            }
        }
    }

    [MenuItem("Tools/UI HoverText File Generate")]
    static void UIHoverTextFileGenerate()
    {
        // 1. PanelBase를 상속받은 모든 클래스 검색
        var panelClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(PanelBase)));

        // 2. Resources/language에 ko.txt 열기
        string filePath = "Assets/Resources/language/ko.txt";
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        // 3. 열린 파일의 모든 줄 읽기
        Dictionary<string, string> hoverTexts = new Dictionary<string, string>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // 각 줄은 다음의 구조를 가지고 있음: $"{UIBehaviour.gameObject.name}:anyText"
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string gameObjectName = parts[0];
                    string hoverText = parts[1];
                    hoverTexts.Add(gameObjectName, hoverText);
                }
            }
        }

        // 4. 이어서 UIBehaviour.gameObject.name과 anyText로 Dictionary를 생성
        foreach (Type panelClass in panelClasses)
        {
            // 5. 해당 PanelBase의 멤버 변수들 중에서 UIBehaviour를 선별
            var uiBehaviours = panelClass.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(UIBehaviour).IsAssignableFrom(f.FieldType));

            foreach (var uiBehaviour in uiBehaviours)
            {
                string gameObjectName = uiBehaviour.Name.ToLower();
                string hoverText = "";

                // 6. Dictionary에 UIBehaviour의 이름이 존재하지 않는다면 추가
                if (!hoverTexts.ContainsKey(gameObjectName))
                {
                    hoverTexts.Add(gameObjectName, hoverText);
                }
            }
        }
        
        var orderd = hoverTexts.OrderBy(p => p.Key);
        // 7. 열린 파일에 입력
        using (StreamWriter writer = new StreamWriter(filePath,false, System.Text.Encoding.UTF8))
        {
            foreach (var hoverText in orderd)
            {
                writer.WriteLine($"{hoverText.Key}:{hoverText.Value}");
            }
        }
        AssetDatabase.Refresh();
    }
}
#endif
