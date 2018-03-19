using NUnit.Framework;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class WrongSittingTest : AssertionHelper
    {
        [Test]
        public void TrueIfTwoEvaluationInRowAreWrong()
        {
            var rule = new WrongSitting();
            var stats1 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = {Current = new Evaluation(100), BeforeCurrent = new Evaluation(100)}};
            var stats2 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = {Current = new Evaluation(0), BeforeCurrent = new Evaluation(100)}};
            var stats3 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = {Current = new Evaluation(100), BeforeCurrent = new Evaluation(0)}};
            var stats4 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = {Current = new Evaluation(0), BeforeCurrent = new Evaluation(0)}};

            Expect(rule.Check(stats1), Is.False);
            Expect(rule.Check(stats2), Is.False);
            Expect(rule.Check(stats3), Is.False);
            Expect(rule.Check(stats4), Is.True);
        }
    }
}