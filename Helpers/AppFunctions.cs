using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace LlamaWebApi.Helpers
{
    public class AppFunctions
    {
        [KernelFunction, Description("Get current date")]
        public string GetDate()
        {
            return DateTime.Now.ToString("MMMM dd, yyyy");
        }
    }
}
