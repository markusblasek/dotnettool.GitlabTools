using System.Text.Json.Serialization;
using CommandLine;
using GitLabTools;

namespace GitlabTools.Tests;

[TestClass]
public class ExpressionUtilsTests
{

    [TestMethod]
    public void GetAttribute_PropertyHasAttribute_ReturnAttribute()
    {
        var result = ExpressionUtils.GetAttribute<JsonPropertyNameAttribute, string>(() => new UtModel().Dummy);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAttribute_PropertyHasNotAttribute_ThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() => ExpressionUtils.GetAttribute<ObsoleteAttribute, string>(() => new UtModel().Dummy));
    }

    [TestMethod]
    public void GetCommandlineArgumentLongName_PropertyHasOptionAttributeWithLongName_ReturnLongName()
    {
        var result = ExpressionUtils.GetCommandlineArgumentLongName(() => new UtModel().Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummy", result);
    }

    [TestMethod]
    public void GetOptionAttribute_PropertyHasOptionAttribute_ReturnAttribute()
    {
        var result = ExpressionUtils.GetOptionAttribute(() => new UtModel().Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummy", result.LongName);
    }

    [TestMethod]
    public void GetJsonPropertyName_PropertyHasJsonPropertyNameAttribute_ReturnName()
    {
        var result = ExpressionUtils.GetJsonPropertyName(() => new UtModel().Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummyJson", result);
    }

    [TestMethod]
    public void GetJsonPropertyNameAttribute_PropertyHasJsonPropertyNameAttribute_ReturnAttribute()
    {
        var result = ExpressionUtils.GetJsonPropertyNameAttribute(() => new UtModel().Dummy);
        Assert.IsNotNull(result);
        Assert.AreEqual("dummyJson", result.Name);
    }

    private class UtModel
    {
        [JsonPropertyName("dummyJson")]
        [Option('a', "dummy")]
        public string Dummy => string.Empty;
    }

    
}
