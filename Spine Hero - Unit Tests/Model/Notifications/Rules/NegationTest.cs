using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications.Rules;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class NegationTest : AssertionHelper
    {
        [Test]
        public void WillInverseRule()
        {
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();
            var trueRule = new Mock<IRule>();
            var falseRule = new Mock<IRule>();
            trueRule.Setup(r => r.Check(stats)).Returns(true);
            falseRule.Setup(r => r.Check(stats)).Returns(false);

            var inverse1 = new Negation(trueRule.Object);
            var inverse2 = new Negation(falseRule.Object);

            Expect(inverse1.Check(stats), Is.False);
            Expect(inverse2.Check(stats), Is.True);
        }
    }
}