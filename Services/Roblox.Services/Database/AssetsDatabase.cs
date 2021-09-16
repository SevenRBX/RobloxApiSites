using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Roblox.Services.Models.Assets;

namespace Roblox.Services.Database
{
    public class AssetsDatabase : IAssetsDatabase
    {
        private IDatabaseConnectionProvider db { get; set; }
        
        public AssetsDatabase(DatabaseConfiguration<dynamic> config)
        {
            db = config.dbConnection;
        }

        public async Task<IEnumerable<AssetEntry>> MultiGetAssetsById(IEnumerable<long> assetIds)
        {
            return await db.connection.QueryAsync<AssetEntry>("SELECT id as assetId, name, description, type_id as assetTypeId, creator_id as creatorId, creator_type as creatorType, created_at as created, updated_at as updated FROM asset WHERE asset.id = ANY (@id)", new
            {
                id = assetIds,
            });
        }

        public async Task<Models.Assets.InsertAssetResponse> InsertAsset(InsertAssetRequest request)
        {
            var result = await db.connection.QuerySingleOrDefaultAsync<Models.Assets.InsertAssetResponse>(
                "INSERT INTO asset (creator_id, creator_type, name, description, type_id) VALUES (@creator_id, @creator_type, @name, @description, @type_id) RETURNING asset.id as assetId, created_at as created",
                new
                {
                    creator_id = request.creatorId,
                    creator_type = request.creatorType,
                    name = request.name,
                    description = request.description,
                    type_id = request.assetType,
                });
            return result;
        }

        public async Task UpdateAsset(Models.Assets.UpdateAssetRequest request)
        {
            // no updating of type_id is intentional - this would break tons of stuff
            await db.connection.ExecuteAsync(
                "UPDATE asset SET name = @name, description = @description, creator_id = @creator_id, creator_type = @creator_type, updated_at = @updated_at WHERE id = @id",
                new
                {
                    name = request.name,
                    description = request.description,
                    creator_id = request.creatorId,
                    creator_type = request.creatorType,
                    updated_at = DateTime.Now,
                    id = request.assetId,
                });
        }
    }
}