namespace IslaNova.Core.Application.Helpers
{
    public static class RandomDigitSequenceHelper
    {
        public static string Generate(int sequenceLength)
        {
            var random = new Random();
            List<int> digits = [];

            for (var i = 0; i < sequenceLength; i++)
            {
                digits.Add(random.Next(0, 10));
            }

            string result = string.Join("", digits);

            return result;
        }
    }
}
