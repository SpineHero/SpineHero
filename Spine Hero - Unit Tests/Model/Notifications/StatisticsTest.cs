using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications
{
    [TestFixture]
    public class StatisticsTest : AssertionHelper
    {
        [Test]
        public void SaveSetCurrentToNewEvaluation()
        {
            var stats = new NotificationStatistics
            {
                Evaluation = new EvaluationStatistics
                {
                    Current = new Evaluation { Posture = Posture.Correct}
                }
            };
            var eval = new Evaluation {Posture = Posture.LeanBackward};

            var stats2 = stats.Add(eval);

            Expect(stats2.Evaluation.Current, EqualTo(eval));
        }

        [Test]
        public void SaveSetBeforeCurrent()
        {
            var eval1 = new Evaluation {Posture = Posture.Correct};
            var eval2 = new Evaluation {Posture = Posture.LeanBackward};
            var stats = new NotificationStatistics
            {
                Evaluation = new EvaluationStatistics
                {
                    Current = eval1
                }
            };

            var stats2 = stats.Add(eval2);

            Expect(stats2.Evaluation.BeforeCurrent, EqualTo(eval1));
        }

        [Test]
        public void SaveSetFirstWrong()
        {
            var eval1 = new Evaluation{SittingQuality = 0, Posture = Posture.Wrong};
            var stats = new NotificationStatistics();
            stats = stats.Add(eval1);
            Expect(stats.Evaluation.FirstWrong, EqualTo(eval1));

            stats = stats.Add(new Evaluation{SittingQuality = 0, Posture = Posture.LeanBackward});
            Expect(stats.Evaluation.FirstWrong, EqualTo(eval1));

            // First time correct
            stats = stats.Add(new Evaluation{ SittingQuality = 100, Posture = Posture.Correct });
            Expect(stats.Evaluation.FirstWrong, EqualTo(eval1));

            // Second time correct
            stats = stats.Add(new Evaluation{ SittingQuality = 99, Posture = Posture.Correct });
            Expect(stats.Evaluation.FirstWrong, Is.Null);
        }

        [Test]
        public void SaveSetFirstUnknown()
        {
            var eval1 = new Evaluation{Posture = Posture.Unknown, SittingQuality = 0};
            var stats = new NotificationStatistics();
            stats = stats.Add(eval1);
            Expect(stats.Evaluation.FirstUnknown, EqualTo(eval1));

            stats = stats.Add(new Evaluation{Posture = Posture.Unknown, SittingQuality = 1});
            Expect(stats.Evaluation.FirstUnknown, EqualTo(eval1));

            // First time not unknown
            stats = stats.Add(new Evaluation{Posture = Posture.LeanBackward});
            Expect(stats.Evaluation.FirstUnknown, EqualTo(eval1));

            // Second time unknown
            stats = stats.Add(new Evaluation{Posture = Posture.Correct});
            Expect(stats.Evaluation.FirstUnknown, Is.Null);
        }
    }
}