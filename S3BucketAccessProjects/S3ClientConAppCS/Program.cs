using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using S3ClientConAppCS;

Console.WriteLine("===========================================");
Console.WriteLine("    AWS S3 Console App Manager (.NET)      ");
Console.WriteLine("===========================================");



// 1. Paste credentials directly from the AWS Lab "AWS Details" panel
//string aKey = ""; 
//string sKey = "";
//string sToken = ""; 

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("aws-settings.json", optional: false, reloadOnChange: true)
    .Build();

var awsSection = config.GetSection("AWS");
string accessKey = awsSection["AKey"]!;
string secretKey = awsSection["SKey"]!;
string sessionToken = awsSection["SToken"]!;


// Required for Voclabs/Academy
//RegionEndpoint region = RegionEndpoint.USEast1; 
// Typically us-east-1 for AWS Labs

string regionName = awsSection["Region"] ?? "us-east-1";
var region = RegionEndpoint.GetBySystemName(regionName);


// 2. Use SessionAWSCredentials for temporary lab access
var credentials = new SessionAWSCredentials(accessKey, secretKey, sessionToken);
var s3Client = new AmazonS3Client(credentials, region);

// 3. Pass s3Client instance to manager class
var s3Manager = new AWSS3Manager(s3Client);

bool exitApp = false;
while (!exitApp)
{
    Console.WriteLine("\n-------------------------------------------");
    Console.WriteLine("               SELECT AN ACTION             ");
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine(" 1. List All Buckets");
    Console.WriteLine(" 2. Create a Bucket");
    Console.WriteLine(" 3. Delete a Bucket");
    Console.WriteLine(" 4. List Files in a Bucket");
    Console.WriteLine(" 5. Upload a Local File");
    Console.WriteLine(" 6. Upload Text Directly");
    Console.WriteLine(" 7. Read File Content (In Memory)");
    Console.WriteLine(" 8. Download File to Local Disk");
    Console.WriteLine(" 9. Delete a File");
    Console.WriteLine(" 0. Exit");
    Console.WriteLine("-------------------------------------------");
    Console.Write("Select an option (0-9): ");

    string choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            await s3Manager.ListBucketsAsync();
            break;

        case "2":
            Console.Write("Enter new bucket name: ");
            string newBucketName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newBucketName))
                await s3Manager.CreateBucketAsync(newBucketName);
            break;

        case "3":
            Console.Write("Enter bucket name to delete: ");
            string delBucketName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(delBucketName))
                await s3Manager.DeleteBucketAsync(delBucketName);
            break;

        case "4":
            Console.Write("Enter bucket name: ");
            string listBucketName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(listBucketName))
                await s3Manager.ListFilesInBucketAsync(listBucketName);
            break;

        case "5":
            Console.Write("Enter target bucket name: ");
            string uploadBucket = Console.ReadLine()?.Trim();
            Console.Write("Enter full local file path (e.g., C:\\temp\\test.txt): ");
            string localFilePath = Console.ReadLine()?.Trim();
            Console.Write("Enter target S3 key/filename (e.g., test.txt): ");
            string uploadKey = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(uploadBucket) && !string.IsNullOrEmpty(localFilePath) && !string.IsNullOrEmpty(uploadKey))
                await s3Manager.UploadFileAsync(uploadBucket, localFilePath, uploadKey);
            break;

        case "6":
            Console.Write("Enter target bucket name: ");
            string textBucket = Console.ReadLine()?.Trim();
            Console.Write("Enter target S3 key/filename (e.g., notes.txt): ");
            string textKey = Console.ReadLine()?.Trim();
            Console.Write("Enter content to save: ");
            string textContent = Console.ReadLine();

            if (!string.IsNullOrEmpty(textBucket) && !string.IsNullOrEmpty(textKey))
                await s3Manager.UploadTextAsync(textBucket, textKey, textContent);
            break;

        case "7":
            Console.Write("Enter bucket name: ");
            string readBucket = Console.ReadLine()?.Trim();
            Console.Write("Enter S3 file key: ");
            string readKey = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(readBucket) && !string.IsNullOrEmpty(readKey))
                await s3Manager.ReadFileContentAsync(readBucket, readKey);
            break;

        case "8":
            Console.Write("Enter bucket name: ");
            string downloadBucket = Console.ReadLine()?.Trim();
            Console.Write("Enter S3 file key: ");
            string downloadKey = Console.ReadLine()?.Trim();
            Console.Write("Enter local destination file path (e.g., C:\\temp\\downloaded.txt): ");
            string destPath = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(downloadBucket) && !string.IsNullOrEmpty(downloadKey) && !string.IsNullOrEmpty(destPath))
                await s3Manager.DownloadFileAsync(downloadBucket, downloadKey, destPath);
            break;

        case "9":
            Console.Write("Enter bucket name: ");
            string delFileBucket = Console.ReadLine()?.Trim();
            Console.Write("Enter S3 file key to delete: ");
            string delFileKey = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(delFileBucket) && !string.IsNullOrEmpty(delFileKey))
                await s3Manager.DeleteFileAsync(delFileBucket, delFileKey);
            break;

        case "0":
            exitApp = true;
            Console.WriteLine("Exiting S3 Manager. Goodbye!");
            break;

        default:
            Console.WriteLine("Invalid option. Please enter a number between 0 and 9.");
            break;
    }
}

/*

//// 4. Create a unique bucket name
//string newBucketName = "my-lab-bucket-" + Guid.NewGuid().ToString().Substring(0, 8);

//// 5. Execute S3 actions
//await s3Manager.CreateBucketAsync(newBucketName);
//Console.WriteLine("\n-----------------------------------\n");
await s3Manager.ListBucketsAsync();

// 6. List files in the newly created bucket
await s3Manager.ListFilesInBucketAsync("test-s3-jd");

// 7. Read the content of a specific file in the bucket
await s3Manager.ReadFileContentAsync("test-s3-jd", "Sample.txt");
*/