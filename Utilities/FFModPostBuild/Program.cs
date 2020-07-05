using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace FFModPostBuild
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(String.Format("Args Length:{0}", args.Length));

            if (args.Length != 2)
                throw new ArgumentException("Not enough parameters");

            for (int i = 0; i < args.Length; i++)
                Console.WriteLine(String.Format("Args[{0}]={1}", i, args[i]));

            if (!Directory.Exists(args[0]))
                throw new ArgumentException("Project output directory does not exist");

            String projectDir = args[0];
            String modName = args[1];

            String userAppData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            String modDir = userAppData + @"\AppData\LocalLow\Oachkatzlschwoaf Interactive\Founders Fortune\mods\" + modName + "\\";

            Console.WriteLine("User's mod directory: " + modDir);

            if (Directory.Exists(modDir))
            {
                Console.WriteLine("Emptying directory");
                Directory.Delete(modDir, true);
            }

            Console.WriteLine("Creating mod directory");
            Directory.CreateDirectory(modDir);

            String configFile = Directory.GetFiles(projectDir, "config.json").FirstOrDefault();

            if (configFile == null) throw new Exception("config.json is missing");

            String modAssemblyName = null;

            using (var strReader = new StringReader(File.ReadAllText(configFile)))
            using (var reader = new JsonTextReader(strReader))
            {
                while(reader.Read())
                {
                    if(reader.Value != null)
                    {
                        if(reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == "name")
                        {
                            reader.Read();

                            modAssemblyName = reader.Value.ToString();
                            break;
                        }
                    }
                }
            }

            if (modAssemblyName == null) throw new Exception("Could not determine assembly name from config file");

            Console.WriteLine("Mod assembly name= " + modAssemblyName);

            foreach (var file in Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories))
            {
                String ext = Path.GetExtension(file);

                if (ext == ".pdb" || ext == ".xml") continue;

                if (ext == ".dll")
                {
                    String fileName = Path.GetFileNameWithoutExtension(file);

                    if (fileName != "0Harmony" && fileName != modAssemblyName && fileName != "ModSettingsFramework") continue;
                }

                String newPath = file.Replace(projectDir, modDir);

                String newPathDir = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(newPathDir)) Directory.CreateDirectory(newPathDir);

                Console.WriteLine("Copying " + Path.GetFileName(file) + " to " + newPath);
                File.Copy(file, newPath);
            }
        }
    }
}
