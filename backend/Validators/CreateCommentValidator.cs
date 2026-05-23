namespace SPA_приложение.Validators
{
	using FluentValidation;
	using SixLabors.ImageSharp;
	using SPA_приложение.DTOs;
	using SPA_приложение.Helpers;

	public class CreateCommentValidator : AbstractValidator<CreateCommentDTO>
	{
		private readonly string[] imageExtensions =
		{
			".jpg",
			".png",
			".gif"
		};

		public CreateCommentValidator()
		{
            RuleFor(x => x.RequestId)
				.NotEqual(Guid.Empty)
				.WithMessage("RequestId is required");

            RuleFor(x => x.UserName)
				.NotEmpty()
				.Matches("^[a-zA-Z0-9]+$");

			RuleFor(x => x.Email)
				.EmailAddress();

			RuleFor(x => x.Homepage)
				.Must(HtmlSanitizerHelper.BeValidUrl)
				.WithMessage("Invalid URL");

			RuleFor(x => x.Text)
				.Cascade(CascadeMode.Stop)
				.Must(HtmlSanitizerHelper.HaveTextContent)
				.WithMessage("Text cannot be empty")
				.Must(HtmlSanitizerHelper.IsValid)
				.WithMessage("Invalid HTML");

			RuleForEach(x => x.Files)
				.Must(BeValidFile)
				.When(x => x.Files != null)
				.WithMessage((dto, file) => $"Invalid file: {file.FileName}");
		}

		private bool BeValidFile(IFormFile file)
		{
			var extension = System.IO.Path.GetExtension(file.FileName).ToLower();

			if (extension == ".txt")
				return file.Length <= 100 * 1024;

			if (!imageExtensions.Contains(extension))
				return false;

			try
			{
				using var stream = file.OpenReadStream();

				var imageInfo = Image.Identify(stream);

				return imageInfo != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
