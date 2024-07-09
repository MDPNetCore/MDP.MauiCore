﻿using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class RemoteInteropProvider : InteropProvider
    {
        // Constants
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


        // Fields
        private readonly IHttpClientFactory _httpClientFactory = null;


        // Constructors
        public RemoteInteropProvider(IHttpClientFactory httpClientFactory)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(httpClientFactory);

            #endregion

            // Default
            _httpClientFactory = httpClientFactory;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest, InteropResource interopResource)
        {
            #region Contracts

            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (interopRequest == null) throw new ArgumentNullException(nameof(interopRequest));
            if (interopResource == null) throw new ArgumentNullException(nameof(interopResource));

            #endregion

            // HttpClient
            var httpClient = _httpClientFactory.CreateClient("DefaultService");
            if (httpClient == null) throw new InvalidOperationException($"{nameof(httpClient)}=null");

            // GetAsync
            var interopResponse = await httpClient.PostAsync<InteropResponse>("/.blazor/interop/invoke", content: new
            {
                serviceUri = $"{httpClient.BaseAddress.Scheme}://{httpClient.BaseAddress.Host}{interopRequest.ServiceUri.PathAndQuery}",
                methodName = interopRequest.MethodName,
                methodParameters = interopRequest.MethodParameters
            });
            if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

            // MethodInfo
            var methodInfo = interopResource.ServiceType.GetMethod(interopRequest.MethodName);
            if (methodInfo == null) throw new InvalidOperationException($"{nameof(methodInfo)}=null");

            // ResultType
            var resultType = methodInfo.ReturnType;
            {
                // Task
                if (resultType == typeof(Task))
                {
                    resultType = null;
                }

                // Task<>
                if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    resultType = resultType.GetGenericArguments().FirstOrDefault();
                }
            }
            if (resultType == null) return interopResponse;

            // ResultString
            var resultString = string.Empty;
            if (string.IsNullOrEmpty(resultString) == true && interopResponse.Result is JsonElement) resultString = this.CreateString((JsonElement)interopResponse.Result);
            if (string.IsNullOrEmpty(resultString) == true && interopResponse.Result != null) resultString = interopResponse.Result.ToString();

            // Result
            interopResponse.Result = this.CreateResult(resultType, resultString);

            // Return
            return interopResponse;
        }

        private object CreateResult(Type resultType, string resultString = null)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(resultType);

            #endregion

            // String
            if (resultType == typeof(string))
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = string.Empty;

                // Result
                var result = resultString;

                // Return
                return result;
            }

            // Convertible
            if (typeof(IConvertible).IsAssignableFrom(resultType) == true)
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = string.Empty;

                // Result
                var result = Convert.ChangeType(resultString, resultType);

                // Return
                return result;
            }

            // Deserialize
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = "{}";

                // Result
                var result = System.Text.Json.JsonSerializer.Deserialize(resultString, resultType, _serializerOptions);

                // Return
                return result;
            }
        }

        private string CreateString(JsonElement jsonElement)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(jsonElement);

            #endregion

            switch (jsonElement.ValueKind)
            {
                // Null 
                case JsonValueKind.Null:
                    return null;

                // Number
                case JsonValueKind.Number:
                    return jsonElement.GetRawText();

                // String
                case JsonValueKind.String:
                    return jsonElement.GetString();
                
                // Bool
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean().ToString();
               
                // Object
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return jsonElement.GetRawText();

                // Unknown
                default:
                    throw new InvalidOperationException($"{nameof(jsonElement.ValueKind)}={jsonElement.ValueKind}");
            }
        }
    }
}
