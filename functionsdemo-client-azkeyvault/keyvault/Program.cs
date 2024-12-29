using System;
/*
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;


class Program
{
    static async Task Main(string[] args)
    {
        // Azure Key Vault URL (replace with your Key Vault URL)
        string keyVaultUrl = "https://sjkeyvaultdemo.vault.azure.net/";

        // The certificate name (replace with your certificate's name in Key Vault)
        string certificateName = "sjcertificate";

        // Azure Function URL (replace with your Azure Function's URL)        
        string functionUrl = @"https://sjfunctionappdemo.azurewebsites.net/api/calcsum?param1=100&param2=200&code=YZh4wdtqnNP0mhsDibz72ddqlNUqLfQ_jkFLxmDP1zxaAzFuYCtITg%3D%3D";

        // Authenticate using Azure CLI credentials
        var credential = new AzureCliCredential();

        // Create the CertificateClient to interact with Key Vault
        var certificateClient = new CertificateClient(new Uri(keyVaultUrl), credential);

        try
        {
            // Retrieve the certificate from Azure Key Vault (without private key)
            var certificate = await certificateClient.GetCertificateAsync(certificateName);

            // Retrieve the private key (if necessary, from the Key Vault secrets)
            var privateKey = await certificateClient.GetCertificateVersionAsync(certificateName, certificate.Value.Properties.Version);

            // Load the certificate and private key into X509Certificate2
            X509Certificate2 clientCertificate = new X509Certificate2(privateKey.Value.Cer);

            // Create an HttpClientHandler and add the client certificate
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(clientCertificate);

            // Create an HttpClient with the handler containing the client certificate
            using var client = new HttpClient(handler);

            // Make the GET request to the Azure Function
            var response = await client.GetAsync(functionUrl);

            // Output the response status code
            Console.WriteLine("Response Status Code: " + response.StatusCode);

            // Read and output the content of the response (if any)
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Content: " + content);

            // Check if the response was successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Request was successful.");
            }
            else
            {
                Console.WriteLine("Request failed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
*/
using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

class Program
{
    static async Task Main(string[] args)
    {
        // Azure Function endpoint
        string azureFunctionUrl = "https://sjfunctionappdemo.azurewebsites.net/api/CalcSum?param1=100&param2=200&code=Ep4lU7h6ipAzBjuxR6fcWmsZZEl6CKicyJyxQ9DuD9EpAzFucJlXjA%3D%3D";

        // Load the client certificate (from file, store, or Azure Key Vault)
        X509Certificate2 clientCertificate = LoadClientCertificate();

        if (clientCertificate == null)
        {
            Console.WriteLine("Client certificate not found!");
            return;
        }
        // Create an HttpClientHandler and set the client certificate
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(clientCertificate);

        // Optional: Bypass SSL errors (not recommended for production)
        //handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        // Create HttpClient with the handler
        using var client = new HttpClient(handler);

        try
        {
            // Make a GET request to the Azure Function
            var response = await client.GetAsync(azureFunctionUrl);

            // Check the response
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseBody}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    private static X509Certificate2 LoadClientCertificate()
    {
        string keyVaultUrl = "https://sjkeyvaultdemo.vault.azure.net/";
        string certificateName = "sjcertificate";

        // Authenticate with DefaultAzureCredential (adjust if needed)
        var credential = new DefaultAzureCredential();

        // Create a CertificateClient to access the Key Vault
        var certificateClient = new CertificateClient(new Uri(keyVaultUrl), credential);

        // Retrieve the certificate with policy
        var certificateWithPolicy = certificateClient.GetCertificate(certificateName);

        // Extract the secret associated with the certificate
        var secretClient = new Azure.Security.KeyVault.Secrets.SecretClient(new Uri(keyVaultUrl), credential);
        var secret = secretClient.GetSecret(certificateName);

        // Decode the PFX
        byte[] pfxBytes = Convert.FromBase64String(secret.Value.Value);

        // Load the certificate
        var certificate = new X509Certificate2(pfxBytes, (string)null, X509KeyStorageFlags.MachineKeySet);

        // Use the certificate (example: print details)
        Console.WriteLine($"Subject: {certificate.Subject}");
        Console.WriteLine($"Issuer: {certificate.Issuer}");
        Console.WriteLine($"Thumbprint: {certificate.Thumbprint}");

        return certificate;        
    }
}
