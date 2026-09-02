using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace S3ClientConAppCS
{
    public class AWSS3Manager
    {
        private readonly IAmazonS3 _s3Client;

        public AWSS3Manager(IAmazonS3 s3Client)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        #region Bucket Operations

        /// <summary>
        /// Checks if an S3 bucket exists and is accessible.
        /// </summary>
        public async Task<bool> DoesBucketExistAsync(string bucketName)
        {
            try
            {
                return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking bucket existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lists all buckets available to the current credentials.
        /// </summary>
        public async Task<List<S3Bucket>> ListBucketsAsync()
        {
            var buckets = new List<S3Bucket>();
            try
            {
                var response = await _s3Client.ListBucketsAsync();
                buckets = response.Buckets;

                Console.WriteLine("S3 Buckets List:");
                foreach (var bucket in buckets)
                {
                    Console.WriteLine($"- Bucket: {bucket.BucketName} | Created: {bucket.CreationDate}");
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error (ListBuckets): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return buckets;
        }

        /// <summary>
        /// Creates a new S3 bucket if it does not already exist.
        /// </summary>
        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            try
            {
                if (await DoesBucketExistAsync(bucketName))
                {
                    Console.WriteLine($"Bucket '{bucketName}' already exists.");
                    return false;
                }

                var request = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                var response = await _s3Client.PutBucketAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"Success! Bucket '{bucketName}' was created successfully.");
                    return true;
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (CreateBucket): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Deletes an empty S3 bucket.
        /// </summary>
        public async Task<bool> DeleteBucketAsync(string bucketName)
        {
            try
            {
                var request = new DeleteBucketRequest
                {
                    BucketName = bucketName
                };

                var response = await _s3Client.DeleteBucketAsync(request);
                Console.WriteLine($"Bucket '{bucketName}' deleted successfully.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (DeleteBucket): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        #endregion

        #region Object Operations

        /// <summary>
        /// Uploads a local file to S3.
        /// </summary>
        public async Task<bool> UploadFileAsync(string bucketName, string filePath, string key)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: Local file '{filePath}' not found.");
                    return false;
                }

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    FilePath = filePath
                };

                await _s3Client.PutObjectAsync(request);
                Console.WriteLine($"Uploaded file '{filePath}' to bucket '{bucketName}' with key '{key}'.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (UploadFile): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Uploads string content directly to S3 without creating a local file.
        /// </summary>
        public async Task<bool> UploadTextAsync(string bucketName, string key, string content)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = content
                };

                await _s3Client.PutObjectAsync(request);
                Console.WriteLine($"Uploaded text content to bucket '{bucketName}' with key '{key}'.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (UploadText): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Downloads a file from S3 and saves it to a local file path.
        /// </summary>
        public async Task<bool> DownloadFileAsync(string bucketName, string key, string destinationPath)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                await response.WriteResponseStreamToFileAsync(destinationPath, append: false, CancellationToken.None);

                Console.WriteLine($"Downloaded file from bucket '{bucketName}' with key '{key}' to '{destinationPath}'.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (DownloadFile): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Reads and returns the text content of a file directly from S3.
        /// </summary>
        public async Task<string> ReadFileContentAsync(string bucketName, string fileKey)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var responseStream = response.ResponseStream;
                using var reader = new StreamReader(responseStream);

                string content = await reader.ReadToEndAsync();

                Console.WriteLine($"\n--- Content of '{fileKey}' ---");
                Console.WriteLine(content);
                Console.WriteLine("-----------------------------------");

                return content;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error (ReadFile): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Lists all objects inside a specific bucket with pagination support.
        /// </summary>
        public async Task<List<S3Object>> ListFilesInBucketAsync(string bucketName)
        {
            var objects = new List<S3Object>();

            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };

                ListObjectsV2Response response;

                Console.WriteLine($"\nFiles in bucket '{bucketName}':");

                // Loop handles pagination for buckets with more than 1,000 objects
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);
                    objects.AddRange(response.S3Objects);

                    foreach (S3Object entry in response.S3Objects)
                    {
                        Console.WriteLine($"- Key: {entry.Key} | Size: {entry.Size} bytes | Last Modified: {entry.LastModified}");
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while ((bool)response.IsTruncated);

                if (objects.Count == 0)
                {
                    Console.WriteLine("  (Bucket is empty)");
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error (ListFiles): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return objects;
        }

        /// <summary>
        /// Deletes a specific object from an S3 bucket.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string bucketName, string key)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                Console.WriteLine($"Deleted file '{key}' from bucket '{bucketName}'.");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error (DeleteFile): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        #endregion
    }
}