using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications.Rules;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class RulesCollectionTest : AssertionHelper
    {
        [Test]
        public void RunCheckOnEveryRuleInCollection()
        {
            var rule1 = new Mock<IRule>();
            var rule2 = new Mock<IRule>();
            var rule3 = new Mock<IRule>();
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();
            rule1.Setup(r => r.Check(stats)).Returns(true);
            rule2.Setup(r => r.Check(stats)).Returns(false);
            rule3.Setup(r => r.Check(stats)).Returns(false);
            var collection = new RulesCollection(new List<IRule>{rule1.Object, rule2.Object}, new List<IRule>{rule3.Object});

            collection.Check(stats);

            rule1.Verify(r => r.Check(stats), Times.Once);
            rule2.Verify(r => r.Check(stats), Times.Once);
            rule3.Verify(r => r.Check(stats), Times.Once);
        }

        [Test]
        public void ReturnsTrueWhenEveryRuleInReturnsTrue()
        {
            var rule1 = new Mock<IRule>();
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();
            rule1.Setup(r => r.Check(stats)).Returns(true);
            var list = new RulesCollection();

            Expect(list.CheckList(new List<IRule> {rule1.Object, rule1.Object}, stats), Is.True);
        }

        [Test]
        public void ReturnsFalseWhenAtLeastOneRuleReturnsFalse()
        {
            var rule1 = new Mock<IRule>();
            var rule2 = new Mock<IRule>();
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();
            rule1.Setup(r => r.Check(stats)).Returns(true);
            rule2.Setup(r => r.Check(stats)).Returns(false);

            var list1 = new RulesCollection(new List<IRule> {rule1.Object, rule2.Object});

            Expect(list1.CheckList(new List<IRule> {rule1.Object, rule2.Object}, stats), Is.False);
            Expect(list1.CheckList(new List<IRule> {rule2.Object, rule1.Object}, stats), Is.False);
            Expect(list1.CheckList(new List<IRule> {rule2.Object, rule2.Object}, stats), Is.False);
        }

        [Test]
        public void AtLeastOneOfTheListMustHaveAllRulesPassing()
        {
            var rule1 = new Mock<IRule>();
            var rule2 = new Mock<IRule>();
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();
            rule1.Setup(r => r.Check(stats)).Returns(true);
            rule2.Setup(r => r.Check(stats)).Returns(false);

            var correctList = new List<IRule> {rule1.Object, rule1.Object};
            var wrongList = new List<IRule> {rule2.Object, rule2.Object};

            var collection1 = new RulesCollection(correctList, correctList);
            var collection2 = new RulesCollection(correctList, wrongList);
            var collection3 = new RulesCollection(wrongList, correctList);
            var collection4 = new RulesCollection(wrongList, wrongList);

            Expect(collection1.Check(stats), Is.True);
            Expect(collection2.Check(stats), Is.True);
            Expect(collection3.Check(stats), Is.True);
            Expect(collection4.Check(stats), Is.False);
        }
    }
}