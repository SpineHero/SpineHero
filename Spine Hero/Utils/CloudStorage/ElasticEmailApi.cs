using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpineHero.Utils.CloudStorage
{
    class ElasticResponse
    {
        public bool success;
        public string error;
    }

    class ElasticEmailApi : ICloudStorage
    {
        public async Task<int> SaveData(string type, string data)
        {
            var values = new NameValueCollection();
            values.Add("apikey", "a1cb69a3-ecf9-4966-95af-00e3f50add1c");
            values.Add("from", "mailer@spinehero.com");
            values.Add("fromName", "Spine Hero");
            values.Add("to", "info@spinehero.com");
            values.Add("subject", "Feedback from Spine Hero application");
            values.Add("bodyText", data);
            values.Add("isTransactional", "true");

            string address = "https://api.elasticemail.com/v2/email/send";

            ElasticResponse response = Send(address, values);

            return 0;
        }

        static ElasticResponse Send(string address, NameValueCollection values)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] apiResponse = client.UploadValues(address, values);
                    var httpResponse = Encoding.UTF8.GetString(apiResponse);

                    ElasticResponse elasticResponse = JsonConvert.DeserializeObject<ElasticResponse>(httpResponse);

                    if (!elasticResponse.success)
                    {
                        throw new CloudStorageException("Sending email failed: " + elasticResponse.error);
                    }
                    return elasticResponse;
                }
                catch (Exception ex)
                {
                    throw new CloudStorageException("Exception caught: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

    }
}
