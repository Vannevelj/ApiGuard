﻿using System;
using System.IO;
using System.Reflection;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Models;
using Newtonsoft.Json;

namespace ApiGuard.Domain
{
    internal class ProjectResolver : IProjectResolver
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public ProjectResolver()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };
        }

        public ProjectInfo GetProjectInfo(Type type)
        {
            var assemblyPath = GetAssemblyFullPath(type.Assembly);
            var bin = Path.DirectorySeparatorChar + "bin";
            var testProjectPath = assemblyPath.Substring(0, assemblyPath.IndexOf(bin, StringComparison.InvariantCultureIgnoreCase));

            return new ProjectInfo
            {
                TestProjectPath = testProjectPath
            };
        }

        /// <summary>
        /// https://stackoverflow.com/a/28319367/1864167
        /// </summary>
        private static string GetAssemblyFullPath(Assembly assembly)
        {
            string codeBasePseudoUrl = assembly.CodeBase; // "pseudo" because it is not properly escaped
            if (codeBasePseudoUrl != null)
            {
                const string filePrefix3 = @"file:///";
                if (codeBasePseudoUrl.StartsWith(filePrefix3))
                {
                    string sPath = codeBasePseudoUrl.Substring(filePrefix3.Length);
                    string bsPath = sPath.Replace('/', '\\');
                    Console.WriteLine("bsPath: " + bsPath);
                    string fp = Path.GetFullPath(bsPath);
                    Console.WriteLine("fp: " + fp);
                    return fp;
                }
            }
            System.Diagnostics.Debug.Assert(false, "CodeBase evaluation failed! - Using Location as fallback.");
            return Path.GetFullPath(assembly.Location);
        }

        public bool ApiFileExists(ProjectInfo projectInfo, Type type)
        {
            var documentPath = projectInfo.GetApiFilePath(type);
            return File.Exists(documentPath);
        }

        internal string SerializeApi(MyType api) => JsonConvert.SerializeObject(api, Formatting.Indented, _serializerSettings);
        internal MyType DeserializeApi(string existingApiJson) => JsonConvert.DeserializeObject<MyType>(existingApiJson, _serializerSettings);

        public void WriteApiToFile(ProjectInfo projectInfo, Type type, MyType api)
        {
            File.WriteAllText(projectInfo.GetApiFilePath(type), SerializeApi(api));
        }

        public MyType ReadApiFromFile(ProjectInfo projectInfo, Type type)
        {
            try
            {
                var existingApiJson = File.ReadAllText(projectInfo.GetApiFilePath(type));
                return DeserializeApi(existingApiJson);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("Unable to find or open the API file", e);
            }
        }
    }
}
