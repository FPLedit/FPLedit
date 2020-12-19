namespace InlineCode
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;
    using System.Xml.Serialization;
    using System.Xml;


    public class GenerateExtensionSignatures : Microsoft.Build.Utilities.Task
    {

        private string _BinaryPath;

        public virtual string BinaryPath
        {
            get { return _BinaryPath; }
            set { _BinaryPath = value; }
        }

        private string _Namespace;

        public virtual string Namespace
        {
            get { return _Namespace; }
            set { _Namespace = value; }
        }

        private string _TypeName;

        public virtual string TypeName
        {
            get { return _TypeName; }
            set { _TypeName = value; }
        }

        private string _PrivateKeyFile;

        public virtual string PrivateKeyFile
        {
            get { return _PrivateKeyFile; }
            set { _PrivateKeyFile = value; }
        }

        private string _PublicKeyFile;

        public virtual string PublicKeyFile
        {
            get { return _PublicKeyFile; }
            set { _PublicKeyFile = value; }
        }

        private string _OutputPath;

        public virtual string OutputPath
        {
            get { return _OutputPath; }
            set { _OutputPath = value; }
        }

        const string HashAlgorithm = "SHA256";

        public string BytesToString(IEnumerable<byte> bytes)
        {
            return string.Join(", ", System.Linq.Enumerable.Select(bytes, b => "0x" + b.ToString("X2")));
        }

        IEnumerable<string> GetHashLines(DirectoryInfo info, string keypath)
        {
            yield return "// File created at " + DateTime.Now.ToString("G");
            yield return "// Do not edit, changes will be overwritten!";

            yield return "namespace " + Namespace;
            yield return "{";
            yield return "    internal sealed partial class " + TypeName;
            yield return "    {";
            yield return "        private readonly AssemblySignature[] generatedSignatures = {";

            var files = info.GetFiles("FPLedit.*.dll");
            foreach (var f in files)
                yield return CreateSignature(f.FullName, keypath);

            yield return "        };";

            var serializer = new XmlSerializer(typeof(RSAParameters));
            using (var stream = File.OpenRead(PublicKeyFile))
            using (var reader = new XmlTextReader(stream) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null })
            {
                var pubkey = (RSAParameters) serializer.Deserialize(reader);

                yield return @"        private static readonly byte[] pubKeyExponent = { " + BytesToString(pubkey.Exponent) + " };";
                yield return @"        private static readonly byte[] pubKeyModulus = { " + BytesToString(pubkey.Modulus) + " };";
            }

            yield return "        private const string hashAlgorithm = \"" + HashAlgorithm + "\";";

            yield return "    }";
            yield return "}";
        }

        string CreateSignature(string fn, string keyfile)
        {
            var bytes = File.ReadAllBytes(fn);
            var sha512 = SHA256.Create();
            var hash = sha512.ComputeHash(bytes);

            var privkey = LoadParameters(keyfile);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(privkey);

            var sigFormatter = new RSAPKCS1SignatureFormatter(rsa);
            sigFormatter.SetHashAlgorithm(HashAlgorithm);

            var signedHashValue = sigFormatter.CreateSignature(hash);

            return "           new AssemblySignature(\"" + Path.GetFileName(fn) + "\", new byte[] { " + BytesToString(signedHashValue) + " }),";
        }

        RSAParameters LoadParameters(string fn)
        {
            var x = new XmlSerializer(typeof(RSAParameters));
            using (var stream = File.OpenRead(fn))
                return (RSAParameters) x.Deserialize(stream);
        }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Creating extension signatures!\nUsing private key " + PrivateKeyFile + "!");
            DirectoryInfo info = new DirectoryInfo(BinaryPath);
            File.WriteAllLines(OutputPath, GetHashLines(info, PrivateKeyFile));
            Log.LogMessage(MessageImportance.High, "Done writing " + OutputPath + "!");
            return true;
        }

    }
}