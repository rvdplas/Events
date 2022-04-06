#load "./references/Action.csx"
/**------------ Include above to support intellisense on Content Hub types in editor ----------------**/
// Script Start
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Contracts.Base;

using System.Linq;
using System.Threading.Tasks;

await RunScriptAsync(MClient, Context);

public static async Task RunScriptAsync(IMClient mClient, IActionScriptContext context)
{
    if (mClient == null)
    {
        return;
    }

    if (context == null)
    {
        return;
    }

    await ExecuteScriptAsync(mClient, context);
}

private static async Task ExecuteScriptAsync(IMClient mClient, IActionScriptContext context){

    var assetEntity = context.Target as IEntity;

    if(assetEntity == null)
    {
        return;
    }

    var relation = await assetEntity.GetRelationAsync<IParentToManyChildrenRelation>("AssetToPublicLink");
    if(relation == null || !relation.Children.Any())
    {
        return;
    }

    relation.Children.Clear();
    await mClient.Entities.SaveAsync(assetEntity);
}
