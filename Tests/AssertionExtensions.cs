using FluentAssertions;
using FluentAssertions.Execution;

namespace Tests
{
    public static class AssertionExtensions
    {
        public static FileAssertion Should(this FileToTest file) => new FileAssertion {File = file};
        public class FileAssertion
        {
            public FileToTest File;
            public AndConstraint<FileAssertion> NotExist(string because = "",params object[] reasonArgs)
            {
                Execute.Assertion
                    .ForCondition(!System.IO.File.Exists(File.Path))
                    .BecauseOf(because,reasonArgs)
                    .FailWith("Did not expect file to exist at {0} {Reason}",File.Path);
                return new AndConstraint<FileAssertion>(this);
            }

            public AndConstraint<FileAssertion> Exist(string because = "", params object[] reasonArgs)
            {
                Execute.Assertion
                    .ForCondition(System.IO.File.Exists(File.Path))
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected file to exist at {0} {Reason}", File.Path);
                return new AndConstraint<FileAssertion>(this);
            }
        }
        public static FileToTest File(string fName) => new FileToTest { Path = fName };
        public class FileToTest {public string Path;}
    }
}