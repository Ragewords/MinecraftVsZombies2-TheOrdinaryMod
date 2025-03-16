using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
using System.Xml;

namespace CustomBuilder.Editor
{
    public class TestRunner : ICallbacks
    {
        [MenuItem("Custom/CI/运行编辑器的测试")]
        static void RunEditorTests()
        {
            Console.WriteLine("Running editor tests");

            var testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };
            testRunner.RegisterCallbacks<TestRunner>(new TestRunner());
            testRunner.Execute(new ExecutionSettings(filter));
        }

        [MenuItem("Custom/CI/运行播放模式的测试")]
        static void RunRuntimeTests()
        {
            Console.WriteLine("Running runtime tests");

            var testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.PlayMode
            };
            testRunner.RegisterCallbacks<TestRunner>(new TestRunner());
            testRunner.Execute(new ExecutionSettings(filter));
        }

        public void RunStarted(ITestAdaptor tests)
        {
            Debug.Log("TestRunner.RunStarted");
        }

        public void RunFinished(ITestResultAdaptor testResults)
        {
            Debug.Log("TestRunner.RunFinished");
            var reportPath = GetArgument("testResults");
            if (String.IsNullOrEmpty(reportPath)) return;

            using (XmlWriter writer = XmlWriter.Create(reportPath, new XmlWriterSettings(){ Indent = true }))
            {
                testResults.ToXml().WriteTo(writer);
            }
        }

        public static string GetArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains(name))
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        public void TestStarted(ITestAdaptor tests)
        {
        }

        public void TestFinished(ITestResultAdaptor testResults)
        {
        }
    }
}