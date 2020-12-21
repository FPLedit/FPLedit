using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FPLedit.Extensibility
{
    internal sealed partial class AssemblySignatureVerifier
    {
        // hashAlgorithm, pubkeyExponent, pubkeyModulus, generatedSignatures are defined in compiler generated file!
        
        private readonly RSAParameters pubkey;

        public AssemblySignatureVerifier()
        {
            pubkey = new RSAParameters()
            {
                Modulus = pubKeyModulus,
                Exponent = pubKeyExponent,
            };
        }

        internal SecurityContext Validate(string fn)
        {
            if (fn == null)
                return SecurityContext.ThirdParty;
            
            var basename = Path.GetFileName(fn);
            var signature = generatedSignatures.FirstOrDefault(s => s.FileName == basename);

            if (signature.FileName == null)
                return SecurityContext.ThirdParty;

            var bytes = File.ReadAllBytes(fn);

            using (var hasher = HashAlgorithm.Create(hashAlgorithm))
            using (var rsa = new RSACryptoServiceProvider())
            {
                var hash = hasher.ComputeHash(bytes);

                rsa.ImportParameters(pubkey);

                var sigDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                sigDeformatter.SetHashAlgorithm(hashAlgorithm);
                return sigDeformatter.VerifySignature(hash, signature.Signature) ? SecurityContext.Official : SecurityContext.ThirdParty;
            }
        }

        private readonly struct AssemblySignature
        {
            public readonly string FileName;
            public readonly byte[] Signature;

            public AssemblySignature(string fn, byte[] signature)
            {
                FileName = fn;
                Signature = signature;
            }
        }
    }
}