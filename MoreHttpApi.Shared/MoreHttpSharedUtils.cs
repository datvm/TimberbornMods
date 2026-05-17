namespace MoreHttpApi.Shared;

public static class MoreHttpApiUtils
{

    public const string EndpointStart = nameof(MoreHttpApi);

    extension(HttpNumericComparisonMode mode)
    {
        public char ToChar() => mode switch
        {
            HttpNumericComparisonMode.Greater => '>',
            HttpNumericComparisonMode.Less => '<',
            HttpNumericComparisonMode.GreaterOrEqual => '≥',
            HttpNumericComparisonMode.LessOrEqual => '≤',
            HttpNumericComparisonMode.Equal => '=',
            HttpNumericComparisonMode.NotEqual => '≠',
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
