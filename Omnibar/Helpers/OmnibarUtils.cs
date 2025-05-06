namespace Omnibar.Helpers;

public static class OmnibarUtils
{

    public static bool MatchText(string keywordLowerCase, string text)
    {
        if (string.IsNullOrEmpty(text)) { return false; }

        int keywordIndex = 0;
        int textIndex = 0;

        text = text.ToLower();

        while (keywordIndex < keywordLowerCase.Length && textIndex < text.Length)
        {
            if (keywordLowerCase[keywordIndex] == text[textIndex])
            {
                keywordIndex++;
            }
            textIndex++;
        }

        var match = keywordIndex == keywordLowerCase.Length;
        return match;
    }

}
