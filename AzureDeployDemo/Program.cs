using System;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

public class Program
{
    public static async Task Main(string[] args)
    {
        // If you have multiple subscriptions, you can hard-code one here.
        // Otherwise, we'll just grab the default.
        string subscriptionId = "4f535c00-dc54-40be-8066-d80c04aa2f4b"; // or e.g. "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        string resourceGroupName = "cSharp";
        string storageAccountName = "cSharpTestIt";  // must be globally unique
        string location = "australiaeast";

        // 1) Authenticate (uses az login / VS / managed identity via DefaultAzureCredential)
        ArmClient armClient = new ArmClient(new DefaultAzureCredential());

        // 2) Get subscription
        SubscriptionResource subscription;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            // Use whichever subscription is set as default for your account
            subscription = await armClient.GetDefaultSubscriptionAsync();
        }
        else
        {
            SubscriptionResource sub = armClient.GetSubscriptionResource(
                SubscriptionResource.CreateResourceIdentifier(subscriptionId));
            subscription = sub;
        }

        Console.WriteLine($"Using subscription: {subscription.Id}");

        // 3) Create or update Resource Group
        ResourceGroupData rgData = new ResourceGroupData(location);
        ArmOperation<ResourceGroupResource> rgLro =
            await subscription.GetResourceGroups()
                .CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, rgData);

        ResourceGroupResource resourceGroup = rgLro.Value;
        Console.WriteLine($"Resource group created: {resourceGroup.Data.Name}");

        // 4) Create Storage Account (Blob capable: StorageV2)
        var storageParams = new StorageAccountCreateOrUpdateContent(
            new StorageSku(StorageSkuName.StandardLrs),   // redundancy: LRS
            StorageKind.StorageV2,                        // supports blobs, queues, tables, files
            location);

        ArmOperation<StorageAccountResource> storageLro =
            await resourceGroup.GetStorageAccounts()
                .CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, storageParams);

        StorageAccountResource storageAccount = storageLro.Value;
        Console.WriteLine($"Storage account created: {storageAccount.Data.Name}");
        Console.WriteLine($"Storage account ID: {storageAccount.Id}");
    }
}
