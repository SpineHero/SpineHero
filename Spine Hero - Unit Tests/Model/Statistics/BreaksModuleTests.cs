using System;
using Caliburn.Micro;
using NUnit.Framework;
using Moq;
using SpineHero.Model.Statistics;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;

namespace SpineHero.UnitTests.Model.Statistics
{
    [TestFixture]
    internal class BreaksModuleTests : AssertionHelper
    {
        private readonly TimeSpan timeHalfLimit = TimeSpan.FromMinutes(Properties.Notifications.Default.BreakNotificationTimeLimit / 2);

        [Test]
        public void TestSittingWithoutBreakForTooLong()
        {
            var ea = new Mock<IEventAggregator>();
            var breaksModule = new BreaksModule(ea.Object);
            
            Expect(breaksModule.SittingWithoutBreakForTooLong(), False);

            var eval = new Evaluation(100, Posture.Correct);

            breaksModule.Handle(eval);

            eval.EvaluatedAt += timeHalfLimit;
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), False);

            eval.EvaluatedAt += timeHalfLimit;
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), True);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), False);

            eval.EvaluatedAt += TimeSpan.FromMinutes(timeHalfLimit.TotalMinutes / 2);
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), False);

            eval.EvaluatedAt += TimeSpan.FromMinutes(timeHalfLimit.TotalMinutes / 2);
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), True);
            Expect(breaksModule.SittingWithoutBreakForTooLong(), False);
        }

        [Test]
        public void TestHandleReset()
        {
            var ea = new Mock<IEventAggregator>();
            var breaksModule = new BreaksModule(ea.Object);

            Expect(breaksModule.SittingStart, EqualTo(DateTime.MinValue));

            var eval = new Evaluation(100, Posture.Correct);
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingStart, EqualTo(eval.EvaluatedAt));

            breaksModule.Handle(new PostureMonitoringStatusChange(false));
            Expect(breaksModule.SittingStart, EqualTo(DateTime.MinValue));
        }

        [Test]
        public void TestUnknownPostureReset()
        {
            var ea = new Mock<IEventAggregator>();
            var breaksModule = new BreaksModule(ea.Object);

            Expect(breaksModule.SittingStart, EqualTo(DateTime.MinValue));

            var eval = new Evaluation(100, Posture.Correct);
            breaksModule.Handle(eval);
            Expect(breaksModule.SittingStart, EqualTo(eval.EvaluatedAt));

            var unknown = new Evaluation(0, Posture.Unknown);
            breaksModule.Handle(unknown);
            Expect(breaksModule.SittingStart, EqualTo(eval.EvaluatedAt));

            unknown.EvaluatedAt += Properties.Notifications.Default.BreakNotificationBreakLength;
            breaksModule.Handle(unknown);
            Expect(breaksModule.SittingStart, EqualTo(DateTime.MinValue));
        }
    }
}