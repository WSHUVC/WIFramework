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
        // 1. PanelBase�� ��ӹ��� ��� Ŭ���� �˻�
        var panelClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(PanelBase)));

        // 2. Resources/language�� ko.txt ����
        string filePath = "Assets/Resources/language/ko.txt";
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        // 3. ���� ������ ��� �� �б�
        Dictionary<string, string> hoverTexts = new Dictionary<string, string>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // �� ���� ������ ������ ������ ����: $"{UIBehaviour.gameObject.name}:anyText"
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string gameObjectName = parts[0];
                    string hoverText = parts[1];
                    hoverTexts.Add(gameObjectName, hoverText);
                }
            }
        }

        // 4. �̾ UIBehaviour.gameObject.name�� anyText�� Dictionary�� ����
        foreach (Type panelClass in panelClasses)
        {
            // 5. �ش� PanelBase�� ��� ������ �߿��� UIBehaviour�� ����
            var uiBehaviours = panelClass.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(UIBehaviour).IsAssignableFrom(f.FieldType));

            foreach (var uiBehaviour in uiBehaviours)
            {
                string gameObjectName = uiBehaviour.Name.ToLower();
                string hoverText = "";

                // 6. Dictionary�� UIBehaviour�� �̸��� �������� �ʴ´ٸ� �߰�
                if (!hoverTexts.ContainsKey(gameObjectName))
                {
                    hoverTexts.Add(gameObjectName, hoverText);
                }
            }
        }
        
        var orderd = hoverTexts.OrderBy(p => p.Key);
        // 7. ���� ���Ͽ� �Է�
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
