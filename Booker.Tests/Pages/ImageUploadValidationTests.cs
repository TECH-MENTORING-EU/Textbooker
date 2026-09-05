using System.Text;
using Booker.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Booker.Tests.Pages;

/// <summary>
/// Characterization of the shared upload gate (commits 8b0e88b/cb6e828): batch
/// limits, extension allow-list, and magic-byte detection that overrides the
/// claimed file name. Any invalid member fails the whole batch.
/// </summary>
public class ImageUploadValidationTests
{
    private static IFormFile Upload(
        byte[] content, string fileName, string contentType = "image/jpeg")
    {
        var file = new FormFile(new MemoryStream(content), 0, content.Length, "Input.Images", fileName)
        {
            // FormFile's ContentType/ContentDisposition setters read Headers - without
            // an initialized HeaderDictionary they throw NullReferenceException.
            Headers = new HeaderDictionary(),
        };
        file.ContentDisposition = $"form-data; name=\"Input.Images\"; filename=\"{fileName}\"";
        file.ContentType = contentType;
        return file;
    }

    private static List<string> Errors(ModelStateDictionary modelState) =>
        modelState["Input.Images"]!.Errors.Select(e => e.ErrorMessage).ToList();

    [Fact]
    public async Task Missing_images_fail_when_required()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(null, requireAtLeastOne: true, modelState);

        Assert.Null(batch);
        Assert.False(modelState.IsValid);
        Assert.Contains(Errors(modelState), e => e.Contains("przynajmniej jedno zdjęcie"));
    }

    [Fact]
    public async Task Missing_images_pass_when_optional()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync([], requireAtLeastOne: false, modelState);

        Assert.NotNull(batch);
        Assert.Empty(batch.Streams);
        Assert.Empty(batch.Extensions);
        Assert.True(modelState.IsValid);
    }

    [Fact]
    public async Task More_than_six_files_fail()
    {
        var modelState = new ModelStateDictionary();
        var files = Enumerable.Range(1, 7).Select(i => Upload(TestImages.Jpeg, $"a{i}.jpg")).ToList();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(files, false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("maksymalnie 6 zdjęć"));
    }

    [Fact]
    public async Task Non_image_content_type_fails()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload(TestImages.Jpeg, "a.pdf", "application/pdf")], false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("nie jest obrazem"));
    }

    [Fact]
    public async Task Empty_file_fails()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload([], "a.jpg")], false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("jest pusty"));
    }

    [Fact]
    public async Task Oversized_file_fails()
    {
        var modelState = new ModelStateDictionary();
        var sixMb = new byte[6 * 1024 * 1024];

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload(sixMb, "a.jpg")], false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("przekracza limit 5 MB"));
    }

    [Fact]
    public async Task Disallowed_extension_fails()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload(TestImages.Jpeg, "a.gif", "image/gif")], false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("niedozwolone rozszerzenie"));
    }

    [Fact]
    public async Task Text_bytes_named_jpg_fail()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload(Encoding.UTF8.GetBytes("definitely not an image"), "fake.jpg")], false, modelState);

        Assert.Null(batch);
        Assert.Contains(Errors(modelState), e => e.Contains("nie jest prawidłowym obrazem"));
    }

    [Fact]
    public async Task Valid_jpeg_returns_the_canonical_extension()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync([Upload(TestImages.Jpeg, "a.jpeg")], false, modelState);

        Assert.NotNull(batch);
        Assert.True(modelState.IsValid);
        var extension = Assert.Single(batch.Extensions);
        Assert.Equal(".jpg", extension); // detected format, not the .jpeg label
    }

    [Fact]
    public async Task Valid_png_is_accepted()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync([Upload(TestImages.Png, "a.png", "image/png")], false, modelState);

        Assert.NotNull(batch);
        Assert.Equal([".png"], batch.Extensions);
    }

    [Fact]
    public async Task One_invalid_member_fails_the_whole_batch()
    {
        var modelState = new ModelStateDictionary();

        var batch = await ImageUploadValidation.ValidateAndReadAsync(
            [Upload(TestImages.Jpeg, "good.jpg"), Upload(Encoding.UTF8.GetBytes("x"), "bad.jpg")], false, modelState);

        Assert.Null(batch);
        Assert.False(modelState.IsValid);
    }
}
