namespace SPA_приложение.DTOs
{
	public class CreateCommentDTO
	{
		public Guid RequestId { get; set; }
		public Guid RootId { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? Homepage { get; set; }
		public string Text { get; set; }
		public int? ParentId { get; set; }

		public List<IFormFile>? Files { get; set; }

		public string CaptchaId { get; set; }
		public string CaptchaAnswer { get; set; }
	}
}
