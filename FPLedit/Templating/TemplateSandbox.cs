using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace FPLedit.Templating
{
    internal class TemplateSandbox
    {
        public string GenerateResult(string code, string[] assemblyReferences, Timetable tt)
        {
            // Compiling in current AppDomain: Not security critical, assemblies will only be written to disk.
            var assemblyPath = new TemplateCompiler().CompileAssembly(code, assemblyReferences);

            var adSetup = new AppDomainSetup()
            {
                ApplicationBase = Path.GetFullPath("sandbox" + DateTime.Now.Ticks),
            };
            var domain = CreateSandboxDomain(adSetup, assemblyPath); // Create new, isolated AppDomain

            // Run template in another AppDomain
            var sandbox = (TemplateSandboxRunner)Activator.CreateInstanceFrom(domain, typeof(TemplateSandboxRunner).Assembly.Location, typeof(TemplateSandboxRunner).FullName).Unwrap();
            sandbox.InstallResolver(AppDomain.CurrentDomain.BaseDirectory);
            var ret = sandbox.RunInAppDomain(tt, assemblyPath);

            AppDomain.Unload(domain);

            File.Delete(assemblyPath); // Cleanup

            return ret;
        }

        private AppDomain CreateSandboxDomain(AppDomainSetup adSetup, string templateAssemblyPath)
        {
            // Create restricted permissions
            var permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var fperm = new FileIOPermission(PermissionState.None)
            {
                AllFiles = FileIOPermissionAccess.NoAccess
            };
            var roAccess = FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery;
            fperm.AddPathList(roAccess, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)); // App directory -> Load Dependencies
            fperm.AddPathList(roAccess, Path.GetDirectoryName(templateAssemblyPath)); // Directory containing templateAssembly
            permSet.AddPermission(fperm);

            // StrongNames of FullTrust-Assemblies
            var sharedAssembly = GetStrongName(typeof(Timetable));
            var clientAssembly = GetStrongName(typeof(Template));

            // Create new, isolated AppDomain
            return AppDomain.CreateDomain("tmpl-run-domain", null, adSetup, permSet, sharedAssembly, clientAssembly);
        }

        private StrongName GetStrongName(Type type)
        {
            AssemblyName aname = type.Assembly.GetName();
            byte[] pubkey = aname.GetPublicKey();
            var blob = new StrongNamePublicKeyBlob(pubkey);
            return new StrongName(blob, aname.Name, aname.Version);
        }
    }
}
