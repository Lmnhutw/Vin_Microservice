using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        public readonly IHttpContextAccessor _ctxAccessor;

        public TokenProvider(IHttpContextAccessor ctxAccessor)
        {
            _ctxAccessor = ctxAccessor;
        }

        public void ClearedToken()
        {
            _ctxAccessor.HttpContext?.Response.Cookies.Delete(StaticDetail.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;
            bool? hasToken = _ctxAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetail.TokenCookie, out token);
            return hasToken is true ? token : null;
        }

        public void SetToken(string token)
        {
            _ctxAccessor.HttpContext?.Response.Cookies.Append(StaticDetail.TokenCookie, token);
        }
    }
}
