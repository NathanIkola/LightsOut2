using jl08lib.Translation;

namespace jl08lib.Tests.Mocks
{
    /// <summary>
    /// A translation service that just echoes the input out
    /// </summary>
    internal class MockStringTranslator : IStringTranslator
    {
        public string Translate(string toTranslate)
        {
            return toTranslate;
        }
    }
}