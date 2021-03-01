using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Function.Read.SQS
{
    public class Function
    {
        private readonly string ApiWebHookUrl = "https://webhook.site/4716b594-548a-4f2f-b157-9a2e74cb0f02";
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine($"Processed message {message.MessageId}");


                var convertMessageBody = JsonSerializer.Deserialize<MessageBody>(message.Body);

                context.Logger.LogLine($"Response Paylod: {convertMessageBody.ResponsePayload}");

                var contentBody = new StringContent(JsonSerializer.Serialize(convertMessageBody.ResponsePayload), Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync(ApiWebHookUrl, contentBody);

                context.Logger.LogLine($"Status Code WebHook API: {result.StatusCode}");

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
