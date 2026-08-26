using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;

namespace Booker.Tests.Infrastructure;

/// <summary>
/// Records S3 traffic so tests can assert on object keys, content types, ACLs and delete
/// calls without a real bucket. Built on NSubstitute because the IAmazonS3 interface is
/// far too large to hand-implement.
/// </summary>
public sealed class S3Recorder
{
    public List<PutObjectRequest> Puts { get; } = new();
    public List<DeleteObjectsRequest> Deletes { get; } = new();

    /// <summary>Set true to simulate a storage outage during deletes.</summary>
    public bool FailDeletes { get; set; }

    public IAmazonS3 BuildClient()
    {
        var client = Substitute.For<IAmazonS3>();

        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                Puts.Add(callInfo.ArgAt<PutObjectRequest>(0)!);
                return new PutObjectResponse();
            });

        client.DeleteObjectsAsync(Arg.Any<DeleteObjectsRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.ArgAt<DeleteObjectsRequest>(0)!;
                Deletes.Add(request);
                if (FailDeletes)
                {
                    throw new AmazonS3Exception("simulated storage outage");
                }
                return new DeleteObjectsResponse();
            });

        return client;
    }
}
