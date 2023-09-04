using System.IO;
using System.Linq;
using System.Reflection;
using FPLedit.Config;
using FPLedit.Extensibility;
using FPLedit.Shared;
using FPLedit.Templating;
using NUnit.Framework;

namespace FPLedit.Tests;

public class BuiltinTemplateTest
{
    private TemplateManager templateManager = null!;
    private const string DEFAULT_TEMPLATE_PATH = "templates";
        
    [SetUp]
    public void SetupTemplates()
    {
        // Load default setting (including extmgr.enabled, to get the default enabled extensions).
        var mainAssembly = typeof(Bootstrapper).Assembly;
        using var defaultConfigFile = mainAssembly.GetManifestResourceStream("FPLedit.Resources.fpledit.conf");
        using var settings = new Settings(defaultConfigFile);

        // Initialize a shallow dummy plugin interface.
        var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
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
            
        // Attach a testing "debugger"
        TemplateDebugger.GetInstance().AttachDebugger(new TestDebugger());
    }

    [Test]
    public void TemplateCompileTest()
    {
        TestTemplateType(null, new Timetable(TimetableType.Linear));
        TestTemplateType(null, new Timetable(TimetableType.Network));
    }
        
    private void TestTemplateType(string? type, Timetable tt)
    {
        var templates = type == null ? templateManager.GetAllTemplates() : templateManager.GetTemplates(type);
        foreach (var t in templates)
            t.GenerateResult(tt);
    }
}

internal sealed class TestDebugger : ITemplateDebugger
{
    private string? generatedCode;
    private string? identifier;

    public void SetContext(JavascriptTemplate template)
    {
        identifier = template.Identifier;
        generatedCode = TemplateDebugger.GetGeneratedCode(template);
    }

    public void Navigate(int line, int column) // We have an error.
    {
        var (start, end) = TemplateDebugger.GetNavigationOffsets(generatedCode!, line, column, 4, 4);
        var codePart = generatedCode!.Substring(start, end - start).Replace($"{line,4}", "--->");
        TestContext.Error.WriteLine(codePart);
    }

    public void OpenDebugger() // We have an error, nop.
    {
    }
}