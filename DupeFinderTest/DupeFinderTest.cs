using System.Collections.Generic;
using Xunit;

namespace DupeFinderTest
{
    public class DupeFinderTest
    {
        [Fact]
        public void FindMatches_FindsMatch_Success()
        {
            DupeFinder.DupeFinder dupeFinder = new DupeFinder.DupeFinder();
            dupeFinder.FindMatches(new List<string>(), new List<string>());
        }
    }
}