/*
 * FPLedit Release-Prozess
 * Erstellt kryptographische Schl�ssel zum Signieren von Erweiterungen
 * Aufruf: generate-keypair.csx
 * Version 0.1 / (c) Manuel Huber 2019
 */

#r "System.Xml.dll"
#load "includes.csx"

using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

if (File.Exists("extensions.pubkey") || File.Exists("extensions.privkey"))
{
    Console.Write("Existing keypair found. Overwrite? (y/N) ");
    var choice = Console.ReadKey();
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
        x.Serialize(stream, pubkey);
    using (var stream = File.OpenWrite(path + ".privkey"))
        x.Serialize(stream, privkey);
}
