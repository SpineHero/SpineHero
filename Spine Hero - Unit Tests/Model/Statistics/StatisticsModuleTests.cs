using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using NUnit.Framework;
using SpineHero.Model.Statistics;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring;
using SpineHero.Properties;

namespace SpineHero.UnitTests.Model.Statistics
{
    [TestFixture]
    class StatisticsModuleTests : IHandle<Posture>
    {
        private IEventAggregator events = new EventAggregator();
        private Posture? posture;

        [SetUp]
        public void Init()
        {
            posture = null;
        }
        

        [Test]
        public void HandleEvaluationTest()
        {
            events.Subscribe(this);
            var sm = new StatisticsModule(events);
            var sit = 79;
            var pos = Posture.LeanBackward;
            var eval = new Evaluation(sit, pos);

            Assert.AreEqual(Posture.Unknown, sm.LastPosture, "LastSittingQuality should be -1 if LastEvaluation is null.");
            Assert.NotNull(sm.Evaluations, "Evaluations should by initialized.");
            Assert.NotNull(sm.SittingQualityAveraged, "SittingQualityAdjusted should by initialized.");
            Assert.IsNull(sm.LastEvaluation);

            events.PublishOnCurrentThread(eval);

            Assert.IsNotNull(sm.LastEvaluation);
            Assert.AreEqual(sit, sm.LastEvaluation.SittingQuality, "LastEvaluation.SittingQuality");
            Assert.AreEqual(pos, sm.LastEvaluation.Posture, "LastEvaluation.Posture");
            Assert.AreEqual(sit, sm.LastSittingQuality, "LastSittingQuality");
            Assert.AreEqual(pos, sm.LastPosture, "LastPosture");
            Assert.AreEqual(sit, sm.LastSittingQualityAveraged, "LastSittingQualityAdjusted");
            Assert.AreEqual(pos, posture, "posture");
            events.Unsubscribe(this);
        }

        [Test]
        public void AdjustSittingQualityTest()
        {
            var sm = new StatisticsModule(events);
            var list = new List<Evaluation>
            {
                new Evaluation(100),
                new Evaluation(100),
                new Evaluation(80),
                new Evaluation(60),
                new Evaluation(40),
                new Evaluation(20),
                new Evaluation(70)
            };
            foreach (var e in list)
            {
                events.PublishOnCurrentThread(e);
            }

            var avg = list.Skip(list.Count - Const.Default.EvaluationMovingAvgCount).Average(x => x.SittingQuality);

            Assert.AreEqual(sm.LastSittingQualityAveraged, avg);
        }

        public void Handle(Posture message)
        {
            posture = message;
        }

        [Test]
        public void RemoveElementsIfOverLimitTest()
        {
            var sm = new StatisticsModule(events);

            Assert.AreEqual(0, sm.Evaluations.Count);
            Assert.AreEqual(0, sm.SittingQualityAveraged.Count);

            for (int i = 0; i < Const.Default.EvaluationListLowerLimit; i++)
            {
                events.PublishOnCurrentThread(new Evaluation(0));
            }
            var expected = Const.Default.EvaluationListLowerLimit;
            Assert.AreEqual(expected, sm.Evaluations.Count);
            Assert.AreEqual(expected, sm.SittingQualityAveraged.Count);

            for (int i = 0; i < Const.Default.EvaluationListMaxLimit; i++)
            {
                events.PublishOnCurrentThread(new Evaluation(0));
            }
            expected += Const.Default.EvaluationListMaxLimit - Const.Default.EvaluationListLowerLimit;
            Assert.AreEqual(expected, sm.Evaluations.Count);
            Assert.AreEqual(expected, sm.SittingQualityAveraged.Count);
        }
    }
}
