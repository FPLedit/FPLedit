using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FPLedit.Extensibility
{
    internal class AssemblySignatureVerifier
    {
        private readonly AssemblySignature[] signatures;
        private readonly RSAParameters pubkey;

        public AssemblySignatureVerifier()
        {
            signatures = LoadSignatures().ToArray();

            var x = new XmlSerializer(typeof(RSAParameters));
            using (var stream = EtoExtensions.GetResource(null, "Resources.extensions.pubkey"))
                pubkey = (RSAParameters)x.Deserialize(stream);
        }

        private IEnumerable<AssemblySignature> LoadSignatures()
        {
            using (var stream = EtoExtensions.GetResource(null, "Resources.extensions.sig"))
            using (var sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.StartsWith("#"))
                        continue;
                    var parts = line.Split(':');
                    if (parts.Length != 2)
                        continue;

                    var signatureArray = new byte[parts[1].Length / 2];
                    for (int i = 0; i < parts[1].Length; i += 2)
                        signatureArray[i / 2] = Convert.ToByte(parts[1].Substring(i, 2), 16);

                    yield return new AssemblySignature()
                    {
                        FileName = parts[0],
                        Signature = signatureArray,
                    };
                }
            }
        }

        internal SecurityContext Validate(string fn)
        {
            var basename = Path.GetFileName(fn);
            var signature = signatures.FirstOrDefault(s => s.FileName == basename);

            if (signature.FileName == null)
                return SecurityContext.ThirdParty;

            var bytes = File.ReadAllBytes(fn);
            var sha512 = SHA256.Create();
            var hash = sha512.ComputeHash(bytes);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(pubkey);

            var sigDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            sigDeformatter.SetHashAlgorithm("SHA256");
            if (sigDeformatter.VerifySignature(hash, signature.Signature))
                return SecurityContext.Official;
            return SecurityContext.ThirdParty;
        }

        private struct AssemblySignature
        {
            public string FileName;
            public byte[] Signature;
        }
    }
}
