using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace FPLedit.Extensibility
{
    internal sealed partial class AssemblySignatureVerifier
    {
        private readonly RSAParameters pubkey;

        public AssemblySignatureVerifier()
        {
            var x = new XmlSerializer(typeof(RSAParameters));
            using (var reader = new XmlTextReader(publicKey, XmlNodeType.Document, null) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null })
                pubkey = (RSAParameters) x.Deserialize(reader);
        }

        internal SecurityContext Validate(string fn)
        {
            var basename = Path.GetFileName(fn);
            var signature = generatedSignatures.FirstOrDefault(s => s.FileName == basename);

            if (signature.FileName == null)
                return SecurityContext.ThirdParty;

            var bytes = File.ReadAllBytes(fn);

            using (var sha512 = SHA256.Create())
            using (var rsa = new RSACryptoServiceProvider())
            {
                var hash = sha512.ComputeHash(bytes);

                rsa.ImportParameters(pubkey);

                var sigDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                sigDeformatter.SetHashAlgorithm("SHA256");
                return sigDeformatter.VerifySignature(hash, signature.Signature) ? SecurityContext.Official : SecurityContext.ThirdParty;
            }
        }

        private struct AssemblySignature
        {
            public readonly string FileName;
            public readonly byte[] Signature;

            public AssemblySignature(string fn, string hexSignature)
            {
                FileName = fn;
                Signature = new byte[hexSignature.Length / 2];
                for (var i = 0; i < hexSignature.Length; i += 2)
                    Signature[i / 2] = Convert.ToByte(hexSignature.Substring(i, 2), 16);
            }
        }
    }
}