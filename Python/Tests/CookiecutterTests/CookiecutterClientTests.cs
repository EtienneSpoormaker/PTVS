﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CookiecutterTools;
using Microsoft.CookiecutterTools.Model;
using Microsoft.CookiecutterTools.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Python;

namespace CookiecutterTests {
    [TestClass]
    public class CookiecutterClientTests {
        private const string GitHubTemplatePath = "https://github.com/audreyr/Cookiecutter-pypackage";
        private const string NoUserConfigFilePath = "";

        private static string LocalTemplatePath => Path.Combine(TestData.GetPath("TestData"), "Cookiecutter", "template");
        private static string LocalTemplateForVSPath => Path.Combine(TestData.GetPath("TestData"), "Cookiecutter", "templateforvs");
        private static string UserConfigFilePath => Path.Combine(TestData.GetPath("TestData"), "Cookiecutter", "userconfig.yaml");

        private ICookiecutterClient _client;
        private MockRedirector _redirector = new MockRedirector();

        private static ContextItem[] LocalTemplateNoUserConfigContextItems { get; } = new ContextItem[] {
            new ContextItem("full_name", ContextItemValueType.String, "Default Full Name"),
            new ContextItem("email", ContextItemValueType.String, "default@email"),
            new ContextItem("github_username", ContextItemValueType.String, "defaultgitusername"),
            new ContextItem("project_name", ContextItemValueType.String, "Default Project Name"),
            new ContextItem("project_slug", ContextItemValueType.String, "{{ cookiecutter.project_name.lower().replace(' ', '_') }}"),
            new ContextItem("pypi_username", ContextItemValueType.String, "{{ cookiecutter.github_username }}"),
            new ContextItem("version", ContextItemValueType.String, "0.1.0"),
            new ContextItem("use_azure", ContextItemValueType.String, "y"),
            new ContextItem("open_source_license", ContextItemValueType.List, "MIT license", new string[] { "MIT license", "BSD license", "ISC license", "Apache Software License 2.0", "GNU General Public License v3", "Not open source" }),
            new ContextItem("port", ContextItemValueType.Numeric, "5000"),
            // Note that _copy_without_render item should not appear
        };

        private static ContextItem[] LocalTemplateForVSNoUserConfigContextItems { get; } = new ContextItem[] {
            new ContextItem("full_name", ContextItemValueType.String, "Default Full Name") { Description="Full name of author." },
            new ContextItem("email", ContextItemValueType.String, "default@email"),
            new ContextItem("github_username", ContextItemValueType.String, "defaultgitusername"),
            new ContextItem("project_name", ContextItemValueType.String, "Default Project Name") { Description="Description for the application."},
            new ContextItem("project_slug", ContextItemValueType.String, "{{ cookiecutter.project_name.lower().replace(' ', '_') }}") { Description="Pythonic name for the application." },
            new ContextItem("pypi_username", ContextItemValueType.String, "{{ cookiecutter.github_username }}"),
            new ContextItem("version", ContextItemValueType.String, "0.1.0"),
            new ContextItem("use_azure", ContextItemValueType.YesNo, "y") { Description="Enable Azure support." },
            new ContextItem("open_source_license", ContextItemValueType.List, "MIT license", new string[] { "MIT license", "BSD license", "ISC license", "Apache Software License 2.0", "GNU General Public License v3", "Not open source" }) { Description="License under which you will distribute the generated files." },
            new ContextItem("port", ContextItemValueType.Numeric, "5000"),
            // Note that _copy_without_render item should not appear
        };

        private static ContextItem[] LocalTemplateWithUserConfigContextItems { get; } = new ContextItem[] {
            new ContextItem("full_name", ContextItemValueType.String, "Configured User"),
            new ContextItem("email", ContextItemValueType.String, "configured@email"),
            new ContextItem("github_username", ContextItemValueType.String, "configuredgithubuser"),
            new ContextItem("project_name", ContextItemValueType.String, "Default Project Name"),
            new ContextItem("project_slug", ContextItemValueType.String, "{{ cookiecutter.project_name.lower().replace(' ', '_') }}"),
            new ContextItem("pypi_username", ContextItemValueType.String, "{{ cookiecutter.github_username }}"),
            new ContextItem("version", ContextItemValueType.String, "0.1.0"),
            new ContextItem("use_azure", ContextItemValueType.String, "y"),
            new ContextItem("open_source_license", ContextItemValueType.List, "BSD license", new string[] { "MIT license", "BSD license", "ISC license", "Apache Software License 2.0", "GNU General Public License v3", "Not open source" }),
            new ContextItem("port", ContextItemValueType.Numeric, "5000"),
            // Note that _copy_without_render item should not appear
        };

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            PythonTestData.Deploy();
        }

        [TestInitialize]
        public void SetupTest() {
            _client = CookiecutterClientProvider.Create(_redirector);
            Assert.IsNotNull(_client, "The system doesn't have any compatible Python interpreters.");
        }

