using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Function.Read.S3Bucket.Domain;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Function.Read.S3Bucket
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Person> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if (s3Event == null)
            {
                return null;
            }

            context.Logger.LogLine($"Bucket Name: {s3Event.Bucket.Name}");
            context.Logger.LogLine($"Object Key: {s3Event.Object.Key}");

            if (s3Event.Bucket.Name != "input-test-function")
            {
                context.Logger.LogLine($"Unexpected event received from bucket {s3Event.Bucket.Name}.");
                throw new Exception("Invalid origin bucket.");
            }

            string responseBody;

            try
            {
                // Lê conteúdo do arquivo
                using (GetObjectResponse contentResponse = await this.S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key))
                using (Stream responseStream = contentResponse.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    //string title = response.Metadata["x-amz-meta-title"];
                    //Console.WriteLine("Object metadata, Title: {0}", title);
                    string contentType = contentResponse.Headers["Content-Type"];
                    context.Logger.LogLine($"Content type: {contentType}");

                    responseBody = reader.ReadToEnd();
                    context.Logger.LogLine($"Content body file: {responseBody}");
                }

                // Apaga o arquivo do bucket
                DeleteObjectResponse deleteResponse = await this.S3Client.DeleteObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
                context.Logger.LogLine($"Delete status code: {deleteResponse.HttpStatusCode}");

                // Não será implementado a escrita direto em uma fila SQS. As informações serão enviadas via "destination" juntamente com todas as informações do evento.

                return JsonSerializer.Deserialize<Person>(responseBody);
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error on object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
