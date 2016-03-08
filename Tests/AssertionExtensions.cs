using FluentAssertions;
using FluentAssertions.Execution;

namespace Tests
{
    public static class AssertionExtensions
    {
        public static FileAssertion Should(this FileToTest file) => new FileAssertion {File = file};
        public static DirectoryAssertion Should(this DirToTest file) => new DirectoryAssertion { Directory = file};
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

        public class DirectoryAssertion
        {
            public DirToTest Directory;
            public AndConstraint<DirectoryAssertion> NotExist(string because = "", params object[] reasonArgs)
            {
                Execute.Assertion
                    .ForCondition(!System.IO.Directory.Exists(Directory.Path))
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Did not expect file to exist at {0} {Reason}", Directory.Path);
                return new AndConstraint<DirectoryAssertion>(this);
            }

            public AndConstraint<DirectoryAssertion> Exist(string because = "", params object[] reasonArgs)
            {
                Execute.Assertion
                    .ForCondition(System.IO.Directory.Exists(Directory.Path))
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected file to exist at {0} {Reason}", Directory.Path);
                return new AndConstraint<DirectoryAssertion>(this);
            }
        }
        

        public static FileToTest File(string fName) => new FileToTest { Path = fName };
        public static DirToTest Dir(string fName) => new DirToTest { Path = fName };
        public class FileToTest {public string Path; }
        public class DirToTest { public string Path; }
    }
}