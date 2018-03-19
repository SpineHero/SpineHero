namespace SpineHero.Model.Notifications.Rules
{
  public class CorrectSitting : IRule
  {
    public bool Check(NotificationStatistics statistics)
    {
      return statistics.Evaluation.Current.IsCorrect() && statistics.Evaluation.BeforeCurrent.IsCorrect();
    }
  }
}