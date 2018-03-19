using NUnit.Framework;
using System;

namespace SpineHero.UnitTests
{
    [TestFixture]
    internal class TestFixtureExample : AssertionHelper
    {
        [OneTimeSetUp]
        public void Init()
        { /* Called once before all tests. */ }

        [OneTimeTearDown]
        public void Cleanup()
        { /* Called once after all tests. */ }

        [SetUp]
        public void InitBeforeTest()
        { /* Called before each method. */ }

        [TearDown]
        public void CleanupAfterTest()
        { /* Called after each method. */ }

        [Test]
        public void TestExample()
        {
            var pi = Math.PI;
            Expect(pi, EqualTo(Math.PI));
        }

        [Test]
        public void TestThrow()
        {
            int d = 0;
            var method = new TestDelegate(() => d = 42 / d);
            Expect(method, Throws.TypeOf<DivideByZeroException>());
        }


        [TestCase(2, 5, 10)]
        public void TestCaseExample(int a, int b, int sum)
        {
            /* Test with parameters. */
            Expect(a * b, EqualTo(sum));
        }
    }
}