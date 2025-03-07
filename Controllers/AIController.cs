using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace LlamaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly Kernel _kernel;

        public AIController(Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost("summarize")]
        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
        {
            string prompt = "Summarize: {{$input}} Today is {{App.GetDate}}.";
            var summarizeFunction = _kernel.CreateFunctionFromPrompt(prompt);

            var result = await _kernel.InvokeAsync(summarizeFunction, new() { ["input"] = request.Text });
            return Ok(new { Summary = result.ToString() });
        }
    }

    public class SummarizeRequest
    {
        public string Text { get; set; }
    }
}
