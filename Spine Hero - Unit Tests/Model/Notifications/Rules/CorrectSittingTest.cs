using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
  [TestFixture]
  public class CorrectSittingTest : AssertionHelper
  {
    [Test]
    public void MustBeTwoCorrectsInRow()
    {
      var correct = new Evaluation {SittingQuality = 100, Posture = Posture.Correct};
      var wrong = new Evaluation {SittingQuality = 0, Posture = Posture.Wrong};
      var stats1 = new SpineHero.Model.Notifications.NotificationStatistics { Evaluation = new EvaluationStatistics
        { Current = correct, BeforeCurrent = correct }};
      var stats2 = new SpineHero.Model.Notifications.NotificationStatistics { Evaluation = new EvaluationStatistics
        { Current = correct, BeforeCurrent = wrong }};
      var stats3 = new SpineHero.Model.Notifications.NotificationStatistics { Evaluation = new EvaluationStatistics
        { Current = wrong, BeforeCurrent = correct }};
      var stats4 = new SpineHero.Model.Notifications.NotificationStatistics { Evaluation = new EvaluationStatistics
        { Current = wrong, BeforeCurrent = wrong }};

      var rule = new CorrectSitting();

      Expect(rule.Check(stats1), Is.True);
      Expect(rule.Check(stats2), Is.False);
      Expect(rule.Check(stats3), Is.False);
      Expect(rule.Check(stats4), Is.False);
    }
  }
}