using System.IO;
using System.Linq;
using System.Reflection;
using FPLedit.Config;
using FPLedit.Extensibility;
using FPLedit.Shared;
using FPLedit.Templating;
using NUnit.Framework;

namespace FPLedit.Tests
{
    public class BuiltinTemplateTest
    {
        private TemplateManager templateManager;
        private const string DEFAULT_TEMPLATE_PATH = "templates";
        
        [SetUp]
        public void SetupTemplates()
        {
            // Load default setting (including extmgr.enabled, to get the default enabled extensions).
            var mainAssembly = typeof(Bootstrapper).Assembly;
            using var defaultConfigFile = mainAssembly.GetManifestResourceStream("FPLedit.Resources.fpledit.conf");
            using var settings = new Settings(defaultConfigFile);

            // Initialize a shallow dummy plugin interface.
            var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pi = new DummyPluginInterface
            {
                settings = settings,
                ExecutableDir = execPath
            };
            PathManager.Instance.AppDirectory = execPath;

            // Load the default extension set.
            var extensionManager = new ExtensionManager(pi);
            extensionManager.LoadExtensions();
            
            // Initialize (already loaded) extensions.
            var enabledPlugins = extensionManager.Plugins.Where(p => p.Enabled);
            foreach (var plugin in enabledPlugins)
                if (plugin.Plugin is ITemplatePlugin tp)
                    tp.InitTemplates(pi, pi.Registry);
            
            // Load templates from files & extensions
            templateManager = new TemplateManager(pi.Registry, pi, DEFAULT_TEMPLATE_PATH);
            templateManager.LoadTemplates(DEFAULT_TEMPLATE_PATH);
        }

        [Test]
        public void TemplateCompileTest()
        {
            TestTemplateType(null, new Timetable(TimetableType.Linear));
            TestTemplateType(null, new Timetable(TimetableType.Network));
        }
        
        private void TestTemplateType(string type, Timetable tt)
        {
        	var templates = type == null ? templateManager.GetAllTemplates() : templateManager.GetTemplates(type);
        	foreach (var t in templates)
        		t.GenerateResult(tt);
        }
    }
}