using NUnit.Framework;
using SpineHero.Utils.CloudStorage;

namespace SpineHero.UnitTests.Utils.CloudStorage
{
    [TestFixture]
    public class SpineHeroWebApiTest : AssertionHelper
    {
        [Test]
        public void DoesNotNeedToProvideApiUrl()
        {
            var api = new SpineHeroWebApi();
            Expect(api.ApiUrl, EqualTo(Properties.Settings.Default.SpineHeroApiUrl));
        }

        [Test]
        public void ThrowNotificationInCaseOfFailure()
        {
            var execute = new AsyncTestDelegate(async delegate
            {
                var api = new SpineHeroWebApi(Properties.Settings.Default.SpineHeroApiUrl, "wrong api key");
                await api.SaveData("type", "data");
            });

            Expect(execute, Throws.TypeOf<CloudStorageException>());
        }
    }
}