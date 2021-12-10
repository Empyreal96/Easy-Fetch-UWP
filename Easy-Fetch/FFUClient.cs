using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace FFUClientCode
{
     public  static class FFUClient
    {

        public static string FoundProductType;
        public static async Task<string> TestFFU()
        {
            string ProductType = "RM-1085";
            string ProductCode = "059X4V1";
            string OperatorCode = "000-88";
            string responseResult = await SearchFFU(ProductType, ProductCode, OperatorCode);
            return responseResult;
        }
         public  static async Task<string> SearchFFU(string ProductType, string ProductCode, string OperatorCode)
        {
            return await SearchFFUAsync(ProductType, ProductCode, OperatorCode);
        }

         public  static async Task<string> SearchFFUAsync(string ProductType, string ProductCode, string OperatorCode)
        {
            if (ProductType == "")
                ProductType = null;
            if (ProductCode == "")
                ProductCode = null;
            if (OperatorCode == "")
                OperatorCode = null;

            if (ProductCode != null)
            {
                ProductCode = ProductCode.ToUpper();
                ProductType = null;
                OperatorCode = null;
            }
            if (ProductType != null)
            {
                ProductType = ProductType.ToUpper();
                if (ProductType.StartsWith("RM") && !ProductType.StartsWith("RM-"))
                    ProductType = "RM-" + ProductType.Substring(2);
            }
            if (OperatorCode != null)
                OperatorCode = OperatorCode.ToUpper();

            DiscoveryQueryParameters DiscoveryQueryParams = new DiscoveryQueryParameters
            {
                manufacturerName = "Microsoft",
                manufacturerProductLine = "Lumia",
                packageType = "Firmware",
                packageClass = "Public",
                manufacturerHardwareModel = ProductType,
                manufacturerHardwareVariant = ProductCode,
                operatorName = OperatorCode
            };
            DiscoveryParameters DiscoveryParams = new DiscoveryParameters
            {
                query = DiscoveryQueryParams
            };

            DataContractJsonSerializer Serializer1 = new DataContractJsonSerializer(typeof(DiscoveryParameters));
            MemoryStream JsonStream1 = new MemoryStream();
            Serializer1.WriteObject(JsonStream1, DiscoveryParams);
            JsonStream1.Seek(0L, SeekOrigin.Begin);
            string JsonContent = new StreamReader(JsonStream1).ReadToEnd();

            Uri RequestUri = new Uri("https://api.swrepository.com/rest-api/discovery/1/package");

            Windows.Web.Http.HttpClient HttpClient = new Windows.Web.Http.HttpClient();
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("SoftwareRepository");

            HttpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

            var Response = await HttpClient.PostAsync(RequestUri, new HttpStringContent(JsonContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json"));

            string JsonResultString = "";
            if (Response.IsSuccessStatusCode)
            {
                JsonResultString = await Response.Content.ReadAsStringAsync();
                //var responseFileName = Response.Content.Headers.ContentDisposition.FileName;
            }

            SoftwarePackage Package = null;
            using (MemoryStream JsonStream2 = new MemoryStream(Encoding.UTF8.GetBytes(JsonResultString)))
            {
                DataContractJsonSerializer Serializer2 = new DataContractJsonSerializer(typeof(SoftwarePackages));
                SoftwarePackages SoftwarePackages = (SoftwarePackages)Serializer2.ReadObject(JsonStream2);
                if (SoftwarePackages != null)
                {
                    Package = SoftwarePackages.softwarePackages.FirstOrDefault<SoftwarePackage>();
                }
            }

            if (Package == null)
                throw new Exception("FFU not found");

            FoundProductType = Package.manufacturerHardwareModel[0];

            SoftwareFile FileInfo = Package.files.Where(f => f.fileName.EndsWith(".ffu", StringComparison.OrdinalIgnoreCase)).First();
            
            Uri FileInfoUri = new Uri("https://api.swrepository.com/rest-api/discovery/fileurl/1/" + Package.id + "/" + FileInfo.fileName);
            var FileInfoString = await HttpClient.GetStringAsync(FileInfoUri);
            

            string FfuUrl = "";
            FileUrlResult FileUrl = null;
            using (MemoryStream JsonStream3 = new MemoryStream(Encoding.UTF8.GetBytes(FileInfoString)))
            {
                DataContractJsonSerializer Serializer3 = new DataContractJsonSerializer(typeof(FileUrlResult));
                FileUrl = (FileUrlResult)Serializer3.ReadObject(JsonStream3);
                if (FileUrl != null)
                {
                    FfuUrl = FileUrl.url;
                }
            }

            HttpClient.Dispose();

            return FfuUrl;
        }


         public  static async Task<string[]> SearchEmergencyFiles(string ProductType)
        {
            ProductType = ProductType.ToUpper();
            if (ProductType.StartsWith("RM") && !ProductType.StartsWith("RM-"))
                ProductType = "RM-" + ProductType.Substring(2);

            Log("Getting Emergency files for: " + ProductType);

            if ((ProductType == "RM-1072") || (ProductType == "RM-1073"))
            {
                Log("Due to mix-up in online-repository, redirecting to emergency files of RM-1113");
                ProductType = "RM-1113";
            }

            List<string> Result = new List<string>();

            var Client = new Windows.Web.Http.HttpClient();
            string Src;
            string FileName;
            string Config = null;
            try
            {
                Config = await Client.GetStringAsync(new Uri(@"https://repairavoidance.blob.core.windows.net/packages/EmergencyFlash/" + ProductType + "/emergency_flash_config.xml"));
            }
            catch
            {
                Log("Emergency files for " + ProductType + " not found");
                return null;
            }
            Client.Dispose();

            Windows.Data.Xml.Dom.XmlDocument Doc = new Windows.Data.Xml.Dom.XmlDocument();
            Doc.LoadXml(Config);

            // Hex
            var Node = Doc.SelectSingleNode("//emergency_flash_config/hex_flasher");
            if (Node != null)
            {
                FileName = GetAttributeValue(Node, "image_path");
                Src = @"https://repairavoidance.blob.core.windows.net/packages/EmergencyFlash/" + ProductType + "/" + FileName;
                Log("Hex-file: " + Src);
                Result.Add(Src);
            }

            // Mbn
            Node = Doc.SelectSingleNode("//emergency_flash_config/mbn_image");
            if (Node != null)
            {
                FileName = GetAttributeValue(Node, "image_path");
                Src = @"https://repairavoidance.blob.core.windows.net/packages/EmergencyFlash/" + ProductType + "/" + FileName;
                Log("Mbn-file: " + Src);
                Result.Add(Src);
            }

            // Ede
            foreach (var SubNode in Doc.SelectNodes("//emergency_flash_config/first_boot_images/first_boot_image"))
            {
                FileName = GetAttributeValue(SubNode, "image_path");
                Src = @"https://repairavoidance.blob.core.windows.net/packages/EmergencyFlash/" + ProductType + "/" + FileName;
                Log("Firehose-programmer-file: " + Src);
                Result.Add(Src);
            }

            // Edp
            foreach (var SubNode in Doc.SelectNodes("//emergency_flash_config/second_boot_firehose_single_image/firehose_image"))
            {
                FileName = GetAttributeValue(SubNode, "image_path");
                Src = @"https://repairavoidance.blob.core.windows.net/packages/EmergencyFlash/" + ProductType + "/" + FileName;
                Log("Firehose-payload-file: " + Src);
                Result.Add(Src);
            }

            return Result.ToArray();
        }

         public  static string GetAttributeValue(IXmlNode xmlNode, string attributeName)
        {
            var i = 0;
            foreach (var aItem in xmlNode.Attributes)
            {
                if (aItem.LocalName.Equals(attributeName) && xmlNode.Attributes[i] != null)
                {
                    return xmlNode.Attributes[i].InnerText;
                }
                i++;
            }
            return "";
        }
         public  static void Log(string logline)
        {
            //Do Something here
        }
    }

#pragma warning disable 0649
    [DataContract]
     public  class FileUrlResult
    {
        [DataMember]
         public  string url;

        [DataMember]
         public  List<string> alternateUrl;

        [DataMember]
         public  long fileSize;

        [DataMember]
         public  List<SoftwareFileChecksum> checksum;
    }
#pragma warning restore 0649

    [DataContract]
    public class DiscoveryQueryParameters
    {
        [DataMember(EmitDefaultValue = false)]
        public string customerName;

        [DataMember(EmitDefaultValue = false)]
        public ExtendedAttributes extendedAttributes;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerHardwareModel;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerHardwareVariant;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerModelName;

        [DataMember]
        public string manufacturerName;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerPackageId;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerPlatformId;

        [DataMember]
        public string manufacturerProductLine;

        [DataMember(EmitDefaultValue = false)]
        public string manufacturerVariantName;

        [DataMember(EmitDefaultValue = false)]
        public string operatorName;

        [DataMember]
        public string packageClass;

        [DataMember(EmitDefaultValue = false)]
        public string packageRevision;

        [DataMember(EmitDefaultValue = false)]
        public string packageState;

        [DataMember(EmitDefaultValue = false)]
        public string packageSubRevision;

        [DataMember(EmitDefaultValue = false)]
        public string packageSubtitle;

        [DataMember(EmitDefaultValue = false)]
        public string packageTitle;

        [DataMember]
        public string packageType;
    }

    [DataContract]
    public class DiscoveryParameters
    {
        [DataMember(Name = "api-version")]
        public string apiVersion;

        [DataMember]
        public DiscoveryQueryParameters query;

        [DataMember]
        public List<string> condition;

        [DataMember]
        public List<string> response;

        public DiscoveryParameters() : this(DiscoveryCondition.Default)
        {
        }

        public DiscoveryParameters(DiscoveryCondition Condition)
        {
            this.apiVersion = "1";
            this.query = new DiscoveryQueryParameters();
            this.condition = new List<string>();
            if (Condition == DiscoveryCondition.All)
            {
                this.condition.Add("all");
                return;
            }
            if (Condition == DiscoveryCondition.Latest)
            {
                this.condition.Add("latest");
                return;
            }
            this.condition.Add("default");
        }
    }

    public enum DiscoveryCondition
    {
        Default,
        All,
        Latest
    }

    [Serializable]
    public class ExtendedAttributes : ISerializable
    {
        public Dictionary<string, string> Dictionary;

        public ExtendedAttributes()
        {
            this.Dictionary = new Dictionary<string, string>();
        }

        protected ExtendedAttributes(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                this.Dictionary = new Dictionary<string, string>();
                SerializationInfoEnumerator Enumerator = info.GetEnumerator();
                while (Enumerator.MoveNext())
                {
                    this.Dictionary.Add(Enumerator.Current.Name, (string)Enumerator.Current.Value);
                }
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                foreach (string Current in this.Dictionary.Keys)
                {
                    info.AddValue(Current, this.Dictionary[Current]);
                }
            }
        }
    }

    [DataContract]
    public class SoftwareFileChecksum
    {
        [DataMember]
        public string type;

        [DataMember]
        public string value;
    }

    [DataContract]
    public class SoftwarePackages
    {
        [DataMember]
        public List<SoftwarePackage> softwarePackages;
    }

    [DataContract]
    public class SoftwarePackage
    {
        [DataMember]
        public List<string> customerName;

        [DataMember]
        public ExtendedAttributes extendedAttributes;

        [DataMember]
        public List<SoftwareFile> files;

        [DataMember]
        public string id;

        [DataMember]
        public List<string> manufacturerHardwareModel;

        [DataMember]
        public List<string> manufacturerHardwareVariant;

        [DataMember]
        public List<string> manufacturerModelName;

        [DataMember]
        public string manufacturerName;

        [DataMember]
        public string manufacturerPackageId;

        [DataMember]
        public List<string> manufacturerPlatformId;

        [DataMember]
        public string manufacturerProductLine;

        [DataMember]
        public List<string> manufacturerVariantName;

        [DataMember]
        public List<string> operatorName;

        [DataMember]
        public List<string> packageClass;

        [DataMember]
        public string packageDescription;

        [DataMember]
        public string packageRevision;

        [DataMember]
        public string packageState;

        [DataMember]
        public string packageSubRevision;

        [DataMember]
        public string packageSubtitle;

        [DataMember]
        public string packageTitle;

        [DataMember]
        public string packageType;
    }

    [DataContract]
    public class SoftwareFile
    {
        [DataMember]
        public List<SoftwareFileChecksum> checksum;

        [DataMember]
        public string fileName;

        [DataMember]
        public long fileSize;

        [DataMember]
        public string fileType;
    }
}
