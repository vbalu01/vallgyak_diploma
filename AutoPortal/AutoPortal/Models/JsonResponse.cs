using Newtonsoft.Json;

namespace AutoPortal.Models
{
    public class JsonResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
