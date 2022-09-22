using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NuspecDependenciesUpdater.Extensions;
using NuspecDependenciesUpdater.Models;
using NuspecDependenciesUpdater.Tests.Properties;

namespace NuspecDependenciesUpdater.Tests
{
    [TestClass]
    public class PackageFileExtensionsTests
    {
        #region ExtractVersionFromPackageReference

        [TestMethod]
        public void TestExtractVersionFromPackageReference()
        {
            // given
            const string version = "1.2.3";
            const string singleVersion = "[1.2.3]";
            const string rangedVersion = "[1.2.3, 2.0)";

            const string expectedVersion = "1.2.3";

            // when
            bool success = false;

            string extractedVersion = null;
            string extractedSingleVersion = null;
            string extractedRangedVersion = null;

            try
            {
                extractedVersion = version.ExtractVersionFromPackageReference();
                extractedSingleVersion = singleVersion.ExtractVersionFromPackageReference();
                extractedRangedVersion = rangedVersion.ExtractVersionFromPackageReference();

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // then
            Assert.IsTrue(success);

            Assert.IsNotNull(extractedVersion);
            Assert.IsNotNull(extractedSingleVersion);
            Assert.IsNotNull(extractedRangedVersion);

            Assert.AreEqual(extractedVersion, expectedVersion);
            Assert.AreEqual(extractedSingleVersion, expectedVersion);
            Assert.AreEqual(extractedRangedVersion, expectedVersion);
        }

        #endregion

        #region GetComponentsFromProjectFile

        [TestMethod]
        public void TestGetComponentsFromProjectFile()
        {
            // given
            FileInfo projectFile = SaveToTempFile(Resources.Engine3D_Adapter_CSPROJ);

            // when
            bool success = false;
            IEnumerable<Component> components = null;
            List<Component> componentsList = null;

            try
            {
                components = projectFile.GetComponentsFromProjectFile();
                if (components != null)
                {
                    componentsList = components.ToList();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // then
            Assert.IsTrue(success);
            Assert.IsNotNull(components);
            Assert.IsNotNull(componentsList);
            Assert.IsTrue(componentsList.Any());

            projectFile.Delete();
        }

        [TestMethod]
        public void TestGetComponentsFromProjectFileOld()
        {
            // given
            FileInfo projectFile = SaveToTempFile(Resources.Engine3D2_CSPROJ);

            // when
            bool success = false;
            IEnumerable<Component> components = null;
            List<Component> componentsList = null;

            try
            {
                components = projectFile.GetComponentsFromProjectFile();
                if (components != null)
                {
                    componentsList = components.ToList();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // then
            Assert.IsTrue(success);
            Assert.IsNotNull(components);
            Assert.IsNotNull(componentsList);
            Assert.IsTrue(!componentsList.Any());

            projectFile.Delete();
        }

        #endregion

        #region SaveToTempFile

        private FileInfo SaveToTempFile(string content)
        {
            FileInfo tempFile = new FileInfo(Path.GetTempFileName());

            using (FileStream fileStream = tempFile.OpenWrite())
            using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            {
                streamWriter.Write(content);
            }

            return tempFile;
        }

        #endregion
    }
}
