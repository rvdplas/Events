#r "nuget: FluentAssertions, 6.1.0"
#r "nuget: Moq, 4.16.1"
#r "nuget: Newtonsoft.Json, 12.0.3"
#r "nuget: Remotion.Linq, 2.2.0"
#r "nuget: Stylelabs.M.Scripting.Types, *"
#r "nuget: Stylelabs.M.Sdk, *"

#load "nuget:ScriptUnit, 0.2.0"
#load "../C - DAM - Remove Public Links.csx"

using System.Collections.Generic;
using System.Runtime;
using System.Threading.Tasks;

using static ScriptUnit;
using FluentAssertions;
using Moq;

using Stylelabs.M.Base.Querying;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Clients;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Logging;
using Stylelabs.M.Sdk.Contracts.Querying;
using Stylelabs.M.Sdk.Factories;
using System;
using Stylelabs.M.Framework.Essentials.LoadOptions;

Console.Clear();
return await AddTestsFrom<TestCases>().Execute();

public class TestCases
{
    private readonly Mock<Stylelabs.M.Sdk.IMClient> _mclientMock;
    private readonly Mock<IActionScriptContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IQueryingClient> _queryingClientMock;
    private readonly Mock<IEntityFactory> _entityFactoryMock;
    private readonly Mock<IEntitiesClient> _entities;

    public TestCases()
    {
        _contextMock = new Mock<IActionScriptContext>();
        _loggerMock = new Mock<ILogger>();
        _mclientMock = new Mock<IMClient>();
        _queryingClientMock = new Mock<IQueryingClient>();
        _entityFactoryMock = new Mock<IEntityFactory>();
        _entities = new Mock<IEntitiesClient>();

        _mclientMock.Setup(x => x.Querying).Returns(_queryingClientMock.Object);
        _mclientMock.Setup(x => x.Logger).Returns(_loggerMock.Object);
        _contextMock.Setup(x => x.TargetId).Returns(1337);
        _mclientMock.Setup(x => x.EntityFactory).Returns(_entityFactoryMock.Object);
        _mclientMock.Setup(x => x.Entities).Returns(_entities.Object);
    }

    public async void EmptyTarget_Should_LogDebugMessage()
    {
        // arrange
        _contextMock.Setup(x => x.Target).Returns((IEntity) null);

        // act
        await ExecuteScriptAsync(_mclientMock.Object, _contextMock.Object);

        // assert
        _loggerMock.Verify(x => x.Debug(It.IsAny<string>()));
    }

    public async void TargetWithoutPublicLinks_Should_LogDebugMessage()
    {
        // arrange
        long entityId = 123456;

        var entity = new Mock<IEntity>();
        entity.Setup(x => x.Id).Returns(entityId);

        _contextMock.Setup(x => x.Target).Returns(entity.Object);

        // act
        await ExecuteScriptAsync(_mclientMock.Object, _contextMock.Object);

        // assert
        _loggerMock.Verify(x => x.Debug($"Target (entityID: {entityId}) doesn't have public links."));
    }


     public async void TargetWithPublicLinks_Should_SaveAsset()
    {
        // arrange
        var fakeRelations = new FakeRelationPublicLink();
        fakeRelations.Add(123);
        fakeRelations.Add(456);

        var entity = new Mock<IEntity>();
        entity.Setup(x => x.Id).Returns(123456);
        entity.Setup(x => x.GetRelationAsync<IParentToManyChildrenRelation>("AssetToPublicLink", MemberLoadOption.LazyLoading))
        .ReturnsAsync(fakeRelations);

        _contextMock.Setup(x => x.Target).Returns(entity.Object);

        // act
        await ExecuteScriptAsync(_mclientMock.Object, _contextMock.Object);

        // assert
        fakeRelations.Children.Count.Should().Be(0);

        //mClient.Entities.SaveAsync(assetEntity);
        _entities.Verify(x => x.SaveAsync(It.IsAny<IEntity>()));
    }
}

internal class FakeRelationPublicLink : IParentToManyChildrenRelation
{
    private IList<long> _children = new List<long>();

    public IList<long> Children => _children;

    public RelationRole Role => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public MemberDefinitionType DefinitionType => throw new NotImplementedException();

    public bool IsTracking => throw new NotImplementedException();

    public bool IsDirty => throw new NotImplementedException();

    public void Add(long id)
    {
        _children.Add(id);
    }

    public void AddRange(IEnumerable<long> ids)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        _children.Clear();
    }

    public IList<long> GetIds()
    {
        throw new NotImplementedException();
    }

    public void MarkClean()
    {
        throw new NotImplementedException();
    }

    public void SetIds(IEnumerable<long> ids)
    {
        throw new NotImplementedException();
    }

    public void StartTracking()
    {
        throw new NotImplementedException();
    }
}
