using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace CustomBuilder.Editor
{
    public class BuilderConfigWindow : EditorWindow
    {
        [MenuItem("Custom/CI/配置窗口")]
        static void Init()
        {
            var window = GetWindow<BuilderConfigWindow>();
            window.titleContent = new GUIContent("打包配置");

            window.Show();
        }

        Vector2 scrollPos;

        List<BuildConfig> configs = new List<BuildConfig>();

        void OnGUI()
        {
            GUILayout.Label("打包设置", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            int buildIndex = -1;

            EditorGUILayout.BeginVertical();
            {
                int? removeIndex = null;
                for (int i = 0; i < configs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // 构建按钮
                        if (GUILayout.Button("▶")) buildIndex = i;
                        // 配置
                        configs[i].Target = (BuildTarget)EditorGUILayout.EnumPopup(configs[i].Target);
                        configs[i].Options = (BuildOptions)EditorGUILayout.EnumFlagsField(configs[i].Options);
                        if (GUILayout.Button("-")) removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (removeIndex != null)
                    configs.RemoveAt(removeIndex.Value);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加"))
            {
                configs.Add(new BuildConfig(BuildTarget.NoTarget, BuildOptions.None));
            }

            if (GUILayout.Button("载入"))
            {
                configs.Clear();
                configs.AddRange(Builder.ReadConfigs());
            }

            if (GUILayout.Button("保存"))
            {
                Builder.WriteConfigs(configs.ToArray());
            }

            EditorGUILayout.EndHorizontal();

            if (buildIndex >= 0)
                Builder.GenericBuild(configs[buildIndex]);
        }
    }
}