        private async Task EnsureCookiecutterInstalledAsync() {
            if (!_client.CookiecutterInstalled) {
                await _client.CreateCookiecutterEnv();
                await _client.InstallPackage();
            }
        }

        [TestMethod]
        public async Task LoadContextNoUserConfig() {
            await EnsureCookiecutterInstalledAsync();

            var actual = await _client.LoadContextAsync(LocalTemplatePath, NoUserConfigFilePath);

            CollectionAssert.AreEqual(LocalTemplateNoUserConfigContextItems, actual, new ContextItemComparer());
        }

        [TestMethod]
        public async Task LoadContextWithUserConfig() {
            await EnsureCookiecutterInstalledAsync();

            var actual = await _client.LoadContextAsync(LocalTemplatePath, UserConfigFilePath);

            CollectionAssert.AreEqual(LocalTemplateWithUserConfigContextItems, actual, new ContextItemComparer());
        }

        [TestMethod]
        public async Task LoadContextForVSNoUserConfig() {
            await EnsureCookiecutterInstalledAsync();

            var actual = await _client.LoadContextAsync(LocalTemplateForVSPath, NoUserConfigFilePath);

            CollectionAssert.AreEqual(LocalTemplateForVSNoUserConfigContextItems, actual, new ContextItemComparer());
        }

        [TestMethod]
        public async Task GenerateWithoutUserConfig() {
            await EnsureCookiecutterInstalledAsync();

            Dictionary<string, string> actual = await GenerateFromLocalTemplate(NoUserConfigFilePath);

            var expected = new Dictionary<string, string>() {
                { "full_name", "Default Full Name" },
                { "email", "default@email" },
                { "github_username", "defaultgitusername" },
                { "project_name", "Default Project Name" },
                { "project_slug", "default_project_name" },
                { "pypi_username", "defaultgitusername" },
                { "version", "0.1.0" },
                { "use_azure", "y" },
                { "open_source_license", "MIT license" },
                { "port", "5000" },
            };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task GenerateWithUserConfig() {
            await EnsureCookiecutterInstalledAsync();

            Dictionary<string, string> actual = await GenerateFromLocalTemplate(UserConfigFilePath);

            var expected = new Dictionary<string, string>() {
                { "full_name", "Configured User" },
                { "email", "configured@email" },
                { "github_username", "configuredgithubuser" },
                { "project_name", "Default Project Name" },
                { "project_slug", "default_project_name" },
                { "pypi_username", "configuredgithubuser" },
                { "version", "0.1.0" },
                { "use_azure", "y" },
                { "open_source_license", "BSD license" },
                { "port", "5000" },
            };
            CollectionAssert.AreEqual(expected, actual);
        }

        private async Task<Dictionary<string, string>> GenerateFromLocalTemplate(string userConfigFilePath) {
            var context = await _client.LoadContextAsync(LocalTemplatePath, userConfigFilePath);

            var output = TestData.GetTempPath("Cookiecutter", true);
            var outputProjectFolder = Path.Combine(output, "project");
            var contextFilePath = Path.Combine(output, "context.json");

            var vm = new CookiecutterViewModel();
            foreach (var item in context) {
                vm.ContextItems.Add(new ContextItemViewModel(item.Name, item.ValueType, item.Description, item.DefaultValue, item.Values));
            }

            vm.SaveUserInput(contextFilePath);

            Directory.CreateDirectory(outputProjectFolder);

            await _client.GenerateProjectAsync(LocalTemplatePath, userConfigFilePath, contextFilePath, outputProjectFolder);

            var reportFilePath = Path.Combine(outputProjectFolder, "report.txt");
            Assert.IsTrue(File.Exists(reportFilePath), "Failed to generate some project files.");
            return ReadReport(reportFilePath);
        }

        internal static Dictionary<string, string> ReadReport(string filePath) {
            var dict = new Dictionary<string, string>();
            var report = File.ReadAllLines(filePath);
            foreach (var line in report) {
                var parts = line.Split(':');
                dict.Add(parts[0], parts[1]);
            }
            return dict;
        }

        class ContextItemComparer : IComparer {
            public int Compare(object x, object y) {
                if (x == y) {
                    return 0;
                }

                var a = x as ContextItem;
                var b = y as ContextItem;

                if (a == null) {
                    return -1;
                }

                if (b == null) {
                    return -1;
                }

                int res;
                res = a.Name.CompareTo(b.Name);
                if (res != 0) {
                    return res;
                }

                res = a.DefaultValue.CompareTo(b.DefaultValue);
                if (res != 0) {
                    return res;
                }

                res = a.ValueType.CompareTo(b.ValueType);
                if (res != 0) {
                    return res;
                }

                res = SafeCompare(a.Description, b.Description);
                if (res != 0) {
                    return res;
                }

                return 0;
            }

            private int SafeCompare(IComparable a, IComparable b) {
                if (a == null) {
                    return b == null ? 0 : -1;
                }

                return a.CompareTo(b);
            }
        }
    }
}
