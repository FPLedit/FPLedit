#nullable enable
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FPLedit.Extensibility
{
    internal sealed partial class AssemblySignatureVerifier
    {
        // hashAlgorithm, generatedSignatures are defined in compiler generated file!

        internal SecurityContext Validate(string? fn)
        {
            if (fn == null)
                return SecurityContext.ThirdParty;
            
            var basename = Path.GetFileName(fn);
            var signature = generatedSignatures.FirstOrDefault(s => s.FileName == basename);

            if (string.IsNullOrEmpty(basename) || signature.FileName == null)
                return SecurityContext.ThirdParty;

            var bytes = File.ReadAllBytes(fn);

            using var hasher = HashAlgorithm.Create(hashAlgorithm);
            if (hasher == null)
                return SecurityContext.ThirdParty;
			var hash = hasher.ComputeHash(bytes);

			return hash.SequenceEqual(signature.Hash) ? SecurityContext.Official : SecurityContext.ThirdParty;
        }

        private record struct AssemblySignature(string FileName, byte[] Hash);
    }
}
