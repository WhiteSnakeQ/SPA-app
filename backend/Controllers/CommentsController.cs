namespace SPA_приложение.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SPA_app.Services.CaptchaS;
    using SPA_app.Services.CommentsS;
    using SPA_app.Services.SeedS;
    using SPA_приложение.DTOs;
    using SPA_приложение.DTOs.Queries;

    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly ICaptchaService _captchaService;

        public CommentsController(ICommentService service, ICaptchaService captchaService)
        {
            _service = service;
            _captchaService = captchaService;
        }

        [HttpPost]
        public async Task<ActionResult<CommentDTO>> Create([FromForm] CreateCommentDTO dto)
        {
            var comment = await _service.Create(dto);
            return Ok( comment );
        }

        [HttpGet]
        public async Task<ActionResult<CommentsPageDTO>> Get([FromQuery] GetCommentsQuery query)
        {
            var result = await _service.GetCommentsCache(query.Page, query.Sort, query.Desc);
            return Ok(result);
        }

        [HttpGet("captcha")]
        public IActionResult GetCaptcha()
        {
            var (id, image) = _captchaService.Generate();

            Response.Headers["Captcha-Id"] = id;

            return File(image, "image/png");
        }

        [HttpGet("seed")]
        public async Task<IActionResult> Seed(
        [FromServices] ISeedService seedService)
        {
            var result = await seedService.SeedComments();
            return Ok(result);
        }
    }
}
