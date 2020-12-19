namespace InlineCode
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class _OfflineDoc_EmbedOfflineFilesTask : Microsoft.Build.Utilities.Task
    {
        private string _InputFile;
        
        public virtual string InputFile {
            get {
                return _InputFile;
            }
            set {
                _InputFile = value;
            }
        }
        
        private string _ContentFolder;
        
        public virtual string ContentFolder {
            get {
                return _ContentFolder;
            }
            set {
                _ContentFolder = value;
            }
        }
        
        private string _OutputFile;
        
        public virtual string OutputFile {
            get {
                return _OutputFile;
            }
            set {
                _OutputFile = value;
            }
        }
        
        private string _UrlBase;
        
        public virtual string UrlBase {
            get {
                return _UrlBase;
            }
            set {
                _UrlBase = value;
            }
        }

        Dictionary<string, string> files = new Dictionary<string, string>();
   
        string ConvertImage(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var ext = Path.GetExtension(path).Replace(".", "");
            return "data:image/" + ext + ";base64," + Convert.ToBase64String(bytes);
        }

        string ConvertLink(string href, string[] anchors)
        {
            if (!href.StartsWith("mailto:") && !href.StartsWith("http") && !href.StartsWith("#") && !href.StartsWith("/files/"))
            {
                href = string.Join("", href.Split('/'));
                return anchors.Contains(href) ? '#' + href : null;
            }
            else if (href.StartsWith("/files/"))
                return UrlBase + href;
            return href;
        }

        void CollectFiles(string dir)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                foreach (string f in Directory.GetFiles(d, "*.png"))
                    files.Add(Path.GetFileName(f), f);
                CollectFiles(d);
            }
        }

        public override bool Execute() {
            Log.LogMessage(MessageImportance.High, "Embedding images for offline use...!");

            CollectFiles(ContentFolder);

            var text = File.ReadAllText(InputFile);
            text = Regex.Replace(text, "<img.+?src=[\"'](.+?)[\"'].*?>", (m) =>
            {
                var path = m.Groups[1].Value;
                var fn = Path.GetFileName(m.Groups[1].Value);
                if (!files.ContainsKey(fn))
                {
                    Log.LogError("File " + fn + " not found!");
                    return m.Groups[0].Value;
                }
                return m.Groups[0].Value.Replace(path, ConvertImage(files[fn]));
            }, RegexOptions.IgnoreCase);

            var anchors = Regex.Matches(text, "id=\\\"(\\w+)\\\"").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();

            text = Regex.Replace(text, "<a\\s+(?:[^>]*\\s+)?href=([\"'])(.*?)\\1>(.*?)</a>", (m) =>
            {
                var path = m.Groups[2].Value;
                var new_href = ConvertLink(path, anchors);
                if (new_href != null)
                    return m.Groups[0].Value.Replace(path, new_href);
                return m.Groups[3].Value;
            }, RegexOptions.IgnoreCase);

            File.WriteAllText(OutputFile, text);

            Log.LogMessage(MessageImportance.High, "Done writing " + OutputFile + "!");
            return true;
        }
    }
}
