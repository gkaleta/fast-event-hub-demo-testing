# fast-event-hub-demo-testing
Build a super fast Eventhub demo - where your C# code sends events to Event Hub. 

### What does the code do?

1. Create a producer client to send events to an event hub.
    - The producer client is responsible for transmitting the events to the event hub, using efficient mechanisms that help to ensure
    low-latency and high throughput.
2. Create a batch of events using the producer client.
3. Add events to the batch.
    - The TryAdd method returns a boolean value that indicates if the event was added to the batch. If the batch is full, the TryAdd
    method will return false.
4. Send the batch of events to the event hub.
    - The producer client uses the AMQP protocol to publish events to the event hub. The AMQP protocol is a compact, efficient, and
    widely used protocol for data transmission. The AMQP protocol has built-in security features that help ensure that data is
    safely transmitted.
5. Close the producer client.
    - When an application has completed using a producer or consumer client, it should be closed. This will ensure that network
    resources and other unmanaged objects are properly cleaned 
 '''
 
 
 
 ### Deployment script
```
az deployment group create --resource-group g-eventhubrg --template-file eventhubcreate.bicep --parameters projectname=g-eventhubproject --debug
```

### Execute bicep file
```
az deployment group create --resource-group g-eventhubrg --template-file eventhubcreate.bicep --parameters projectName=project-g   
```
> Note that you need to change the projectName

### Output from event hub bicep should look like this, scroll FAST!
```
{
  "id": "/subscriptions/x/resourceGroups/g-eventhubrg/providers/Microsoft.Resources/deployments/eventhubcreate",
  "location": null,
  "name": "eventhubcreate",
  "properties": {
    "correlationId": "f477156f-8970-4c7b-ba8f-50fc87e9f0a8",
    "debugSetting": null,
    "dependencies": [
      {
        "dependsOn": [
          {
            "id": "/subscriptions/x/resourceGroups/g-eventhubrg/providers/Microsoft.EventHub/namespaces/project-gns",
            "resourceGroup": "g-eventhubrg",
            "resourceName": "project-gns",
            "resourceType": "Microsoft.EventHub/namespaces"
          }
        ],
        "id": "/subscriptions/x/resourceGroups/g-eventhubrg/providers/Microsoft.EventHub/namespaces/project-gns/eventhubs/project-g",
        "resourceGroup": "g-eventhubrg",
        "resourceName": "project-gns/project-g",
        "resourceType": "Microsoft.EventHub/namespaces/eventhubs"
      }
    ],
    "duration": "PT1M17.8158322S",
    "error": null,
    "mode": "Incremental",
    "onErrorDeployment": null,
    "outputResources": [
      {
        "id": "/subscriptions/x/resourceGroups/g-eventhubrg/providers/Microsoft.EventHub/namespaces/project-gns",
        "resourceGroup": "g-eventhubrg"
      },
      {
        "id": "/subscriptions/x/resourceGroups/g-eventhubrg/providers/Microsoft.EventHub/namespaces/project-gns/eventhubs/project-g",
        "resourceGroup": "g-eventhubrg"
      }
    ],
    "outputs": null,
    "parameters": {
      "eventHubSku": {
        "type": "String",
        "value": "Standard"
      },
      "location": {
        "type": "String",
        "value": "northeurope"
      },
      "projectName": {
        "type": "String",
        "value": "project-g"
      }
    },
    "parametersLink": null,
    "providers": [
      {
        "id": null,
        "namespace": "Microsoft.EventHub",
        "providerAuthorizationConsentState": null,
        "registrationPolicy": null,
        "registrationState": null,
        "resourceTypes": [
          {
            "aliases": null,
            "apiProfiles": null,
            "apiVersions": null,
            "capabilities": null,
            "defaultApiVersion": null,
            "locationMappings": null,
            "locations": [
              "northeurope"
            ],
            "properties": null,
            "resourceType": "namespaces",
            "zoneMappings": null
          },
          {
            "aliases": null,
            "apiProfiles": null,
            "apiVersions": null,
            "capabilities": null,
            "defaultApiVersion": null,
            "locationMappings": null,
            "locations": [
              null
            ],
            "properties": null,
            "resourceType": "namespaces/eventhubs",
            "zoneMappings": null
          }
        ]
      }
    ],
    "provisioningState": "Succeeded",
    "templateHash": "391399028417703756",
    "templateLink": null,
    "timestamp": "2023-03-27T12:10:39.614243+00:00",
    "validatedResources": null
  },
  "resourceGroup": "g-eventhubrg",
  "tags": null,
  "type": "Microsoft.Resources/deployments"
}
```

### validate the deployment 
```
az resource list --resource-group g-eventhubrg --debug
```

### create a new dotnet console app 
> Remove --framework if you want to use the default framework
```
dotnet new console --framework net7.0
```

### add the nuget package for event hub if on Windows
```
Install-Package Azure.Messaging.EventHubs
```

### add the nuget package for event hub if on Linux or Mac
```
dotnet add package Azure.Messaging.EventHubs
```

### add the nuget package for indentity
```
dotnet add package Azure.Identity
```

### Get the the connection string from the event hub namespace and hubname and insert it into the code below
```c#
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;

// number of events to be sent to the event hub
int numOfEvents = 3;

// The Event Hubs client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when events are being published or read regularly.
// TODO: Replace the <EVENT_HUB_NAMESPACE> and <HUB_NAME> placeholder values
EventHubProducerClient producerClient = new EventHubProducerClient(
    "xxxxxx.servicebus.windows.net",
    "xxxxx",
    new DefaultAzureCredential());

// Create a batch of events 
using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

for (int i = 1; i <= numOfEvents; i++)
{
    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {i}"))))
    {
        // if it is too large for the batch
        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
    }
}

try
{
    // Use the producer client to send the batch of events to the event hub
    await producerClient.SendAsync(eventBatch);
    Console.WriteLine($"A batch of {numOfEvents} events has been published.");
}
finally
{
    await producerClient.DisposeAsync();
}

```

### If you want to use the connection string from the event hub namespace you can use the command below
```
az eventhubs namespace authorization-rule keys list --resource-group g-eventhubrg --namespace-name project-gns --name RootManageSharedAccessKey --query primaryConnectionString --output tsv
```



