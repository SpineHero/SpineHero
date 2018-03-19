using System;
using OpenCvSharp;
using PostSharp.Patterns.Contracts;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.DepthWatcher
{
    public class DepthImagePostureEvaluator
    {
        public bool IsUnknownPosture(BodyPoints current)
        {
            if (current == null) return true;
            var head = current.Head;
            var body = current.Body;
            var headTooBig = head.Width > body.Width * 0.9;
            var headTooSmall = head.Width < body.Width / 5;
            var headTooFar =
                Point.Distance(head[head.Size.Width / 2, head.Size.Height / 2].Point,
                    body[body.Size.Width / 2].Point) > body.Width * 1.5;
            var headZero = (int)head.Average() == 0;
            var headTooClose = head[head.Size.Width / 2].Point.Y > body[body.Size.Width / 2].Point.Y;
            return headTooBig || headTooSmall || headTooFar || headZero || headTooClose;
        }

        [LogMethodCall]
        public PostureEvaluation EvaluatePoints(BodyPoints current, [NotNull]BodyPoints calibration)
        {
            if (current == null || IsUnknownPosture(current)) return new PostureEvaluation(-1, Posture.Unknown);

            var curHead = current.Head;
            var curBody = current.Body;
            var calHead = calibration.Head;
            var calBody = calibration.Body;

            int bmw = curBody.Size.Width / 2;
            int bmh = curBody.Size.Height / 2;
            int hmw = curHead.Size.Width / 2;

            double hTooClose = curHead.Average() - 30;
            hTooClose = hTooClose < 0 ? -hTooClose : 0;

            double hTooFar = curHead.Average() - 130;
            hTooFar = hTooFar < 0 ? 0 : hTooFar;

            double htbDifference1 = (calHead.Average() - calBody[bmw].Value) - (curHead.Average() - curBody[bmw].Value);
            double htbDifference2 = (calHead.Average() - calBody[bmw, bmh].Value) - (curHead.Average() - curBody[bmw, bmh].Value);

            double hd1 = (curBody[bmw].Point.Y - curHead[hmw].Point.Y);
            double tan = (calBody.Points[bmw].Point.Y - calHead[hmw].Point.Y) / calHead.Average();
            double hd2 = tan * calHead.Average();
            double htbDistance = hd1 - hd2;

            double hx = curHead[hmw].Point.X - curBody[bmw].Point.X;
            double hy = curBody[bmw].Point.Y - curHead[hmw].Point.Y - calHead.Height;
            hy = hy < 0 ? -hy : 0;

            double bLeft = calBody.AverageColumn(bmw - 1) - curBody.AverageColumn(bmw - 1);
            double bRight = calBody.AverageColumn(bmw + 1) - curBody.AverageColumn(bmw + 1);
            double hPitch = curHead.AverageRow(0) - curHead.AverageRow(curHead.Size.Width - 1);

            var values = new double[9];
            values[0] = hTooClose;
            values[1] = hTooFar;
            values[2] = htbDifference1;
            values[3] = htbDifference2;
            values[4] = htbDistance;
            values[5] = hx;
            values[6] = hy;
            values[7] = bLeft - bRight;
            values[8] = hPitch;

            return new PostureEvaluation(values);
        }
    }

    public class PostureEvaluation
    {
        private readonly int[,] range = { { 10, 30 }, { 10, 30 }, { 8, 20 }, { 8, 20 }, { 15, 30 }, { 10, 30 }, { 15, 30 }, { 15, 30 }, { 7, 15 } };
        public static readonly string[] Names = { "HtC", "HtF", "Hb1", "Hb2", "HbD", "Hx", "Hy", "Blr", "HPi" };
        private readonly int[] sittingQuality = new int[Names.Length];

        public PostureEvaluation(int sittingQuality, Posture detectedPosture)
        {
            SittingQuality = sittingQuality;
            DetectedPosture = detectedPosture;
        }

        public PostureEvaluation(double[] values)
        {
            Values = values;
            SittingQuality = GetSittingQuality();
            DetectedPosture = GetDetectedPosture();
        }

        public int SittingQuality { get; set; }

        public Posture DetectedPosture { get; set; }

        public double[] Values { get; set; }

        public int GetSittingQuality()
        {
            int min = 100;
            for (int i = 0; i < Values.Length; i++)
            {
                int sq = GetSittingQuality(i);
                if (sq < min) min = sq;
                sittingQuality[i] = sq;
            }
            return min;
        }

        public int GetSittingQuality(int i)
        {
            var low = range[i, 0];
            var high = range[i, 1];
            var value = Math.Abs(Values[i]);
            if (value < low) return 100;
            if (value < high) return 100 - (int)((value - low) / (high - low) * 100);
            return 0;
        }

        public bool IsWrong(int i)
        {
            return sittingQuality[i] <= 50;
        }

        public Posture GetDetectedPosture()
        {
            if (IsWrong(4) && IsWrong(6) && (IsWrong(7) || !IsWrong(8))) return Posture.Unknown;

            if (IsWrong(0) || Values[2] > 0 && IsWrong(2) || Values[3] > 0 && IsWrong(3)) return Posture.LeanForward;
            if (IsWrong(1) || Values[2] < 0 && IsWrong(2) || Values[3] < 0 && IsWrong(3)) return Posture.LeanBackward;

            if (IsWrong(8) || IsWrong(7)) return Posture.Wrong;
            if (IsWrong(4) || IsWrong(6)) return Values[0] > 0 ? Posture.LeanForward : Posture.Slouch;
            if (IsWrong(5)) return Values[5] < 0 ? Posture.LeanRight : Posture.LeanLeft;

            return SittingQuality <= 50 ? Posture.Wrong : Posture.Correct;
        }
    }
}