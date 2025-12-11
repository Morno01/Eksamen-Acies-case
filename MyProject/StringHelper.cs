namespace MyProject;

/// <summary>
/// Utility class providing various string manipulation methods
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Checks if a string is a valid email address
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var localPart = parts[0];
        var domainPart = parts[1];

        if (string.IsNullOrWhiteSpace(localPart) || string.IsNullOrWhiteSpace(domainPart))
            return false;

        if (!domainPart.Contains('.'))
            return false;

        return true;
    }

    /// <summary>
    /// Truncates a string to a specified length and adds ellipsis if needed
    /// </summary>
    public static string Truncate(string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (maxLength < 0)
            throw new ArgumentException("Max length cannot be negative", nameof(maxLength));

        if (value.Length <= maxLength)
            return value;

        var truncatedLength = maxLength - suffix.Length;
        if (truncatedLength < 0)
            truncatedLength = 0;

        return value.Substring(0, truncatedLength) + suffix;
    }

    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized)
    /// </summary>
    public static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var words = value.ToLower().Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }

        return string.Join(" ", words);
    }

    /// <summary>
    /// Counts the number of words in a string
    /// </summary>
    public static int CountWords(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var words = value.Split(new[] { ' ', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries);

        return words.Length;
    }

    /// <summary>
    /// Reverses a string
    /// </summary>
    public static string Reverse(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Checks if a string is a palindrome (reads the same forwards and backwards)
    /// </summary>
    public static bool IsPalindrome(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        var cleaned = value.Replace(" ", "").ToLower();
        return cleaned == Reverse(cleaned);
    }

    /// <summary>
    /// Removes all non-alphanumeric characters from a string
    /// </summary>
    public static string RemoveNonAlphanumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return new string(value.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    /// <summary>
    /// Converts a camelCase or PascalCase string to snake_case
    /// </summary>
    public static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLower(value[0]));

        for (int i = 1; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                result.Append('_');
                result.Append(char.ToLower(value[i]));
            }
            else
            {
                result.Append(value[i]);
            }
        }

        return result.ToString();
    }
}
