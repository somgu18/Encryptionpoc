using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using SBPGP;
using PgpCore;

namespace Encryptionpoc
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static  void Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";
            //Blobconnect();
            //string lic = "AF479A648A42969644F109C690E12B1402F11DBD9EB213B43821FD62B787AE111989D1C38A5E2278F9D19F3D1D6AD85D87B7DA6DBAEDC72150960800413FB48E6067B17B03A5AB32A4417F35B4A17DA29FF2C9512DBC2D7AAE5CE117889C2FDC64CB65C6F6A9F1891D0CEEE134994DFF0DC19B95ABFDC55161B144E9482299618BE29FA9C8EFB89EB666049899C11907610B664CDAA723A1E18820A18A671B68C88C661854CC1B4DC48BA8806ED30AF02DAB7B25A63DE63258CE2F616F93D040DA6BC54212072542DBD41F7A343485A23C9AEF476404980F00B0125997FC7A4869186411F543FB4ED74A897E46B75351983715EEF95E6E443B25D156D010A57A";
            //string pub_key = @"C:\Users\somgu\Desktop\SFPGPKey_Statoil.asc";
            //string path = @"C:\Users\somgu\Desktop\New folder\abc.csv";
            // StreamWriter str = null;
            // encrypt(path, pub_key, lic, str);
            // Pgpencrypt();


            
            var Blobstring = "DefaultEndpointsProtocol=https;AccountName=integrationstoragepoc;AccountKey=gqzjEhHzScLzkADuvT8bDu3K662W43Ln+WXQ5DsOSu9/NYct+78IttE7OSyNbPd3I5iJhGU8MloM3HLfuETy/Q==;EndpointSuffix=core.windows.net";



            var containername = "ssisintegrationdata";

            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(Blobstring, containername);


            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient("stdiv-3loadcsvpoc.csv");
            BlobClient key = container.GetBlobClient("SFPGPKey_StatoilTest.asc");
            BlobClient output = container.GetBlobClient("LOG.pgp");
            output.DeleteIfExists();

            // BlobUpload();
            var sourceStream = new MemoryStream();
            var keyStream = new MemoryStream();
            var targetStream = new MemoryStream();

            blob.DownloadTo(sourceStream);
            sourceStream.Position = 0;

            key.DownloadTo(keyStream);
            keyStream.Position = 0;
            using (PGP pgp = new PGP())

             pgp.EncryptStream(sourceStream, targetStream, keyStream, true, true);
            targetStream.Position = 0;
            output.Upload(targetStream);


            //return new OkObjectResult(responseMessage);
        }

        

        public static void Blobconnect()
        {
            var Blobstring = "DefaultEndpointsProtocol=https;AccountName=integrationstoragepoc;AccountKey=gqzjEhHzScLzkADuvT8bDu3K662W43Ln+WXQ5DsOSu9/NYct+78IttE7OSyNbPd3I5iJhGU8MloM3HLfuETy/Q==;EndpointSuffix=core.windows.net";



       var containername = "ssisintegrationdata";

            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(Blobstring, containername);
            

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient("stdiv-3loadcsvpoc.csv");
            BlobClient key = container.GetBlobClient("SFPGPKey_StatoilTest.asc");
            BlobClient output = container.GetBlobClient("log.pgp");

            Response<BlobDownloadInfo> download = blob.Download();
            using (FileStream file = File.OpenWrite(@"C:\Users\somgu\Desktop\New folder\abc.csv"))
            {
                download.Value.Content.CopyTo(file);
            }
         


        }



        public static void encrypt(string filename, string pub_key, string lic, StreamWriter str)
        {

            // Initialize streams and check the message body for content.
            // If there is no content then the message is pointless.
            //str.WriteLine("just entered encrypt" + DateTime.Now.ToShortTimeString());
            Stream inStream = new MemoryStream(File.ReadAllBytes(filename));
            MemoryStream outStream = new MemoryStream();
            //  str.WriteLine("just before license" + DateTime.Now.ToShortTimeString());
            SBUtils.Unit.SetLicenseKey(lic);
            TElPGPWriter pgpWriter = new TElPGPWriter();
            SBPGPKeys.TElPGPKeyring encrKeyring = new SBPGPKeys.TElPGPKeyring();
            SBPGPKeys.TElPGPKeyring signKeyring = new SBPGPKeys.TElPGPKeyring();
            pgpWriter.Filename = filename;

            Stream EncryptingKeysStream = new MemoryStream(File.ReadAllBytes(pub_key));
            Stream SigningKeysStream = null;

            signKeyring.Load(SigningKeysStream, null, true);
            encrKeyring.Load(EncryptingKeysStream, null, true);

            pgpWriter.EncryptingKeys = encrKeyring;
            pgpWriter.SigningKeys = signKeyring;

            pgpWriter.Encrypt(inStream, outStream, 0);
            outStream.Position = 0;
            File.WriteAllBytes(filename, outStream.ToArray());
        }


        public static void Pgpencrypt()
        {
            using (PGP pgp = new PGP())

            using (FileStream inputFileStream = new FileStream(@"C:\Users\somgu\Desktop\New folder\abc.csv", FileMode.Open))
            using (Stream outputFileStream = File.Create(@"C:\Users\somgu\Desktop\New folder\abc.pgp"))
            using (Stream publicKeyStream = new FileStream(@"C:\Users\somgu\Desktop\SFPGPKey_Statoil.asc", FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, publicKeyStream, true, true);
        }

     


        public static void BlobUpload()
        {
            var Blobstring = "DefaultEndpointsProtocol=https;AccountName=integrationstoragepoc;AccountKey=gqzjEhHzScLzkADuvT8bDu3K662W43Ln+WXQ5DsOSu9/NYct+78IttE7OSyNbPd3I5iJhGU8MloM3HLfuETy/Q==;EndpointSuffix=core.windows.net";



            var containername = "ssisintegrationdata";

            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(Blobstring, containername);

            
            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient("log.pgp");
           
            



            using (FileStream file = File.OpenRead(@"C:\Users\somgu\Desktop\New folder\abc.pgp"))
            {
                blob.Upload(file);
            }


        }




    }



}

