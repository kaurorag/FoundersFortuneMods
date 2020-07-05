using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FFModPreBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Args Length: {args.Length}");

            if (args.Length != 3)
                throw new ArgumentException("Not enough parameters");

            for (int i = 0; i < args.Length; i++)
                Console.WriteLine($"Args[{i}]={args[i]}");

            String csProjPath = args[0];
            String projectName = args[1];
            String modKitDir = args[2];

            String projectDir = Path.GetDirectoryName(csProjPath);

            if (!Directory.Exists(projectDir))
                throw new Exception("Invalid project directory");

            String bundleDir = modKitDir + "\\BuiltAssetBundles";
            String bundleFileName = projectName + ".ffasset";
            String bundlePath = $"{bundleDir}\\{bundleFileName}";

            if (!Directory.Exists(bundleDir))
            {
                Console.WriteLine($"Warning: {bundleDir} does not exist.  Skipping");
                return;
            }

            if (!File.Exists(bundlePath))
            {
                Console.WriteLine($"Warning: {bundlePath} does not exist. Skipping");
                return;
            }

            String targetPath = $"{projectDir}\\{bundleFileName}";
            Console.WriteLine($"Copying {bundlePath} to {targetPath}");
            File.Copy(bundlePath, targetPath, true);

            AddBundleFileToProject(csProjPath, bundleFileName);

            Console.WriteLine($"{projectName} PreBuild success");
        }

        private static void AddBundleFileToProject(string csProjPath, string bundleFileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(csProjPath);

            bool foundBundleNode = false;
            bool foundCopyParamNode = false;
            XmlNode lastItemGroup = null;
            XmlNode noneItemGroup = null;

            for (int i = 0; i < doc.ChildNodes[1].ChildNodes.Count && !foundBundleNode && noneItemGroup == null; i++)
            {
                XmlNode node = doc.ChildNodes[1].ChildNodes[i];

                if (node.Name != "ItemGroup") continue;

                lastItemGroup = node;

                if (node.ChildNodes.Count == 0 || node.ChildNodes[0].Name != "None") continue;

                noneItemGroup = node;

                for (int n = 0; n < noneItemGroup.ChildNodes.Count && !foundBundleNode; n++)
                {
                    XmlNode noneNode = noneItemGroup.ChildNodes[n];

                    if (noneNode.Attributes["Include"].Value == bundleFileName)
                    {
                        Console.WriteLine("The bundle file is already included in the project");
                        foundBundleNode = true;

                        foreach (XmlNode iNode in noneNode.ChildNodes)
                        {
                            if (iNode.Name == "CopyToOutputDirectory")
                            {
                                foundCopyParamNode = true;
                                break;
                            }
                        }

                        if (!foundCopyParamNode)
                        {
                            Console.WriteLine("Setting the file 'Copy To Output Directory' parameter to Copy if Newer");
                            noneNode.AppendChild(doc.CreateElement("CopyToOutputDirectory", doc.DocumentElement.NamespaceURI)).InnerText = "PreserveNewest";
                        }
                    }
                }
            }

            if (!foundBundleNode)
            {
                if (noneItemGroup == null)
                {
                    if (lastItemGroup == null)
                        throw new Exception("Could not add file setting to project.  Set it manually to Copy if Newer");

                    noneItemGroup = doc.ChildNodes[1].InsertAfter(doc.CreateElement("ItemGroup", doc.DocumentElement.NamespaceURI), lastItemGroup);
                }

                XmlNode noneNode = noneItemGroup.AppendChild(doc.CreateElement("None", doc.DocumentElement.NamespaceURI));
                noneNode.Attributes.Append(doc.CreateAttribute("Include")).Value = bundleFileName;
                noneNode.AppendChild(doc.CreateElement("CopyToOutputDirectory", doc.DocumentElement.NamespaceURI)).InnerText = "PreserveNewest";
            }

            if (!foundBundleNode || !foundCopyParamNode)
            {
                Console.WriteLine("Saving project");
                doc.Save(csProjPath);
            }
        }
    }
}
