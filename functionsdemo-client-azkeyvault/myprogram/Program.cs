using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Define the URL with query parameters
        string url = @"https://sjfunctionappdemo.azurewebsites.net/api/calcsum?param1=100&param2=200&code=YZh4wdtqnNP0mhsDibz72ddqlNUqLfQ_jkFLxmDP1zxaAzFuYCtITg%3D%3D";

        // Azure Key Vault URL and certificate name
        string keyVaultUrl = "https://sjkeyvaultdemo.vault.azure.net/";
        string certificateName = "sjcertificate";
        string privateKeySecretName = "https://sjkeyvaultdemo.vault.azure.net/secrets/sjcertificate/ada2eb0b9bf94d539ed588990ab883f2";  // Secret name that stores the private key (if required)

        // Create a CertificateClient using Managed Identity or Azure AD credentials
        var credential = new ManagedIdentityCredential();
        var certificateClient = new CertificateClient(new Uri(keyVaultUrl), credential);
        var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

        try
        {
            // Retrieve the certificate from Key Vault
            KeyVaultCertificate certificate = await certificateClient.GetCertificateAsync(certificateName);

            // If you need the private key, retrieve it from Key Vault (stored as a secret)
            KeyVaultSecret privateKeySecret = await secretClient.GetSecretAsync(privateKeySecretName);

            // Convert the Key Vault certificate to an X509Certificate2 object
            X509Certificate2 certificateObj = new X509Certificate2(certificate.Cer);

            // If you have the private key in a separate secret, you can combine it here
            // Create the certificate with the private key (if needed):
            X509Certificate2 certificateWithPrivateKey = new X509Certificate2(certificateObj.RawData, privateKeySecret.Value);

            // Set up the HttpClientHandler with the certificate
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificateWithPrivateKey);

            var httpClient = new HttpClient(handler);

            // Make the HTTP GET request
            var response = await httpClient.GetAsync(url);

            // Output the status code of the response
            Console.WriteLine("Response Status Code: " + response.StatusCode);

            // Read and output the content of the response (if it's not empty)
            string content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Content: " + content);

            // Optionally, check if the response was successful (2xx status code)
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("The request was successful.");
            }
            else
            {
                Console.WriteLine("The request failed.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}
