using System.IO;
using System.Linq;
using FPLedit.Config;
using NUnit.Framework;

namespace FPLedit.Tests
{
    public class CoreComponenentsTest
    {
        [Test]
        public void PathManagerTests()
        {
            PathManager.Instance.AppFilePath = "/test_root/fpledit/fpledit.exe";
            Assert.AreEqual("/test_root/fpledit/fpledit.exe", PathManager.Instance.AppFilePath);
            Assert.AreEqual("/test_root/fpledit", PathManager.Instance.AppDirectory);
            PathManager.Instance.AppFilePath = "F:/test_root/fpledit/fpledit.exe";
            Assert.AreEqual("F:/test_root/fpledit/fpledit.exe", PathManager.Instance.AppFilePath);
            Assert.AreEqual("F:/test_root/fpledit", PathManager.Instance.AppDirectory);
        }

        [Test]
        public void RegisterStoreTests()
        {
            using (var registry = new RegisterStore())
            {
                var test11 = new Test11();
                var test112 = new Test11();
                var test12 = new Test12();
                var test21 = new Test21();
                registry.Register<ITest1>(test11);
                registry.Register<ITest1>(test12);
                registry.Register<ITest1>(test112);
                registry.Register<ITest2>(test21);
                
                Assert.IsTrue(registry.GetRegistered<ITest1>().SequenceEqual(new ITest1[]{ test11, test12, test112 }));
                Assert.IsTrue(registry.GetRegistered<ITest2>().SequenceEqual(new ITest2[]{ test21}));
            }
        }

        [Test]
        public void SettingsTest()
        {
            PathManager.Instance.AppFilePath = Path.GetTempFileName(); // Set root to temp
            var settings = new Settings();
            
            //TODO: finish
        }
    }
    
    internal interface ITest1 {}
    internal interface ITest2 {}
    internal class Test11 : ITest1 {}
    internal class Test12 : ITest1 {}
    internal class Test21 : ITest2 {}
}