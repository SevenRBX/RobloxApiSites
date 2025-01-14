﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Roblox.ApiClientBase;
using Roblox.Marketplace.Client.Model;
using Roblox.Marketplace.Client.Models;

namespace Roblox.Marketplace.Client
{
    public class MarketplaceV1Client : IMarketplaceV1Client
    {
        private IGuardedApiClientBase clientBase { get; set; }
        
        public MarketplaceV1Client(string url, string apiKey)
        {
            clientBase = new GuardedApiClientBase(url, "v1", apiKey);
        }


        public async Task<IEnumerable<AssetEntry>> GetProductsByAssetId(IEnumerable<long> assetIds)
        {
            // this is temporary until multiget is added to backend
            var tasks = new List<Task<AssetEntry>>();
            foreach (var id in assetIds)
            {
                tasks.Add(GetProductByAssetId(id));
            }

            return await Task.WhenAll(tasks);
        }

        public async Task<AssetEntry> GetProductByAssetId(long assetId)
        {
            var req = new Dictionary<string, string>() { { "assetId", assetId.ToString() } };
            try
            {
                var result = await clientBase.ExecuteHttpRequest(null, HttpMethod.Get, req, null, null, null, null,
                    "GetProductByAssetId");
                return JsonSerializer.Deserialize<AssetEntry>(result.body);
            }
            catch (ApiClientException e)
            {
                if (e.HasError("RecordNotFound"))
                {
                    throw new ProductNotFoundForAsset(assetId, e);
                }
                throw;
            }
        }

        public async Task<CreateResponse> SetProduct(AssetEntry request)
        {
            var result = await clientBase.ExecuteHttpRequest(null, HttpMethod.Post, null, null, null,
                JsonSerializer.Serialize(request), null, "UpdateProductByAssetId");
            return JsonSerializer.Deserialize<CreateResponse>(result.body);
        }
    }
}