using Newtonsoft.Json.Linq;
using StudentEnrollServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;

namespace StudentEnrollServiceAPI.Controllers
{
    public class EnrollmentServiceController : ApiController
    {

        [HttpPost]
        public IHttpActionResult GetAllCustomerSearchRecords([FromBody]  string name)
        {
            SampleModel obj = new SampleModel();
            return Ok(obj);
        }

        [HttpGet]
        public IHttpActionResult GetServiceConfig(string env)
        {
            string _ConfigJsonFilePath = HostingEnvironment.MapPath("/Config/ServiceAPIConfig.json");
            string json = File.ReadAllText(_ConfigJsonFilePath);
            JObject jObject = JObject.Parse(json);

            JArray customArrary = (JArray)jObject["servicePort"];
            var configs = new List<ConfigModel>();
            ConfigModel result = new ConfigModel();
            if (customArrary != null)
            {

                ConfigModel _columnModel = null;
                foreach (JToken item in customArrary)
                {
                    List<SampleModel> connnectionList = new List<SampleModel>();
                    _columnModel = new ConfigModel();
                    _columnModel.env = item["env"]?.ToString() ?? "";
                    _columnModel.apiPort = item["apiPort"]?.ToString() ?? "";
                    _columnModel.url = item["url"].ToString();
                    foreach (JToken obj in item["config"])
                    {
                        SampleModel connnectionObj = new SampleModel();
                        connnectionObj.connectionname = obj["connectionname"]?.ToString() ?? "";
                        connnectionObj.hostipaddress = EncryptString(obj["hostipaddress"]?.ToString() ?? "", _columnModel.apiPort);
                        connnectionObj.orderentryport = EncryptString(obj["orderentryport"]?.ToString() ?? "", _columnModel.apiPort);
                        connnectionObj.customerserviceport = EncryptString(obj["customerserviceport"]?.ToString() ?? "", _columnModel.apiPort);
                        connnectionObj.database = EncryptString(obj["database"]?.ToString() ?? "", _columnModel.apiPort);
                        connnectionObj.adcredentials = Convert.ToBoolean(obj["adcredentials"]);
                        connnectionList.Add(connnectionObj);
                    }
                    _columnModel.sampleModel = new List<SampleModel>();
                    _columnModel.sampleModel = connnectionList;
                    _columnModel.domain = GetUserDoamin();
                    configs.Add(_columnModel);
                }

                result = configs.FirstOrDefault(x => x.env.Equals(env));
            }

            return Ok(result);
        }

        public static string EncryptString(string data, string portNumber)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                string sumofdate = (DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year).ToString();

                string domainName = GetUserDoamin();
                domainName = domainName.PadRight(3, '!').Substring(0, 3);
                string strUserName = portNumber.ToString().PadRight(6, '$').ToUpper().Substring(0, 6) + domainName.Substring(1, 1).ToUpper() + domainName.Substring(0, 1).ToUpper();
                string encryptKey = $"{domainName}{sumofdate}{strUserName}";
                encryptKey = encryptKey.PadRight(16, '#');
                var aes = new AesCryptoServiceProvider();
                var ivData = aes.IV;
                string ivKey = Convert.ToBase64String(ivData, 0, ivData.Length).Substring(0, 16);

                byte[] keybytes = Encoding.UTF8.GetBytes(encryptKey);
                byte[] iv = Encoding.UTF8.GetBytes(ivKey);

                //DECRYPT FROM CRIPTOJS
                //var encrypted = Convert.FromBase64String(data);
                string decriptedFromJavascript = EncryptStringToBytes(data, keybytes, iv);
                string firstHalfKey = ivKey.Substring(0, 8);
                string secondHalfKey = ivKey.Substring(8, 8);
                decriptedFromJavascript = secondHalfKey + decriptedFromJavascript.ToString() + firstHalfKey;

                return decriptedFromJavascript.ToString();
            }
            return "";
        }

        static string EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted, 0, encrypted.Length);
        }

        public static string GetUserDoamin()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            IPAddress address = IPAddress.Parse(myIP);
            IPHostEntry entry = Dns.GetHostEntry(address);
            string[] splittedArray = entry.HostName.Split('.');
            return splittedArray[1];
        }


    }
}
