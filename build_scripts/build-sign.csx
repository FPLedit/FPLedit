/*
 * FPLedit Release-Prozess
 * Erstellt kryptographische Signaturen der Erweiterungen
 * Aufruf: build-sign.csx $(SolutionDir) $(TargetDir)
 * Version 0.2 / (c) Manuel Huber 2020
 */

#r "System.Xml.dll"

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Collections.Generic;

var code_path = Path.GetFullPath(Args[0]);
var bin_path = Path.GetFullPath(Args[1]);

var signature_output_path = Path.Combine(code_path, "build_tmp/extensions.sig");
var keypath = Path.Combine(code_path, "build_scripts/extensions.privkey");

/*
 * TASK: Generate extension signatures
 */
Console.Write("Prebuild: Erzeuge Erweiterungs-Signaturen... ");
DirectoryInfo info = new DirectoryInfo(bin_path);
File.WriteAllLines(signature_output_path, GetHashLines(info).ToArray());
Console.WriteLine("Fertig!");

IEnumerable<string> GetHashLines(DirectoryInfo info)
{
    yield return "# File created at " + DateTime.Now.ToString("G");
    var files = info.GetFiles("FPLedit.*.dll");
    foreach (var f in files)
        yield return CreateSignature(f.FullName, keypath);
}

/*
 * Helper functions: Create a signature of a given file
 */
string CreateSignature(string fn, string keyfile)
{
    var bytes = File.ReadAllBytes(fn);
    var sha512 = SHA256.Create();
    var hash = sha512.ComputeHash(bytes);

    var privkey = LoadParameters(keyfile);

    var rsa = new RSACryptoServiceProvider();
    rsa.ImportParameters(privkey);

    var sigFormatter = new RSAPKCS1SignatureFormatter(rsa);
    sigFormatter.SetHashAlgorithm("SHA256");

    var signedHashValue = sigFormatter.CreateSignature(hash);

    string hex = BitConverter.ToString(signedHashValue).Replace("-", "");
    return Path.GetFileName(fn) + ":" + hex;
}

RSAParameters LoadParameters(string fn)
{
    var x = new XmlSerializer(typeof(RSAParameters));
    using (var stream = File.OpenRead(fn))
        return (RSAParameters)x.Deserialize(stream);
}
