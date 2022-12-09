using Xunit;

namespace blazorSBIFS.Server.Unit_Tests;
using blazorSBIFS.Server.Tools;

public class HashStringXUnitTests
{
    [Fact]
    public void HashStringTest()
    {
       string testString = "CheckingIfYourPasswordIsStrongEnough";
       string actual = SecurityTools.HashString(testString);
       string expected = "9EE84C51A490B9135DF3423352B761D89BA5AB9BF30F75AC1F87497CA5EC7A8D";
       Assert.Equal(expected, actual);
    }
    [Fact]
    public void HashStringIsNullTest()
    {
       string testString = null;
       string actual = SecurityTools.HashString(testString);
       string expected = null;
       Assert.NotEqual(expected, actual);
    }
}