using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkMonkey
{
    public class Scriptlet
    {
        public string Name { get; set; }
        public string Script { get; set; }
        public string UrlMatch { get; internal set; }
        public bool Disabled { get; set; }

        public string Path { get; set; }
        const string urlMatchPrefix = "//UrlMatch:";
        const string namePrefix = "//Name:";
        const string disableFlag = "//Disabled";
        public Scriptlet(string path)
        {
            Path = path;
            var lines = File.ReadAllLines(path);

            var chk = lines.SingleOrDefault(l => l.StartsWith(urlMatchPrefix));
            if (string.IsNullOrEmpty(chk))
            {
                throw new ApplicationException($"{path}: Missing {urlMatchPrefix} setting");
            }
            else
            {
                UrlMatch = chk.Substring(urlMatchPrefix.Length);
                chk = lines.SingleOrDefault(l => l.StartsWith(namePrefix));
                Name = chk == null ? System.IO.Path.GetFileName(path) : chk.Substring(namePrefix.Length);
                Disabled = lines.Any(l => l == disableFlag);
                Script = string.Join("\n", lines.Where(l => !l.StartsWith("//")).ToArray());
            }
        }

        public void Toggle(bool enabled)
        {
            if (enabled != !Disabled)
            {
                var lines = File.ReadAllLines(Path).ToList();
                var disabledLine = lines.SingleOrDefault(l => l == disableFlag);
                if (enabled && disabledLine != null) lines.Remove(disabledLine);
                if (!enabled && disabledLine == null) lines.Insert(0, disableFlag);
                File.WriteAllLines(Path, lines);
            }
        }
    }
}
