namespace MyProject.Tests;

public class StringHelperTests
{
    #region IsValidEmail Tests

    [Fact]
    public void IsValidEmail_ValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = StringHelper.IsValidEmail(email);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidEmail_ValidEmailWithSubdomain_ReturnsTrue()
    {
        // Arrange
        var email = "user@mail.example.com";

        // Act
        var result = StringHelper.IsValidEmail(email);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidEmail_NullOrWhitespace_ReturnsFalse(string email)
    {
        // Act
        var result = StringHelper.IsValidEmail(email);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@@example.com")]
    [InlineData("user@domain")]
    public void IsValidEmail_InvalidEmail_ReturnsFalse(string email)
    {
        // Act
        var result = StringHelper.IsValidEmail(email);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Truncate Tests

    [Fact]
    public void Truncate_StringShorterThanMaxLength_ReturnsOriginal()
    {
        // Arrange
        var value = "Hello";

        // Act
        var result = StringHelper.Truncate(value, 10);

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Truncate_StringLongerThanMaxLength_ReturnsTruncatedWithEllipsis()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = StringHelper.Truncate(value, 8);

        // Assert
        Assert.Equal("Hello...", result);
    }

    [Fact]
    public void Truncate_CustomSuffix_UsesCustomSuffix()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = StringHelper.Truncate(value, 8, "---");

        // Assert
        Assert.Equal("Hello---", result);
    }

    [Fact]
    public void Truncate_NullString_ReturnsNull()
    {
        // Act
        var result = StringHelper.Truncate(null, 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Truncate_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = StringHelper.Truncate("", 10);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Truncate_NegativeMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var value = "Hello";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => StringHelper.Truncate(value, -1));
    }

    [Fact]
    public void Truncate_MaxLengthZero_ReturnsOnlySuffix()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = StringHelper.Truncate(value, 0);

        // Assert
        Assert.Equal("...", result);
    }

    #endregion

    #region ToTitleCase Tests

    [Fact]
    public void ToTitleCase_LowercaseString_ReturnsCapitalized()
    {
        // Arrange
        var value = "hello world";

        // Act
        var result = StringHelper.ToTitleCase(value);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ToTitleCase_UppercaseString_ReturnsCapitalized()
    {
        // Arrange
        var value = "HELLO WORLD";

        // Act
        var result = StringHelper.ToTitleCase(value);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ToTitleCase_MixedCaseString_ReturnsCapitalized()
    {
        // Arrange
        var value = "hELLo WoRLd";

        // Act
        var result = StringHelper.ToTitleCase(value);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToTitleCase_NullOrWhitespace_ReturnsOriginal(string value)
    {
        // Act
        var result = StringHelper.ToTitleCase(value);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ToTitleCase_SingleWord_ReturnsCapitalized()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = StringHelper.ToTitleCase(value);

        // Assert
        Assert.Equal("Hello", result);
    }

    #endregion

    #region CountWords Tests

    [Fact]
    public void CountWords_SimpleString_ReturnsCorrectCount()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void CountWords_MultipleSpaces_ReturnsCorrectCount()
    {
        // Arrange
        var value = "Hello    World    Test";

        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void CountWords_WithTabs_ReturnsCorrectCount()
    {
        // Arrange
        var value = "Hello\tWorld\tTest";

        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void CountWords_WithNewlines_ReturnsCorrectCount()
    {
        // Arrange
        var value = "Hello\nWorld\nTest";

        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(3, result);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    public void CountWords_NullOrWhitespace_ReturnsZero(string value, int expected)
    {
        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CountWords_SingleWord_ReturnsOne()
    {
        // Arrange
        var value = "Hello";

        // Act
        var result = StringHelper.CountWords(value);

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region Reverse Tests

    [Fact]
    public void Reverse_SimpleString_ReturnsReversed()
    {
        // Arrange
        var value = "Hello";

        // Act
        var result = StringHelper.Reverse(value);

        // Assert
        Assert.Equal("olleH", result);
    }

    [Fact]
    public void Reverse_StringWithSpaces_ReturnsReversed()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = StringHelper.Reverse(value);

        // Assert
        Assert.Equal("dlroW olleH", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Reverse_NullOrEmpty_ReturnsOriginal(string value)
    {
        // Act
        var result = StringHelper.Reverse(value);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Reverse_SingleCharacter_ReturnsSameCharacter()
    {
        // Arrange
        var value = "A";

        // Act
        var result = StringHelper.Reverse(value);

        // Assert
        Assert.Equal("A", result);
    }

    #endregion

    #region IsPalindrome Tests

    [Theory]
    [InlineData("racecar")]
    [InlineData("level")]
    [InlineData("noon")]
    [InlineData("A")]
    public void IsPalindrome_ValidPalindrome_ReturnsTrue(string value)
    {
        // Act
        var result = StringHelper.IsPalindrome(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPalindrome_PalindromeWithSpaces_ReturnsTrue()
    {
        // Arrange
        var value = "race car";

        // Act
        var result = StringHelper.IsPalindrome(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPalindrome_MixedCase_ReturnsTrue()
    {
        // Arrange
        var value = "RaceCar";

        // Act
        var result = StringHelper.IsPalindrome(value);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("world")]
    [InlineData("test")]
    public void IsPalindrome_NotPalindrome_ReturnsFalse(string value)
    {
        // Act
        var result = StringHelper.IsPalindrome(value);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPalindrome_NullOrEmpty_ReturnsTrue(string value)
    {
        // Act
        var result = StringHelper.IsPalindrome(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region RemoveNonAlphanumeric Tests

    [Fact]
    public void RemoveNonAlphanumeric_StringWithSpecialCharacters_ReturnsOnlyAlphanumeric()
    {
        // Arrange
        var value = "Hello, World! 123";

        // Act
        var result = StringHelper.RemoveNonAlphanumeric(value);

        // Assert
        Assert.Equal("HelloWorld123", result);
    }

    [Fact]
    public void RemoveNonAlphanumeric_OnlyAlphanumeric_ReturnsOriginal()
    {
        // Arrange
        var value = "Hello123";

        // Act
        var result = StringHelper.RemoveNonAlphanumeric(value);

        // Assert
        Assert.Equal("Hello123", result);
    }

    [Fact]
    public void RemoveNonAlphanumeric_OnlySpecialCharacters_ReturnsEmpty()
    {
        // Arrange
        var value = "!@#$%^&*()";

        // Act
        var result = StringHelper.RemoveNonAlphanumeric(value);

        // Assert
        Assert.Equal("", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemoveNonAlphanumeric_NullOrEmpty_ReturnsOriginal(string value)
    {
        // Act
        var result = StringHelper.RemoveNonAlphanumeric(value);

        // Assert
        Assert.Equal(value, result);
    }

    #endregion

    #region ToSnakeCase Tests

    [Fact]
    public void ToSnakeCase_PascalCase_ReturnsSnakeCase()
    {
        // Arrange
        var value = "HelloWorld";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("hello_world", result);
    }

    [Fact]
    public void ToSnakeCase_CamelCase_ReturnsSnakeCase()
    {
        // Arrange
        var value = "helloWorld";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("hello_world", result);
    }

    [Fact]
    public void ToSnakeCase_MultipleWords_ReturnsSnakeCase()
    {
        // Arrange
        var value = "HelloWorldTest";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("hello_world_test", result);
    }

    [Fact]
    public void ToSnakeCase_SingleWord_ReturnsLowercase()
    {
        // Arrange
        var value = "Hello";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ToSnakeCase_AllLowercase_ReturnsOriginal()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("hello", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToSnakeCase_NullOrWhitespace_ReturnsOriginal(string value)
    {
        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ToSnakeCase_ConsecutiveUppercase_ReturnsSnakeCase()
    {
        // Arrange
        var value = "HTTPRequest";

        // Act
        var result = StringHelper.ToSnakeCase(value);

        // Assert
        Assert.Equal("h_t_t_p_request", result);
    }

    #endregion
}
