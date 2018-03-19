using NUnit.Framework;
using SpineHero.Common.Resources;
using System;
using System.Collections.Generic;
using System.Reflection;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.PostureMonitoring
{
    [TestFixture]
    internal class ResultMessageTests : AssertionHelper
    {
        [TestCase(0, Posture.Wrong)]
        [TestCase(30, Posture.Wrong)]
        [TestCase(40, Posture.Correct)]
        [TestCase(100, Posture.Correct)]
        public void ResultMessageAndEvaluationTest(int sq, Posture p)
        {
            Expect(PostureHelper.GetPosture(sq), EqualTo(p), "PH");
            Expect(new ResultMessage("", sq, 100).DetectedPosture, EqualTo(p), "RM");
            Expect(new Evaluation(sq).Posture, EqualTo(p), "Eval");
        }

        [Test]
        public void AverageResultMessageProcessorTest()
        {
            var list = new List<ResultMessage>();
            var processor = new AverageResultMessageProcessor();

            Assert.Throws<ArgumentNullException>(() => processor.ProcessResults(null));

            var eval = processor.ProcessResults(list);
            Expect(eval, Null, "Empty");

            list.Add(new ResultMessage("", 100, 100, Posture.Correct));
            eval = processor.ProcessResults(list);
            Expect(eval.SittingQuality, EqualTo(100), "One");
            Expect(eval.Posture, EqualTo(PostureHelper.GetPosture(100)), "One");

            list[0] = new ResultMessage("", 0, 100, Posture.Slouch);
            list.Add(new ResultMessage("", 20, 100, Posture.LeanBackward));
            eval = processor.ProcessResults(list);
            Expect(eval.SittingQuality, EqualTo(10), "Two");
            Expect(eval.Posture, EqualTo(Posture.Slouch), "Two");
        }

        [Test]
        public void LeanLeftAndRightImageRepresentation()
        {
            var leftPath = SpineHero.Utils.Extentions.PostureHelper.GetImageRepresentation(Posture.LeanLeft);
            var rightPath = SpineHero.Utils.Extentions.PostureHelper.GetImageRepresentation(Posture.LeanRight);
            var assembly = Assembly.GetAssembly(typeof (AppBootstrapper));
            Expect(leftPath,  EqualTo(ResourceHelper.GetResourcePath(@"Resources\Images\StickBoy\LeanLeft.png" , assembly)));
            Expect(rightPath, EqualTo(ResourceHelper.GetResourcePath(@"Resources\Images\StickBoy\LeanRight.png", assembly)));
        }
    }
}