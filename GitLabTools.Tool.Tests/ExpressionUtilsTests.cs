using CommandLine;
using GitLabTools;

namespace GitlabTools.Tests;

[TestClass]
public class ExpressionUtilsTests
{
    [TestMethod]
    public void GetAttribute_PropertyHasAttribute_ReturnAttribute()
    {
        var result = ExpressionUtils.GetAttribute<OptionAttribute, string>(() => UtModel.Dummy);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAttribute_PropertyHasNotAttribute_ThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() => ExpressionUtils.GetAttribute<ObsoleteAttribute, string>(() => UtModel.Dummy));
    }

    [TestMethod]
    public void GetAttribute_FunctionIsUsed_ThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() => ExpressionUtils.GetAttribute<ObsoleteAttribute, string>(() => UtModel.Do()));
    }

    [TestMethod]
    public void GetCommandlineArgumentLongName_PropertyHasOptionAttributeWithLongName_ReturnLongName()
    {
        var result = ExpressionUtils.GetCommandlineArgumentLongName(() => UtModel.Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummy", result);
    }

    [TestMethod]
    public void GetOptionAttribute_PropertyHasOptionAttribute_ReturnAttribute()
    {
        var result = ExpressionUtils.GetOptionAttribute(() => UtModel.Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummy", result.LongName);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class UtModel
    {
        [Option('a', "dummy")]
        public static string Dummy => string.Empty;

        public static string Do() => string.Empty;
    }
}
