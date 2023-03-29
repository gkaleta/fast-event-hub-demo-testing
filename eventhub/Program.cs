using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;

// number of events to be sent to the event hub
int numOfEvents = 50000;

// The Event Hubs client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when events are being published or read regularly.
// TODO: Replace the <CONNECTION_STRING> and <HUB_NAME> placeholder values
EventHubProducerClient producerClient = new EventHubProducerClient(
    "Endpoint=sb://project-gns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xxxx",
    "project-g");

// Create a batch of events 
using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

for (int i = 1; i <= numOfEvents; i++)
{
    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event{i}"))))
    {
        // if it is too large for the batch
        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
    }
}

try
{
    // Use the producer client to send the batch of events to the event hub
    var currentDate = DateTime.Now;
    var EventData = new EventData("this is my strong body");
    await producerClient.SendAsync(eventBatch);
    Console.WriteLine($"Time: {currentDate} A batch of {numOfEvents} events has been published.");
}
finally
{
    await producerClient.DisposeAsync();
    Console.WriteLine($"Time: {DateTime.Now} Closing the producer client and cleaning up.");
}