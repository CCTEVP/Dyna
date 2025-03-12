using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Dyna.Player.Services
{
    public class QueryParameterService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueryParameterService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetQueryParameterValue(string key)
        {
            if (_httpContextAccessor.HttpContext == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            return _httpContextAccessor.HttpContext.Request.Query.TryGetValue(key, out var value) ? value.ToString() : null;
        }

        public Dictionary<string, string> GetAllQueryParameters()
        {
            var queryParameters = new Dictionary<string, string>();

            if (_httpContextAccessor.HttpContext == null)
            {
                return queryParameters;
            }

            foreach (var queryParam in _httpContextAccessor.HttpContext.Request.Query)
            {
                queryParameters[queryParam.Key] = queryParam.Value.ToString();
            }

            return queryParameters;
        }
    }
}
