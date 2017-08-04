using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ofscrm.PluginRegistration.Tests
{
    [TestClass]
    public class UpdatePluginsCommandTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            UpdatePluginsCommandOptions options = new UpdatePluginsCommandOptions
            {
                PluginsFolder = "C:\\Users\\lghersi\\Projects\\ClientEngage\\Sources\\Product\\Development\\Build\\plugins"
            };
            UpdatePluginsCommand command = new UpdatePluginsCommand(options);

            command.ExecuteCommand();

            Assert.Inconclusive();
        }
    }
}
