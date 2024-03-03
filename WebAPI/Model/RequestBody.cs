using Core.Model.Enums;

namespace WebAPI.Model {
    public class RequestBody<T> {
        public T data { get; set; }
        public string secretKey { get; set; }
        public string transaction { get; set; }
    }
}
