/*
 * FPLedit Release-Prozess
 * Erstellt kryptographische Schlüssel zum Signieren von Erweiterungen
 * Aufruf: generate-keypair.csx
 * Version 0.2 / (c) Manuel Huber 2020
 */

#r "System.Xml.dll"

using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

if (File.Exists("extensions.pubkey") || File.Exists("extensions.privkey"))
{
    Console.Write("Existing keypair found. Overwrite? (y/N) ");
    var choice = Console.ReadKey();
    Console.WriteLine(); // Finish line
    if (choice.KeyChar == 'y')
        KeyGen("extensions");
}
else
    KeyGen("extensions");

/*
 * Helper function: Generate a new keypair and serialize
 */
void KeyGen(string path)
{
    var rsa = new RSACryptoServiceProvider(2048);
    var privkey = rsa.ExportParameters(true);
    var pubkey = rsa.ExportParameters(false);

    var x = new XmlSerializer(typeof(RSAParameters));
    using (var stream = File.OpenWrite(path + ".pubkey")) 
    {
        x.SetLength(0);
        x.Serialize(stream, pubkey);
    }
    using (var stream = File.OpenWrite(path + ".privkey"))
    {
        x.SetLength(0);
        x.Serialize(stream, privkey);
    }
}

