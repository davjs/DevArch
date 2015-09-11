// <copyright file="AnalyserTest.cs">Copyright ©  2015</copyright>
using System;
using Analysis;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests
{
    /// <summary>This class contains parameterized unit tests for Analyser</summary>
    [PexClass(typeof(Analyser))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class AnalyserTest
    {
        /// <summary>Test stub for SplitNameSpaceName(String)</summary>
        [PexMethod]
        [PexAllowedException(typeof(NullReferenceException))]
        public Analyser.NameSpace SplitNameSpaceNameTest([PexAssumeUnderTest]Analyser target, string name)
        {
            Analyser.NameSpace result = target.SplitNameSpaceName(name);
            return result;
            // TODO: add assertions to method AnalyserTest.SplitNameSpaceNameTest(Analyser, String)
        }
    }
}
